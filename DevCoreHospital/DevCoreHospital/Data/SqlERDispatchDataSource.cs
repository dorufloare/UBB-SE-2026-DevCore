using DevCoreHospital.Configuration;
using DevCoreHospital.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevCoreHospital.Data
{
    public sealed class SqlERDispatchDataSource : IERDispatchDataSource
    {
        private readonly string _connectionString;

        public SqlERDispatchDataSource(string? connectionString = null)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? AppSettings.ConnectionString
                : connectionString;

            EnsureReq4Schema();
        }

        public IReadOnlyList<DoctorProfile> GetAvailableDoctors()
        {
            return GetDoctorsByStatus(DoctorStatus.AVAILABLE);
        }

        public IReadOnlyList<DoctorProfile> GetDoctorsInExamination()
        {
            return GetDoctorsByStatus(DoctorStatus.IN_EXAMINATION);
        }

        public IReadOnlyList<DoctorProfile> GetDoctorsNotWorkingNow()
        {
            var doctors = LoadDoctorProfiles(DateTime.Now);

            return doctors
                .Where(d => !d.ScheduleStart.HasValue || !d.ScheduleEnd.HasValue)
                .Where(d => d.Status == DoctorStatus.OFF_DUTY || d.Status == DoctorStatus.AVAILABLE)
                .OrderBy(d => d.FullName)
                .ToList();
        }

        public IReadOnlyList<ERRequest> GetPendingRequests()
        {
            return LoadAllRequests()
                .Where(request => IsPendingStatus(request.Status))
                .OrderBy(request => request.CreatedAt)
                .ToList();
        }

        public int CreateIncomingRequest(string specialization, string location)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO dbo.ER_Requests (specialization, [location], created_at, [status], assigned_doctor_id, assigned_doctor_name)
                        OUTPUT INSERTED.request_id
                        VALUES (@Specialization, @Location, GETDATE(), 'PENDING', NULL, NULL);";

                    AddParameter(command, "@Specialization", specialization);
                    AddParameter(command, "@Location", location);

                    return (int)command.ExecuteScalar()!;
                }
            }
        }

        public ERRequest? GetRequestById(int requestId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT request_id, specialization, location, created_at, status, assigned_doctor_id, assigned_doctor_name
                        FROM dbo.ER_Requests
                        WHERE request_id = @RequestId;";
                    AddParameter(command, "@RequestId", requestId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new ERRequest
                        {
                            Id = reader.GetInt32(0),
                            Specialization = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Location = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            CreatedAt = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                            Status = reader.IsDBNull(4) ? "PENDING" : reader.GetString(4),
                            AssignedDoctorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            AssignedDoctorName = reader.IsDBNull(6) ? null : reader.GetString(6)
                        };
                    }
                }
            }
        }

        public DoctorProfile? GetDoctorById(int doctorId)
        {
            var now = DateTime.Now;
            var staff = GetStaffById(doctorId);
            if (staff is null || !IsDoctorRole(staff.Role))
                return null;

            var activeShift = GetShiftsByStaffId(doctorId)
                .Where(shift => IsShiftActiveNow(shift, now))
                .OrderBy(shift => shift.StartTime)
                .FirstOrDefault();

            return BuildDoctorProfile(staff, activeShift);
        }

        public void UpdateRequestStatus(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE dbo.ER_Requests
                        SET status = @Status,
                            assigned_doctor_id = @AssignedDoctorId,
                            assigned_doctor_name = @AssignedDoctorName
                        WHERE request_id = @RequestId;";

                    AddParameter(command, "@Status", status);
                    AddParameter(command, "@AssignedDoctorId", (object?)assignedDoctorId ?? DBNull.Value);
                    AddParameter(command, "@AssignedDoctorName", (object?)assignedDoctorName ?? DBNull.Value);
                    AddParameter(command, "@RequestId", requestId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDoctorStatus(int doctorId, DoctorStatus status)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Staff
                        SET status = @Status
                        WHERE staff_id = @DoctorId;";
                    AddParameter(command, "@Status", status.ToString());
                    AddParameter(command, "@DoctorId", doctorId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private IReadOnlyList<DoctorProfile> GetDoctorsByStatus(DoctorStatus status)
        {
            var doctors = LoadDoctorProfiles(DateTime.Now);

            return doctors
                .Where(d => d.Status == status)
                .Where(d => d.ScheduleStart.HasValue && d.ScheduleEnd.HasValue)
                .OrderBy(d => d.FullName)
                .ToList();
        }

        private IReadOnlyList<DoctorProfile> LoadDoctorProfiles(DateTime now)
        {
            var staffRows = GetAllStaffRows();
            var shiftRows = GetAllShiftRows();

            var activeShiftByStaffId = shiftRows
                .Where(shift => IsShiftActiveNow(shift, now))
                .GroupBy(shift => shift.StaffId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(shift => shift.StartTime).First());

            return staffRows
                .Where(staff => IsDoctorRole(staff.Role))
                .Select(staff =>
                {
                    activeShiftByStaffId.TryGetValue(staff.StaffId, out var activeShift);
                    return BuildDoctorProfile(staff, activeShift);
                })
                .ToList();
        }

        private IReadOnlyList<ERRequest> LoadAllRequests()
        {
            var requests = new List<ERRequest>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT request_id, specialization, location, created_at, status, assigned_doctor_id, assigned_doctor_name
                        FROM dbo.ER_Requests
                        ORDER BY created_at;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ERRequest
                            {
                                Id = reader.GetInt32(0),
                                Specialization = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                Location = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                CreatedAt = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                                Status = reader.IsDBNull(4) ? "PENDING" : reader.GetString(4),
                                AssignedDoctorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                AssignedDoctorName = reader.IsDBNull(6) ? null : reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return requests;
        }

        private StaffRow? GetStaffById(int staffId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT staff_id, first_name, last_name, role, specialization, status
                        FROM Staff
                        WHERE staff_id = @StaffId;";
                    AddParameter(command, "@StaffId", staffId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new StaffRow
                        {
                            StaffId = reader.GetInt32(0),
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Role = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Specialization = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                    }
                }
            }
        }

        private IReadOnlyList<StaffRow> GetAllStaffRows()
        {
            var staffRows = new List<StaffRow>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT staff_id, first_name, last_name, role, specialization, status
                        FROM Staff
                        ORDER BY staff_id;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            staffRows.Add(new StaffRow
                            {
                                StaffId = reader.GetInt32(0),
                                FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Role = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Specialization = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return staffRows;
        }

        private IReadOnlyList<ShiftRow> GetShiftsByStaffId(int staffId)
        {
            var shiftRows = new List<ShiftRow>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT staff_id, location, start_time, end_time, is_active, status
                        FROM Shifts
                        WHERE staff_id = @StaffId;";
                    AddParameter(command, "@StaffId", staffId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shiftRows.Add(new ShiftRow
                            {
                                StaffId = reader.GetInt32(0),
                                Location = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                StartTime = reader.GetDateTime(2),
                                EndTime = reader.GetDateTime(3),
                                IsActive = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
                                Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return shiftRows;
        }

        private IReadOnlyList<ShiftRow> GetAllShiftRows()
        {
            var shiftRows = new List<ShiftRow>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT staff_id, location, start_time, end_time, is_active, status
                        FROM Shifts;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shiftRows.Add(new ShiftRow
                            {
                                StaffId = reader.GetInt32(0),
                                Location = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                StartTime = reader.GetDateTime(2),
                                EndTime = reader.GetDateTime(3),
                                IsActive = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
                                Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return shiftRows;
        }

        private static DoctorProfile BuildDoctorProfile(StaffRow staff, ShiftRow? activeShift)
        {
            return new DoctorProfile
            {
                DoctorId = staff.StaffId,
                FullName = BuildFullName(staff.FirstName, staff.LastName),
                Specialization = ResolveSpecialization(staff.Specialization),
                Status = ParseStatus(staff.Status),
                Location = activeShift?.Location ?? string.Empty,
                ScheduleStart = activeShift?.StartTime,
                ScheduleEnd = activeShift?.EndTime
            };
        }

        private static string BuildFullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }

        private static string ResolveSpecialization(string specialization)
        {
            return string.IsNullOrWhiteSpace(specialization) ? "General" : specialization.Trim();
        }

        private static bool IsShiftActiveNow(ShiftRow shift, DateTime now)
        {
            if (shift.StartTime > now || shift.EndTime < now)
                return false;

            if (shift.IsActive.HasValue && shift.IsActive.Value)
                return true;

            return string.Equals(NormalizeToken(shift.Status), "ACTIVE", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPendingStatus(string status)
        {
            return string.Equals(NormalizeToken(status), "PENDING", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDoctorRole(string role)
        {
            return string.Equals(NormalizeToken(role), "DOCTOR", StringComparison.OrdinalIgnoreCase);
        }

        private static DoctorStatus ParseStatus(string? raw)
        {
            var token = NormalizeToken(raw).Replace(" ", "_");
            return Enum.TryParse<DoctorStatus>(token, true, out var status)
                ? status
                : DoctorStatus.OFF_DUTY;
        }

        private static string NormalizeToken(string? raw)
        {
            return (raw ?? string.Empty).Trim();
        }

        private static void AddParameter(SqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private sealed class StaffRow
        {
            public int StaffId { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Specialization { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        private sealed class ShiftRow
        {
            public int StaffId { get; set; }
            public string Location { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool? IsActive { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        private void EnsureReq4Schema()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        IF OBJECT_ID(N'dbo.ER_Requests', N'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.ER_Requests
                            (
                                request_id INT IDENTITY(101,1) PRIMARY KEY,
                                specialization VARCHAR(100) NOT NULL,
                                [location] VARCHAR(100) NOT NULL,
                                created_at DATETIME NOT NULL CONSTRAINT DF_ER_Requests_created_at DEFAULT GETDATE(),
                                [status] VARCHAR(50) NOT NULL,
                                assigned_doctor_id INT NULL,
                                assigned_doctor_name VARCHAR(200) NULL,
                                CONSTRAINT CK_ER_Requests_status CHECK (UPPER([status]) IN ('PENDING','ASSIGNED','UNMATCHED','COMPLETED')),
                                CONSTRAINT FK_ER_Requests_staff FOREIGN KEY (assigned_doctor_id) REFERENCES dbo.Staff(staff_id)
                            );
                        END;";

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

