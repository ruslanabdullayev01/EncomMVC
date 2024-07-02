using System.ComponentModel.DataAnnotations;

namespace Encom.ViewModels.Account
{
    public class RegisterVM
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string CheckPassword { get; set; }
        public bool IsRemember { get; set; }
    }
}
