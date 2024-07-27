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
    public class LicensesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public LicensesController(AppDbContext db
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
            IQueryable<License> query = _db.Licenses.AsNoTracking().Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            return View(PageNatedList<License>.Create(query, pageIndex, 10, 10));
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
        public async Task<IActionResult> Create(List<License> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            #region Image
            if (models[0].Photo != null)
            {
                if (!(models[0].Photo.CheckFileContenttype("image/jpeg") || models[0].Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("[0].Photo", $"{models[0].Photo.FileName} is not the correct format");
                }

                if (models[0].Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                }

                models[0].ImagePath = await models[0].Photo.CreateFileAsync(_env, "src", "assets", "images");
            }
            else
            {
                ModelState.AddModelError("[0].Photo", "Image is empty");
            }
            #endregion

            License? temp = await _db.Licenses.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            string currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (License item in models)
            {
                item.ImagePath = models[0].ImagePath;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                await _db.Licenses.AddAsync(item);
            }

            #region Validations
            for (int i = 0; i < models.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(models[i].Name))
                {
                    ModelState.AddModelError($"[{i}].Name", "The Name field is required.");
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

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            License? firstLicense = await _db.Licenses.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstLicense == null) return NotFound();

            List<License> licenses = await _db.Licenses
                .Where(c => c.LanguageGroup == firstLicense.LanguageGroup && c.IsDeleted == false).ToListAsync();

            return View(licenses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<License> licenses)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (id == null) return BadRequest();

            License? firstLicense = await _db.Licenses.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            if (firstLicense == null) return NotFound();

            List<License> dbLicenses = await _db
                .Licenses
                .Where(c => c.LanguageGroup == firstLicense.LanguageGroup && c.IsDeleted == false).ToListAsync();

            if (dbLicenses == null || dbLicenses.Count == 0) return NotFound();

            #region Image
            if (licenses[0].Photo != null)
            {
                if (!(licenses[0].Photo.CheckFileContenttype("image/jpeg") || licenses[0].Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("[0].Photo", $"{licenses[0].Photo.FileName} is not the correct format");
                }

                if (licenses[0].Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                }

                string previousFilePath = dbLicenses[0].ImagePath;
                if (previousFilePath != null)
                {
                    FileHelper.DeleteFile(previousFilePath, _env, "src", "assets", "images");
                }
                string imagePath = await licenses[0].Photo.CreateFileAsync(_env, "src", "assets", "images");
                foreach (License license in dbLicenses)
                {
                    license.ImagePath = imagePath;
                }
            }
            #endregion

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (License item in licenses)
            {
                License? dbLicense = dbLicenses.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                dbLicense.Name = item.Name != null ? item.Name.Trim() : null;
                dbLicense.UpdatedAt = DateTime.UtcNow.AddHours(4);
                dbLicense.UpdatedBy = currentUsername;
            }

            #region Validations
            for (int i = 0; i < licenses.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(licenses[i].Name))
                {
                    ModelState.AddModelError($"[{i}].Name", "The Name field is required.");
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

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            License? temp = await _db
                .Licenses
                .AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);

            if (temp == null) return NotFound();

            License? license = await _db.Licenses
                .AsNoTracking()
                .Include(x => x.Language)
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            if (license == null) return BadRequest();

            return View(license);
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            License? firstLicense = await _db.Licenses.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstLicense == null) return NotFound();

            List<License> licenses = await _db.Licenses
                                       .Where(c => c.LanguageGroup == firstLicense.LanguageGroup && c.IsDeleted == false)
                                       .ToListAsync();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (License license in licenses)
            {
                if (license == null) return NotFound();
                FileHelper.DeleteFile(license.ImagePath, _env, "src", "assets", "images");
                license.IsDeleted = true;
                license.DeletedBy = currentUsername;
                license.DeletedAt = DateTime.UtcNow.AddHours(4);
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });

        }
        #endregion
    }
}
