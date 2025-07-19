
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

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

