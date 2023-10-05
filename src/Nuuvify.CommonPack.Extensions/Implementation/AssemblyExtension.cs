using System.Reflection;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class AssemblyExtension
    {


        public static string GetApplicationNameByAssembly
        {

            get
            {
                var entryAssembly = Assembly.GetEntryAssembly().GetName().Name;

                var appCustomName = entryAssembly?.Replace("Nuuvify.", "")
                                                  .Replace(".WebApi", "");


                return appCustomName;

            }
        }

        public static string GetApplicationBuildNumber
        {
            get
            {
                var buildNumber = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

                return buildNumber;
            }
        }
        public static string GetApplicationVersion
        {

            get
            {
                var applicationVersion = $"{Assembly.GetEntryAssembly().GetName().Version.Major}." +
                                     $"{Assembly.GetEntryAssembly().GetName().Version.Minor}." +
                                     $"{Assembly.GetEntryAssembly().GetName().Version.Build}";


                return applicationVersion;
            }
        }

    }
}
