using Microsoft.EntityFrameworkCore;
using Bookify.Data.Models;

namespace Bookify.Data.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
    {
        return await _dbSet
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync()
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}

