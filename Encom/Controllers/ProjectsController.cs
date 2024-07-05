using Encom.DAL;
using Encom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _db;
        public ProjectsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Project>? projects = await _db.Projects
               .AsNoTracking()
               .Include(x=>x.ProjectPhotos)
               .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
               .ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();

            Project? temp = await _db.Projects
                //.Include(a => a.Language)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted == false);

            if (temp is null) return NotFound();

            //UNDONE: Check language including
            Project? project = await _db.Projects
                //.Include(x => x.Language)
                .Include(x => x.ProjectPhotos)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp!.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name);

            if (project == null) return View(temp);

            return View(project);
        }
    }
}
