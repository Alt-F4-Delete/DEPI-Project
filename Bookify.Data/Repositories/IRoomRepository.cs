using Bookify.Data.Models;

namespace Bookify.Data.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null);
    Task<Room?> GetRoomWithTypeAsync(int id);
    Task<IEnumerable<Room>> GetRoomsWithTypeAsync();
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
}

