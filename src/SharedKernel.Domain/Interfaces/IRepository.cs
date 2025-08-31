using System.Linq.Expressions;

namespace SharedKernel.Domain.Interfaces;

/// <summary>
/// Defines a generic repository interface for basic CRUD operations and querying.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages</typeparam>
public interface IRepository<TEntity> 
    where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A collection of all entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A collection of entities that match the predicate</returns>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The first entity that matches the predicate, or null if no match is found</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository.
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to update</param>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes multiple entities from the repository.
    /// </summary>
    /// <param name="entities">The entities to remove</param>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Checks if any entity exists that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>True if any entity matches the predicate, otherwise false</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The number of entities that match the predicate</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}