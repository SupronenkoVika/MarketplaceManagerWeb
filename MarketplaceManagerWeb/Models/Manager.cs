using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceManagerWeb.Models
{
    [Table("Managers")]
    public class Manager
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ManagerID { get; set; }

        [Required]
        [MaxLength(20)]
        public string ManagerLName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ManagerFName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ManagerMName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ManagerLog { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ManagerPass { get; set; } = string.Empty;

        public bool ManagerIsAdmin { get; set; } = false;

        // Навигационное свойство - связь с таблицей Sales
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}