// File: Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SIMS.Domain;

namespace SIMS.WebApp.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        // === PHƯƠNG THỨC NÀY ĐÃ ĐƯỢC THAY ĐỔI HOÀN TOÀN ===
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Vẫn trả về thành công để không tiết lộ thông tin người dùng
                    return new JsonResult(new { success = true, message = "Nếu tài khoản của bạn tồn tại, một email đã được gửi đi." });
                }

                // Tạo mã reset
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Logic gửi email (được giả lập)
                // await _emailSender.SendEmailAsync(...)

                // Trả về thành công VÀ KÈM THEO MÃ RESET cho mục đích demo
                return new JsonResult(new { success = true, resetCode = code });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return new JsonResult(new { success = false, errors = errors });
        }
    }
}