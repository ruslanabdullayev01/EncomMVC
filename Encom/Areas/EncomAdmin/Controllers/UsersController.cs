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
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        public UsersController(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;

        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<User> query = _db.Users.AsNoTracking();
            return View(PageNatedList<User>.Create(query, pageIndex, 10, 10));
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return Json(new { success = true });
        }
        #endregion
    }
}
