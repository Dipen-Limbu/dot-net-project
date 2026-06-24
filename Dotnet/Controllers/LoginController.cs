using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
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


        //for forgot password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(userEdit edit)
        {
            if (edit.EmailAddress != null)
            {
                Random r = new Random();
                HttpContext.Session.SetString("token", r.Next(9999).ToString());
                var token = HttpContext.Session.GetString("token");
                var user = _context.UserLists.Where(u => u.EmailAddress
                == edit.EmailAddress).FirstOrDefault();
                if (user != null)
                {
                    SmtpClient s = new()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        Credentials = new NetworkCredential("np05cp4a240011@iic.edu.np", "avfx doau pemf urkx"),
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network

                    };

                    MailMessage m = new()
                    {
                        From = new MailAddress("np05cp4a240011@iic.edu.np"),
                        Subject = "Forgot password token",
                        Body = $@"<p class='text-red-800' style='background-color:red;'>Forgot Password</p>
                        <p style = 'background-color:blue;' > EmailTokenProvider ={token}",
                        IsBodyHtml = true,
                    };

                    m.To.Add(user.EmailAddress);
                    s.Send(m);
                    //return Json("Success");
                    return RedirectToAction("VerifyToken", new { email = user.EmailAddress });
                }

                else { 
                ModelState.AddModelError("", "this email is not registered");
                    return View(edit);

                }
            }

        return Json("Failed");

        }

        [HttpGet]
        public IActionResult VerifyToken(string email)
        {
            return View(new userEdit { EmailAddress = email});
        }

        [HttpPost]
        public IActionResult VerifyToken(userEdit e)
        {
            var token = HttpContext.Session.GetString("token");
            if (token == e.EmailToken)
            {
                var et = _protector.Protect(e.EmailToken!);
                return RedirectToAction("ResetPassword",
                    new userEdit { EmailAddress = e.EmailAddress, EmailToken = et });
            }

            else
            { 
            return Json("Failed");
            }
        }


        // for reset password

        [HttpGet]
        public IActionResult ResetPassword(userEdit e)
        {
            try
            {
                //return Json(e);
                var token = HttpContext.Session.GetString("token");
                var eToken = _protector.Unprotect(e.EmailToken);
                if (token == eToken)
                {
                    return View(new ChangePassword { EmailAddress = e.EmailAddress });
                }

                else 
                {
                 return RedirectToAction("ForgotPassword");
                }
            }

            catch (Exception ex)
            {
                return RedirectToAction("ForgotPassword");
            }
        }

        [HttpPost]
        public IActionResult ResetPassword(ChangePassword model)
        {
            if (model.NewPassword == model.ConfirmPassword)
            {
                var user = _context.UserLists.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
                if (user != null)
                {
                    user.UserPassword = _protector.Protect(model.NewPassword);
                    _context.Update(user);
                    _context.SaveChanges();
                    return RedirectToAction("Login");
                }
            }

            else
            {
                ModelState.AddModelError("", "Password does not match");
                return View(model);
            }

            // return RedirectToAction("ForgotPassword");
            return Json("error");
        }


    }
}
