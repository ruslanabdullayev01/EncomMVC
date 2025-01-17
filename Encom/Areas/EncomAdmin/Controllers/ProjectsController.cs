﻿using Encom.DAL;
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
                .Where(x => !x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.ProjectPhotos);
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

            #region Image
            bool fileErrorAdded = false;
            foreach (var item in models)
            {
                List<ProjectPhoto> projectImages = new List<ProjectPhoto>();

                Project? temp = await _db.Projects.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                string currentUsername = _userManager.GetUserName(HttpContext.User);
                item.ProjectPhotos = projectImages;
                item.LanguageGroup = temp != null ? temp.LanguageGroup + 1 : 1;
                item.CreatedAt = DateTime.UtcNow.AddHours(4);
                item.CreatedBy = currentUsername;

                if (models[0].Files != null && models[0].Files.Count() > 0)
                {
                    int orderNumber = 0;
                    foreach (IFormFile file in models[0].Files)
                    {
                        orderNumber++;
                        if (!(file.CheckFileContenttype("image/jpeg") || file.CheckFileContenttype("image/png")))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                fileErrorAdded = true;
                            }
                        }

                        if (file.CheckFileLength(5120))
                        {
                            if (!fileErrorAdded)
                            {
                                ModelState.AddModelError("[0].Files", $"Photo must be less than 5 mb");
                                fileErrorAdded = true;
                            }
                        }

                        ProjectPhoto projectImage = new()
                        {
                            ImagePath = await file.CreateDynamicFileAsync(item.LanguageGroup, _env, "src", "assets", "images"),
                            OrderNumber = orderNumber,
                            Project = item
                        };

                        projectImages.Add(projectImage);
                    }
                }
                else
                {
                    if (!fileErrorAdded)
                    {
                        ModelState.AddModelError("[0].Files", "Image is empty");
                        fileErrorAdded = true;
                    }
                }

                await _db.Projects.AddAsync(item);
            }
            #endregion

            #region Validations
            for (int i = 0; i < models.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(models[i].Title))
                {
                    ModelState.AddModelError($"[{i}].Title", "The Title field is required.");
                }
                if (string.IsNullOrWhiteSpace(models[i].Description))
                {
                    ModelState.AddModelError($"[{i}].Description", "The Description field is required.");
                }
            }

            var validationErrors = new Dictionary<string, string[]>();
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
            bool fileErrorAdded = false;
            foreach (var item in projects)
            {
                var dbProject = dbProjectsList.FirstOrDefault(s => s.LanguageId == item.LanguageId);
                if (dbProject != null)
                {
                    string? currentUsername = _userManager.GetUserName(HttpContext.User);
                    dbProject.Title = item.Title != null ? item.Title.Trim() : null;
                    dbProject.Description = item.Description != null ? item.Description.Trim() : null;
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
                                        ImagePath = await file.CreateDynamicFileAsync(dbProject.LanguageGroup, _env, "src", "assets", "images"),
                                        OrderNumber = dbProject.ProjectPhotos.Count + 1
                                    };

                                    newPhotos.Add(projectImage);
                                }
                                else
                                {
                                    if (!fileErrorAdded)
                                    {
                                        ModelState.AddModelError("[0].Files", $"Photo must be less than 5 mb");
                                        fileErrorAdded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (!fileErrorAdded)
                                {
                                    ModelState.AddModelError("[0].Files", $"{file.FileName} is not the correct format");
                                    fileErrorAdded = true;
                                }
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
                        OrderNumber = dbProject.ProjectPhotos.Count + 1,
                        Project = dbProject
                    });
                }
            }

            #region Validations
            for (int i = 0; i < projects.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(projects[i].Title))
                {
                    ModelState.AddModelError($"[{i}].Title", "The Title field is required.");
                }
                if (string.IsNullOrWhiteSpace(projects[i].Description))
                {
                    ModelState.AddModelError($"[{i}].Description", "The Description field is required.");
                }
            }

            var validationErrors = new Dictionary<string, string[]>();
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
        }

        #region Order Number
        public async Task<IActionResult> UpdateOrder(int? id)
        {
            if (id == null) return BadRequest();

            ViewBag.Languages = await _db.Languages.ToListAsync();

            Project? temp = await _db.Projects
                .Include(p => p.ProjectPhotos)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (temp == null) return NotFound();

            List<ProjectPhoto> currentPhotos = await _db.ProjectPhotos
                .Where(x => x.ProjectId == temp.Id)
                .OrderBy(x => x.OrderNumber)
                .ToListAsync();

            return View(currentPhotos);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(List<ProjectPhoto> photos)
        {
            if (photos == null || photos.Count == 0) return BadRequest();

            var firstPhoto = photos.FirstOrDefault();
            if (firstPhoto == null || string.IsNullOrEmpty(firstPhoto.ImagePath)) return NotFound();

            var dbPhotos = await _db.ProjectPhotos
                .Include(p => p.Project)
                .Where(p => photos.Select(photo => photo.ImagePath).Contains(p.ImagePath))
                .ToListAsync();

            foreach (var photo in photos)
            {
                var matchedPhotos = dbPhotos
                    .Where(p => p.ImagePath == photo.ImagePath)
                    .ToList();

                foreach (var matchedPhoto in matchedPhotos)
                {
                    matchedPhoto.OrderNumber = photo.OrderNumber;
                    _db.Entry(matchedPhoto).Property(x => x.OrderNumber).IsModified = true;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Delete Image
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
                .Include(x => x.ProjectPhotos.OrderBy(x => x.OrderNumber))
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
            return Json(new { success = true });

        }
        #endregion

    }
}
