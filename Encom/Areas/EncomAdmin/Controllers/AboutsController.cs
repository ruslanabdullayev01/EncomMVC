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
    public class AboutsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public AboutsController(AppDbContext db, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }


        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<About> query = _db.Abouts
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                .Include(x => x.AboutFiles);
            ViewBag.DataCount = query.Count();
            return View(PageNatedList<About>.Create(query, pageIndex, 10, 10));
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
        public async Task<IActionResult> Create(List<About> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            #region Image
            bool fileErrorAdded = false;
            foreach (var item in models)
            {
                List<AboutFile> aboutFiles = new();
                About? temp = await _db.Abouts.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                string? currentUsername = _userManager.GetUserName(HttpContext.User);
                item.AboutFiles = aboutFiles;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                if (models[0].Files != null && models[0].Files.Count() > 0)
                {
                    int orderNumber = 0;
                    foreach (IFormFile file in models[0].Files)
                    {
                        orderNumber++;
                        if (!(file.CheckFileContenttype("image/jpeg") ||
                              file.CheckFileContenttype("image/png") ||
                              file.CheckFileContenttype("video/mp4") ||
                              file.CheckFileContenttype("video/avi")))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                fileErrorAdded = true;
                            }
                        }

                        if (file.CheckFileLength(20480))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"File must be less than 20 mb");
                                fileErrorAdded = true;
                            }
                        }

                        AboutFile aboutFile = new()
                        {
                            FilePath = await file.CreateDynamicFileAsync(item.LanguageGroup, _env, "src", "assets", "images"),
                            OrderNumber = orderNumber,
                            About = item
                        };

                        aboutFiles.Add(aboutFile);
                    }
                }
                else
                {
                    if (!fileErrorAdded)
                    {
                        ModelState.AddModelError("[0].Files", "Image is empty");
                        fileErrorAdded = true;
                    }
                }
                await _db.Abouts.AddAsync(item);
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
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            About? firstAbout = await _db.Abouts
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstAbout == null) return NotFound();

            List<About> abouts = await _db.Abouts
                .Where(c => c.LanguageGroup == firstAbout.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.AboutFiles.OrderBy(x => x.OrderNumber))
                .ToListAsync();

            return View(abouts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<About> abouts)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (id == null) return BadRequest();

            About? firstAbout = await _db.Abouts
                .Include(x => x.AboutFiles)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstAbout == null) return NotFound();

            List<About> dbAboutsList = await _db.Abouts
                .Where(c => c.LanguageGroup == firstAbout.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.AboutFiles)
                .ToListAsync();

            if (dbAboutsList == null || dbAboutsList.Count == 0) return NotFound();

            List<AboutFile> newFiles = new();

            bool fileErrorAdded = false;
            foreach (var item in abouts)
            {
                var dbAbout = dbAboutsList.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                if (dbAbout != null)
                {
                    string? currentUsername = _userManager.GetUserName(HttpContext.User);
                    dbAbout.Title = item.Title != null ? item.Title.Trim() : null;
                    dbAbout.Description = item.Description != null ? item.Description.Trim() : null;
                    dbAbout.UpdatedAt = DateTime.UtcNow.AddHours(4);
                    dbAbout.UpdatedBy = currentUsername;

                    if (item.Files != null && item.Files.Count() > 0)
                    {
                        foreach (IFormFile file in item.Files)
                        {
                            if (file.CheckFileContenttype("image/jpeg") ||
                              file.CheckFileContenttype("image/png") ||
                              file.CheckFileContenttype("video/mp4") ||
                              file.CheckFileContenttype("video/avi"))
                            {
                                if (!file.CheckFileLength(20480))
                                {
                                    AboutFile aboutFile = new()
                                    {
                                        FilePath = await file.CreateDynamicFileAsync(dbAbout.LanguageGroup, _env, "src", "assets", "images"),
                                        OrderNumber = dbAbout.AboutFiles.Count + 1
                                    };

                                    newFiles.Add(aboutFile);
                                }
                                else
                                {
                                    if (!fileErrorAdded)
                                    {
                                        ModelState.AddModelError("[0].Files", $"File must be less than 20 mb");
                                        fileErrorAdded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (!fileErrorAdded)
                                {
                                    ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                    fileErrorAdded = true;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var dbAbout in dbAboutsList)
            {
                foreach (var file in newFiles)
                {
                    dbAbout.AboutFiles.Add(new AboutFile
                    {
                        FilePath = file.FilePath,
                        OrderNumber = dbAbout.AboutFiles.Count + 1,
                        About = dbAbout
                    });
                }
            }

            #region Validations
            for (int i = 0; i < abouts.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(abouts[i].Title))
                {
                    ModelState.AddModelError($"[{i}].Title", "The Title field is required.");
                }
                if (string.IsNullOrWhiteSpace(abouts[i].Description))
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
        }

        #region Order Number
        public async Task<IActionResult> UpdateOrder(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            About? temp = await _db.Abouts
                .Include(p => p.AboutFiles)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (temp == null) return NotFound();

            List<AboutFile> currentFiles = await _db.AboutFiles
                .Where(x => x.AboutId == temp.Id)
                .OrderBy(x => x.OrderNumber)
                .ToListAsync();

            return View(currentFiles);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(List<AboutFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest();

            var firstFile = files.FirstOrDefault();
            if (firstFile == null || string.IsNullOrEmpty(firstFile.FilePath)) return NotFound();

            var dbFiles = await _db.AboutFiles
                .Include(p => p.About)
                .Where(p => files.Select(file => file.FilePath).Contains(p.FilePath))
                .ToListAsync();

            foreach (var file in files)
            {
                var matchedPhotos = dbFiles
                    .Where(p => p.FilePath == file.FilePath)
                    .ToList();

                foreach (var matchedPhoto in matchedPhotos)
                {
                    matchedPhoto.OrderNumber = file.OrderNumber;
                    _db.Entry(matchedPhoto).Property(x => x.OrderNumber).IsModified = true;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Delete Image
        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int? id, string? filePath)
        {
            if (id == null || string.IsNullOrEmpty(filePath)) return BadRequest();

            About? about = await _db.Abouts
                .Include(p => p.AboutFiles)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (about == null) return NotFound();

            List<AboutFile> filesToDelete = await _db.AboutFiles
                .Include(x => x.About)
                .Where(x => x.FilePath == filePath && x.About.LanguageGroup == about.LanguageGroup)
                .ToListAsync();

            if (filesToDelete != null && filesToDelete.Count > 0)
            {
                foreach (var fileToDelete in filesToDelete)
                {
                    FileHelper.DeleteFile(fileToDelete.FilePath, _env, "src", "assets", "images");
                    _db.AboutFiles.Remove(fileToDelete);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Files deleted successfully." });
        }
        #endregion

        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            About? temp = await _db.Abouts
                .AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);
            if (temp == null) return NotFound();

            About? about = await _db.Abouts.AsNoTracking()
                .Include(x => x.Language)
                .Include(x => x.AboutFiles.OrderBy(x => x.OrderNumber))
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            if (about == null) return BadRequest();

            return View(about);
        }
        #endregion
    }
}
