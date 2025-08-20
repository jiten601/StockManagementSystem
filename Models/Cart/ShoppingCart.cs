using System.Collections.Generic;

namespace StockManagementSystem.Models.Cart
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int id)
        {
            Items.RemoveAll(i => i.Id == id);
        }

        public decimal GetTotal()
        {
            return Items.Sum(i => i.Total);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
