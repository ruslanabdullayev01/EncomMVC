using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class AboutFile
    {
        public int Id { get; set; }
        [NotMapped]
        public IFormFile? File { get; set; }
        public string? FilePath { get; set; }
        public int OrderNumber { get; set; }

        public About? About { get; set; }
        public int AboutId { get; set; }
    }
}
