using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Middleware
{

    ///<inheritdoc/>
    public partial class ConfigurationCustom : IConfigurationCustom
    {

        public string ApplicationVersion
        {
            get
            {
                return _requestConfiguration.ApplicationVersion;
            }
        }
        public string ApplicationBuild
        {
            get
            {
                return _requestConfiguration.BuildNumber;
            }
        }


        public string ApplicationRelease
        {
            get
            {
                return _requestConfiguration.ApplicationRelease;
            }
        }

        public string EnvironmentName
        {
            get
            {
                return _hostEnvironment.EnvironmentName;
            }

            set
            {
                _hostEnvironment.EnvironmentName = value;
            }
        }
        public string ApplicationName
        {
            get
            {
                return _hostEnvironment.ApplicationName;
            }
            set
            {
                _hostEnvironment.ApplicationName = value;
            }
        }
        public string ContentRootPath
        {
            get
            {
                return _hostEnvironment.ContentRootPath;
            }
            set
            {
                _hostEnvironment.ContentRootPath = value;
            }
        }

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly RequestConfiguration _requestConfiguration;



        public ConfigurationCustom(IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            IOptions<RequestConfiguration> options)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _requestConfiguration = options?.Value;
        }


        public string GetCorrelationId()
        {
            return _requestConfiguration?.CorrelationId;
        }

        public string GetSectionValue(string key)
        {
            var dicSection = GetSection(key);

            var stringValue = dicSection
                .Values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (stringValue?.Count() > 0)
                return stringValue[0];
            else
                return string.Empty;

        }

        ///<inheritdoc/>
        public TConfiguration ConfigurationOptions<TConfiguration>(string getSection) where TConfiguration : class
        {

            var typeInfo = typeof(TConfiguration).GetTypeInfo();
            var configurationInstance = Activator.CreateInstance(typeInfo, false);
            var classConfiguration = (TConfiguration)configurationInstance;


            var options = new ConfigureFromConfigurationOptions<TConfiguration>(
                 _configuration.GetSection(getSection));

            options.Configure(classConfiguration);


            return classConfiguration;
        }

        ///<inheritdoc/>
        public IDictionary<string, string> GetSection(string key)
        {
            var section = _configuration.GetSection(key);

            if (section is null)
            {
                return new Dictionary<string, string>();
            }

            if (section.Key is null || section.Value is null)
            {
                return new Dictionary<string, string>();
            }

            var sections = new Dictionary<string, string>()
            {
                { section?.Key.ToString(), section?.Value.ToString() }

            };

            return sections;

        }
        ///<inheritdoc/>
        public IDictionary<string, string> GetChildren(string path)
        {

            var section = _configuration.GetSection(path)
                .GetChildren()?
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))?
                .ToDictionary(x => x.Key, x => x.Value);

            return section;
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        public bool IsDevelopment()
        {
            return _hostEnvironment.IsDevelopment();

        }

        public bool IsEnvironment(string name)
        {
            return _hostEnvironment.IsEnvironment(name);

        }

        public bool IsProduction()
        {
            return _hostEnvironment.IsProduction();

        }

        public bool IsStaging()
        {
            return _hostEnvironment.IsStaging();
        }

    }

}
