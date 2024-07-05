using Encom.DAL;
using Encom.Models;
using Encom.ViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
namespace Encom.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new()
            {
                Projects = await _db.Projects
                    .AsNoTracking()
                    .Include(x=>x.ProjectPhotos)
                    .Where(x=>!x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                    .OrderBy(x=>x.CreatedAt)
                    .Take(4)
                    .ToListAsync(),
                Licenses = await _db.Licenses
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                    .OrderBy(x => x.CreatedAt)
                    .Take(4)
                    .ToListAsync(),
                News = await _db.News
                    .AsNoTracking()
                    .Include(x => x.NewsPhotos)
                    .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                    .OrderBy(x => x.CreatedAt)
                    .Take(3)
                    .ToListAsync(),
            };
            return View(homeVM);
        }

        #region Change Language
        public IActionResult ChangeLanguage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return Redirect(Request.Headers["Referer"].ToString());
        }
        #endregion
    }
}
