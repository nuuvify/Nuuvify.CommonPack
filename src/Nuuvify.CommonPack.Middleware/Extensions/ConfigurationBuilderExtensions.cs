
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>Fornece extensões de configuração para startup e secrets montados.</summary>

    /// <summary>Adiciona variáveis de ambiente em memória durante o startup.</summary>
    /// <param name="builder">Builder de configuração a ser alterado.</param>
    /// <param name="prefix">Prefixo ordinal usado para filtrar as variáveis.</param>
    /// <param name="removePrefix">Indica se o prefixo deve ser removido das chaves.</param>
    /// <param name="logger">Logger opcional. Nenhum valor ou nome de secret é registrado.</param>
    /// <returns>O mesmo builder recebido.</returns>
    public static IConfigurationBuilder AddEnvironmentVariablesToMemoryCollection(
        this IConfigurationBuilder builder,
        string prefix,
        bool removePrefix = false,
        ILogger logger = null)
    {
        return AddEnvironmentVariablesToMemory(builder, prefix, removePrefix, removeVariavel: false, logger, nameof(AddEnvironmentVariablesToMemoryCollection));
    }

    /// <summary>Adiciona variáveis de ambiente em memória, preservando a assinatura legada.</summary>
    /// <param name="builder">Builder de configuração a ser alterado.</param>
    /// <param name="prefix">Prefixo ordinal usado para filtrar as variáveis.</param>
    /// <param name="removeVariavel">Remove as variáveis somente do processo atual após capturá-las.</param>
    /// <param name="logger">Logger opcional. Nenhum valor ou nome de secret é registrado.</param>
    /// <returns>O mesmo builder recebido.</returns>
    public static IConfigurationBuilder AddEnvironmentVariablesToKeyPerFile(
        this IConfigurationBuilder builder,
        string prefix,
        bool removeVariavel = true,
        ILogger logger = null)
    {
        return AddEnvironmentVariablesToMemory(builder, prefix, removePrefix: false, removeVariavel, logger, nameof(AddEnvironmentVariablesToKeyPerFile));
    }

    /// <summary>Adiciona secrets montados por Docker, Podman ou Kubernetes usando o provider oficial KeyPerFile.</summary>
    /// <param name="builder">Builder de configuração a ser alterado.</param>
    /// <param name="directoryPath">Diretório montado contendo um secret por arquivo.</param>
    /// <param name="optional">Permite a ausência do diretório quando verdadeiro; o padrão é fail-closed.</param>
    /// <param name="reloadOnChange">Habilita recarga por alteração de arquivo; o padrão é falso.</param>
    /// <returns>O mesmo builder recebido.</returns>
    /// <remarks>O provider converte <c>__</c> nos nomes dos arquivos em <c>:</c>. Permissões, montagem e rotação pertencem ao runtime ou orquestrador.</remarks>
    public static IConfigurationBuilder AddContainerSecrets(
        this IConfigurationBuilder builder,
        string directoryPath,
        bool optional = false,
        bool reloadOnChange = false)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("O diretório de secrets é obrigatório.", nameof(directoryPath));

        return builder.AddKeyPerFile(directoryPath, optional, reloadOnChange);
    }

    private static IConfigurationBuilder AddEnvironmentVariablesToMemory(
        IConfigurationBuilder builder,
        string prefix,
        bool removePrefix,
        bool removeVariavel,
        ILogger logger,
        string operationName)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (string.IsNullOrWhiteSpace(prefix))
            return builder;

        var environmentVariables = new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry.Key is string key && key.StartsWith(prefix, StringComparison.Ordinal))
            {
                environmentVariables[key] = entry.Value?.ToString();
            }
        }

        if (environmentVariables.Count == 0)
            return builder;

        logger?.LogDebug("Variáveis de ambiente para {operationName} com prefixo informado: {count}", operationName, environmentVariables.Count);

        var configurationValues = new Dictionary<string, string>();
        foreach (var variable in environmentVariables)
        {
            var key = removePrefix ? variable.Key.Substring(prefix.Length) : variable.Key;
            configurationValues[key.Replace("__", ":", StringComparison.Ordinal)] = variable.Value;
        }

        _ = builder.AddInMemoryCollection(configurationValues);

        if (removeVariavel)
        {
            foreach (var variable in environmentVariables)
                Environment.SetEnvironmentVariable(variable.Key, null);
        }

        return builder;
    }
}

