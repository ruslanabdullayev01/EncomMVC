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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CertificatesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public CertificatesController(AppDbContext db
            , IWebHostEnvironment env
            , UserManager<User> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        #region Index
        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Certificate> query = _db.Certificates.Where(x => !x.IsDeleted);
            return View(PageNatedList<Certificate>.Create(query, pageIndex, 10, 10));
        }
        #endregion

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Certificate model)
        {
            if (model.Photo != null)
            {
                string currentUsername = _userManager.GetUserName(HttpContext.User);
                if (!(model.Photo.CheckFileContenttype("image/jpeg") || model.Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("Photo", $"{model.Photo.FileName} is not the correct format");
                }

                if (model.Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("Photo", $"Photo must be less than 5 mb");
                }

                model.ImagePath = await model.Photo.CreateFileAsync(_env, "src", "assets", "images"); // UNDONE: Source deyise biler
                model.CreatedAt = DateTime.UtcNow.AddHours(4);
                model.CreatedBy = currentUsername;
            }
            else
            {
                ModelState.AddModelError("Photo", "Image is empty");
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

            await _db.AddAsync(model);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
        #endregion

        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Certificate? dbCertificate = await _db.Certificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (dbCertificate == null) return NotFound();

            return View(dbCertificate);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Certificate certificate)
        {
            if (id == null) return BadRequest();

            Certificate? dbCertificate = await _db.Certificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (dbCertificate == null) return NotFound();

            #region Image
            if (certificate.Photo != null)
            {
                if (!(certificate.Photo.CheckFileContenttype("image/jpeg") || certificate.Photo.CheckFileContenttype("image/png")))
                {
                    ModelState.AddModelError("Photo", $"{certificate.Photo.FileName} is not the correct format");
                }

                if (certificate.Photo.CheckFileLength(5120))
                {
                    ModelState.AddModelError("Photo", $"Photo must be less than 5 mb");
                }

                string previousFilePath = dbCertificate.ImagePath;
                if (previousFilePath != null)
                {
                    FileHelper.DeleteFile(previousFilePath, _env, "src", "assets", "images");
                }

                dbCertificate.ImagePath = await certificate.Photo.CreateFileAsync(_env, "src", "assets", "images");
            }
            #endregion
            string currentUsername = _userManager.GetUserName(HttpContext.User);

            dbCertificate.UpdatedBy = currentUsername;
            dbCertificate.UpdatedAt = DateTime.UtcNow.AddHours(4);

            var validationErrors = new Dictionary<string, string[]>();
            if (!ModelState.IsValid)
            {
                validationErrors = ModelState.ToDictionary(
                    err => err.Key,
                    err => err.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return Json(new { success = false, errors = validationErrors });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true } );
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Certificate? certificate = await _db.Certificates
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (certificate == null) return NotFound();

            if (certificate.ImagePath != null)
            {
                FileHelper.DeleteFile(certificate.ImagePath, _env, "src", "assets", "images");
            }

            _db.Certificates.Remove(certificate);
            await _db.SaveChangesAsync();
            return Json(new { success = true });

        }
        #endregion
    }
}
