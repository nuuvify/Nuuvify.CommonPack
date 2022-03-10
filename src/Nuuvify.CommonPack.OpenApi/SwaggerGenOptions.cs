using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nuuvify.CommonPack.OpenApi
{
    public class SwaggerGenOptionsConfigure : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly IConfiguration _config;
        private readonly SwaggerInfoModel _swaggerInfoModel;

        public SwaggerGenOptionsConfigure(IApiVersionDescriptionProvider provider,
            IConfiguration config)
        {
            _provider = provider;
            _config = config;


            _swaggerInfoModel = new SwaggerInfoModel(
                developerName: _config.GetSection("SwaggerInfo:DesenvolvedorNome").Value,
                developerEmail: _config.GetSection("SwaggerInfo:DesenvolvedorEmail").Value,
                licenseType: _config.GetSection("SwaggerInfo:LicencaTipo").Value,
                urlRepository: _config.GetSection("SwaggerInfo:UrlRepositoryVsts").Value,
                urlTermService: _config.GetSection("SwaggerInfo:TermoDeServico").Value,
                urlLicense: _config.GetSection("SwaggerInfo:LicencaUrl").Value
            );


        }


        public void Configure(SwaggerGenOptions options)
        {

            foreach (var description in _provider.ApiVersionDescriptions)
            {
                _swaggerInfoModel.VersionName = description.GroupName;


                options.SwaggerDoc(name: _swaggerInfoModel.VersionName,
                    info: _swaggerInfoModel.CreateInfoForApiVersion());
            }


        }


    }
}
