using Encom.DAL;
using Encom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _db;
        public NewsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<News>? news = await _db.News
               .AsNoTracking()
               .Include(x => x.NewsPhotos)
               .OrderByDescending(x=>x.CreatedAt)
               .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
               .ToListAsync();
            return View(news);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();

            News? temp = await _db.News
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted == false);

            if (temp is null) return NotFound();

            //UNDONE: Check language including
            News? news = await _db.News
                //.Include(x => x.Language)
                .Include(x => x.NewsPhotos)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp!.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name);

            if (news == null) return View(temp);

            return View(news);
        }
    }
}
