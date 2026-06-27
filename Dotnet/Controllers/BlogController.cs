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

        public BlogController(CrudContext context,
            DataSecurityProvider p, IDataProtectionProvider provider,
            IWebHostEnvironment env)
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
                    Title = e.Title,
                    PostDescription = e.PostDescription,
                    Content = e.Content,
                    PublishedDate = e.PublishedDate,
                    AuthorId = e.AuthorId,
                    UploadUserName = e.Author.FullName,
                    UserProfile = e.Author.UserPhoto,
                    EncId = _protector.Protect(e.PostId.ToString())
                }).ToList();
            return View(blogs);
        }
        public IActionResult Details(string id)
        {
            int postId = Convert.ToInt32(_protector.Unprotect(id));

            var blog = _context.BlogPosts
                .Include(x => x.Author)
                .Where(x => x.PostId == postId)
                .Select(e => new BlogPostEdit
                {
                    PostId = e.PostId,
                    Title = e.Title,
                    PostDescription = e.PostDescription,
                    Content = e.Content,
                    PublishedDate = e.PublishedDate,
                    UploadUserName = e.Author.FullName,
                    UserProfile = e.Author.UserPhoto
                })
                .FirstOrDefault();

            return View("ViewDetails", blog);
        }
        //post: BlogController/Create

        public ActionResult AddBlog()
        {
            return View();
        }

        //post: BlogController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBlog(BlogPostEdit edit)
        {
            try
            {
                // Calculate next PostId (DB has ValueGeneratedNever so we must assign it)
                short maxid;
                if (_context.BlogPosts.Any())
                {
                    maxid = Convert.ToInt16(_context.BlogPosts.Max(x => x.PostId) + 1);
                }
                else
                {
                    maxid = 1;
                }
                edit.PostId = maxid;

                // Handle optional image upload
                if (edit.BlogFile != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(edit.BlogFile.FileName);
                    string uploadDir = Path.Combine(_env.WebRootPath, "BlogImage");
                    Directory.CreateDirectory(uploadDir); // ensure folder exists
                    string filePath = Path.Combine(uploadDir, fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        edit.BlogFile.CopyTo(stream);
                    }
                    edit.Content = fileName;
                }
                else
                {
                    // No image uploaded – use empty string so non-nullable Content is satisfied
                    edit.Content = edit.Content ?? string.Empty;
                }

                BlogPost p = new()
                {
                    PostId = edit.PostId,
                    Title = edit.Title,
                    Content = edit.Content,
                    PostDescription = edit.PostDescription,
                    PublishedDate = DateTime.Now,
                    AuthorId = Convert.ToInt16(User.Identity.Name)
                };

                _context.Add(p);
                _context.SaveChanges();
                return RedirectToAction("Index", "Blog");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving blog post: " + ex.Message);
                return View(edit);
            }
        }




    }
}