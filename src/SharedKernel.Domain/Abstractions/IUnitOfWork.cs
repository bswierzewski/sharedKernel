namespace SharedKernel.Domain.Abstractions;

/// <summary>
/// Defines a unit of work pattern that maintains a list of objects affected by a business transaction 
/// and coordinates writing out changes and resolving concurrency problems.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The number of state entries written to the underlying database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}