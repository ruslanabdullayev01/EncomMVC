using Encom.DAL;
using Encom.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Encom.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _db;
        public LayoutService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<string> GetCurrentLangauge()
        {
            string CurrentLangauge = CultureInfo.CurrentCulture.Name;
            return CurrentLangauge;
        }

        public async Task<IEnumerable<Setting>> GetSettings()
        {
            IEnumerable<Setting>? settings = await _db.Settings.Include(a => a.Language).ToListAsync();
            return settings;
        }
    }
}
