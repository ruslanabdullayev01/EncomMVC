using Encom.DAL;
using Encom.Helpers;
using Encom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Areas.EncomAdmin.Controllers
{
    [Area(nameof(EncomAdmin))]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public ProjectsController(AppDbContext db, IWebHostEnvironment env, UserManager<User> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Project> query = _db.Projects
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name);
            return View(PageNatedList<Project>.Create(query, pageIndex, 10, 10));
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
        public async Task<IActionResult> Create(List<Project> models)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(models);
            }

            #region Image
            foreach (var item in models)
            {
                List<ProjectPhoto> projectImages = new List<ProjectPhoto>();
                if (models[0].Files != null && models[0].Files.Count() > 0)
                {
                    int orderNumber = 0;
                    foreach (IFormFile file in models[0].Files)
                    {
                        orderNumber++;
                        if (!(file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png")))
                        {
                            ModelState.AddModelError("[0].Photo", $"{file.FileName} is not the correct format");
                            return View(models);
                        }

                        if (file.CheckFileLength(5120))
                        {
                            ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                            return View(models);
                        }

                        ProjectPhoto projectImage = new()
                        {
                            ImagePath = await file.CreateDynamicFileAsync(_env, "src", "assets", "images"),
                            OrderNumber = orderNumber,
                            Project = item
                        };

                        projectImages.Add(projectImage);
                    }
                }
                else
                {
                    ModelState.AddModelError("[0].Photo", "Image is empty");
                    return View(models);
                }
                Project? temp = await _db.Projects.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                string currentUsername = _userManager.GetUserName(HttpContext.User);
                item.ProjectPhotos = projectImages;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;
                await _db.Projects.AddAsync(item);
            }
            #endregion

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            Project? firstProject = await _db.Projects
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstProject == null) return NotFound();

            List<Project> projects = await _db.Projects
                .Where(c => c.LanguageGroup == firstProject.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.ProjectPhotos)
                .ToListAsync();

            return View(projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, List<Project> projects)
        {
            ViewBag.Languages = await _db.Languages.ToListAsync();

            if (!ModelState.IsValid) return View(projects);

            if (id == null) return BadRequest();

            Project? firstProject = await _db.Projects
                .Include(x => x.ProjectPhotos)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstProject == null) return NotFound();

            List<Project> dbProjectsList = await _db.Projects
                .Where(c => c.LanguageGroup == firstProject.LanguageGroup && c.IsDeleted == false)
                .Include(x => x.ProjectPhotos)
                .ToListAsync();

            if (dbProjectsList == null || dbProjectsList.Count == 0) return NotFound();

            List<ProjectPhoto> newPhotos = new List<ProjectPhoto>();

            foreach (var item in projects)
            {
                var dbProject = dbProjectsList.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                if (dbProject != null)
                {
                    string? currentUsername = _userManager.GetUserName(HttpContext.User);
                    dbProject.Title = item.Title.Trim();
                    dbProject.Description = item.Description.Trim();
                    dbProject.UpdatedAt = DateTime.UtcNow.AddHours(4);
                    dbProject.UpdatedBy = currentUsername;

                    if (item.Files != null && item.Files.Count() > 0)
                    {
                        foreach (IFormFile file in item.Files)
                        {
                            if (file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png"))
                            {
                                if (!file.CheckFileLength(5120))
                                {
                                    ProjectPhoto projectImage = new()
                                    {
                                        ImagePath = await file.CreateDynamicFileAsync(_env, "src", "assets", "images"),
                                        OrderNumber = dbProject.ProjectPhotos.Count + 1
                                    };

                                    newPhotos.Add(projectImage);
                                }
                                else
                                {
                                    ModelState.AddModelError("[0].Photo", $"Photo must be less than 5 mb");
                                    return View(projects);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("[0].Photo", $"{file.FileName} is not the correct format");
                                return View(projects);
                            }
                        }
                    }
                }
            }

            foreach (var dbProject in dbProjectsList)
            {
                foreach (var photo in newPhotos)
                {
                    dbProject.ProjectPhotos.Add(new ProjectPhoto
                    {
                        ImagePath = photo.ImagePath,
                        Project = dbProject
                    });
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> UpdateOrder(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ViewBag.Languages = await _db.Languages.ToListAsync();

            Project? temp = await _db.Projects
                .Include(p => p.ProjectPhotos)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (temp == null)
            {
                return NotFound("Project not found.");
            }

            List<ProjectPhoto> currentPhotos = await _db.ProjectPhotos
                .Where(x => x.ProjectId == temp.Id)
                .OrderBy(x => x.OrderNumber)
                .ToListAsync();

            return View(currentPhotos);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateOrder(List<ProjectPhoto> photos)
        {
            if (photos == null || photos.Count == 0)
            {
                return BadRequest("No data received.");
            }

            foreach (var photo in photos)
            {
                _db.Entry(photo).Property(x => x.OrderNumber).IsModified = true;
            }

            try
            {
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Failed to update photos: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage(int? id, string? imagePath)
        {
            if (id == null || string.IsNullOrEmpty(imagePath)) return BadRequest();

            Project? project = await _db.Projects
                .Include(p => p.ProjectPhotos)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (project == null) return NotFound();

            List<ProjectPhoto> photosToDelete = await _db.ProjectPhotos
                .Include(x => x.Project)
                .Where(x => x.ImagePath == imagePath && x.Project.LanguageGroup == project.LanguageGroup)
                .ToListAsync();

            if (photosToDelete != null && photosToDelete.Count > 0)
            {
                foreach (var photoToDelete in photosToDelete)
                {
                    FileHelper.DeleteFile(photoToDelete.ImagePath, _env, "src", "assets", "images");
                    _db.ProjectPhotos.Remove(photoToDelete);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Photos deleted successfully." });
        }
        #endregion

        #region Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? temp = await _db.Projects
                .AsNoTracking()
                .Include(a => a.Language)
                .FirstOrDefaultAsync(s => s.IsDeleted == false && s.Id == id);
            if (temp == null) return NotFound();

            Project? project = await _db.Projects.AsNoTracking()
                .Include(x => x.Language)
                .Include(x => x.ProjectPhotos.OrderBy(x=>x.OrderNumber))
                .FirstOrDefaultAsync(x => x.LanguageGroup == temp.LanguageGroup && x.Language!.Culture == CultureInfo.CurrentCulture.Name); ;
            if (project == null) return BadRequest();
            return View(project);
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            List<Language> languages = await _db.Languages.ToListAsync();
            ViewBag.Languages = languages;

            Project? firstProject = await _db.Projects.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (firstProject == null) return NotFound();

            List<Project> projects = await _db.Projects
                                       .Where(c => c.LanguageGroup == firstProject.LanguageGroup && c.IsDeleted == false)
                                       .ToListAsync();

            string? currentUsername = _userManager.GetUserName(HttpContext.User);

            List<ProjectPhoto> projectImages = await _db.ProjectPhotos.Where(x => x.ProjectId == id).ToListAsync();
            if (projectImages != null && projectImages.Count > 0)
            {
                foreach (ProjectPhoto photo in projectImages)
                {
                    FileHelper.DeleteFile(photo.ImagePath, _env, "src", "assets", "images");
                    _db.ProjectPhotos.RemoveRange(projectImages);
                }
            }

            foreach (Project project in projects)
            {
                if (project == null) return NotFound();
                project.IsDeleted = true;
                project.DeletedBy = currentUsername;
                project.DeletedAt = DateTime.UtcNow.AddHours(4);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        #endregion

    }
}
