using Microsoft.EntityFrameworkCore.Storage;
using Bookify.Data.Repositories;

namespace Bookify.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Models.RoomType>? _roomTypes;
    private IRoomRepository? _rooms;
    private IBookingRepository? _bookings;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Models.RoomType> RoomTypes
    {
        get
        {
            _roomTypes ??= new Repository<Models.RoomType>(_context);
            return _roomTypes;
        }
    }

    public IRoomRepository Rooms
    {
        get
        {
            _rooms ??= new RoomRepository(_context);
            return _rooms;
        }
    }

    public IBookingRepository Bookings
    {
        get
        {
            _bookings ??= new BookingRepository(_context);
            return _bookings;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

