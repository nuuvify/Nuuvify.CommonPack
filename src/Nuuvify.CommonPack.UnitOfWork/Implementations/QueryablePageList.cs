
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// This class is obsolete. Use the extension methods ToPagedList and ToPagedListAsync from IQueryableExtensions instead.
/// </summary>
[Obsolete("Use the extension methods ToPagedList and ToPagedListAsync from IQueryableExtensions (Nuuvify.CommonPack.UnitOfWork namespace) instead. This class will be removed in a future version.", false)]
public class QueryablePageList : IIQueryablePageList
{
    ///<inheritdoc/>
    public IPagedList<T> ToPagedList<T>(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        int indexFrom = 0)
    {

        return source.ToPagedList<T>(pageIndex, pageSize, indexFrom);
    }

    ///<inheritdoc/>
    public async Task<IPagedList<T>> ToPagedListAsync<T>(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        int indexFrom = 0,
        CancellationToken cancellationToken = default)
    {

        return await source.ToPagedListAsync<T>(pageIndex, pageSize, indexFrom, cancellationToken);

    }

}

