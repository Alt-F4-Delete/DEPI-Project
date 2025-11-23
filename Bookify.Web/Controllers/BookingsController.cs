using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Bookify.Data.Models;
using Bookify.Services.Interfaces;
using Bookify.Web.Models;

namespace Bookify.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BookingsController> _logger;
    private readonly IConfiguration _configuration;

    public BookingsController(
        IBookingService bookingService,
        IRoomService roomService,
        UserManager<ApplicationUser> userManager,
        ILogger<BookingsController> logger,
        IConfiguration configuration)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userManager = userManager;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
            return View(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookings");
            TempData["Error"] = "An error occurred while loading your bookings.";
            return View(new List<Bookify.Data.Models.Booking>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var cart = System.Text.Json.JsonSerializer.Deserialize<CartViewModel>(cartJson);
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // For now, we'll process the first item in the cart
            // In a production app, you might want to handle multiple items
            var cartItem = cart.Items.First();

            // Verify availability one more time
            var isAvailable = await _roomService.IsRoomAvailableAsync(
                cartItem.RoomId, 
                cartItem.CheckInDate, 
                cartItem.CheckOutDate);

            if (!isAvailable)
            {
                TempData["Error"] = "The selected room is no longer available for the specified dates.";
                return RedirectToAction("Index", "Cart");
            }

            // Create booking
            var booking = await _bookingService.CreateBookingAsync(
                user.Id,
                cartItem.RoomId,
                cartItem.CheckInDate,
                cartItem.CheckOutDate);

            // Create Stripe checkout session
            var stripeSecretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                // If Stripe is not configured, just confirm the booking
                await _bookingService.ConfirmBookingAsync(booking.Id, "manual_payment");
                HttpContext.Session.Remove("Cart");
                TempData["Success"] = "Booking confirmed successfully!";
                return RedirectToAction("Details", new { id = booking.Id });
            }

            StripeConfiguration.ApiKey = stripeSecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cartItem.TotalPrice * 100), // Convert to cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{cartItem.RoomTypeName} - Room {cartItem.RoomNumber}",
                                Description = $"Check-in: {cartItem.CheckInDate:MM/dd/yyyy}, Check-out: {cartItem.CheckOutDate:MM/dd/yyyy}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = Url.Action("PaymentSuccess", "Bookings", new { bookingId = booking.Id }, Request.Scheme)!,
                CancelUrl = Url.Action("PaymentCancel", "Bookings", new { bookingId = booking.Id }, Request.Scheme)!
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during checkout");
            TempData["Error"] = "An error occurred during checkout. Please try again.";
            return RedirectToAction("Index", "Cart");
        }
    }

    public async Task<IActionResult> PaymentSuccess(int bookingId, string session_id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            // Confirm the booking with Stripe payment intent
            await _bookingService.ConfirmBookingAsync(bookingId, session_id, session_id);
            
            // Clear cart
            HttpContext.Session.Remove("Cart");
            
            TempData["Success"] = "Payment successful! Your booking has been confirmed.";
            return RedirectToAction("Details", new { id = bookingId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            TempData["Error"] = "An error occurred while confirming your payment.";
            return RedirectToAction("Index");
        }
    }

    public IActionResult PaymentCancel(int bookingId)
    {
        TempData["Error"] = "Payment was cancelled. Your booking is still pending.";
        return RedirectToAction("Details", new { id = bookingId });
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (booking.UserId != user.Id && !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            return View(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking details");
            TempData["Error"] = "An error occurred while loading booking details.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || booking.UserId != user.Id)
            {
                return Forbid();
            }

            await _bookingService.CancelBookingAsync(id);
            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking");
            TempData["Error"] = "An error occurred while cancelling the booking.";
            return RedirectToAction("Details", new { id });
        }
    }
}

