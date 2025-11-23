using Bookify.Data.Repositories;

namespace Bookify.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<Models.RoomType> RoomTypes { get; }
    IRoomRepository Rooms { get; }
    IBookingRepository Bookings { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

