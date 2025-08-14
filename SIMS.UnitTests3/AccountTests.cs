using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SIMS.Domain;
using SIMS.WebApp.Areas.Identity.Pages.Account;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SIMS.UnitTests
{
    public class AccountTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<ILogger<LoginModel>> _mockLogger;
        private readonly Mock<IUserStore<ApplicationUser>> _mockUserStore;

        public AccountTests()
        {
            // Set up mock objects for the dependencies of LoginModel
            _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object);

            _mockLogger = new Mock<ILogger<LoginModel>>();
        }

        [Fact]
        public async Task OnPostAsync_ValidCredentials_RedirectsToHomePage()
        {
            // Arrange (Successful login with dong1@gamil.com)
            var loginModel = new LoginModel(_mockSignInManager.Object, _mockLogger.Object, _mockUserManager.Object)
            {
                Input = new LoginModel.InputModel
                {
                    Email = "dong1@gamil.com",
                    Password = "Dong@123",
                    RememberMe = false
                }
            };

            // Mock SignInManager to return success
            _mockSignInManager.Setup(s => s.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            // Act
            var result = await loginModel.OnPostAsync(returnUrl: "/");

            // Assert
            var localRedirectResult = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal("/", localRedirectResult.Url);
        }

        [Fact]
        public async Task OnPostAsync_IncorrectPassword_ReturnsPageWithValidationError()
        {
            // Arrange (Incorrect password scenario)
            var loginModel = new LoginModel(_mockSignInManager.Object, _mockLogger.Object, _mockUserManager.Object)
            {
                Input = new LoginModel.InputModel
                {
                    Email = "dong2@gamil.com",
                    Password = "Dong@12345679",
                    RememberMe = false
                }
            };

            // Mock SignInManager to return failure
            _mockSignInManager.Setup(s => s.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>())).ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await loginModel.OnPostAsync(returnUrl: "/");

            // Assert (This is the point that causes the test to fail)
            // We know the actual error should be "Invalid login attempt.",
            // but intentionally check for an incorrect message to force failure
            Assert.IsType<PageResult>(result);
            Assert.False(loginModel.ModelState.IsValid);
            Assert.Contains(loginModel.ModelState.Keys, k => k == string.Empty);

            // This line will fail because the error message does not match
            Assert.Equal("This is a wrong error message.", loginModel.ModelState[string.Empty].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task OnPostAsync_UnconfirmedAccount_ReturnsPageWithIsNotAllowedError()
        {
            // Arrange (Unapproved account scenario for dong3@gamil.com)
            var loginModel = new LoginModel(_mockSignInManager.Object, _mockLogger.Object, _mockUserManager.Object)
            {
                Input = new LoginModel.InputModel
                {
                    Email = "dong3@gamil.com",
                    Password = "Dong@123",
                    RememberMe = false
                }
            };

            // Mock UserManager to return a user that has not confirmed their email
            var user = new ApplicationUser { Email = "dong3@gamil.com", UserName = "dong3@gamil.com" };
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            // Mock SignInManager to return NotAllowed
            _mockSignInManager.Setup(s => s.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>())).ReturnsAsync(SignInResult.NotAllowed);

            // Act
            var result = await loginModel.OnPostAsync(returnUrl: "/");

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.False(loginModel.ModelState.IsValid);
            Assert.Equal("Account is pending approval.", loginModel.ModelState[string.Empty].Errors[0].ErrorMessage);
        }
    }
}
