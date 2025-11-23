using Bookify.Data.Models;

namespace Bookify.Services.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(string userId, int roomId, DateTime checkIn, DateTime checkOut, string? specialRequests = null);
    Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task ConfirmBookingAsync(int bookingId, string stripePaymentIntentId, string? stripeSessionId = null);
    Task CancelBookingAsync(int bookingId);
}

