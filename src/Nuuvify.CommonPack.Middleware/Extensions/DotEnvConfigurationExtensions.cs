using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.Middleware.Extensions;

/// <summary>
/// Carrega configuração de um arquivo dotenv sem alterar o ambiente do processo.
/// </summary>
public static class DotEnvConfigurationExtensions
{
    /// <summary>
    /// Adiciona um arquivo dotenv como fonte de baixa precedência da configuração.
    /// </summary>
    /// <param name="builder">Builder da aplicação.</param>
    /// <param name="path">Caminho do arquivo dotenv; por padrão usa `.env` na raiz do conteúdo.</param>
    /// <param name="optional">Indica se a ausência do arquivo deve ser aceita.</param>
    /// <param name="logger">Logger opcional para diagnóstico sem valores de configuração.</param>
    /// <returns>O mesmo builder para composição.</returns>
    public static IHostApplicationBuilder AddDotEnvConfiguration(
        this IHostApplicationBuilder builder,
        string path = null,
        bool optional = true,
        ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var envPath = string.IsNullOrWhiteSpace(path)
            ? Path.Combine(builder.Environment.ContentRootPath, ".env")
            : path;

        if (!File.Exists(envPath))
        {
            if (!optional)
                throw new FileNotFoundException("O arquivo dotenv obrigatório não foi encontrado.", envPath);

            return builder;
        }

        var values = Parse(File.ReadLines(envPath));
        builder.Configuration.Sources.Insert(0, new MemoryConfigurationSource
        {
            InitialData = values
        });
        logger?.LogDebug("Arquivo dotenv adicionado à configuração: {Path}", envPath);
        return builder;
    }

    private static Dictionary<string, string> Parse(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=', StringComparison.Ordinal);
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            if (key.Length == 0)
                continue;

            values[key.Replace("__", ":", StringComparison.Ordinal)] = line[(separator + 1)..].Trim();
        }

        return values;
    }
}
