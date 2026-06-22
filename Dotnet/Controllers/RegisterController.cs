using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Controllers
{
    public class RegisterController : Controller
    {
        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public RegisterController(CrudContext context, DataSecurityProvider p, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.key);
            _env = env;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(userEdit u)
        {
            //return Json(u); //to check the data coming from form
            try
            {
                var users = _context.UserLists.Where(x => x.EmailAddress == u.EmailAddress).FirstOrDefault();
                if (users == null)
                {
                    short maxid;
                    if (_context.UserLists.Any())
                        maxid = Convert.ToInt16(_context.UserLists.Max(x => x.UserId) + 1);
                    else
                        maxid = 1;
                    u.UserId = maxid;

                    if (u.UserFile != null)
                    {
                        var fileName = "UserImage" + Guid.NewGuid() + Path.GetExtension(u.UserFile.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "UserImage", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            u.UserFile.CopyTo(stream);
                        }
                        u.UserPhoto = fileName;
                    }

                    UserList userList = new()
                    {
                        EmailAddress = u.EmailAddress,
                        FullName = u.FullName,
                        CurrentAddress = u.CurrentAddress,
                        UserPhoto = u.UserPhoto,
                        UserId = u.UserId,
                        UserPassword = _protector.Protect(u.UserPassword),
                        UserRole = "Admin"
                    };

                    //return Json(userList);
                    _context.Add(userList);
                    _context.SaveChanges();

                    // first one is method and second one is controller
                    return RedirectToAction("Login", "Login");
                    //return Json("Registered Successfully");
                }

                else
                {
                    ModelState.AddModelError("", "useralready exist with this email");
                    return View(u);
                }

            }

            catch {
                ModelState.AddModelError("", "User Registration Failed. Please try Again");
                return View(u);
            }
        }
        public IActionResult Index()
        {
            return RedirectToAction("Register");
        }
    }
}
