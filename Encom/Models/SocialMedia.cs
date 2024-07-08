namespace Encom.Models
{
    public class SocialMedia : BaseEntity
    {
        public required string Facebook { get; set; }
        public required string Twitter { get; set; }
        public required string Telegram { get; set; }
        public required string Linkedin { get; set; }
    }
}
