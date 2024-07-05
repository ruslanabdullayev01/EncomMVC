using Encom.DAL;
using Encom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Controllers
{
    public class ServicesController : Controller
    {
        private readonly AppDbContext _db;
        public ServicesController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Service>? services = await _db.Services
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                .ToListAsync();
            return View(services);
        }
    }
}
