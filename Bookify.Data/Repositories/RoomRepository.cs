using Microsoft.EntityFrameworkCore;
using Bookify.Data.Models;

namespace Bookify.Data.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null)
    {
        var query = _dbSet
            .Include(r => r.RoomType)
            .Where(r => r.IsAvailable);

        if (roomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        // Get rooms that don't have conflicting bookings
        var conflictingBookings = await _context.Bookings
            .Where(b => b.Status != "Cancelled" &&
                       ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                        (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                        (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut)))
            .Select(b => b.RoomId)
            .ToListAsync();

        return await query
            .Where(r => !conflictingBookings.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<Room?> GetRoomWithTypeAsync(int id)
    {
        return await _dbSet
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Room>> GetRoomsWithTypeAsync()
    {
        return await _dbSet
            .Include(r => r.RoomType)
            .ToListAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var room = await _dbSet
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null || !room.IsAvailable)
            return false;

        var hasConflict = await _context.Bookings
            .AnyAsync(b => b.RoomId == roomId &&
                          b.Status != "Cancelled" &&
                          ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                           (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                           (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut)));

        return !hasConflict;
    }
}

