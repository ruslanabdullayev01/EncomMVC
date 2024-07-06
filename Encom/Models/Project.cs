using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class Project : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        public List<IFormFile>? Files { get; set; }
        public List<ProjectPhoto>? ProjectPhotos { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}
