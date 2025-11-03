namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

/// <summary>
/// Provides the interface(s) for paged list of any type.
/// </summary>
/// <typeparam name="T">The type for paging.</typeparam>
public interface IPagedList<T> : IQueryableCustom
{
    /// <summary>
    /// Gets the index start value.
    /// </summary>
    /// <value>The index start value.</value>
    int IndexFrom { get; }
    /// <summary>
    /// Gets the page index (current).
    /// </summary>
    int PageIndex { get; }
    /// <summary>
    /// Gets the page size.
    /// </summary>
    int PageSize { get; }
    /// <summary>
    /// Gets the total count of the list of type <typeparamref name="T"/>
    /// </summary>
    int TotalCount { get; }
    /// <summary>
    /// Gets the total pages.
    /// </summary>
    int TotalPages { get; }
    /// <summary>
    /// Gets the current page items.
    /// </summary>
    IList<T> Items { get; }
    /// <summary>
    /// Gets the has previous page.
    /// </summary>
    /// <value>The has previous page.</value>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Gets the has next page.
    /// </summary>
    /// <value>The has next page.</value>
    bool HasNextPage { get; }

    /// <summary>
    /// Gets the number of items to skip before taking the page.
    /// Calculated as (PageIndex - IndexFrom) * PageSize.
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets the number of items to take for the current page.
    /// </summary>
    int Take { get; }
}
