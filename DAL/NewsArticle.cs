using DAL;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLL.Entities
{
    public class NewsArticle
    {
        [Key]
        public Guid NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }

        public Guid CategoryId { get; set; }
        public bool NewsStatus { get; set; }
        public Guid CreatedById { get; set; }
        public Guid UpdatedById { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<Tag> Tags { get; set; } = [];
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public SystemAccount CreatedBy { get; }

        [ForeignKey(nameof(UpdatedById))]
        public SystemAccount UpdatedBy { get; set; }
    }
}
