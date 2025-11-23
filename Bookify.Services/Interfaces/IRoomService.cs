using Bookify.Data.Models;

namespace Bookify.Services.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null);
    Task<Room?> GetRoomByIdAsync(int id);
    Task<IEnumerable<RoomType>> GetAllRoomTypesAsync();
    Task<RoomType?> GetRoomTypeByIdAsync(int id);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
    decimal CalculateTotalPrice(decimal pricePerNight, DateTime checkIn, DateTime checkOut);
}

