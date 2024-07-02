using Microsoft.AspNetCore.Identity;

namespace Encom.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
