using Encom.DAL;
using Encom.Models;
using Encom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.VieComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        public FooterViewComponent(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            FooterVM footerVM = new FooterVM
            {
                SocialMedia = await _db.SocialMedias.FirstOrDefaultAsync(x => !x.IsDeleted)
            };

            return View(footerVM);
        }
    }
}
