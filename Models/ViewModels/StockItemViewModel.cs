using System.ComponentModel.DataAnnotations;

namespace StockManagementSystem.Models.ViewModels
{
    public class StockItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

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

        // For display purposes
        public string? CategoryName { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }
    }
} 