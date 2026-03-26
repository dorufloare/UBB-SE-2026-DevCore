using System.Collections.Generic;
using DevCoreHospital.Models;
using System.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace DevCoreHospital.Data
{
    public class DatabaseManager
    {
        public string connectionString { get; set; }
        
        public List<Staff> GetStaff()
        {
            // return some dummy data for now, we will implement the actual database connection later
            List<Staff> staffList = new List<Staff>();
            staffList.Add(new Doctor(1, "John", "Doe", "0700-000 000", true, "Cardiology", "12345", DoctorStatus.AVAILABLE));
            staffList.Add(new Doctor(2, "Jane", "Smith", "0700-000 001", false, "Neurology", "54321", DoctorStatus.IN_EXAMINATION));
            staffList.Add(new Doctor(3, "Emily", "Johnson", "0700-000 002", true, "Pediatrics", "67890", DoctorStatus.OFF_DUTY));
            staffList.Add(new Pharmacyst(4, "Anna", "Doe", "0700-000 003", false, "BPS"));
            staffList.Add(new Pharmacyst(5, "Mary", "Christmas", "0700-000 004", true, "ASHP"));
            return staffList;
        }

        public List<Shift> GetShifts()
        {
            // return some dummy data for now, we will implement the actual database connection later
            List<Shift> shiftList = new List<Shift>();
            shiftList.Add(new Shift("1", "1", "Cardiology", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE));
            shiftList.Add(new Shift("2", "2", "Neurology", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED));
            shiftList.Add(new Shift("3", "3", "Pediatrics", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.COMPLETED));
            return shiftList;
        }
}