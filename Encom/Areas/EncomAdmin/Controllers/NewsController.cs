using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area(nameof(EncomAdmin))]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class NewsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public NewsController(AppDbContext db
            , IWebHostEnvironment env
            , UserManager<User> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<News> query = _db.News.AsNoTracking().Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            return View(PageNatedList<News>.Create(query, pageIndex, 10, 10));
        }

        #endregion

        #region Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<News> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(models);
            }

            #region Image
            if (models[0].Files != null && models[0].Files.Count() > 0)
            {
                List<NewsPhoto> newsImages = new List<NewsPhoto>();
                foreach (IFormFile file in models[0].Files)
                {
                    if (!(file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png")))
                    {
                        ModelState.AddModelError("[0].Photo", $"{file.FileName} is not the correct format");
                        return View(models);
                    }

                    if (file.CheckFileLength(5120))
                    {
                        ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                        return View(models);
                    }

                    NewsPhoto newsImage = new()
                    {
                        ImagePath = await file.CreateFileAsync(_env, "src", "images")
                    };

                    newsImages.Add(newsImage);
                }
                models[0].NewsPhotos = newsImages;
            }
            else
            {
                ModelState.AddModelError("[0].Photo", "Image is empty");
                return View(models);
            }
            #endregion

            News? temp = await _db.News.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            string currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (News item in models)
            {
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                await _db.News.AddAsync(item);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null) return BadRequest();

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            News? firstNews = await _db.News.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstNews == null) return NotFound();

            List<News> news = await _db.News
                .Where(c => c.LanguageGroup == firstNews.LanguageGroup && c.IsDeleted == false).ToListAsync();

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<News> news)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (!ModelState.IsValid) return View(news);

            if (id == null) return BadRequest();

            News? firstNews = await _db.News.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            if (firstNews == null) return NotFound();

            List<News> dbNewss = await _db.News.Where(c => c.LanguageGroup == firstNews.LanguageGroup && c.IsDeleted == false).ToListAsync();

            if (dbNewss == null || dbNewss.Count == 0) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(news);
            }

            #region Image
            if (news[0].Files != null && news[0].Files.Count() > 0)
            {
                List<NewsPhoto> newsImages = new List<NewsPhoto>();
                foreach (IFormFile file in news[0].Files)
                {
                    if (!(file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png")))
                    {
                        ModelState.AddModelError("[0].Photo", $"{file.FileName} is not the correct format");
                        return View(news);
                    }

                    if (file.CheckFileLength(5120))
                    {
                        ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                        return View(news);
                    }

                    NewsPhoto newsImage = new()
                    {
                        ImagePath = await file.CreateFileAsync(_env, "src", "images")
                    };

                    newsImages.Add(newsImage);
                }
                dbNewss[0].NewsPhotos.AddRange(newsImages);
            }
            else
            {
                ModelState.AddModelError("[0].Photo", "Image is empty");
                return View(news);
            }
            #endregion

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (News item in news)
            {
                News? dbNews = dbNewss.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                dbNews.Title = item.Title.Trim();
                dbNews.Description = item.Description.Trim();
                dbNews.UpdatedAt = DateTime.UtcNow.AddHours(4);
                dbNews.UpdatedBy = currentUsername;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteImage(int? id, int? imageId)
        {
            if (id == null) return BadRequest();

            if (imageId == null) return BadRequest();

            News? news = await _db
                .News
                .Include(p => p.NewsPhotos)
                .FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == id);

            if (news == null) return NotFound();

            if (!news.NewsPhotos.Any(pi => pi.Id == imageId)) return BadRequest();

            if (news.NewsPhotos.Count <= 1)
            {
                return BadRequest();
            }

            List<News> allNews = await _db.News.Where(x => x.LanguageGroup == news.LanguageGroup).ToListAsync();
            foreach (News item in allNews)
            {
                FileHelper.DeleteFile((item.NewsPhotos.FirstOrDefault(x => x.Id == imageId).ImagePath), _env, "assets", "images");
                _db.NewsPhotos.Remove(item.NewsPhotos.FirstOrDefault(x => x.Id == imageId));
            }

            //product.ProductImages.FirstOrDefault(p => p.Id == imageId).IsDeleted = true;
            //product.ProductImages.FirstOrDefault(p => p.Id == imageId).DeletedBy = "System";
            //product.ProductImages.FirstOrDefault(p => p.Id == imageId).DeletedAt = DateTime.UtcNow.AddHours(4);

            await _db.SaveChangesAsync();
            return PartialView("_ProductImagePartial", news.NewsPhotos.ToList());
        }
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News? temp = await _db.News.AsNoTracking().Include(a => a.Language).FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);
            if (temp == null) return NotFound();

            News? news = await _db.News.AsNoTracking()
                .Include(x => x.Language)
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name); ;
            if (news == null) return BadRequest();
            return View(news);
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            News? firstNews = await _db.News.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstNews == null) return NotFound();

            List<News> newses = await _db.News
                                       .Where(c => c.LanguageGroup == firstNews.LanguageGroup && c.IsDeleted == false)
                                       .ToListAsync();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            List<NewsPhoto> newsImages = await _db.NewsPhotos.Where(x => x.NewsId == id).ToListAsync();
            if (newsImages != null && newsImages.Count > 0)
            {
                foreach (NewsPhoto photo in newsImages)
                {
                    FileHelper.DeleteFile(photo.ImagePath, _env, "src", "images");
                    _db.NewsPhotos.RemoveRange(newsImages);
                }
            }

            foreach (News news in newses)
            {
                if (news == null) return NotFound();
                news.IsDeleted = true;
                news.DeletedBy = currentUsername;
                news.DeletedAt = DateTime.UtcNow.AddHours(4);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
