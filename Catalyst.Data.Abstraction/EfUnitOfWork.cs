using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Catalyst.Data.Abstraction;

public class EfUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _db;
    private IDbContextTransaction? _trx;
    public EfUnitOfWork(TContext db) => _db = db;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_trx is null)
            _trx = await _db.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
            if (_trx != null) await _trx.CommitAsync(ct);
        }
        catch
        {
            if (_trx != null) await _trx.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (_trx != null)
            {
                await _trx.DisposeAsync();
                _trx = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_trx != null)
        {
            await _trx.RollbackAsync(ct);
            await _trx.DisposeAsync();
            _trx = null;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public void Dispose()
    {
        _trx?.Dispose();
        _db.Dispose();
    }
}