using Encom.DAL;
using Encom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Controllers
{
    public class AboutController : Controller
    {
        private readonly AppDbContext _db;
        public AboutController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            About? about = await _db.Abouts
                .AsNoTracking()
                .Include(x => x.AboutFiles)
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            return View(about);
        }
    }
}
