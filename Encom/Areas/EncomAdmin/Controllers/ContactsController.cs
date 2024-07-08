using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area(nameof(EncomAdmin))]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ContactsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public ContactsController(AppDbContext db, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Contact> query = _db.Contacts.AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            ViewBag.DataCount = query.Count();
            return View(PageNatedList<Contact>.Create(query, pageIndex, 10, 10));
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
        public async Task<IActionResult> Create(List<Contact> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();
            if (!ModelState.IsValid) return View(models);

            Contact? temp = await _db.Contacts.OrderByDescending(a => a.Id).FirstOrDefaultAsync();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            #region Validations
            var validationErrors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(models[0].Email))
            {
                ModelState.AddModelError("[0].Email", "The Email field is required.");
            }
            if (string.IsNullOrWhiteSpace(models[0].Number))
            {
                ModelState.AddModelError("[0].Number", "The Number field is required.");
            }
            if (string.IsNullOrWhiteSpace(models[0].MapIFrame))
            {
                ModelState.AddModelError("[0].MapIFrame", "The Map link field is required.");
            }

            for (int i = 0; i < models.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(models[i].Address))
                {
                    ModelState.AddModelError($"[{i}].Address", "The Address field is required.");
                }
            }

            if (!ModelState.IsValid)
            {
                validationErrors = ModelState.ToDictionary(
                    err => err.Key,
                    err => err.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return Json(new { success = false, errors = validationErrors });
            }
            #endregion

            foreach (Contact item in models)
            {
                item.Email = models[0].Email;
                item.Number = models[0].Number;
                item.MapIFrame = models[0].MapIFrame;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                await _db.Contacts.AddAsync(item);
            }


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

            Contact? firstContact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstContact == null) return NotFound();

            List<Contact> contacts = await _db.Contacts
                .Where(c => c.LanguageGroup == firstContact.LanguageGroup && c.IsDeleted == false).ToListAsync();

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<Contact> contacts)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (!ModelState.IsValid) return View(contacts);

            if (id == null) return BadRequest();

            Contact? firstContact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
            if (firstContact == null) return NotFound();

            List<Contact> dbContacts = await _db.Contacts
                .Where(c => c.LanguageGroup == firstContact.LanguageGroup && c.IsDeleted == false)
                .ToListAsync();

            if (dbContacts == null || dbContacts.Count == 0) return NotFound();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            foreach (Contact item in contacts)
            {
                Contact? dbContact = dbContacts.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                if (dbContact != null)
                {
                    dbContact.Email = contacts[0].Email;
                    dbContact.Number = contacts[0].Number;
                    dbContact.MapIFrame = contacts[0].MapIFrame;
                    dbContact.Address = item.Address;
                    dbContact.UpdatedAt = DateTime.UtcNow.AddHours(4);
                    dbContact.UpdatedBy = currentUsername;
                }
            }

            #region Validations
            var validationErrors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(contacts[0].Email))
            {
                ModelState.AddModelError("[0].Email", "The Email field is required.");
            }
            if (string.IsNullOrWhiteSpace(contacts[0].Number))
            {
                ModelState.AddModelError("[0].Number", "The Number field is required.");
            }
            if (string.IsNullOrWhiteSpace(contacts[0].MapIFrame))
            {
                ModelState.AddModelError("[0].MapIFrame", "The Map link field is required.");
            }

            for (int i = 0; i < contacts.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(contacts[i].Address))
                {
                    ModelState.AddModelError($"[{i}].Address", "The Address field is required.");
                }
            }

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

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            Contact? temp = await _db.Contacts.AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);
            if (temp == null) return NotFound();

            Contact? contact = await _db.Contacts
                .AsNoTracking()
                .Include(x => x.Language)
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            if (contact == null) return BadRequest();
            return View(contact);
        }
        #endregion
    }
}
