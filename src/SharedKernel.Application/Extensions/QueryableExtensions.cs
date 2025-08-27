using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Common;

namespace SharedKernel.Application.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paginated result with async execution.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A paginated result.</returns>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var count = await source.CountAsync(cancellationToken);

        if (count == 0)
            return PaginatedResult<T>.Empty(pageNumber, pageSize);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts an IQueryable to a paginated result with synchronous execution.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result.</returns>
    public static PaginatedResult<T> ToPaginatedResult<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var count = source.Count();

        if (count == 0)
            return PaginatedResult<T>.Empty(pageNumber, pageSize);

        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<T>(items, count, pageNumber, pageSize);
    }
}