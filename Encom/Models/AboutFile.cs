using System.ComponentModel.DataAnnotations.Schema;

namespace Encom.Models
{
    public class AboutFile
    {
        public int Id { get; set; }
        [NotMapped]
        public IFormFile? File { get; set; }
        public string? FilePath { get; set; }

        public bool IsMain { get; set; }

        public About? About { get; set; }
        public int AboutId { get; set; }
    }
}
