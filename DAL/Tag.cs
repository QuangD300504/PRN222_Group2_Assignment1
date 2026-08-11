using System.ComponentModel.DataAnnotations;

namespace BLL.Entities
{
    public class Tag
    {
        [Key]
        public Guid TagId { get; set; }
        public string TagName { get; set; }
        public string Note { get; set; }

        public ICollection<NewsArticle> NewsArticles { get; set; } = [];
    }
}
