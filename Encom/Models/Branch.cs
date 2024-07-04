namespace Encom.Models
{
    public class Branch : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }

        //UNDONE: Branch deyislecek

        public int? LanguageId { get; set; }
        public Language? Language { get; set; }
        public int LanguageGroup { get; set; }
    }
}
