using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceManagerWeb.Models
{
    [Table("Sales")]
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; }

        [Required]
        public int ManagerID { get; set; }

        [ForeignKey("ManagerID")]
        public virtual Manager? Manager { get; set; }

        [Required]
        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [Required]
        public int MarketplaceID { get; set; }

        [ForeignKey("MarketplaceID")]
        public virtual Marketplace? Marketplace { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Commission { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal LogisticsCost { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal NetProfit { get; set; }
    }
}