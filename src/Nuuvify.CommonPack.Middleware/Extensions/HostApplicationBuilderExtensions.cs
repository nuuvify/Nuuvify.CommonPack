
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{

    public static string PathSecrets { get; private set; }

    /// <summary>
    /// Define qual o caminho de secrets para o sistema operacional atual, o defult para cada ambiente é:
    /// <para>Linux: "/run/secrets/"</para>
    /// <para>Windows: "C:\Users\seu_usuario\AppData\Local\Docker\secrets"</para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="customPathWindows"></param>
    /// <param name="customPathLinux"></param>
    /// <param name="logger"></param>
    public static string SetPathSecretsToOSPlatform(this IHostApplicationBuilder builder,
        string customPathWindows = null,
        string customPathLinux = null,
        ILogger logger = null)
    {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {

            logger?.LogDebug("Plataforma: Windows");

            if (string.IsNullOrWhiteSpace(customPathWindows))
            {
                PathSecrets = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Docker", "secrets");
            }
            else
            {

                if (Path.GetInvalidPathChars().Any(x => customPathWindows.Contains(x)))
                    throw new ArgumentException("O caminho informado contem caracteres invalidos", nameof(customPathWindows));

                PathSecrets = customPathWindows;
            }

        }
        else
        {

            logger?.LogDebug("Plataforma: Linux");

            if (string.IsNullOrWhiteSpace(customPathLinux))
            {
                PathSecrets = "/run/secrets/";
            }
            else
            {
                if (Path.GetInvalidPathChars().Any(x => customPathLinux.Contains(x)))
                    throw new ArgumentException("O caminho informado contem caracteres invalidos", nameof(customPathLinux));

                PathSecrets = customPathLinux;

            }

        }

        if (logger != null)
        {
            int nroFiles = 0;
            if (Directory.Exists(PathSecrets))
                nroFiles = Directory.GetFiles(PathSecrets).Length;

            logger.LogDebug($"Path configs: {PathSecrets} = {nroFiles} file(s)");
        }

        return PathSecrets;

    }

    /// <summary>
    /// Obtem a pasta contendo Docker secrets para o sistema operacional atual.
    /// <para>Caso a propriedade <see cref="PathSecrets"/> esteja null, o metodo <see cref="SetPathSecretsToOSPlatform"/> sera chamado</para> 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static string GetPathSecretsToOSPlatform(this IHostApplicationBuilder builder, ILogger logger = null)
    {
        if (string.IsNullOrWhiteSpace(PathSecrets))
            _ = builder.SetPathSecretsToOSPlatform(logger: logger);

        return PathSecrets;
    }

}

