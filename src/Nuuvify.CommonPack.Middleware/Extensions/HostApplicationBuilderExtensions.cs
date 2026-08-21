
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{

    [Obsolete("Use GetContainerSecretsPath para resolver o caminho e AddContainerSecrets para registrar o provider. Consulte README.md#configuração.", error: false)]
    public static string PathSecrets { get; private set; }

    /// <summary>
    /// Resolve o caminho padrão de secrets montados para o sistema operacional atual.
    /// </summary>
    /// <param name="builder">Builder da aplicação.</param>
    /// <param name="customPathWindows">Caminho opcional para Windows.</param>
    /// <param name="customPathLinux">Caminho opcional para Linux e outros sistemas.</param>
    /// <returns>Caminho do diretório de secrets.</returns>
    public static string GetContainerSecretsPath(
        this IHostApplicationBuilder builder,
        string customPathWindows = null,
        string customPathLinux = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? customPathWindows
            : customPathLinux;
        if (!string.IsNullOrWhiteSpace(path))
        {
            if (Path.GetInvalidPathChars().Any(path.Contains))
                throw new ArgumentException("O caminho informado contém caracteres inválidos.", nameof(path));

            return path;
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Docker", "secrets")
            : "/run/secrets/";
    }

    /// <summary>
    /// Define qual o caminho de secrets para o sistema operacional atual, o defult para cada ambiente é:
    /// <para>Linux: "/run/secrets/"</para>
    /// <para>Windows: "C:\Users\seu_usuario\AppData\Local\Docker\secrets"</para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="customPathWindows"></param>
    /// <param name="customPathLinux"></param>
    /// <param name="logger"></param>
    [Obsolete("Use GetContainerSecretsPath para resolver o caminho e AddContainerSecrets para registrar o provider. Consulte README.md#configuração.", error: false)]
    public static string SetPathSecretsToOSPlatform(this IHostApplicationBuilder builder,
        string customPathWindows = null,
        string customPathLinux = null,
        ILogger logger = null)
    {
        PathSecrets = builder.GetContainerSecretsPath(customPathWindows, customPathLinux);
        logger?.LogDebug("Caminho de secrets resolvido para a plataforma atual: {PathSecrets}", PathSecrets);

        return PathSecrets;

    }

    /// <summary>
    /// Obtem a pasta contendo Docker secrets para o sistema operacional atual.
    /// <para>Caso a propriedade <see cref="PathSecrets"/> esteja null, o metodo <see cref="SetPathSecretsToOSPlatform"/> sera chamado</para> 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    [Obsolete("Use GetContainerSecretsPath para resolver o caminho e AddContainerSecrets para registrar o provider. Consulte README.md#configuração.", error: false)]
    public static string GetPathSecretsToOSPlatform(this IHostApplicationBuilder builder, ILogger logger = null)
    {
        if (string.IsNullOrWhiteSpace(PathSecrets))
            PathSecrets = builder.GetContainerSecretsPath();

        logger?.LogDebug("Caminho de secrets atual: {PathSecrets}", PathSecrets);

        return PathSecrets;
    }

}

