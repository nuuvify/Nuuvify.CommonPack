using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Collections;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork
{

    internal static class IQueryableExtensions
    {

        public static IPagedList<T> ToPagedList<T>(this IQueryable<T> source,
            int pageIndex,
            int pageSize,
            int indexFrom)
            => new PagedList<T>(source, pageIndex, pageSize, indexFrom);

        /// <summary>
        /// Converts the specified source to <see cref="IPagedList{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageIndex">The index of the page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="indexFrom">The start index value. indexFrom must always be less than pageIndex</param>
        /// <param name="cancellationToken">to observe while waiting for the task to complete.</param>
        /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source,
            int pageIndex,
            int pageSize,
            int indexFrom = 0,
            CancellationToken cancellationToken = default)
        {

            ValidateArguments(indexFrom, pageIndex);


            var count = await source.CountAsync(cancellationToken)
                                    .ConfigureAwait(false);

            var items = await source.Skip((pageIndex - indexFrom) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken)
                                    .ConfigureAwait(false);

            var pagedList = new PagedList<T>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                IndexFrom = indexFrom,
                TotalCount = count,
                Items = items,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            return pagedList;
        }

        private static void ValidateArguments(int indexFrom, int pageIndex)
        {

            if (indexFrom > pageIndex)
            {
                throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
            }

        }

    }

}

