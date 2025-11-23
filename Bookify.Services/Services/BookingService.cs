using Bookify.Data.Models;
using Bookify.Data.UnitOfWork;
using Bookify.Services.Interfaces;

namespace Bookify.Services.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoomService _roomService;

    public BookingService(IUnitOfWork unitOfWork, IRoomService roomService)
    {
        _unitOfWork = unitOfWork;
        _roomService = roomService;
    }

    public async Task<Booking> CreateBookingAsync(string userId, int roomId, DateTime checkIn, DateTime checkOut, string? specialRequests = null)
    {
        // Validate dates
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be before check-out date.");

        if (checkIn < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");

        // Check room availability
        var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, checkIn, checkOut);
        if (!isAvailable)
            throw new InvalidOperationException("The selected room is not available for the specified dates.");

        // Get room with type to calculate price
        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null)
            throw new ArgumentException("Room not found.");

        // Calculate total price
        var totalPrice = _roomService.CalculateTotalPrice(room.RoomType.PricePerNight, checkIn, checkOut);

        // Create booking
        var booking = new Booking
        {
            UserId = userId,
            RoomId = roomId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = totalPrice,
            Status = "Pending",
            SpecialRequests = specialRequests,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return booking;
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
    {
        return await _unitOfWork.Bookings.GetUserBookingsAsync(userId);
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _unitOfWork.Bookings.GetBookingWithDetailsAsync(id);
    }

    public async Task ConfirmBookingAsync(int bookingId, string stripePaymentIntentId, string? stripeSessionId = null)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking not found.");

            if (booking.Status != "Pending")
                throw new InvalidOperationException("Only pending bookings can be confirmed.");

            // Double-check availability
            var isAvailable = await _roomService.IsRoomAvailableAsync(
                booking.RoomId, 
                booking.CheckInDate, 
                booking.CheckOutDate);

            if (!isAvailable)
                throw new InvalidOperationException("Room is no longer available for the specified dates.");

            // Update booking status
            booking.Status = "Confirmed";
            booking.StripePaymentIntentId = stripePaymentIntentId;
            booking.StripeSessionId = stripeSessionId;
            booking.ConfirmedAt = DateTime.UtcNow;

            _unitOfWork.Bookings.Update(booking);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null)
            throw new ArgumentException("Booking not found.");

        if (booking.Status == "Cancelled")
            throw new InvalidOperationException("Booking is already cancelled.");

        booking.Status = "Cancelled";
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();
    }
}

