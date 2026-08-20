using Microsoft.Extensions.Configuration;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class ConfigurationBuilderExtensionsTests
{
    [Fact]
    public void PrefixoNuloOuVazioRetornaMesmoBuilderSemAdicionarFonte()
    {
        var builder = new ConfigurationBuilder();

        Assert.Same(builder, builder.AddEnvironmentVariablesToMemoryCollection(null));
        Assert.Same(builder, builder.AddEnvironmentVariablesToMemoryCollection(string.Empty));
        Assert.Empty(builder.Sources);
    }

    [Fact]
    public void PrefixoUsaComparacaoOrdinalEConverteDelimitador()
    {
        var prefix = $"Nuuvify_{Guid.NewGuid():N}_";
        var differentCaseName = $"{prefix.ToUpperInvariant()}Database__Password";

        try
        {
            Environment.SetEnvironmentVariable(differentCaseName, "ignored");
            var ordinalConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariablesToMemoryCollection(prefix)
                .Build();

            Assert.Null(ordinalConfiguration["Database:Password"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(differentCaseName, null);
        }
    }

    [Fact]
    public void RemovePrefixTrueRemovePrefixoEConverteDelimitador()
    {
        var prefix = $"NUUVIFY_{Guid.NewGuid():N}_";
        var variableName = $"{prefix}Database__Password";

        try
        {
            Environment.SetEnvironmentVariable(variableName, "secret");

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariablesToMemoryCollection(prefix, removePrefix: true)
                .Build();

            Assert.Equal("secret", configuration["Database:Password"]);
            Assert.Null(configuration[variableName]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, null);
        }
    }

    [Fact]
    public void ValoresVaziosELoggerOpcionalSaoPreservados()
    {
        var prefix = $"NUUVIFY_{Guid.NewGuid():N}_";
        var variableName = $"{prefix}Empty";

        try
        {
            Environment.SetEnvironmentVariable(variableName, string.Empty);

            if (Environment.GetEnvironmentVariable(variableName) == null)
                return;

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariablesToMemoryCollection(prefix, logger: null)
                .Build();

            Assert.Equal(string.Empty, configuration[variableName]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, null);
        }
    }

    [Fact]
    public void MetodoLegadoDisponibilizaConfiguracaoEUsaMemoria()
    {
        var prefix = $"NUUVIFY_{Guid.NewGuid():N}_";
        var variableName = $"{prefix}Database__Password";

        try
        {
            Environment.SetEnvironmentVariable(variableName, "secret");

            var builder = new ConfigurationBuilder();
            builder.AddEnvironmentVariablesToKeyPerFile(prefix, removeVariavel: false);
            var configuration = builder.Build();

            Assert.Equal("secret", configuration[$"{prefix}Database:Password"]);
            Assert.DoesNotContain(builder.Sources, source => source.GetType().Name == "KeyPerFileConfigurationSource");
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, null);
        }
    }

    [Fact]
    public void MetodoLegadoRemoveVariavelSomenteQuandoSolicitado()
    {
        var prefix = $"NUUVIFY_{Guid.NewGuid():N}_";
        var retainedName = $"{prefix}Retained";
        var removedName = $"{prefix}Removed";

        try
        {
            Environment.SetEnvironmentVariable(retainedName, "retained");
            Environment.SetEnvironmentVariable(removedName, "removed");

            _ = new ConfigurationBuilder().AddEnvironmentVariablesToKeyPerFile(prefix, removeVariavel: false);
            Assert.Equal("retained", Environment.GetEnvironmentVariable(retainedName));

            _ = new ConfigurationBuilder().AddEnvironmentVariablesToKeyPerFile(prefix, removeVariavel: true);
            Assert.Null(Environment.GetEnvironmentVariable(retainedName));
            Assert.Null(Environment.GetEnvironmentVariable(removedName));
        }
        finally
        {
            Environment.SetEnvironmentVariable(retainedName, null);
            Environment.SetEnvironmentVariable(removedName, null);
        }
    }

    [Fact]
    public void AddContainerSecretsUsaKeyPerFileSemAlterarArquivos()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), $"nuuvify-secrets-{Guid.NewGuid():N}");
        var secretPath = Path.Combine(directoryPath, "Database__Password");

        try
        {
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(secretPath, "secret");

            var builder = new ConfigurationBuilder()
                .AddContainerSecrets(directoryPath);
            var configuration = builder.Build();

            Assert.Equal("secret", configuration["Database:Password"]);
            Assert.True(File.Exists(secretPath));
            Assert.Contains(builder.Sources, source => source.GetType().Name == "KeyPerFileConfigurationSource");
        }
        finally
        {
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public void AddContainerSecretsFalhaQuandoDiretorioObrigatorioNaoExiste()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), $"nuuvify-missing-{Guid.NewGuid():N}");

        Assert.Throws<DirectoryNotFoundException>(() => new ConfigurationBuilder()
            .AddContainerSecrets(directoryPath)
            .Build());
    }

    [Fact]
    public void AddContainerSecretsOpcionalAceitaDiretorioAusente()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), $"nuuvify-optional-{Guid.NewGuid():N}");

        var configuration = new ConfigurationBuilder()
            .AddContainerSecrets(directoryPath, optional: true)
            .Build();

        Assert.Null(configuration["Database:Password"]);
    }
}
