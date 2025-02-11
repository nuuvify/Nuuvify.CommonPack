using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.OpenApi
{
    public static class UseSwaggerSetup
    {
        /// <summary>
        /// Caso esteja recebendo erro, tente executar o endpoint da sua aplicação com o complemento: /swagger/v1/swagger.json
        /// <para>
        /// Exemplo: http://localhost:5000/swagger/v1/swagger.json
        /// </para>
        /// <para>
        /// Pode haver algum endpoint conflitante, com mesmo verbo e mesma route, nesse endpoint será possivel ver o erro
        /// </para>
        /// </summary>
        /// <param name="app"></param>
        public static void UseSwaggerConfiguration(this WebApplication app)
        {

            app.UseSwagger();
            app.MapWhen(
                context => context.Request.Path.StartsWithSegments("/swagger"),
                appBuilder => appBuilder.Use(next =>
                {
                    var provider = appBuilder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                    return app.UseSwaggerUI(c =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                            c.RoutePrefix = "docs";
                        }
                    }).Build();
                })
            );

        }
    }
}
