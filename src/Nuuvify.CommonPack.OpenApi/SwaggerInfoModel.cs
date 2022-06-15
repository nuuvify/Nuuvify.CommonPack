using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.OpenApi.Models;

namespace Nuuvify.CommonPack.OpenApi
{
    internal class SwaggerInfoModel
    {


        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public string BuildVersion { get; private set; }
        public string VersionName { get; set; }
        public string DeveloperName { get; private set; }
        public string DeveloperEmail { get; private set; }
        public string LicenseType { get; private set; }
        public Uri UrlLicenseValid { get; private set; }
        public Uri UrlTermsValid { get; private set; }
        public Uri UrlAppValid { get; private set; }


        public string PlatformNameHost()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"{OSPlatform.Windows}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"{OSPlatform.Linux}";
            }
            else
            {
                return RuntimeInformation.OSDescription;
            }

        }

        public SwaggerInfoModel(string developerName, string developerEmail, string licenseType,
            string urlRepository, string urlTermService, string urlLicense)
        {
            DeveloperName = developerName;
            DeveloperEmail = developerEmail;
            LicenseType = licenseType;


            AppName = Assembly.GetEntryAssembly()
                .GetName()
                .Name?
                .Replace(".WebApi", "")
                .Replace(".", "");

            BuildVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            AppVersion = $"{Assembly.GetEntryAssembly().GetName().Version.Major}." +
                         $"{Assembly.GetEntryAssembly().GetName().Version.Minor}." +
                         $"{Assembly.GetEntryAssembly().GetName().Version.Build}";


            Uri.TryCreate(urlRepository, UriKind.RelativeOrAbsolute, out Uri _urlAppValid);
            Uri.TryCreate(urlTermService, UriKind.RelativeOrAbsolute, out Uri _urlTermsValid);
            Uri.TryCreate(urlLicense, UriKind.RelativeOrAbsolute, out Uri _urlLicenseValid);

            UrlAppValid = _urlAppValid;
            UrlTermsValid = _urlTermsValid;
            UrlLicenseValid = _urlLicenseValid;
        }


        public OpenApiInfo CreateInfoForApiVersion()
        {


            var info = new OpenApiInfo
            {
                Title = AppName,
                Version = VersionName,
                Description = $@"Api documentation
## OS ##
{PlatformNameHost()}
## Application version ##
{AppVersion}
## Build number ##
{BuildVersion}",

                Contact = new OpenApiContact
                {
                    Name = DeveloperName,
                    Email = DeveloperEmail,
                    Url = UrlAppValid,
                },
                TermsOfService = UrlTermsValid,
                License = new OpenApiLicense
                {
                    Name = $"License Type: {LicenseType}",
                    Url = UrlLicenseValid
                },
            };

            return info;
        }
    }
}