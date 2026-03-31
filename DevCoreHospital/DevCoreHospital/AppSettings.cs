using Microsoft.UI.Xaml.Media;

namespace DevCoreHospital.Configuration;

public static class AppSettings
{
    public const string ConnectionString =
        @"Data Source=PATRICKPC\SQLEXPRESS;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

    public const int DefaultDoctorId = 1;
}



/*
 
<x:String>Emergency</x:String>
<x:String>Cardiologist</x:String>
<x:String>Surgeon</x:String>
<x:String>Neurology</x:String>
<x:String>Pediatrics</x:String>
<x:String>Oncologist</x:String>
<x:String>Pharmacist BPS</x:String>
<x:String>Pharmacist BCACP</x:String>
<x:String>Pharmacist BCCP</x:String>
<x:String>Pharmacist BCCCP</x:String>
<x:String>Pharmacist BCIDP</x:String>
 
 */