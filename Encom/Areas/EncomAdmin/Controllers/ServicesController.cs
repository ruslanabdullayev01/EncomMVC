using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Globalization;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area("EncomAdmin")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ServicesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public ServicesController(AppDbContext db
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
            IQueryable<Service> query = _db.Services.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            return View(PageNatedList<Service>.Create(query, pageIndex, 10, 10));
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
        public async Task<IActionResult> Create(List<Service> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();
            //if (!ModelState.IsValid)
            //{
            //    return View(models);
            //}
            #region Image
            if (models[0].Photo != null)
            {
                if (!(models[0].Photo.CheckFileContenttype("image/jpeg") || models[0].Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("[0].Photo", $"{models[0].Photo.FileName} is not the correct format");
                    //return View(models);
                }

                if (models[0].Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("[0].Photo", $"Icon must be less than 5 mb");
                    //return View(models);
                }

                models[0].IconPath = await models[0].Photo.CreateFileAsync(_env, "src", "assets", "images");
            }
            else
            {
                ModelState.AddModelError("[0].Photo", "Icon is empty");
                //return View(models);
            }
            #endregion

            Service temp = await _db.Services.OrderByDescending(a => a.Id).FirstOrDefaultAsync();

            string currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (Service item in models)
            {
                item.IconPath = models[0].IconPath;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                await _db.Services.AddAsync(item);
            }

            #region Validations
            for (int i = 0; i < models.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(models[i].Name))
                {
                    ModelState.AddModelError($"[{i}].Name", "The Name field is required.");
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

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            Service? firstService = await _db.Services.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstService == null) return NotFound();

            List<Service> services = await _db.Services
                .Where(c => c.LanguageGroup == firstService.LanguageGroup && c.IsDeleted == false).ToListAsync();

            return View(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<Service> services)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            //if (!ModelState.IsValid) return View(services);

            if (id == null) return BadRequest();

            Service? firstService = await _db.Services.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            if (firstService == null) return NotFound();

            List<Service> dbServices = await _db.Services
                .Where(c => c.LanguageGroup == firstService.LanguageGroup && c.IsDeleted == false).ToListAsync();

            if (dbServices == null || dbServices.Count == 0) return NotFound();

            #region Image
            if (services[0].Photo != null)
            {
                if (!(services[0].Photo.CheckFileContenttype("image/jpeg") || services[0].Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("[0].Photo", $"{services[0].Photo.FileName} is not the correct format");
                    //return View(services[0]);
                }

                if (services[0].Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("[0].Photo", $"Icon must be less than 5 mb");
                    //return View(services[0]);
                }

                if (ModelState.IsValid)
                {
                    string previousFilePath = dbServices[0].IconPath;
                    if (previousFilePath != null)
                    {
                        FileHelper.DeleteFile(previousFilePath, _env, "src", "assets", "images");
                    }
                    string imagePath = await services[0].Photo.CreateFileAsync(_env, "src", "assets", "images");
                    foreach (Service service in dbServices)
                    {
                        service.IconPath = imagePath;
                    }
                }
            }
            #endregion

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (Service item in services)
            {
                Service? dbService = dbServices.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                dbService.Name = item.Name;
                dbService.Description = item.Description;
                dbService.UpdatedAt = DateTime.UtcNow.AddHours(4);
                dbService.UpdatedBy = currentUsername;
            }

            #region Validations
            for (int i = 0; i < services.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(services[i].Name))
                {
                    ModelState.AddModelError($"[{i}].Name", "The Name field is required.");
                }
                if (string.IsNullOrWhiteSpace(services[i].Description))
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

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            Service? firstService = await _db.Services.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstService == null) return NotFound();

            List<Service> services = await _db.Services
                                       .Where(c => c.LanguageGroup == firstService.LanguageGroup && c.IsDeleted == false)
                                       .ToListAsync();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            foreach (Service service in services)
            {
                if (service == null) return NotFound();
                FileHelper.DeleteFile(service.IconPath, _env, "src", "assets", "images");
                service.IsDeleted = true;
                service.DeletedBy = currentUsername;
                service.DeletedAt = DateTime.UtcNow.AddHours(4);
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
            //return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            Service? temp = await _db.Services.AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);

            Service? service = await _db.Services
                .AsNoTracking()
                .Include(x => x.Language)
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language.Culture == CultureInfo.CurrentCulture.Name);
            if (service == null) return BadRequest();
            return View(service);
        }
        #endregion
    }
}
