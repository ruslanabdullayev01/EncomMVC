using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class News : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        //UNDONE: Date ???

        [NotMapped]
        public List<IFormFile>? Files { get; set; }
        public List<NewsPhoto>? NewsPhotos { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}
