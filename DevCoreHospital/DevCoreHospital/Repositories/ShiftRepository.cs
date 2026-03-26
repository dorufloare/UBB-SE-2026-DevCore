using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevCoreHospital.Data;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public class ShiftRepository
    {
        private List<Shift> _shiftList;
        private DatabaseManager _dbManager;

        public ShiftRepository(DatabaseManager dbManager)
        {
            this._shiftList = new List<Shift>();
            this._dbManager = dbManager;
        }

        public void LoadShifts()
        {
            this._shiftList = _dbManager.GetShifts();
        }
        public void AddShift(Shift newShift)
        {
            // Here you would add code to save the new shift to the database
            // For now, we will just add it to the local list
            _shiftList.Add(newShift);
        }
        public void CancelShift(int shiftId)
        {
            var shiftToCancel = _shiftList.FirstOrDefault(shift => shift.Id == shiftId);
            if (shiftToCancel != null)
            {
                // Here you would add code to remove the shift from the database
                // For now, we will just remove it from the local list
                _shiftList.Remove(shiftToCancel);
            }
        }
        public List<Shift> GetShifts()
        {
            return _shiftList;
        }
        public List<Shift> GetShiftsByStaffID(int staffId)
        {
            var shifts = _shiftList.Where(shift => shift.AppointedStaff.StaffID == staffId).ToList();
            return shifts;
        }
        public List<Shift> GetActiveShifts()
        {
            var activeShifts = _shiftList.Where(shift => shift.Status == ShiftStatus.ACTIVE).ToList();
            return activeShifts;
        }
        public float GetWeeklyHours(int staffId)
        {
            var shifts = GetShiftsByStaffID(staffId);
            float totalHours = 0;
            var monday = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
            var sunday = monday.AddDays(6);

            foreach (var shift in shifts)
            {
                if (shift.StartTime >= monday && shift.EndTime <= sunday)
                {
                    totalHours += (float)(shift.EndTime - shift.StartTime).TotalHours;
                }
            }
            return totalHours;
        }

        public void UpdateShiftStatus(int shiftId, ShiftStatus status)
        {
            var shiftToUpdate = _shiftList.FirstOrDefault(shift => shift.Id == shiftId);
            if (shiftToUpdate != null)
            {
                shiftToUpdate.Status = status;
                // Here you would add code to update the shift status in the database
            }
        }
    }
}
