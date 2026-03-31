using DevCoreHospital.Configuration;
using DevCoreHospital.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevCoreHospital.Data
{
    public sealed class SqlFatigueShiftDataSource : IFatigueShiftDataSource
    {
        private const double MaxWeeklyHours = 60.0;
        private static readonly TimeSpan MinRestGap = TimeSpan.FromHours(12);

        private readonly string _connectionString;

        public SqlFatigueShiftDataSource(string? connectionString = null)
        {
            _connectionString = string.IsNullOrWhiteSpace(connectionString)
                ? AppSettings.ConnectionString
                : connectionString;
        }

        public IReadOnlyList<RosterShift> GetShiftsForWeek(DateTime weekStart)
        {
            var shifts = new List<RosterShift>();
            var start = StartOfWeek(weekStart);
            var end = start.AddDays(7);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT sh.shift_id,
                               sh.staff_id,
                               st.first_name,
                               st.last_name,
                               st.role,
                               st.specialization,
                               st.certification,
                               sh.start_time,
                               sh.end_time,
                               sh.status
                        FROM Shifts sh
                        INNER JOIN Staff st ON st.staff_id = sh.staff_id
                        WHERE sh.start_time < @WeekEnd
                          AND sh.end_time > @WeekStart
                        ORDER BY sh.start_time;";

                    AddParameter(command, "@WeekStart", start);
                    AddParameter(command, "@WeekEnd", end);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                            if (IsCancelledStatus(status))
                                continue;

                            shifts.Add(new RosterShift
                            {
                                Id = reader.GetInt32(0),
                                StaffId = reader.GetInt32(1),
                                StaffName = BuildFullName(
                                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3)),
                                Role = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Specialization = ResolveSpecialization(
                                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    reader.IsDBNull(6) ? string.Empty : reader.GetString(6)),
                                Start = reader.GetDateTime(7),
                                End = reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return shifts;
        }

        public IReadOnlyList<RosterShift> GetAllShifts()
        {
            var shifts = new List<RosterShift>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT sh.shift_id,
                               sh.staff_id,
                               st.first_name,
                               st.last_name,
                               st.role,
                               st.specialization,
                               st.certification,
                               sh.start_time,
                               sh.end_time,
                               sh.status
                        FROM Shifts sh
                        INNER JOIN Staff st ON st.staff_id = sh.staff_id
                        ORDER BY sh.start_time;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                            if (IsCancelledStatus(status))
                                continue;

                            shifts.Add(new RosterShift
                            {
                                Id = reader.GetInt32(0),
                                StaffId = reader.GetInt32(1),
                                StaffName = BuildFullName(
                                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3)),
                                Role = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Specialization = ResolveSpecialization(
                                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    reader.IsDBNull(6) ? string.Empty : reader.GetString(6)),
                                Start = reader.GetDateTime(7),
                                End = reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return shifts;
        }

        public IReadOnlyList<StaffProfile> GetStaffProfiles()
        {
            var profiles = new List<StaffProfile>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var staffSchema = GetStaffSchemaCapabilities(connection);

                using (SqlCommand command = connection.CreateCommand())
                {
                    var isAvailableProjection = staffSchema.HasIsAvailable ? "is_available" : "NULL AS is_available";
                    var isActiveProjection = staffSchema.HasIsActive ? "is_active" : "NULL AS is_active";
                    var statusProjection = staffSchema.HasStatus ? "[status]" : "NULL AS [status]";

                    command.CommandText = $@"
                        SELECT staff_id,
                               first_name,
                               last_name,
                               role,
                               specialization,
                               certification,
                               {isAvailableProjection},
                               {isActiveProjection},
                               {statusProjection}
                        FROM Staff
                        ORDER BY staff_id;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                            if (staffSchema.HasIsActive && !GetBooleanOrDefault(reader, 7, true))
                                continue;
                            if (staffSchema.HasStatus && IsInactiveStatus(status))
                                continue;

                            profiles.Add(new StaffProfile
                            {
                                StaffId = reader.GetInt32(0),
                                FullName = BuildFullName(
                                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2)),
                                Role = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Specialization = ResolveSpecialization(
                                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5)),
                                IsAvailable = staffSchema.HasIsAvailable
                                    ? GetBooleanOrDefault(reader, 6, true)
                                    : true
                            });
                        }
                    }
                }
            }

            return profiles;
        }

        public double GetMonthlyWorkedHours(int staffId, int year, int month)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    var monthStart = new DateTime(year, month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    command.CommandText = @"
                        SELECT start_time, end_time, status
                        FROM Shifts
                        WHERE staff_id = @StaffId;";

                    AddParameter(command, "@StaffId", staffId);

                    var totalHours = 0.0;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            if (IsCancelledStatus(status))
                                continue;

                            totalHours += GetOverlapHours(
                                reader.GetDateTime(0),
                                reader.GetDateTime(1),
                                monthStart,
                                monthEnd);
                        }
                    }

                    return totalHours;
                }
            }
        }

        public bool ReassignShift(int shiftId, int newStaffId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        DateTime? shiftStart = null;
                        DateTime? shiftEnd = null;

                        using (SqlCommand loadShift = connection.CreateCommand())
                        {
                            loadShift.Transaction = transaction;
                            loadShift.CommandText = "SELECT start_time, end_time FROM Shifts WHERE shift_id = @ShiftId;";
                            AddParameter(loadShift, "@ShiftId", shiftId);

                            using (SqlDataReader reader = loadShift.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    shiftStart = reader.GetDateTime(0);
                                    shiftEnd = reader.GetDateTime(1);
                                }
                            }
                        }

                        if (!shiftStart.HasValue || !shiftEnd.HasValue)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        var staffSchema = GetStaffSchemaCapabilities(connection, transaction);

                        using (SqlCommand candidateCheck = connection.CreateCommand())
                        {
                            candidateCheck.Transaction = transaction;

                            var isAvailableProjection = staffSchema.HasIsAvailable ? "is_available" : "NULL AS is_available";
                            var isActiveProjection = staffSchema.HasIsActive ? "is_active" : "NULL AS is_active";
                            var statusProjection = staffSchema.HasStatus ? "[status]" : "NULL AS [status]";

                            candidateCheck.CommandText = $@"
                                SELECT {isAvailableProjection},
                                       {isActiveProjection},
                                       {statusProjection}
                                FROM Staff
                                WHERE staff_id = @NewStaffId;";
                            AddParameter(candidateCheck, "@NewStaffId", newStaffId);

                            using (SqlDataReader reader = candidateCheck.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    transaction.Rollback();
                                    return false;
                                }

                                if (staffSchema.HasIsAvailable && !GetBooleanOrDefault(reader, 0, true))
                                {
                                    transaction.Rollback();
                                    return false;
                                }

                                if (staffSchema.HasIsActive && !GetBooleanOrDefault(reader, 1, true))
                                {
                                    transaction.Rollback();
                                    return false;
                                }

                                var candidateStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                                if (staffSchema.HasStatus && IsInactiveStatus(candidateStatus))
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }

                        var existingShifts = new List<(DateTime Start, DateTime End)>();
                        var overlapShifts = new List<(DateTime Start, DateTime End)>();
                        using (SqlCommand candidateShiftsCommand = connection.CreateCommand())
                        {
                            candidateShiftsCommand.Transaction = transaction;
                            candidateShiftsCommand.CommandText = @"
                                SELECT start_time, end_time, status
                                FROM Shifts
                                WHERE staff_id = @NewStaffId
                                  AND shift_id <> @ShiftId;";
                            AddParameter(candidateShiftsCommand, "@NewStaffId", newStaffId);
                            AddParameter(candidateShiftsCommand, "@ShiftId", shiftId);

                            using (SqlDataReader reader = candidateShiftsCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var existingStart = reader.GetDateTime(0);
                                    var existingEnd = reader.GetDateTime(1);
                                    var existingStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                                    if (!IsCancelledStatus(existingStatus))
                                        existingShifts.Add((existingStart, existingEnd));

                                    if (IsBlockingStatusForOverlap(existingStatus))
                                        overlapShifts.Add((existingStart, existingEnd));
                                }
                            }
                        }

                        if (overlapShifts.Any(s => s.Start < shiftEnd.Value && s.End > shiftStart.Value))
                        {
                            transaction.Rollback();
                            return false;
                        }

                        if (!RespectsRestGap(shiftStart.Value, shiftEnd.Value, existingShifts))
                        {
                            transaction.Rollback();
                            return false;
                        }

                        var weekStart = StartOfWeek(shiftStart.Value);
                        var weekEnd = weekStart.AddDays(7);
                        var existingHours = existingShifts.Sum(s => GetOverlapHours(s.Start, s.End, weekStart, weekEnd));
                        var reassignedHours = GetOverlapHours(shiftStart.Value, shiftEnd.Value, weekStart, weekEnd);
                        if (existingHours + reassignedHours > MaxWeeklyHours)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        using (SqlCommand update = connection.CreateCommand())
                        {
                            update.Transaction = transaction;
                            update.CommandText = "UPDATE Shifts SET staff_id = @NewStaffId WHERE shift_id = @ShiftId;";
                            AddParameter(update, "@NewStaffId", newStaffId);
                            AddParameter(update, "@ShiftId", shiftId);

                            var rows = update.ExecuteNonQuery();
                            transaction.Commit();
                            return rows > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

        private static StaffSchemaCapabilities GetStaffSchemaCapabilities(SqlConnection connection, SqlTransaction? transaction = null)
        {
            return new StaffSchemaCapabilities(
                ColumnExists(connection, transaction, "Staff", "is_active"),
                ColumnExists(connection, transaction, "Staff", "is_available"),
                ColumnExists(connection, transaction, "Staff", "status"));
        }

        private static bool ColumnExists(SqlConnection connection, SqlTransaction? transaction, string tableName, string columnName)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @TableName
                      AND COLUMN_NAME = @ColumnName;";

                AddParameter(command, "@TableName", tableName);
                AddParameter(command, "@ColumnName", columnName);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static string BuildFullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }

        private static string ResolveSpecialization(string specialization, string certification)
        {
            if (!string.IsNullOrWhiteSpace(specialization))
                return specialization;
            if (!string.IsNullOrWhiteSpace(certification))
                return certification;

            return "General";
        }

        private static bool IsCancelledStatus(string status)
        {
            return string.Equals(status.Trim(), "CANCELLED", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsInactiveStatus(string status)
        {
            return string.Equals(status.Trim(), "INACTIVE", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlockingStatusForOverlap(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return true;

            var normalized = status.Trim();
            return normalized.Equals("SCHEDULED", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase);
        }

        private static bool GetBooleanOrDefault(SqlDataReader reader, int ordinal, bool defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetBoolean(ordinal);
        }

        private static bool RespectsRestGap(DateTime proposedStart, DateTime proposedEnd, IReadOnlyList<(DateTime Start, DateTime End)> existingShifts)
        {
            var previousShift = existingShifts
                .Where(s => s.End <= proposedStart)
                .OrderByDescending(s => s.End)
                .FirstOrDefault();
            if (previousShift != default && (proposedStart - previousShift.End) < MinRestGap)
                return false;

            var nextShift = existingShifts
                .Where(s => s.Start >= proposedEnd)
                .OrderBy(s => s.Start)
                .FirstOrDefault();
            if (nextShift != default && (nextShift.Start - proposedEnd) < MinRestGap)
                return false;

            return true;
        }

        private static double GetOverlapHours(DateTime shiftStart, DateTime shiftEnd, DateTime windowStart, DateTime windowEnd)
        {
            var overlapStart = shiftStart > windowStart ? shiftStart : windowStart;
            var overlapEnd = shiftEnd < windowEnd ? shiftEnd : windowEnd;
            return overlapEnd <= overlapStart ? 0 : (overlapEnd - overlapStart).TotalHours;
        }

        private static void AddParameter(SqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private sealed class StaffSchemaCapabilities
        {
            public StaffSchemaCapabilities(bool hasIsActive, bool hasIsAvailable, bool hasStatus)
            {
                HasIsActive = hasIsActive;
                HasIsAvailable = hasIsAvailable;
                HasStatus = hasStatus;
            }

            public bool HasIsActive { get; }
            public bool HasIsAvailable { get; }
            public bool HasStatus { get; }
        }
    }
}

