using System.ComponentModel.DataAnnotations;

namespace KogiExportHub.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public int AvailableQuantity { get; set; }
        
        // Add this property for Flutterwave subaccount integration
        public string? SellerSubaccountId { get; set; }
        
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public decimal SubTotal => Items.Sum(item => item.TotalPrice);
        public decimal GovernmentTax => SubTotal * 0.075m; // 7.5% VAT
        public decimal PlatformFee => SubTotal * 0.025m; // 2.5% platform fee
        public decimal Total => SubTotal + GovernmentTax + PlatformFee;
        public int TotalItems => Items.Sum(item => item.Quantity);
        
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }
        
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveItem(productId);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
        }
        
        public void Clear()
        {
            Items.Clear();
        }
    }

    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new Cart();
        
        [Required]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}