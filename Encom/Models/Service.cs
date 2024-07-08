using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class Service : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? IconPath { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}

