using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceManagerWeb.Models
{
    [Table("Marketplaces")]
    public class Marketplace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MarketplaceID { get; set; }

        [Required]
        [MaxLength(50)]
        public string MarketplaceName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Стоимость логистики (руб.)")]
        public decimal LogisticsCost { get; set; } = 0;

        // Навигационное свойство
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}