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
            var query = _db.News
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.NewsPhotos);
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

            //if (!ModelState.IsValid)
            //{
            //    return View(models);
            //}

            #region Image
            bool fileErrorAdded = false;
            foreach (var item in models)
            {
                List<NewsPhoto> newsImages = new();

                News? temp = await _db.News.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                string currentUsername = _userManager.GetUserName(HttpContext.User);
                item.NewsPhotos = newsImages;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;

                if (models[0].Files != null && models[0].Files.Count() > 0)
                {
                    int orderNumber = 0;
                    foreach (IFormFile file in models[0].Files)
                    {
                        orderNumber++;
                        if (!(file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png")))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                fileErrorAdded = true;
                            }
                            //return View(models);
                        }

                        if (file.CheckFileLength(5120))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"Photo must be less than 5 mb");
                                fileErrorAdded = true;
                            }
                            //return View(models);
                        }

                        NewsPhoto newsImage = new()
                        {
                            ImagePath = await file.CreateDynamicFileAsync(item.LanguageGroup, _env, "src", "assets", "images"),
                            OrderNumber = orderNumber,
                            News = item
                        };

                        newsImages.Add(newsImage);
                    }
                }
                else
                {
                    if (!fileErrorAdded)
                    {
                        ModelState.AddModelError("[0].Files", "Image is empty");
                        fileErrorAdded = true;
                    }
                    //return View(models);
                }

                await _db.News.AddAsync(item);
            }
            #endregion

            #region Validations
            for (int i = 0; i < models.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(models[i].Title))
                {
                    ModelState.AddModelError($"[{i}].Title", "The Title field is required.");
                }
                if (string.IsNullOrWhiteSpace(models[i].Description))
                {
                    ModelState.AddModelError($"[{i}].Description", "The Description field is required.");
                }
            }

            var validationErrors = new Dictionary<string, string[]>();
            if (!ModelState.IsValid)
            {
                validationErrors = ModelState.ToDictionary(
                    err => err.Key,
                    err => err.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return Json(new { success = false, errors = validationErrors });
            }
            #endregion

            await _db.SaveChangesAsync();
            return Json(new { success = true });
            //return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            News? firstNews = await _db.News
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstNews == null) return NotFound();

            List<News> news = await _db.News
                .Where(c => c.LanguageGroup == firstNews.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.NewsPhotos)
                .ToListAsync();

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<News> news)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            //if (!ModelState.IsValid) return View(news);

            if (id == null) return BadRequest();

            News? firstNews = await _db.News
                .Include(x => x.NewsPhotos)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstNews == null) return NotFound();

            List<News> dbNewsList = await _db.News
                .Where(c => c.LanguageGroup == firstNews.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.NewsPhotos)
                .ToListAsync();

            if (dbNewsList == null || dbNewsList.Count == 0) return NotFound();

            List<NewsPhoto> newPhotos = new();

            bool fileErrorAdded = false;
            foreach (var item in news)
            {
                var dbNews = dbNewsList.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                if (dbNews != null)
                {
                    string? currentUsername = _userManager.GetUserName(HttpContext.User);
                    dbNews.Title = item.Title != null ? item.Title.Trim() : null;
                    dbNews.Description = item.Description != null ? item.Description.Trim() : null;
                    dbNews.UpdatedAt = DateTime.UtcNow.AddHours(4);
                    dbNews.UpdatedBy = currentUsername;

                    if (item.Files != null && item.Files.Count() > 0)
                    {
                        foreach (IFormFile file in item.Files)
                        {
                            if (file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png"))
                            {
                                if (!file.CheckFileLength(5120))
                                {
                                    NewsPhoto newsImage = new()
                                    {
                                        ImagePath = await file.CreateDynamicFileAsync(dbNews.LanguageGroup, _env, "src", "assets", "images"),
                                        OrderNumber = dbNews.NewsPhotos.Count + 1
                                    };

                                    newPhotos.Add(newsImage);
                                }
                                else
                                {
                                    if (!fileErrorAdded)
                                    {
                                        ModelState.AddModelError("[0].Files", $"Photo must be less than 5 mb");
                                        fileErrorAdded = true;
                                    }
                                    //return View(news);
                                }
                            }
                            else
                            {
                                if (!fileErrorAdded)
                                {
                                    ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                    fileErrorAdded = true;
                                }
                                //return View(news);
                            }
                        }
                    }
                }
            }

            foreach (var dbNews in dbNewsList)
            {
                foreach (var photo in newPhotos)
                {
                    dbNews.NewsPhotos.Add(new NewsPhoto
                    {
                        ImagePath = photo.ImagePath,
                        OrderNumber = dbNews.NewsPhotos.Count + 1,
                        News = dbNews
                    });
                }
            }

            #region Validations
            for (int i = 0; i < news.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(news[i].Title))
                {
                    ModelState.AddModelError($"[{i}].Title", "The Title field is required.");
                }
                if (string.IsNullOrWhiteSpace(news[i].Description))
                {
                    ModelState.AddModelError($"[{i}].Description", "The Description field is required.");
                }
            }

            var validationErrors = new Dictionary<string, string[]>();
            if (!ModelState.IsValid)
            {
                validationErrors = ModelState.ToDictionary(
                    err => err.Key,
                    err => err.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return Json(new { success = false, errors = validationErrors });
            }
            #endregion

            await _db.SaveChangesAsync();
            return Json(new { success = true });
            //return RedirectToAction(nameof(Index));
        }

        #region Order Number
        public async Task<IActionResult> UpdateOrder(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            News? temp = await _db.News
                .Include(p => p.NewsPhotos)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (temp == null) return NotFound();

            List<NewsPhoto> currentPhotos = await _db.NewsPhotos
                .Where(x => x.NewsId == temp.Id)
                .OrderBy(x => x.OrderNumber)
                .ToListAsync();

            return View(currentPhotos);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(List<NewsPhoto> photos)
        {
            if (photos == null || photos.Count == 0) return BadRequest();

            var firstPhoto = photos.FirstOrDefault();
            if (firstPhoto == null || string.IsNullOrEmpty(firstPhoto.ImagePath)) return NotFound();

            var dbPhotos = await _db.NewsPhotos
                .Include(p => p.News)
                .Where(p => photos.Select(photo => photo.ImagePath).Contains(p.ImagePath))
                .ToListAsync();

            foreach (var photo in photos)
            {
                var matchedPhotos = dbPhotos
                    .Where(p => p.ImagePath == photo.ImagePath)
                    .ToList();

                foreach (var matchedPhoto in matchedPhotos)
                {
                    matchedPhoto.OrderNumber = photo.OrderNumber;
                    _db.Entry(matchedPhoto).Property(x => x.OrderNumber).IsModified = true;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int? id, string? imagePath)
        {
            if (id == null || string.IsNullOrEmpty(imagePath)) return BadRequest();

            News? news = await _db.News
                .Include(p => p.NewsPhotos)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (news == null) return NotFound();

            List<NewsPhoto> photosToDelete = await _db.NewsPhotos
                .Include(x => x.News)
                .Where(x => x.ImagePath == imagePath && x.News.LanguageGroup == news.LanguageGroup)
                .ToListAsync();

            if (photosToDelete != null && photosToDelete.Count > 0)
            {
                foreach (var photoToDelete in photosToDelete)
                {
                    FileHelper.DeleteFile(photoToDelete.ImagePath, _env, "src", "assets", "images");
                    _db.NewsPhotos.Remove(photoToDelete);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Photos deleted successfully." });
        }
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News? temp = await _db.News
                .AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);
            if (temp == null) return NotFound();

            News? news = await _db.News.AsNoTracking()
                .Include(x => x.Language)
                .Include(x => x.NewsPhotos.OrderBy(x => x.OrderNumber))
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
                    FileHelper.DeleteFile(photo.ImagePath, _env, "src", "assets", "images");
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
            return Json(new { success = true });
            //return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
