
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{


    /// <summary>
    /// Adiciona as variaveis de ambiente com o prefixo informado, como collection imMemory, para ser usado com
    /// GetSection() ou GetValue() do IConfiguration
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="prefix"></param>
    /// <param name="removePrefix"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddEnvironmentVariablesToMemoryCollection(
        this IConfigurationBuilder builder,
        string prefix,
        bool removePrefix = false,
        ILogger logger = null)
    {


        if (string.IsNullOrWhiteSpace(prefix))
            return builder;


        var environmentVariables = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(entry => entry.Key.ToString().StartsWith(prefix))
            .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.ToString());

        if (environmentVariables == null || environmentVariables.Count == 0)
            return builder;

        if (logger != null)
            logger.LogDebug("Variaveis de ambiente {className} com prefixo {prefix} encontradas: {environmentVariables}", nameof(AddEnvironmentVariablesToMemoryCollection), prefix, environmentVariables.Count);

        var environmentVariablesDictionary = new Dictionary<string, string>();

        string keyWithoutPrefix = string.Empty;
        foreach (var kvp in environmentVariables)
        {
            if (removePrefix)
                keyWithoutPrefix = kvp.Key.Substring(prefix.Length);
            else
                keyWithoutPrefix = kvp.Key.Replace("__", ":");

            environmentVariablesDictionary[keyWithoutPrefix] = kvp.Value;
        }


        builder.AddInMemoryCollection(environmentVariablesDictionary);
        return builder;

    }

    /// <summary>
    /// Obtem a lista de variaveis de ambiente com o prefixo informado, depois cria um arquivo temporario, para adicionar esse arquivo como KeyPerFile, entao remove as variaveis de ambiente
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="prefix">Prefixo da lista de variaveis</param>
    /// <param name="removeVariavel">Depois de incluido em KeyPerFile, remove as variaveis de ambiente</param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddEnvironmentVariablesToKeyPerFile(
        this IConfigurationBuilder builder,
        string prefix,
        bool removeVariavel = true,
        ILogger logger = null)
    {


        if (string.IsNullOrWhiteSpace(prefix))
            return builder;


        var environmentVariables = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(entry => entry.Key.ToString().StartsWith(prefix))
            .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.ToString());

        if (environmentVariables == null || environmentVariables.Count == 0)
            return builder;


        if (logger != null)
            logger.LogDebug("Variaveis de ambiente {className} com prefixo {prefix} encontradas: {environmentVariables}", nameof(AddEnvironmentVariablesToKeyPerFile), prefix, environmentVariables.Count);

        var tempPathAndFile = Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName(),
            "secrets");

        string pathSecretFile = string.Empty;

        if (Directory.Exists(tempPathAndFile))
            Directory.Delete(tempPathAndFile, true);

        Directory.CreateDirectory(tempPathAndFile);

        foreach (var kpf in environmentVariables)
        {
            pathSecretFile = Path.Combine(tempPathAndFile, kpf.Key);
            File.WriteAllText(pathSecretFile, kpf.Value);

            if (removeVariavel)
                Environment.SetEnvironmentVariable(kpf.Key, null);

        }

        builder.AddKeyPerFile(tempPathAndFile, optional: true);
        Directory.Delete(tempPathAndFile, true);

        return builder;

    }


}

