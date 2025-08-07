// File: SIMS.WebApp/ViewModels/UnifiedUserViewModel.cs
namespace SIMS.WebApp.ViewModels
{
    public class UnifiedUserViewModel
    {
        public string? UserId { get; set; } // Sẽ là null nếu chỉ là hồ sơ SV
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
    }
}