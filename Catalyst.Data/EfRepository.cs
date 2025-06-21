using Catalyst.Data.Abstraction;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalyst.Data;

public abstract class EfRepository<T> : IRepository<T>
    where T : BaseEntity
{
    protected readonly DbContext Db;

    public EfRepository(DbContext db) => Db = db;

    public async Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = ApplySpecification(spec);

        if (query.Provider is not IAsyncQueryProvider)
        {
            throw new InvalidOperationException(
                "The query provider does not support async operations. " +
                "Ensure you are using LinqKit.Microsoft.EntityFrameworkCore.");
        }

        return await query.ToListAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // Apply the specification specifically for counting purposes
        // The SpecificationEvaluator should handle ignoring OrderBy/Paging when forCount is true
        var query = ApplySpecification(spec);
        if (query.Provider is not IAsyncQueryProvider)
        {
            throw new InvalidOperationException(
                "The query provider does not support async operations. " +
                "Ensure you are using LinqKit.Microsoft.EntityFrameworkCore.");
        }

        // The query returned by ApplySpecification should still be an EF Core IQueryable
        // if AsExpandable() and the evaluator worked correctly.
        return await query.CountAsync(ct); // This is line 48 where the error occurred
    }


    public Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        Db.Set<T>().Add(entity);
        return Task.FromResult(entity);
    }

    public void Update(T entity)
        => Db.Set<T>().Update(entity);

    public void Delete(T entity)
        => Db.Set<T>().Remove(entity);

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Using FindAsync is generally best for primary key lookup
        return await Db.Set<T>().FindAsync([id], ct);
    }

    // Helper to consistently apply specification (including AsExpandable)
    protected virtual IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        // 1. Start with EF Core’s DbSet<T>
        IQueryable<T> query = Db.Set<T>();


        // 2. Apply includes, ordering, paging, etc.
        query = SpecificationEvaluator.GetQuery(query, spec);

        // 3. Finally wrap with AsExpandable()
        return query.AsExpandable();
    }
}