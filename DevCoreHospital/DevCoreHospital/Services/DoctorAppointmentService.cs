using DevCoreHospital.Data;
using DevCoreHospital.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DevCoreHospital.Services
{
    public sealed class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly SqlConnectionFactory _sqlFactory;

        public DoctorAppointmentService(SqlConnectionFactory sqlFactory)
        {
            _sqlFactory = sqlFactory;
        }

        // ====================================================================
        // METODELE VECHI
        // ====================================================================

        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skip, int take)
        {
            var items = new List<Appointment>();

            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var doctorsTable = await ResolveDoctorsTableAsync(conn);
            var appointmentsTable = await ResolveAppointmentsTableAsync(conn);

            var from = fromDate.Date;
            var to = from.AddDays(8);

            var sql = $@"
SELECT 
    a.Id, a.DoctorId, d.FirstName + ' ' + d.LastName AS DoctorName, a.PatientName,
    CAST(a.[Date] AS datetime2) AS [Date], a.StartTime, a.EndTime, 
    ISNULL(a.Status, '') AS [Status], ISNULL(a.Type, '') AS [Type], ISNULL(a.Location, '') AS [Location]
FROM {appointmentsTable} a
INNER JOIN {doctorsTable} d ON d.id = a.DoctorId
WHERE a.DoctorId = @DoctorId
  AND CAST(a.[Date] AS date) >= @FromDate
  AND CAST(a.[Date] AS date) < @ToDate
ORDER BY a.[Date], a.StartTime
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParameter(cmd, "@DoctorId", doctorUserId);
            AddParameter(cmd, "@FromDate", from);
            AddParameter(cmd, "@ToDate", to);
            AddParameter(cmd, "@Skip", skip);
            AddParameter(cmd, "@Take", take);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(MapReaderToAppointment(reader));
            }
            return items;
        }

        public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync()
        {
            var result = new List<(int DoctorId, string DoctorName)>();

            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var doctorsTable = await ResolveDoctorsTableAsync(conn);

            var sql = $"SELECT id AS DoctorId, FirstName + ' ' + LastName AS DoctorName FROM {doctorsTable} ORDER BY FirstName;";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add((GetInt(reader, "DoctorId"), GetString(reader, "DoctorName")));

            return result;
        }

        public async Task<AppointmentDetails?> GetAppointmentDetailsAsync(int appointmentId)
        {
            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var appointmentsTable = await ResolveAppointmentsTableAsync(conn);
            var sql = $"SELECT * FROM {appointmentsTable} WHERE Id = @Id;";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParameter(cmd, "@Id", appointmentId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new AppointmentDetails
            {
                Id = GetInt(reader, "Id"),
                DoctorId = GetInt(reader, "DoctorId"),
                Date = GetDateTime(reader, "Date"),
                StartTime = GetTimeSpan(reader, "StartTime"),
                EndTime = GetTimeSpan(reader, "EndTime"),
                Status = GetNullableString(reader, "Status"),
                Type = GetNullableString(reader, "Type"),
                Location = GetNullableString(reader, "Location")
            };
        }

        // ====================================================================
        // METODELE TALE NOI PENTRU ADMIN (Implementează regulile de business)
        // ====================================================================

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId)
        {
            var items = new List<Appointment>();
            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var appointmentsTable = await ResolveAppointmentsTableAsync(conn);
            var sql = $"SELECT * FROM {appointmentsTable} WHERE DoctorId = @DoctorId ORDER BY [Date], StartTime;";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParameter(cmd, "@DoctorId", doctorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new Appointment
                {
                    Id = GetInt(reader, "Id"),
                    DoctorId = GetInt(reader, "DoctorId"),
                    PatientName = GetString(reader, "PatientName"),
                    Date = GetDateTime(reader, "Date"),
                    StartTime = GetTimeSpan(reader, "StartTime"),
                    EndTime = GetTimeSpan(reader, "EndTime"),
                    Status = GetNullableString(reader, "Status")
                });
            }
            return items;
        }

        public async Task BookAppointmentAsync(Appointment appointment)
        {
            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var table = await ResolveAppointmentsTableAsync(conn);
            var sql = $@"
                INSERT INTO {table} (PatientName, DoctorId, Date, StartTime, EndTime, Status) 
                VALUES (@PatientName, @DoctorId, @Date, @StartTime, @EndTime, 'Scheduled')";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParameter(cmd, "@PatientName", appointment.PatientName);
            AddParameter(cmd, "@DoctorId", appointment.DoctorId);
            AddParameter(cmd, "@Date", appointment.Date.Date);
            AddParameter(cmd, "@StartTime", appointment.StartTime);
            AddParameter(cmd, "@EndTime", appointment.EndTime);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task FinishAppointmentAsync(Appointment appointment)
        {
            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var apptTable = await ResolveAppointmentsTableAsync(conn);
            var docTable = await ResolveDoctorsTableAsync(conn);

            // 1. Setăm statusul la Finished
            using DbCommand cmdUpdateAppt = conn.CreateCommand();
            cmdUpdateAppt.CommandText = $"UPDATE {apptTable} SET Status = 'Finished' WHERE Id = @Id";
            AddParameter(cmdUpdateAppt, "@Id", appointment.Id);
            await cmdUpdateAppt.ExecuteNonQueryAsync();

            // 2. REGULA DE BUSINESS: Verificăm dacă doctorul mai are alte programări "Scheduled"
            using DbCommand cmdCheck = conn.CreateCommand();
            cmdCheck.CommandText = $"SELECT COUNT(*) FROM {apptTable} WHERE DoctorId = @DocId AND Status = 'Scheduled'";
            AddParameter(cmdCheck, "@DocId", appointment.DoctorId);
            int activeAppointments = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());

            // 3. Dacă nu mai are, îl punem AVAILABLE
            if (activeAppointments == 0)
            {
                using DbCommand cmdUpdateDoc = conn.CreateCommand();
                cmdUpdateDoc.CommandText = $"UPDATE {docTable} SET DoctorStatus = 'AVAILABLE' WHERE id = @DocId";
                AddParameter(cmdUpdateDoc, "@DocId", appointment.DoctorId);
                await cmdUpdateDoc.ExecuteNonQueryAsync();
            }
        }

        public async Task CancelAppointmentAsync(Appointment appointment)
        {
            using DbConnection conn = _sqlFactory.Create();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var table = await ResolveAppointmentsTableAsync(conn);
            var sql = $"UPDATE {table} SET Status = 'Canceled' WHERE Id = @Id";

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParameter(cmd, "@Id", appointment.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ====================================================================
        // METODE DE AJUTOR (PRIVATE)
        // ====================================================================

        private Appointment MapReaderToAppointment(DbDataReader reader)
        {
            return new Appointment
            {
                Id = GetInt(reader, "Id"),
                DoctorId = GetInt(reader, "DoctorId"),
                DoctorName = GetNullableString(reader, "DoctorName"),
                PatientName = GetNullableString(reader, "PatientName"),
                Date = GetDateTime(reader, "Date"),
                StartTime = GetTimeSpan(reader, "StartTime"),
                EndTime = GetTimeSpan(reader, "EndTime"),
                Status = GetNullableString(reader, "Status"),
                Type = GetNullableString(reader, "Type"),
                Location = GetNullableString(reader, "Location")
            };
        }

        private static async Task<string> ResolveDoctorsTableAsync(DbConnection conn)
        {
            var candidates = new[] { "[Doctors]", "[dbo].[Doctors]", "[doctor]" };
            foreach (var t in candidates)
                if (await TableExistsWithColumns(conn, t, "FirstName")) return t;
            return "Doctors"; // Fallback
        }

        private static async Task<string> ResolveAppointmentsTableAsync(DbConnection conn)
        {
            var candidates = new[] { "[Appointments]", "[dbo].[Appointments]", "[appointment]" };
            foreach (var t in candidates)
                if (await TableExistsWithColumns(conn, t, "DoctorId", "PatientName")) return t;
            return "Appointments"; // Fallback
        }

        private static async Task<bool> TableExistsWithColumns(DbConnection conn, string tableExpression, params string[] requiredColumns)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT TOP 0 * FROM {tableExpression};";
                using var reader = await cmd.ExecuteReaderAsync();

                var schema = reader.GetColumnSchema();
                var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in schema)
                    if (!string.IsNullOrWhiteSpace(c.ColumnName))
                        cols.Add(c.ColumnName!);

                foreach (var req in requiredColumns)
                    if (!cols.Contains(req)) return false;

                return true;
            }
            catch { return false; }
        }

        private static void AddParameter(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static int GetInt(DbDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
        private static string GetString(DbDataReader r, string col) => r.GetString(r.GetOrdinal(col));
        private static string GetNullableString(DbDataReader r, string col)
        {
            try { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? string.Empty : Convert.ToString(r.GetValue(i)) ?? string.Empty; }
            catch { return string.Empty; } // Fallback dacă lipsește coloana în vechile query-uri
        }
        private static DateTime GetDateTime(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col); var v = r.GetValue(i);
            return v is DateTime dt ? dt : Convert.ToDateTime(v);
        }
        private static TimeSpan GetTimeSpan(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col); var val = r.GetValue(i);
            if (val is TimeSpan ts) return ts;
            if (val is DateTime dt) return dt.TimeOfDay;
            return TimeSpan.Parse(val?.ToString() ?? "00:00:00");
        }
    }
}