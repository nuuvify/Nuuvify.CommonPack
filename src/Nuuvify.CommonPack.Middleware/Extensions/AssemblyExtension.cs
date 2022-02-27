using System.Reflection;
using System.Text;

namespace Nuuvify.CommonPack.Middleware.Extensions
{
    internal static class AssemblyExtension
    {


        public static string GetApplicationNameByAssembly
        {

            get
            {
                var entryAssembly = Assembly.GetEntryAssembly().GetName().Name;

                var appCustomName = entryAssembly?
                    .Replace("Nuuvify.", "")
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
                var applicationVersion = new StringBuilder()
                    .Append($"{Assembly.GetEntryAssembly().GetName().Version.Major}.")
                    .Append($"{Assembly.GetEntryAssembly().GetName().Version.Minor}.")
                    .Append($"{Assembly.GetEntryAssembly().GetName().Version.Build}");


                return applicationVersion.ToString();
            }
        }

    }
}
