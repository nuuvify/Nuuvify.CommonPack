using System;
using System.Linq;

namespace Nuuvify.CommonPack.HealthCheck.Helpers
{
    public static class UriExtension
    {


        /// <summary>
        /// Verifica se a Uri possui um segmento com "segmentSearch" caso contrario, sera incluido na Uri <br/>
        /// Se for informado "relativeUrlComplement" isso também sera incluido na Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="segmentSearch">Segmento a ser procurado, exemplo: "api/"</param>
        /// <param name="relativeUrlComplement">Complemento que devera ser incluido na mesma Uri, exemplo: hc-ui-api</param>
        /// <returns></returns>
        public static Uri HasSegment(this Uri uri, string segmentSearch, string relativeUrlComplement = null)
        {
            if (!string.IsNullOrWhiteSpace(relativeUrlComplement) &&
                !relativeUrlComplement.EndsWith("/"))
            {
                relativeUrlComplement += "/";
            }
            if (!string.IsNullOrWhiteSpace(segmentSearch) &&
                !segmentSearch.EndsWith("/"))
            {
                segmentSearch += "/";
            }

            var segments = uri.Segments;
            if (segments.Any(x => x.Equals(segmentSearch, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(relativeUrlComplement))
                {
                    return uri;
                }

                return new Uri(uri, $"{relativeUrlComplement}");
            }

            if (string.IsNullOrWhiteSpace(relativeUrlComplement))
            {
                return new Uri(uri, $"{segmentSearch}");
            }

            return new Uri(uri, $"{segmentSearch}{relativeUrlComplement}");

        }

    }
}