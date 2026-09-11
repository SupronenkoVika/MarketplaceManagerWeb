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

        [Required(ErrorMessage = "Дата продажи обязательна")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата продажи")]
        public DateTime SaleDate { get; set; }

        [Required]
        public int ManagerID { get; set; }

        [ForeignKey("ManagerID")]
        public Manager? Manager { get; set; }

        [Required]
        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public Product? Product { get; set; }

        [Required]
        public int MarketplaceID { get; set; }

        [ForeignKey("MarketplaceID")]
        public Marketplace? Marketplace { get; set; }

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