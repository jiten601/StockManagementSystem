using System.ComponentModel.DataAnnotations;

namespace StockManagementSystem.Models.ViewModels
{
    public class BuyItemViewModel
    {
        public int StockItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int AvailableQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please enter a quantity of at least 1.")]
        public int QuantityToBuy { get; set; }
    }
}



