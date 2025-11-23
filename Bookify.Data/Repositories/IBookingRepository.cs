using Bookify.Data.Models;

namespace Bookify.Data.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
    Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync();
    Task<Booking?> GetBookingWithDetailsAsync(int id);
}

