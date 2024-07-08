using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Globalization;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area("EncomAdmin")]
    [Authorize(Roles = "SuperAdmin, Admin")]

    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public SettingsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Setting> query = _db.Settings
                .AsNoTracking()
                .Where(x => x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                .OrderByDescending(c => c.Id);

            return View(PageNatedList<Setting>.Create(query, pageIndex, 10, 10));
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {

            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (id == null) return BadRequest();

            Setting? firstSettings = await _db.Settings.FirstOrDefaultAsync(c => c.Id == id);

            if (firstSettings == null) return NotFound();

            List<Setting> settings = await _db.Settings.Where(c => c.LanguageGroup == firstSettings.LanguageGroup).ToListAsync();

            foreach (Setting setting in settings)
            {
                if (setting == null) return NotFound();
            }

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<Setting> settings)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            //if (!ModelState.IsValid) return View(settings);

            if (id == null) return BadRequest();

            Setting? firstSetting = await _db.Settings.FirstOrDefaultAsync(c => c.Id == id);

            if (firstSetting == null) return NotFound();

            List<Setting> dbSettings = new List<Setting>();

            dbSettings = await _db.Settings.Where(c => c.LanguageGroup == firstSetting.LanguageGroup).ToListAsync();

            if (dbSettings == null || dbSettings.Count == 0) return NotFound();

            foreach (Setting setting in dbSettings)
            {
                if (setting == null) return NotFound();
            }

            foreach (Setting setting in settings)
            {
                Setting? dbSetting = dbSettings.FirstOrDefault(s => s.LanguageId == setting.LanguageId);

                if (dbSetting == null) return NotFound();
                dbSetting.Value = setting.Value;
            }

            #region Validations
            for (int i = 0; i < settings.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(settings[i].Value))
                {
                    ModelState.AddModelError($"[{i}].Value", "The Value field is required.");
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
            //return RedirectToAction("Index", "Settings");

        }
        #endregion

        #region Create, Delete
        //#region Create
        //public async Task<IActionResult> Create()
        //{
        //    ViewBag.Languages = await _db.Languages.ToListAsync();
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(List<Setting> settings)
        //{
        //    ViewBag.Languages = await _db.Languages.ToListAsync();
        //    if (!ModelState.IsValid) return View(settings);

        //    if (await _db.Settings.AnyAsync(c => c.Key.ToLower() == settings[0].Key.Trim().ToLower()))
        //    {
        //        ModelState.AddModelError("Name", $"Bu {settings[0].Key} key movcuddur");
        //        return View(settings);
        //    }

        //    Setting test = await _db.Settings.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
        //    foreach (Setting setting in settings)
        //    {
        //        setting.Key = settings[0].Key.Trim();
        //        if (setting.Key == null)
        //        {
        //            ModelState.AddModelError("[0].Key", "Key is cannot null");
        //            return View(setting);
        //        }
        //        setting.LanguageGroup = test != null ? test.LanguageGroup + 1 : 1;
        //        await _db.Settings.AddAsync(setting);
        //    }


        //    await _db.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}
        //#endregion

        //#region Delete
        //public async Task<IActionResult> Delete(int? id)
        //{

        //    if (id == null) return BadRequest();

        //    List<Language> languages = await _db.Languages.ToListAsync();
        //    ViewBag.Languages = languages;

        //    Setting? firstSetting = await _db.Settings.FirstOrDefaultAsync(c => c.Id == id);

        //    if (firstSetting == null) return NotFound();

        //    List<Setting> settings = await _db.Settings.Where(c => c.LanguageGroup == firstSetting.LanguageGroup).ToListAsync();

        //    foreach (Setting setting1 in settings)
        //    {
        //        if (setting1 == null) return NotFound();
        //        _db.Settings.Remove(setting1);
        //    }

        //    await _db.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));

        //}
        //#endregion
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            Setting? temp = await _db.Settings.AsNoTracking().Include(a => a.Language).FirstOrDefaultAsync(x => x.Id == id);

            Setting? setting = await _db.Settings
                .AsNoTracking()
                .Include(x => x.Language)
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp!.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name); ;
            if (setting == null) return BadRequest();
            return View(setting);
        }
        #endregion
    }
}
