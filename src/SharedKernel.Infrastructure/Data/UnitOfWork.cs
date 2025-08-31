using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Abstractions;

namespace SharedKernel.Infrastructure.Data;

/// <summary>
/// Generic implementation of Unit of Work pattern using Entity Framework Core.
/// Maintains a list of objects affected by a business transaction and coordinates 
/// writing out changes and resolving concurrency problems.
/// </summary>
/// <typeparam name="TContext">The type of DbContext to use</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;

    /// <summary>
    /// Initializes a new instance of UnitOfWork with the specified DbContext
    /// </summary>
    /// <param name="context">Entity Framework database context</param>
    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Saves all pending changes to the underlying data store
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The number of state entries written to the underlying database</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}