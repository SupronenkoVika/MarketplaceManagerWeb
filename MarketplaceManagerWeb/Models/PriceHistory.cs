using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceManagerWeb.Models
{
    [Table("PriceHistory")]
    public class PriceHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OldPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? NewPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OldCostPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? NewCostPrice { get; set; }

        [Required]
        public DateTime ChangedDate { get; set; } = DateTime.Now;

        public int? ChangedByManagerID { get; set; }

        [ForeignKey("ChangedByManagerID")]
        public virtual Manager? ChangedByManager { get; set; }
    }
}