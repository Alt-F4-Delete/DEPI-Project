using Microsoft.AspNetCore.Mvc;
using Bookify.Services.Interfaces;

namespace Bookify.Web.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(DateTime? checkIn, DateTime? checkOut, int? roomTypeId)
    {
        try
        {
            var roomTypes = await _roomService.GetAllRoomTypesAsync();
            ViewBag.RoomTypes = roomTypes;

            if (checkIn.HasValue && checkOut.HasValue)
            {
                var availableRooms = await _roomService.GetAvailableRoomsAsync(
                    checkIn.Value, 
                    checkOut.Value, 
                    roomTypeId);
                
                ViewBag.CheckIn = checkIn.Value;
                ViewBag.CheckOut = checkOut.Value;
                ViewBag.SelectedRoomTypeId = roomTypeId;
                
                return View(availableRooms);
            }

            return View(new List<Bookify.Data.Models.Room>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading rooms");
            TempData["Error"] = "An error occurred while loading rooms. Please try again.";
            return View(new List<Bookify.Data.Models.Room>());
        }
    }

    public async Task<IActionResult> Details(int id, DateTime? checkIn, DateTime? checkOut)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;

            if (checkIn.HasValue && checkOut.HasValue)
            {
                var isAvailable = await _roomService.IsRoomAvailableAsync(
                    id, 
                    checkIn.Value, 
                    checkOut.Value);
                ViewBag.IsAvailable = isAvailable;

                if (isAvailable)
                {
                    var totalPrice = _roomService.CalculateTotalPrice(
                        room.RoomType.PricePerNight, 
                        checkIn.Value, 
                        checkOut.Value);
                    ViewBag.TotalPrice = totalPrice;
                }
            }

            return View(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading room details");
            TempData["Error"] = "An error occurred while loading room details.";
            return RedirectToAction(nameof(Index));
        }
    }
}

