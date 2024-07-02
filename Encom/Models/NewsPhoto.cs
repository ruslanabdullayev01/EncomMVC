using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class NewsPhoto
    {
        public int Id { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? ImagePath { get; set; }

        public News? News { get; set; }
        public int NewsId { get; set; }
    }
}
