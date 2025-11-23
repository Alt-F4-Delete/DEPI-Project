using Bookify.Data.Models;
using Bookify.Data.UnitOfWork;
using Bookify.Services.Interfaces;

namespace Bookify.Services.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null)
    {
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be before check-out date.");

        if (checkIn < DateTime.Today)
            throw new ArgumentException("Check-in date cannot be in the past.");

        return await _unitOfWork.Rooms.GetAvailableRoomsAsync(checkIn, checkOut, roomTypeId);
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _unitOfWork.Rooms.GetRoomWithTypeAsync(id);
    }

    public async Task<IEnumerable<RoomType>> GetAllRoomTypesAsync()
    {
        return await _unitOfWork.RoomTypes.GetAllAsync();
    }

    public async Task<RoomType?> GetRoomTypeByIdAsync(int id)
    {
        return await _unitOfWork.RoomTypes.GetByIdAsync(id);
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            return false;

        return await _unitOfWork.Rooms.IsRoomAvailableAsync(roomId, checkIn, checkOut);
    }

    public decimal CalculateTotalPrice(decimal pricePerNight, DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be before check-out date.");

        var nights = (checkOut - checkIn).Days;
        return pricePerNight * nights;
    }
}

