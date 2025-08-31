using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Interfaces;

namespace SharedKernel.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing generic CRUD operations using Entity Framework Core.
/// Implements IRepository interface with standard database operations for any entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages</typeparam>
public class BaseRepository<TEntity> : IRepository<TEntity> 
    where TEntity : class
{
    /// <summary>
    /// Entity Framework database context for database operations
    /// </summary>
    protected readonly DbContext Context;
    
    /// <summary>
    /// Entity Framework DbSet for the specific entity type
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Initializes a new instance of the BaseRepository with the specified database context
    /// </summary>
    /// <param name="context">Entity Framework database context</param>
    public BaseRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The entity if found, otherwise null</returns>
    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets all entities from the repository
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A collection of all entities</returns>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A collection of entities that match the predicate</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the first entity that matches the specified predicate
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The first entity that matches the predicate, or null if no match is found</returns>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Adds multiple entities to the repository
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    /// <summary>
    /// Updates multiple entities in the repository
    /// </summary>
    /// <param name="entities">The entities to update</param>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    /// <summary>
    /// Removes multiple entities from the repository
    /// </summary>
    /// <param name="entities">The entities to remove</param>
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Checks if any entity exists that matches the specified predicate
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>True if any entity matches the predicate, otherwise false</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts the number of entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">An expression to filter entities</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The number of entities that match the predicate</returns>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(predicate, cancellationToken);
    }
}