namespace DevCoreHospital.Models
{
    public interface IStaff
    {
        int StaffID { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string ContactInfo { get; set; }
        bool Available { get; set; }
    }
}