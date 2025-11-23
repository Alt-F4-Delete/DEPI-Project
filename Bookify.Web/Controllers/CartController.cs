using Microsoft.AspNetCore.Mvc;
using Bookify.Services.Interfaces;
using Bookify.Web.Models;

namespace Bookify.Web.Controllers;

public class CartController : Controller
{
    private readonly IRoomService _roomService;
    private readonly ILogger<CartController> _logger;

    public CartController(IRoomService roomService, ILogger<CartController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int roomId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            if (checkIn >= checkOut)
            {
                TempData["Error"] = "Check-in date must be before check-out date.";
                return RedirectToAction("Details", "Rooms", new { id = roomId, checkIn, checkOut });
            }

            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, checkIn, checkOut);
            if (!isAvailable)
            {
                TempData["Error"] = "This room is not available for the selected dates.";
                return RedirectToAction("Details", "Rooms", new { id = roomId, checkIn, checkOut });
            }

            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = new CartItem
            {
                RoomId = roomId,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType.Name,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                PricePerNight = room.RoomType.PricePerNight,
                TotalPrice = _roomService.CalculateTotalPrice(room.RoomType.PricePerNight, checkIn, checkOut)
            };

            cart.Items.Add(cartItem);
            SaveCart(cart);

            TempData["Success"] = "Room added to cart successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding room to cart");
            TempData["Error"] = "An error occurred while adding the room to cart.";
            return RedirectToAction("Details", "Rooms", new { id = roomId });
        }
    }

    [HttpPost]
    public IActionResult RemoveFromCart(int index)
    {
        var cart = GetCart();
        if (index >= 0 && index < cart.Items.Count)
        {
            cart.Items.RemoveAt(index);
            SaveCart(cart);
            TempData["Success"] = "Item removed from cart.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove("Cart");
        TempData["Success"] = "Cart cleared.";
        return RedirectToAction(nameof(Index));
    }

    private CartViewModel GetCart()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson))
        {
            return new CartViewModel();
        }

        return System.Text.Json.JsonSerializer.Deserialize<CartViewModel>(cartJson) ?? new CartViewModel();
    }

    private void SaveCart(CartViewModel cart)
    {
        var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString("Cart", cartJson);
    }
}

