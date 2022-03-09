using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;

namespace Nuuvify.CommonPack.OpenApi
{
    public static class SwaggerGenXmlComments
    {

        public static void Configuration(this IServiceCollection services)
        {

            services.AddSwaggerGen(options =>
            {

                options.ExampleFilters();
                options.EnableAnnotations();


                options.OperationFilter<AddHeaderOperationFilter>("CorrelationId", "Correlation Id for the request", false);

                var documentFile = string.Empty;
                var baseDirectory = AppContext.BaseDirectory;

                var filesXml = Directory.GetFiles(baseDirectory, ".*.xml", SearchOption.TopDirectoryOnly);

                foreach (var item in filesXml)
                {
                    documentFile = XmlCommentsFilePath(baseDirectory, item);
                    if (documentFile != null)
                    {
                        options.IncludeXmlComments(documentFile);
                    }
                }


            });

        }


        public static string XmlCommentsFilePath(string baseDirectory, string fileXml)
        {

            var documentFile = Path.Combine(baseDirectory, fileXml);

            if (!File.Exists(documentFile))
                documentFile = Path.Combine(baseDirectory, "docs", fileXml);

            if (!File.Exists(documentFile))
                return null;

            return documentFile;
        }
    }
}
