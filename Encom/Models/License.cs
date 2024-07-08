using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class License : BaseEntity
    {
        public string? Name { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? ImagePath { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}
