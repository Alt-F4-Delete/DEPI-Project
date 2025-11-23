namespace Bookify.Web.Models;

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    
    public decimal Total => Items.Sum(item => item.TotalPrice);
}

public class CartItem
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
}

