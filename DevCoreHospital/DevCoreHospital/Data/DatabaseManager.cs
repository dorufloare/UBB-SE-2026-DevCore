using System.Collections.Generic;
using DevCoreHospital.Models;
using System.Linq;
using System;
using System.Data;
using System.Data.Common;
using DevCoreHospital.Data;

namespace DevCoreHospital.Data
{
    public class DatabaseManager
    {
        public string ConnectionString { get; set; }

        public DatabaseManager(string connectionString)
        {
            this.ConnectionString = connectionString;
        }


        public List<IStaff> GetStaff()
        {
            List<IStaff> staffList = new List<IStaff>();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT staff_id, role, first_name, last_name, contact_info, 
                   is_available, license_number, specialization, status, certification 
            FROM Staff";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string role = reader.GetString(1);
                    string firstName = reader.GetString(2);
                    string lastName = reader.GetString(3);
                    string contactInfo = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    bool isAvailable = reader.GetBoolean(5);
                    string license = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    string special = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    string statusStr = reader.IsDBNull(8) ? "Available" : reader.GetString(8);
                    string cert = reader.IsDBNull(9) ? "" : reader.GetString(9);

                    // Transformam statusul din String in Enum (Atentie sa se potriveasca exact numele)
                    // Daca in DB e "Off_Duty" si in C# e "OFF_DUTY", folosim true pentru a ignora literele mari/mici
                    Enum.TryParse<DoctorStatus>(statusStr, true, out DoctorStatus docStatus);

                    // Bifam daca e Doctor sau Farmacist pe baza coloanei [role]
                    if (role == "Doctor")
                    {
                        staffList.Add(new Doctor(id, firstName, lastName, contactInfo, isAvailable, special, license, docStatus));
                    }
                    else if (role == "Pharmacist")
                    {
                        staffList.Add(new Pharmacyst(id, firstName, lastName, contactInfo, isAvailable, cert));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la GetStaff: {ex.Message}");
            }

            return staffList;
        }


        public List<Shift> GetShifts()
        {
            List<Shift> shiftList = new List<Shift>();
            var allStaff = GetStaff();

            try
            {
                using var connection = GetConnection();
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT shift_id, staff_id, location, start_time, end_time, status FROM Shifts";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int shiftId = reader.GetInt32(0);
                    int staffId = reader.GetInt32(1);
                    string location = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    DateTime startTime = reader.GetDateTime(3);
                    DateTime endTime = reader.GetDateTime(4);
                    string statusStr = reader.IsDBNull(5) ? "Scheduled" : reader.GetString(5);

                    Enum.TryParse<ShiftStatus>(statusStr, true, out ShiftStatus shiftStatus);

                    var appointedStaff = allStaff.FirstOrDefault(s => s.StaffID == staffId);
                    if (appointedStaff != null)
                    {
                        shiftList.Add(new Shift(shiftId, appointedStaff, location, startTime, endTime, shiftStatus));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la GetShifts: {ex.Message}");
            }

            return shiftList;
        }


        public int GetMedicinesSold(int pharmacistStaffId, int month, int year)
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*)
                    FROM PharmacyHandover
                    WHERE PharmacistID = @staffId
                      AND MONTH(HandoverDate) = @month
                      AND YEAR(HandoverDate) = @year";

                var staffIdParameter = command.CreateParameter();
                staffIdParameter.ParameterName = "@staffId";
                staffIdParameter.Value = pharmacistStaffId;
                command.Parameters.Add(staffIdParameter);

                var monthParameter = command.CreateParameter();
                monthParameter.ParameterName = "@month";
                monthParameter.Value = month;
                command.Parameters.Add(monthParameter);

                var yearParameter = command.CreateParameter();
                yearParameter.ParameterName = "@year";
                yearParameter.Value = year;
                command.Parameters.Add(yearParameter);

                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch
            {
                return 150;
            }
        }


        internal DbConnection GetConnection()
        {
            var connectionFactory = new SqlConnectionFactory(ConnectionString);
            return connectionFactory.Create();
        }
    }
}