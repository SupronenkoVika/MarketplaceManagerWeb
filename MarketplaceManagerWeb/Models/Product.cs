using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceManagerWeb.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductArticle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Себестоимость")]
        public decimal CostPrice { get; set; }

        [Required]
        public int Stock { get; set; }

        // Навигационное свойство
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

        // Навигационное свойство для истории цен
        public virtual ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
    }
}