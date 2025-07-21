using KogiExportHub.Models;
using Newtonsoft.Json;

namespace KogiExportHub.Services
{
    public interface ICartService
    {
        Cart GetCart(HttpContext context);
        void SaveCart(HttpContext context, Cart cart);
        void AddToCart(HttpContext context, CartItem item);
        void RemoveFromCart(HttpContext context, int productId);
        void UpdateCartItemQuantity(HttpContext context, int productId, int quantity);
        void ClearCart(HttpContext context);
    }

    public class CartService : ICartService
    {
        private const string CartSessionKey = "KogiExportHub_Cart";

        public Cart GetCart(HttpContext context)
        {
            var cartJson = context.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }
            
            return JsonConvert.DeserializeObject<Cart>(cartJson) ?? new Cart();
        }

        public void SaveCart(HttpContext context, Cart cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            context.Session.SetString(CartSessionKey, cartJson);
        }

        public void AddToCart(HttpContext context, CartItem item)
        {
            var cart = GetCart(context);
            cart.AddItem(item);
            SaveCart(context, cart);
        }

        public void RemoveFromCart(HttpContext context, int productId)
        {
            var cart = GetCart(context);
            cart.RemoveItem(productId);
            SaveCart(context, cart);
        }

        public void UpdateCartItemQuantity(HttpContext context, int productId, int quantity)
        {
            var cart = GetCart(context);
            cart.UpdateQuantity(productId, quantity);
            SaveCart(context, cart);
        }

        public void ClearCart(HttpContext context)
        {
            var cart = new Cart();
            SaveCart(context, cart);
        }
    }
}