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
    public class SocialMediasController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public SocialMediasController(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<SocialMedia> query = _db.SocialMedias.AsNoTracking().Where(x => !x.IsDeleted);
            ViewBag.DataCount = query.Count();
            return View(PageNatedList<SocialMedia>.Create(query, pageIndex, 5, 5));
        }
        #endregion

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SocialMedia model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            SocialMedia socialMedia = new()
            {
                Facebook = model.Facebook,
                Linkedin = model.Linkedin,
                Twitter = model.Twitter,
                Telegram = model.Telegram,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                CreatedBy = currentUsername
            };

            await _db.SocialMedias.AddAsync(socialMedia);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null) return BadRequest();

            SocialMedia? dbSocialMedia = await _db.SocialMedias.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);

            if (dbSocialMedia == null) return BadRequest();

            return View(dbSocialMedia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, SocialMedia socialMedia)
        {
            if (!ModelState.IsValid) return View(socialMedia);

            if (id == null) return BadRequest();

            SocialMedia? dbSocialMedia = await _db
                .SocialMedias
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (dbSocialMedia == null) return NotFound();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            dbSocialMedia.Facebook = socialMedia.Facebook;
            dbSocialMedia.Twitter = socialMedia.Twitter;
            dbSocialMedia.Linkedin = socialMedia.Linkedin;
            dbSocialMedia.Telegram = socialMedia.Telegram;
            dbSocialMedia.UpdatedAt = DateTime.UtcNow.AddHours(4);
            dbSocialMedia.UpdatedBy = currentUsername;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
