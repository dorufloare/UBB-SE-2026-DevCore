using DevCoreHospital.Models;

namespace DevCoreHospital.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private static UserRole roleType = UserRole.Doctor;

        public int UserId { get; } = 1;

        public UserRole RoleType
        {
            get => roleType;
            set => roleType = value;
        }

        public string Role => RoleType.ToString();
    }
}