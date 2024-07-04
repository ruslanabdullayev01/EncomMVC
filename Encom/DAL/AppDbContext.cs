using Encom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Encom.DAL
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {

        public DbSet<License> Licenses { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<NewsPhoto> NewsPhotos { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<ProjectPhoto> ProjectPhotos { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<SocialMedia> SocialMedias { get; set; }
        public DbSet<Appeal> Appeals { get; set; }
        public DbSet<About> Abouts { get; set; }
        public DbSet<AboutFile> AboutFiles { get; set; }
        public DbSet<User> User { get; set; }
        //UNDONE: Branches changes
        //public DbSet<Branch> Branches { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Setting> Settings { get; set; }
    }
}
