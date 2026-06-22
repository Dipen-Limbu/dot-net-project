using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace Dotnet.Controllers
{
    public class LoginController : Controller
    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;


        public LoginController(CrudContext context, DataSecurityProvider p, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.key);
            _env = env;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(userEdit uEdit)
        {
            //return Json(uEdit);
            var users = _context.UserLists.ToList();
            if (users != null)
            {
                var u = users.Where(x => x.EmailAddress.ToUpper().Equals(uEdit.EmailAddress.ToUpper()) && _protector.Unprotect(x.UserPassword).Equals(uEdit.UserPassword)).FirstOrDefault();
                if (u != null)
                {
                    List<Claim> claims = new()
                    {
                    new Claim(ClaimTypes.Name, u.UserId.ToString()),
                    new Claim(ClaimTypes.Role, u.UserRole),
                    new Claim("FullName", u.FullName),
                    new Claim("image",u.UserPhoto),
                    new Claim("address",u.CurrentAddress),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    return RedirectToAction("Dashboard");
                }
            }

            else
            {
                ModelState.AddModelError("", "Invalid User");
            }
            return View(uEdit);

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }




        [Authorize]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePassword c)
        {
            var u = _context.UserLists.Where(e => e.UserId ==
            Convert.ToInt16(User.Identity!.Name)).First();
            if (_protector.Unprotect(u.UserPassword) != c.CurrentPassword)
            {
                ModelState.AddModelError("", "Current Password is Incorrect");
                return View();
            }

            else
            {
                if (c.NewPassword == c.ConfirmPassword)
                {
                    u.UserPassword = _protector.Protect(c.NewPassword);
                    _context.Update(u);
                    _context.SaveChanges();

                    //Add a success message to the TempData
                    TempData["Success"] = "Your password has been changes successfully!";
                    return View();
                }

                else
                {
                    ModelState.AddModelError("", "Confimr password does not match");
                    return View(c);
                }
            }

            TempData["Error"] = "An error occurred while changing your password. Please try again.";
            return View();


        }

    }
}
