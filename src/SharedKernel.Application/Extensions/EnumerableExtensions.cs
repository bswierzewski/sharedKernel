using SharedKernel.Application.Common;

namespace SharedKernel.Application.Extensions;

/// <summary>
/// Extension methods for IEnumerable to support pagination.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an IEnumerable to a paginated result.
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result.</returns>
    public static PaginatedResult<T> ToPaginatedResult<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var items = source.ToList();
        var count = items.Count;

        if (count == 0)
            return PaginatedResult<T>.Empty(pageNumber, pageSize);

        var pageItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<T>(pageItems, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts an IEnumerable to a paginated result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated result.</returns>
    public static Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(source.ToPaginatedResult(pageNumber, pageSize));
    }
}