using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.OpenApi
{
    public static class UseSwaggerSetup
    {
        /// <summary>
        /// Se precisar indicar manualmente um caminho para swaggger inclua a seguinte entrada no appsettings <br/>
        /// "VirtualPath": "api"
        /// Caso esteja recebendo erro, tente executar o endpoint da sua aplicação com o complemento: /swagger/v1/swagger.json
        /// <para>
        /// Exemplo: http://localhost:5000/swagger/v1/swagger.json
        /// </para>
        /// <para>
        /// Pode haver algum endpoint conflitante, com mesmo verbo e mesma route, nesse endpoint será possivel ver o erro
        /// </para>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        /// <param name="provider"></param>
        public static void UseSwaggerConfiguration(this IApplicationBuilder app,
            IConfiguration configuration,
            IApiVersionDescriptionProvider provider)
        {

            var vpath = configuration.GetSection("VirtualPath")?.Value?.Trim() ?? string.Empty;
            vpath = string.IsNullOrWhiteSpace(vpath) ? "/swagger/" : $"/{vpath}/swagger/";

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"{vpath}{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    options.RoutePrefix = "docs";
                }
            });
        }
    }
}
