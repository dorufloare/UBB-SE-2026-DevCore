using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories;

public interface IStaffRepository
{
    Doctor? GetDoctorBySpecialization(string spec);

    IStaff? FindByStaffCode(string staffCode);
}
