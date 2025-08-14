// File: SIMS.WebApp/ViewModels/UnifiedUserViewModel.cs
namespace SIMS.WebApp.ViewModels
{
    public class UnifiedUserViewModel
    {
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // THAY ĐỔI: Sử dụng boolean thay vì string để kiểm tra
        public bool HasAccount { get; set; }
        public bool IsBlocked { get; set; }
    }
}