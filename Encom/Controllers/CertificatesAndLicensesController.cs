using Encom.DAL;
using Encom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Controllers
{
    public class CertificatesAndLicensesController : Controller
    {
        private readonly AppDbContext _db;
        public CertificatesAndLicensesController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            CertificateAndLicenseVM certificateAndLicenseVM = new()
            {
                Certificates = await _db.Certificates.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(),
                Licenses = await _db.Licenses
                    .AsNoTracking()
                    .Where(x=>!x.IsDeleted && x.Language!.Culture == CultureInfo.CurrentCulture.Name)
                    .ToListAsync()
            };
            return View(certificateAndLicenseVM);
        }
    }
}
