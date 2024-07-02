using System.ComponentModel.DataAnnotations;

namespace Encom.Models
{
    public class Appeal:BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        [Phone] public string Phone { get; set; }
        [EmailAddress] public string Email { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string? ReadedBy { get; set; }
    }
}
