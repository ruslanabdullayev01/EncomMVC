using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area("EncomAdmin")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class AppealsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SmtpSetting _smtpSetting;
        public AppealsController(AppDbContext db
            , UserManager<User> userManager
            , SmtpSetting smtpSetting)
        {
            _db = db;
            _userManager = userManager;
            _smtpSetting = smtpSetting;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1, string? isReadFilter = "unread")
        {
            IQueryable<Appeal> query = _db.Appeals.Where(item =>
                (isReadFilter == "read" && item.IsRead && !item.IsDeleted) ||
                (isReadFilter == "unread" && !item.IsRead && !item.IsDeleted) ||
                (isReadFilter == "all" && !item.IsDeleted)
            ).OrderByDescending(x => x.Id);

            return View(PageNatedList<Appeal>.Create(query, pageIndex, 10, 10));
        }

        [HttpPost]
        public IActionResult Index(string? isReadFilter = "false")
        {
            return RedirectToAction("Index", isReadFilter);
        }
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            Appeal? appeal = await _db.Appeals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (appeal == null) return BadRequest();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);
            appeal.IsRead = true;
            if (appeal.ReadedBy == null)
            {
                appeal.ReadedBy = currentUsername;
            }
            await _db.SaveChangesAsync();

            return View(appeal);
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Appeal? appeal = await _db.Appeals
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (appeal == null) return NotFound();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            appeal.IsDeleted = true;
            appeal.DeletedBy = currentUsername;
            appeal.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _db.SaveChangesAsync();
            return Json(new { success = true });
            //return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
