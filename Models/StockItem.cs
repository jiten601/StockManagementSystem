using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementSystem.Models
{
    public class StockItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        public DateTime PurchaseDate { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Supplier { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinimumQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReorderPoint { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;
        public virtual ApplicationUser? UpdatedByUser { get; set; }
    }
} 