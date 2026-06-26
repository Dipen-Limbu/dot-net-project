using Dotnet.Models;
using Dotnet.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Controllers
{
    public class BlogController : Controller
    {

        private readonly CrudContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public BlogController(CrudContext context, DataSecurityProvider p, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(p.key);
            _env = env;
        }
        public ActionResult Index()
        {

            var blogs = _context.BlogPosts
                .Include(b => b.Author)
                .Select(e => new BlogPostEdit
                { 
                    PostId = e.PostId,
                    AuthorId = e.AuthorId,
                    Title = e.Title,
                    PostDescription = e.PostDescription,
                    Content = e.Content,
                    PublishedDate = e.PublishedDate,
                    UploadUserName = e.Author.FullName,
                    UserProfile = e.Author.UserPhoto,
                    EncId = _protector.Protect(e.PostId.ToString())

                }).ToList();
            return View(blogs);



        }

        [HttpGet]
        public ActionResult AddBlog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBlog(BlogPostEdit edit)
        {
            short maxid;
            try
            {
                if (_context.BlogPosts.Any())
                { 
                    maxid = Convert.ToInt16(_context.BlogPosts.Max(x => x.PostId) + 1);
                
                }
                else
                { 
                    maxid = 1;
                edit.PostId = maxid;
                
                }

                if (edit.BlogFile != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(edit.BlogFile.FileName);
                    string filePath = Path.Combine(_env.WebRootPath, "BlogImage", fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        edit.BlogFile.CopyTo(stream);
                    }
                    edit.Content = fileName;
                }

                BlogPost p = new()
                {
                    PostId = edit.PostId,
                    Title = edit.Title,
                    Content = edit.Content,
                    PostDescription = edit.PostDescription,
                    PublishedDate = DateTime.Now,
                    //edit.PublishedDate,
                    AuthorId = Convert.ToInt16(User.Identity.Name)
                };

                _context.Add(p);
                _context.SaveChanges();
                //return Content("Success");
                return RedirectToAction("Index");
            }

            catch
            {
                return View();
            }
        }
    }
}
