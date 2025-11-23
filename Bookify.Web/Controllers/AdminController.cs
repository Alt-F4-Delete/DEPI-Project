using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bookify.Data.Models;
using Bookify.Data.UnitOfWork;
using Bookify.Services.Interfaces;
using Bookify.Web.Models;

namespace Bookify.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoomService _roomService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUnitOfWork unitOfWork,
        IRoomService roomService,
        ILogger<AdminController> logger)
    {
        _unitOfWork = unitOfWork;
        _roomService = roomService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region Rooms Management

    public async Task<IActionResult> Rooms()
    {
        try
        {
            var rooms = await _unitOfWork.Rooms.GetRoomsWithTypeAsync();
            return View(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading rooms");
            TempData["Error"] = "An error occurred while loading rooms.";
            return View(new List<Room>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateRoom()
    {
        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name");
        return View(new RoomViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoom(RoomViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var room = new Room
                {
                    RoomNumber = model.RoomNumber,
                    RoomTypeId = model.RoomTypeId,
                    IsAvailable = model.IsAvailable,
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Rooms.AddAsync(room);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Room created successfully!";
                return RedirectToAction(nameof(Rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                TempData["Error"] = "An error occurred while creating the room.";
            }
        }

        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoom(int id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var model = new RoomViewModel
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            RoomTypeId = room.RoomTypeId,
            IsAvailable = room.IsAvailable,
            Notes = room.Notes
        };

        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", room.RoomTypeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoom(RoomViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(model.Id ?? 0);
                if (room == null)
                {
                    return NotFound();
                }

                room.RoomNumber = model.RoomNumber;
                room.RoomTypeId = model.RoomTypeId;
                room.IsAvailable = model.IsAvailable;
                room.Notes = model.Notes;

                _unitOfWork.Rooms.Update(room);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Room updated successfully!";
                return RedirectToAction(nameof(Rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room");
                TempData["Error"] = "An error occurred while updating the room.";
            }
        }

        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
        ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", model.RoomTypeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _unitOfWork.Rooms.Remove(room);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Room deleted successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room");
            TempData["Error"] = "An error occurred while deleting the room.";
        }

        return RedirectToAction(nameof(Rooms));
    }

    #endregion

    #region Room Types Management

    public async Task<IActionResult> RoomTypes()
    {
        try
        {
            var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync();
            return View(roomTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading room types");
            TempData["Error"] = "An error occurred while loading room types.";
            return View(new List<RoomType>());
        }
    }

    [HttpGet]
    public IActionResult CreateRoomType()
    {
        return View(new RoomTypeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoomType(RoomTypeViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var roomType = new RoomType
                {
                    Name = model.Name,
                    Description = model.Description,
                    PricePerNight = model.PricePerNight,
                    MaxOccupancy = model.MaxOccupancy,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.RoomTypes.AddAsync(roomType);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Room type created successfully!";
                return RedirectToAction(nameof(RoomTypes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room type");
                TempData["Error"] = "An error occurred while creating the room type.";
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoomType(int id)
    {
        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
        if (roomType == null)
        {
            return NotFound();
        }

        var model = new RoomTypeViewModel
        {
            Id = roomType.Id,
            Name = roomType.Name,
            Description = roomType.Description,
            PricePerNight = roomType.PricePerNight,
            MaxOccupancy = roomType.MaxOccupancy,
            ImageUrl = roomType.ImageUrl,
            IsActive = roomType.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoomType(RoomTypeViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(model.Id ?? 0);
                if (roomType == null)
                {
                    return NotFound();
                }

                roomType.Name = model.Name;
                roomType.Description = model.Description;
                roomType.PricePerNight = model.PricePerNight;
                roomType.MaxOccupancy = model.MaxOccupancy;
                roomType.ImageUrl = model.ImageUrl;
                roomType.IsActive = model.IsActive;

                _unitOfWork.RoomTypes.Update(roomType);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = "Room type updated successfully!";
                return RedirectToAction(nameof(RoomTypes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room type");
                TempData["Error"] = "An error occurred while updating the room type.";
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoomType(int id)
    {
        try
        {
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            // Check if there are rooms using this type
            var roomsWithType = await _unitOfWork.Rooms.FindAsync(r => r.RoomTypeId == id);
            if (roomsWithType.Any())
            {
                TempData["Error"] = "Cannot delete room type. There are rooms using this type.";
                return RedirectToAction(nameof(RoomTypes));
            }

            _unitOfWork.RoomTypes.Remove(roomType);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Room type deleted successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room type");
            TempData["Error"] = "An error occurred while deleting the room type.";
        }

        return RedirectToAction(nameof(RoomTypes));
    }

    #endregion

    #region Bookings

    public async Task<IActionResult> Bookings()
    {
        try
        {
            var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync();
            return View(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookings");
            TempData["Error"] = "An error occurred while loading bookings.";
            return View(new List<Booking>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> BookingDetails(int id)
    {
        try
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking details");
            TempData["Error"] = "An error occurred while loading booking details.";
            return RedirectToAction(nameof(Bookings));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(int id, string status)
    {
        try
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid booking status.";
                return RedirectToAction(nameof(BookingDetails), new { id });
            }

            booking.Status = status;
            if (status == "Confirmed" && !booking.ConfirmedAt.HasValue)
            {
                booking.ConfirmedAt = DateTime.UtcNow;
            }

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Booking status updated to {status} successfully!";
            return RedirectToAction(nameof(BookingDetails), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking status");
            TempData["Error"] = "An error occurred while updating booking status.";
            return RedirectToAction(nameof(BookingDetails), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        try
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _unitOfWork.Bookings.Remove(booking);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Booking deleted successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking");
            TempData["Error"] = "An error occurred while deleting the booking.";
        }

        return RedirectToAction(nameof(Bookings));
    }

    #endregion
}
