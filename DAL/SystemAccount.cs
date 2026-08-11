using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class SystemAccount
    {
        [Key]
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public string AccountEmail { get; set; }
        public short AccountRole { get; set; }

    }
}
