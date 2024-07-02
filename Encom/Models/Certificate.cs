using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class Certificate : BaseEntity
    {
        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? ImagePath { get; set; }
    }
}
