using Encom.Constants;
using Encom.DAL;
using Encom.Models;
using Encom.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Encom.Controllers
{
    public class AccountsController : Controller
    {
        #region Constructor
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _db;
        public AccountsController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _db = db;
        }
        #endregion

        #region Login

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            User? User = await _userManager.FindByNameAsync(loginVM.Username);

            if (User == null)
            {
                ModelState.AddModelError("", "Username or password is wrong");
                return View(loginVM);
            }

            if (!await _userManager.CheckPasswordAsync(User, loginVM.Password))
            {
                ModelState.AddModelError("", "Username or password is wrong");
                return View(loginVM);
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(User, loginVM.Password, true, true);

            if (User.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Account is blocked");
                return View(loginVM);
            }

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Username or password is wrong");
                return View(loginVM);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "EncomAdmin" });
        }
        #endregion

        #region SignUp
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                    foreach (var error in ModelState[key].Errors)
                        ModelState.AddModelError(string.Empty, error.ErrorMessage);
                return View(registerVM);
            }

            User newUser = new User
            {
                UserName = registerVM.Username,
                Email = registerVM.Email,
                FullName = registerVM.FullName
            };

            IdentityResult identityResult = await _userManager.CreateAsync(newUser, registerVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(registerVM);
            }
            await _userManager.AddToRoleAsync(newUser, UserRoles.Admin.ToString());
            return RedirectToAction("Index", "Users", new { area = "EncomAdmin" });
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Dashboard", new { area = "EncomAdmin" });
        }
        #endregion
    }
}
