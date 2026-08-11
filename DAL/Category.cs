using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLL.Entities
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }

        
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<NewsArticle> NewsArticles { get; set; } = [];

        [ForeignKey(nameof(ParentCategoryId))]
        public Category? ParentCategory { get; set; }
    }
}
