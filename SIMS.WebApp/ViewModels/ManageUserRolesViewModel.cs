// File: SIMS.WebApp/ViewModels/ManageUserRolesViewModel.cs
// Chúng ta sẽ tái sử dụng và sửa đổi file này cho đơn giản

namespace SIMS.WebApp.ViewModels
{
    // ViewModel này bây giờ dùng để quản lý một vai trò duy nhất
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // Chứa tên của vai trò đang được chọn
        public string SelectedRole { get; set; } = string.Empty;

        // Danh sách tất cả các vai trò có sẵn trong hệ thống
        public List<string> Roles { get; set; } = new();
    }
}
