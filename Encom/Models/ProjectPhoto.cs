using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class ProjectPhoto
    {
        public int Id { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? ImagePath { get; set; }

        public Project? Project { get; set; }
        public int ProjectId { get; set; }
    }
}
