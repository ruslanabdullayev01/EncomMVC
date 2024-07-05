namespace Encom.Models
{
    public class Contact : BaseEntity
    {
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Number { get; set; }
        public string? MapIFrame { get; set; }

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}
