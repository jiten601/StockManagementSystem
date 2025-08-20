using Microsoft.AspNetCore.Mvc;
using StockManagementSystem.Models.Cart;
using StockManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace StockManagementSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            var item = _context.StockItems.Find(id);
            if (item != null)
            {
                var cart = GetCart();
                cart.AddItem(new CartItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Quantity = quantity,
                    Price = item.Price
                });
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            cart.RemoveItem(id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        private ShoppingCart GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (cartJson == null)
            {
                var cart = new ShoppingCart();
                SaveCart(cart);
                return cart;
            }
            return JsonConvert.DeserializeObject<ShoppingCart>(cartJson) ?? new ShoppingCart();
        }

        private void SaveCart(ShoppingCart cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}
