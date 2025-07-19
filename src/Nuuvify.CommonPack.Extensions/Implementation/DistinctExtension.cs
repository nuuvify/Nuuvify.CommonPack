using System;
using System.Collections.Generic;
using System.Linq;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class DistinctExtension
    {

        /// <summary>
        /// Substitua o "and" por "duplo e comercial"
        /// <example>
        /// <code>
        /// IEnumerable{Produto} produtosSemRepeticao =
        ///     produtos.Distinct((p1, p2) => 
        ///         p1.Nome == p2.Nome and
        ///         p1.CodigoNbm = p2.CodigoNbm,
        ///         x => x.GetHashCode()
        ///     );
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparerEquals"></param>
        /// <param name="comparerGetHashCode"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> comparerEquals,
            Func<TSource, int> comparerGetHashCode)
        {

            var resultDistinct = source.Distinct(
               new CustomGenericComparer<TSource>(
                   comparerEquals,
                   comparerGetHashCode)
            );

            return resultDistinct;

        }
       

    }

}