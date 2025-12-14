using System.Collections.Generic;

namespace AdaskoTheBeAsT.ValueSql.Runtime;

/// <summary>
/// Represents a paged result with items and total count.
/// Used for efficient pagination queries using NextResult pattern.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Creates a new paged result.
    /// </summary>
    public PagedResult(List<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    /// <summary>
    /// The items for the current page.
    /// </summary>
    public List<T> Items { get; }

    /// <summary>
    /// The total count of all items (before pagination).
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// The number of items in the current page.
    /// </summary>
    public int PageSize => Items.Count;

    /// <summary>
    /// Whether there are more items after this page.
    /// </summary>
    public bool HasMore(int skip) => skip + Items.Count < TotalCount;

    /// <summary>
    /// Calculates total pages for a given page size.
    /// </summary>
    public int TotalPages(int pageSize) => (TotalCount + pageSize - 1) / pageSize;
}
