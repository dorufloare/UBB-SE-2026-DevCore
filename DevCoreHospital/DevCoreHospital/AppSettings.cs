using Microsoft.UI.Xaml.Media;

namespace DevCoreHospital.Configuration;

public static class AppSettings
{
    public const string ConnectionString =
        @"Data Source=.\SQLEXPRESS;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;User ID=gaboruu;Password=gabi123;";

    public static int DefaultDoctorId { get; set; } = 1;
}
