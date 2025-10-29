# Nuuvify.CommonPack.Extensions

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Build Status - Main](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=main)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=qas)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=qas)

Biblioteca fundamental com implementação do Notification Pattern e extensões utilitárias essenciais. Fornece uma base sólida para aplicações .NET com tratamento padronizado de notificações, extensões para strings, coleções, enums e outros tipos comuns.

## 📋 Índice

- [Nuuvify.CommonPack.Extensions](#nuuvifycommonpackextensions)
  - [📋 Índice](#-índice)
  - [Funcionalidades](#funcionalidades)
  - [Instalação](#instalação)
    - [Via Package Manager Console](#via-package-manager-console)
    - [Via .NET CLI](#via-net-cli)
    - [Via PackageReference](#via-packagereference)
  - [Dependências](#dependências)
    - [NuGet Packages](#nuget-packages)
    - [Framework](#framework)
  - [Configuração](#configuração)
  - [Uso](#uso)
    - [Notification Pattern](#notification-pattern)
      - [NotifiableR - Classe Base](#notifiabler---classe-base)
      - [NotificationR - Estrutura de Notificação](#notificationr---estrutura-de-notificação)
    - [String Extensions](#string-extensions)
      - [Manipulação de Caracteres Especiais](#manipulação-de-caracteres-especiais)
      - [Formatação e Conversão](#formatação-e-conversão)
    - [Enum Extensions](#enum-extensions)
      - [Conversões e Descrições](#conversões-e-descrições)
    - [Domain Entity](#domain-entity)
      - [Classe Base para Entidades](#classe-base-para-entidades)
    - [JSON Extensions](#json-extensions)
      - [Conversores Customizados](#conversores-customizados)
    - [DateTime Extensions](#datetime-extensions)
  - [Exemplos Práticos](#exemplos-práticos)
    - [Exemplo 1: Validação de Formulário com Notifications](#exemplo-1-validação-de-formulário-com-notifications)
    - [Exemplo 2: Processamento de Dados com Logging](#exemplo-2-processamento-de-dados-com-logging)
    - [Exemplo 3: Service com Multiple Validations](#exemplo-3-service-com-multiple-validations)
  - [API Reference](#api-reference)
    - [Core Extensions](#core-extensions)
      - [AssemblyExtension](#assemblyextension)
      - [StringExtensionMethods](#stringextensionmethods)
      - [StringExtensionToMethods](#stringextensiontomethods)
      - [EnumExtensionMethods](#enumextensionmethods)
      - [DateTimeExtensions](#datetimeextensions)
      - [DictionaryExtension](#dictionaryextension)
      - [ObjectExtension](#objectextension)
    - [Validation \& Notification](#validation--notification)
      - [NotifiableR](#notifiabler)
      - [NotificationR](#notificationr)
      - [DataAnnotationExtension](#dataannotationextension)
      - [ValidatedNotNullExtensionMethods](#validatednotnullextensionmethods)
    - [Advanced Extensions](#advanced-extensions)
      - [ExpressionExtension](#expressionextension)
      - [ReflectionConstantsExtension](#reflectionconstantsextension)
      - [DistinctExtension](#distinctextension)
    - [JSON \& Converters](#json--converters)
      - [JsonTypesExtensions](#jsontypesextensions)
      - [NullToDefaultValueConverter](#nulltodefaultvalueconverter)
      - [JsonDateTimeConverters](#jsondatetimeconverters)
    - [Logging](#logging)
      - [NuuvifyLogFormatter](#nuuvifylogformatter)
      - [NuuvifyLogSetupExtensions](#nuuvifylogsetupextensions)
    - [Helpers \& Utilities](#helpers--utilities)
      - [DomainEntity](#domainentity)
      - [CustomGenericComparer](#customgenericcomparer)
      - [CacheTimeService](#cachetimeservice)
      - [Constants](#constants)
      - [FileData](#filedata)
      - [WebProxyConfigureMethod](#webproxyconfiguremethod)
    - [Interfaces](#interfaces)
      - [IDataAnnotationCustom](#idataannotationcustom)
      - [INotPersistingAsTable](#inotpersistingastable)
      - [IRepositoryValidation](#irepositoryvalidation)
    - [Métodos Legados (Notification Pattern)](#métodos-legados-notification-pattern)
      - [NotifiableR (Continuação)](#notifiabler-continuação)
      - [NotificationR (Continuação)](#notificationr-continuação)
    - [String Extensions (Continuação)](#string-extensions-continuação)
      - [StringExtensionMethods (Continuação)](#stringextensionmethods-continuação)
    - [Enum Extensions (Continuação)](#enum-extensions-continuação)
      - [EnumExtensionMethods (Continuação)](#enumextensionmethods-continuação)
    - [Domain Entity (Continuação)](#domain-entity-continuação)
      - [DomainEntity (Continuação)](#domainentity-continuação)
    - [JSON Extensions (Continuação)](#json-extensions-continuação)
      - [NullToDefaultValueConverter (Continuação)](#nulltodefaultvalueconverter-continuação)
  - [Troubleshooting](#troubleshooting)
    - [Problemas Comuns](#problemas-comuns)
      - [NullReferenceException com Strings](#nullreferenceexception-com-strings)
      - [Notificações não aparecendo](#notificações-não-aparecendo)
      - [Enum conversions falhando](#enum-conversions-falhando)
    - [Logs e Debugging](#logs-e-debugging)
  - [Changelog](#changelog)
  - [📞 Suporte](#-suporte)

## Funcionalidades

- ✅ **Notification Pattern** para tratamento padronizado de erros e validações
- ✅ **String Extensions** com manipulação de caracteres especiais e formatação
- ✅ **Enum Extensions** com conversões e descrições automáticas
- ✅ **Domain Entity** classe base para entidades de domínio
- ✅ **JSON Extensions** com conversores customizados
- ✅ **DateTime Extensions** para manipulações de data/hora
- ✅ **Dictionary Extensions** para operações em dicionários
- ✅ **Object Extensions** para reflexão e validações
- ✅ **Logging Integration** com Microsoft.Extensions.Logging
- ✅ **Data Annotations** com validações customizadas
- ✅ **Compatibilidade .NET Standard 2.1** para máxima portabilidade

## Instalação

### Via Package Manager Console
```powershell
Install-Package Nuuvify.CommonPack.Extensions
```

### Via .NET CLI
```bash
dotnet add package Nuuvify.CommonPack.Extensions
```

### Via PackageReference
```xml
<PackageReference Include="Nuuvify.CommonPack.Extensions" Version="X.X.X" />
```

## Dependências

### NuGet Packages

| Package                                        | Version | Descrição                                                          |
| ---------------------------------------------- | ------- | ------------------------------------------------------------------ |
| **Microsoft.CSharp**                           | 4.7.0   | Suporte para recursos do C# como dynamic e interpolação de strings |
| **System.ComponentModel.Annotations**          | 5.0.0   | Data Annotations para validação de modelos                         |
| **System.Runtime.Extensions**                  | 4.3.1   | Extensões fundamentais do runtime .NET                             |
| **System.Diagnostics.DiagnosticSource**        | 8.0.1   | Infraestrutura para diagnósticos e observabilidade                 |
| **Microsoft.Extensions.Logging.Abstractions**  | 8.0.3   | Abstrações do sistema de logging                                   |
| **Microsoft.Extensions.Logging.Configuration** | 8.0.1   | Configuração para sistema de logging                               |
| **Microsoft.Extensions.Logging.Console**       | 8.0.1   | Provider de console para logging                                   |
| **Microsoft.Extensions.Logging**               | 8.0.1   | Sistema de logging unificado                                       |
| **Microsoft.Extensions.Options**               | 8.0.2   | Padrão Options para configuração                                   |
| **System.Text.Json**                           | 8.0.5   | Serialização/deserialização JSON de alta performance               |

### Framework

- **.NET Standard 2.1**: Garante compatibilidade com .NET Core 3.0+, .NET 5+, .NET 6+, .NET 8+ e .NET Framework 4.7.2+

## Configuração

Esta biblioteca não requer configuração específica para uso básico. Para recursos de logging:

```csharp
// Program.cs ou Startup.cs
using Microsoft.Extensions.Logging;

// Configurar logging (opcional)
builder.Services.AddLogging(configure => configure.AddConsole());
```

## Uso

### Notification Pattern

#### NotifiableR - Classe Base

```csharp
public class PessoaService : NotifiableR
{
    public void ValidarPessoa(string nome, int idade)
    {
        if (string.IsNullOrEmpty(nome))
        {
            AddNotification("Nome", "Nome é obrigatório");
        }

        if (idade < 18)
        {
            AddNotification("Idade", "Pessoa deve ser maior de idade");
        }

        // Verificar se há erros
        if (!IsValid())
        {
            // Processar notificações
            foreach (var notification in Notifications)
            {
                Console.WriteLine($"{notification.Property}: {notification.Message}");
            }
        }
    }

    public void ProcessarComLog(ILogger logger)
    {
        // Validações que podem gerar notificações
        ValidarPessoa("", 15);

        // Log automático das notificações
        var notifications = LoggerNotifications(logger, "ProcessarComLog");

        // As notificações são automaticamente logadas como Warning
    }
}
```

#### NotificationR - Estrutura de Notificação

```csharp
public class ValidationService : NotifiableR
{
    public void ProcessarDados(List<string> dados)
    {
        for (int i = 0; i < dados.Count; i++)
        {
            if (string.IsNullOrEmpty(dados[i]))
            {
                // Adicionar notificação com ID do agregado
                AddNotification("Dados", $"Item {i} está vazio", aggregateId: i.ToString());
            }
        }

        // Remover notificações específicas se necessário
        if (dados.Count > 100)
        {
            RemoveNotification("Dados");
        }

        // Limpar todas as notificações
        RemoveNotifications(removeAll: true);
    }

    public void AdicionarNotificacoesExternas(IList<NotificationR> notificacoesExternas)
    {
        // Adicionar notificações de outros serviços
        AddNotifications(notificacoesExternas);
    }
}
```

### String Extensions

#### Manipulação de Caracteres Especiais

```csharp
using Nuuvify.CommonPack.Extensions.Implementation;

public class TextProcessingService
{
    public void ProcessarTextos()
    {
        string textoComEspeciais = "João & Maria - R$ 1.500,00!";

        // Remover caracteres especiais (mantém apenas ASCII 20-7E)
        string limpo = textoComEspeciais.RemoveSpecialChars();
        // Resultado: "Joo  Maria  R 1500"

        // Manter apenas letras e números
        string alphanumerico = textoComEspeciais.GetLettersAndNumbersOnly();
        // Resultado: "JooMaria1500"

        // Remover acentos mas manter estrutura
        string semAcentos = "José María Piñón".RemoveAccent();
        // Resultado: "Jose Maria Pinon"

        // Extrair apenas números
        string apenasNumeros = "ABC-123-DEF-456".GetNumbers();
        // Resultado: "123456"
    }

    public void FormatacaoSegura()
    {
        string texto = null;

        // Métodos seguros que não geram NullReferenceException
        string upper = texto.ToUpperNotNull(); // Retorna null
        string lower = texto.ToLowerNotNull(); // Retorna null
        string trimmed = texto.TrimNotNull();  // Retorna null

        // Ao invés de:
        // string upper = texto?.ToUpper(); // Syntax mais verbosa
    }
}
```

#### Formatação e Conversão

```csharp
public class FormattingService
{
    public void ExemplosFormatacao()
    {
        // Conversão para Title Case
        string nome = "joão da silva";
        string titleCase = StringExtensionMethods.ToTitleCase(nome);
        // Resultado: "João Da Silva"

        // Limpeza mantendo alguns caracteres
        string codigo = "ABC-123_DEF@456";
        string limpo = codigo.RemoveCharsKeepChars("-", "_");
        // Resultado: "ABC-123_DEF456"

        // Limpeza mantendo diacríticos (acentos)
        string textoAcentuado = "José & María!";
        string mantendoAcentos = textoAcentuado.RemoveCharsKeepDiacritics();
        // Resultado: "José María"
    }
}
```

### Enum Extensions

#### Conversões e Descrições

```csharp
using System.ComponentModel;
using Nuuvify.CommonPack.Extensions.Implementation;

public enum StatusPedido
{
    [Description("Aguardando Pagamento")]
    AguardandoPagamento = 1,

    [Description("Pagamento Confirmado")]
    PagamentoConfirmado = 2,

    [Description("Em Preparação")]
    EmPreparacao = 3
}

public class PedidoService
{
    public void ProcessarEnums()
    {
        var status = StatusPedido.AguardandoPagamento;

        // Obter descrição do enum
        string descricao = status.GetDescription();
        // Resultado: "Aguardando Pagamento"

        // Converter string para número do enum
        string statusTexto = "PagamentoConfirmado";
        int numeroEnum = statusTexto.ToEnumNumero<StatusPedido>();
        // Resultado: 2

        // Converter número para texto do enum
        int numero = 3;
        string textoEnum = numero.ToEnumTexto<StatusPedido>();
        // Resultado: "EmPreparacao"

        // Verificar se string é um enum válido
        bool isValid = "StatusInvalido".IsEnum<StatusPedido>(out int resultado);
        // Resultado: false, resultado = int.MaxValue
    }
}
```

### Domain Entity

#### Classe Base para Entidades

```csharp
public class ProdutoEntity : DomainEntity
{
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }
    public bool Ativo { get; private set; }

    public ProdutoEntity(string nome, decimal preco)
    {
        // Id já é gerado automaticamente no constructor base
        Nome = nome;
        Preco = preco;
        Ativo = true;

        // Propriedades de auditoria são preenchidas automaticamente:
        // - DataCadastro
        // - UsuarioCadastro (preenchido pelo SaveChanges do repository)
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        Preco = novoPreco;
        // DataAlteracao e UsuarioAlteracao serão preenchidos pelo SaveChanges
    }

    // Sobrescrever propriedades de auditoria se necessário
    [JsonIgnore]
    public override DateTimeOffset DataCadastro => base.DataCadastro;
}

// Uso
public class ProdutoService : NotifiableR
{
    public ProdutoEntity CriarProduto(string nome, decimal preco)
    {
        if (string.IsNullOrEmpty(nome))
        {
            AddNotification("Nome", "Nome do produto é obrigatório");
            return null;
        }

        if (preco <= 0)
        {
            AddNotification("Preco", "Preço deve ser maior que zero");
            return null;
        }

        return new ProdutoEntity(nome, preco);
    }
}
```

### JSON Extensions

#### Conversores Customizados

```csharp
using System.Text.Json;
using Nuuvify.CommonPack.Extensions.JsonConverter;

public class ConfigurationService
{
    public void SerializacaoPersonalizada()
    {
        var produto = new ProdutoEntity("Notebook", 2500.00m);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = {
                new NullToDefaultValueConverter() // Converte nulls para valores padrão
            }
        };

        string json = JsonSerializer.Serialize(produto, options);

        // NullToDefaultValueConverter converte propriedades null para valores padrão:
        // - string null → ""
        // - int? null → 0
        // - DateTime? null → DateTime.MinValue
    }
}
```

### DateTime Extensions

```csharp
using Nuuvify.CommonPack.Extensions.Implementation;

public class AgendaService
{
    public void ManipulacoesData()
    {
        var agora = DateTime.Now;

        // Extensões para DateTime (exemplos conceituais)
        // As implementações específicas dependem dos métodos disponíveis na biblioteca

        // Formatação segura
        DateTime? dataOpcional = null;
        string dataFormatada = dataOpcional.ToStringNotNull(); // Retorna null ao invés de exception
    }
}
```

## Exemplos Práticos

### Exemplo 1: Validação de Formulário com Notifications

```csharp
public class UsuarioService : NotifiableR
{
    public UsuarioEntity CriarUsuario(CriarUsuarioCommand command)
    {
        // Validar nome
        if (string.IsNullOrWhiteSpace(command.Nome))
        {
            AddNotification("Nome", "Nome é obrigatório");
        }
        else if (command.Nome.Length < 2)
        {
            AddNotification("Nome", "Nome deve ter pelo menos 2 caracteres");
        }

        // Validar email usando extension
        var emailLimpo = command.Email?.TrimNotNull()?.ToLowerNotNull();
        if (string.IsNullOrEmpty(emailLimpo) || !IsValidEmail(emailLimpo))
        {
            AddNotification("Email", "Email inválido");
        }

        // Validar idade
        if (command.Idade < 18)
        {
            AddNotification("Idade", "Usuário deve ser maior de idade");
        }

        // Se há erros, retornar null
        if (!IsValid())
        {
            return null;
        }

        // Criar entidade com dados limpos
        var nomeFormatado = StringExtensionMethods.ToTitleCase(command.Nome);
        return new UsuarioEntity(nomeFormatado, emailLimpo, command.Idade);
    }

    public List<string> ObterErrosFormatados()
    {
        return Notifications.Select(n => $"{n.Property}: {n.Message}").ToList();
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
```

### Exemplo 2: Processamento de Dados com Logging

```csharp
public class ImportacaoService : NotifiableR
{
    private readonly ILogger<ImportacaoService> _logger;

    public ImportacaoService(ILogger<ImportacaoService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessarArquivoAsync(List<string> linhas)
    {
        for (int i = 0; i < linhas.Count; i++)
        {
            var linha = linhas[i];

            // Limpar dados
            var dadosLimpos = linha.RemoveSpecialChars().TrimNotNull();

            if (string.IsNullOrEmpty(dadosLimpos))
            {
                AddNotification("Linha", $"Linha {i + 1} está vazia ou inválida", aggregateId: i.ToString());
                continue;
            }

            // Extrair números se necessário
            var numeros = dadosLimpos.GetNumbers();
            if (numeros.Length < 5)
            {
                AddNotification("Dados", $"Linha {i + 1} não possui dados suficientes", aggregateId: i.ToString());
            }
        }

        // Log todas as notificações de uma vez
        if (Notifications.Any())
        {
            var notificacoesComLog = LoggerNotifications(_logger, "ProcessarArquivo");

            // Notificações são automaticamente logadas como Warning:
            // "Validação em: ProcessarArquivo Linha Linha 1 está vazia ou inválida ..."
        }

        // Processar apenas linhas válidas
        await ProcessarLinhasValidasAsync();
    }

    private async Task ProcessarLinhasValidasAsync()
    {
        // Lógica de processamento das linhas válidas
        await Task.Delay(100); // Simular processamento

        // Limpar notificações após processamento bem-sucedido
        RemoveNotifications(removeAll: true);
    }
}
```

### Exemplo 3: Service com Multiple Validations

```csharp
public class PedidoService : NotifiableR
{
    public PedidoEntity ProcessarPedido(PedidoCommand command)
    {
        // Limpar dados de entrada
        var clienteFormatado = command.Cliente?.TrimNotNull()?.ToTitleCase();
        var observacoes = command.Observacoes?.RemoveSpecialChars();

        // Validações básicas
        ValidarDadosBasicos(clienteFormatado, command);

        // Validações de itens
        ValidarItens(command.Itens);

        // Validações de enum
        ValidarStatus(command.StatusTexto);

        // Se há erros, retornar null
        if (!IsValid())
        {
            return null;
        }

        // Criar entidade com dados processados
        var pedido = new PedidoEntity(clienteFormatado, observacoes);

        foreach (var item in command.Itens)
        {
            pedido.AdicionarItem(item.Produto.ToTitleCase(), item.Quantidade, item.Valor);
        }

        return pedido;
    }

    private void ValidarDadosBasicos(string cliente, PedidoCommand command)
    {
        if (string.IsNullOrEmpty(cliente))
        {
            AddNotification("Cliente", "Nome do cliente é obrigatório");
        }

        if (command.DataPedido == default)
        {
            AddNotification("DataPedido", "Data do pedido é obrigatória");
        }
    }

    private void ValidarItens(List<ItemPedidoCommand> itens)
    {
        if (itens == null || !itens.Any())
        {
            AddNotification("Itens", "Pedido deve ter pelo menos um item");
            return;
        }

        for (int i = 0; i < itens.Count; i++)
        {
            var item = itens[i];

            if (string.IsNullOrEmpty(item.Produto?.TrimNotNull()))
            {
                AddNotification("Produto", $"Produto do item {i + 1} é obrigatório", aggregateId: i.ToString());
            }

            if (item.Quantidade <= 0)
            {
                AddNotification("Quantidade", $"Quantidade do item {i + 1} deve ser maior que zero", aggregateId: i.ToString());
            }

            if (item.Valor <= 0)
            {
                AddNotification("Valor", $"Valor do item {i + 1} deve ser maior que zero", aggregateId: i.ToString());
            }
        }
    }

    private void ValidarStatus(string statusTexto)
    {
        if (!string.IsNullOrEmpty(statusTexto))
        {
            bool isValidStatus = statusTexto.IsEnum<StatusPedido>(out int statusNumero);
            if (!isValidStatus)
            {
                AddNotification("Status", $"Status '{statusTexto}' não é válido");
            }
        }
    }

    public void RemoverErrosEspecificos()
    {
        // Remover notificações de um campo específico
        RemoveNotification("Cliente");

        // Ou adicionar notificações de outro serviço
        var outroServico = new ValidacaoService();
        outroServico.ValidarRegrasNegocio();
        AddNotifications(outroServico.Notifications);
    }
}
```

## API Reference

### Core Extensions

#### AssemblyExtension

Métodos para obter informações do assembly da aplicação.

```csharp
public static class AssemblyExtension
{
    // Obtém o nome da aplicação
    public static string GetApplicationNameByAssembly(this Assembly assembly);

    // Obtém o número do build
    public static string GetApplicationBuildNumber(this Assembly assembly);

    // Obtém a versão da aplicação
    public static string GetApplicationVersion(this Assembly assembly);
}
```

**Exemplo de uso**:
```csharp
var assembly = Assembly.GetExecutingAssembly();
var appName = assembly.GetApplicationNameByAssembly();
var version = assembly.GetApplicationVersion();
var buildNumber = assembly.GetApplicationBuildNumber();

Console.WriteLine($"{appName} v{version} (Build: {buildNumber})");
```

#### StringExtensionMethods

Métodos de extensão para manipulação avançada de strings.

```csharp
public static partial class StringExtensionMethods
{
    // Remove caracteres especiais (mantém apenas ASCII 20-7E)
    public static string RemoveSpecialChars(this string text);

    // Mantém apenas letras e números
    public static string GetLettersAndNumbersOnly(this string text);

    // Obtém apenas caracteres Unicode
    public static string GetUnicodeChars(this string text);

    // Remove caracteres mas mantém diacríticos (acentos)
    public static string RemoveCharsKeepDiacritics(this string text);

    // Remove caracteres mas mantém os especificados
    public static string RemoveCharsKeepChars(this string text, params string[] keepChars);

    // Remove acentuação
    public static string RemoveAccent(this string text);

    // Extrai apenas números
    public static string GetNumbers(this string text);

    // Substring seguro (null-safe)
    public static string SubstringNotNull(this string value, int start, int length);

    // Converte para Title Case
    public static string ToTitleCase(this string text);

    // Remove \r\n e outros caracteres especificados
    public static string GetReturnMessageWithoutRn(this string returnMessage, string otherCharToRemove = null);
}
```

**Exemplo de uso**:
```csharp
string texto = "José & Maria - R$ 1.500,00!";

// Diferentes tipos de limpeza
string semEspeciais = texto.RemoveSpecialChars();        // "Jos  Maria  R 1500"
string apenasAlnum = texto.GetLettersAndNumbersOnly();   // "JosMaria1500"
string semAcentos = texto.RemoveAccent();                // "Jose & Maria - R$ 1.500,00!"
string apenasNumeros = texto.GetNumbers();               // "1500"

// Mantendo caracteres específicos
string comTracos = texto.RemoveCharsKeepChars("-", "$"); // "José  Maria - R$ 150000"

// Title Case
string nome = "joão da silva".ToTitleCase();             // "João Da Silva"
```

#### StringExtensionToMethods

Métodos de conversão de string com proteção contra null.

```csharp
public static partial class StringExtensionMethods
{
    // ToUpper invariante seguro
    public static string ToUpperInvariantNotNull(this string value);

    // ToUpper seguro
    public static string ToUpperNotNull(this string value);

    // ToLower seguro
    public static string ToLowerNotNull(this string value);

    // ToLower invariante seguro
    public static string ToLowerInvariantNotNull(this string value);

    // Trim seguro
    public static string TrimNotNull(this string value);

    // ToString seguro
    public static string ToStringNotNull(this string value);
}
```

**Exemplo de uso**:
```csharp
string texto = null;

// Métodos tradicionais causariam NullReferenceException
// string upper = texto.ToUpper(); // ❌ Exception!

// Métodos seguros retornam null
string upper = texto.ToUpperNotNull();           // null
string lower = texto.ToLowerNotNull();           // null
string trimmed = texto.TrimNotNull();            // null

// Útil em chains
string resultado = input?.TrimNotNull()?.ToLowerNotNull();
```

#### EnumExtensionMethods

Métodos de extensão para trabalhar com enums.

```csharp
public static class EnumExtensionMethods
{
    // Obtém a descrição do enum (via [Description])
    public static string GetDescription(this Enum GenericEnum);

    // Converte string para número do enum
    public static int ToEnumNumero<T>(this string value);

    // Converte string para código do enum
    public static string ToEnumCodigo<T>(this string value);

    // Converte int para código do enum
    public static string ToEnumCodigo<T>(this int value);

    // Converte int para descrição do enum
    public static string ToEnumDescricao<T>(this int value);

    // Converte string para descrição do enum
    public static string ToEnumDescricao<T>(this string value);

    // Verifica se string é um enum válido
    public static bool IsEnum<T>(this string value, out int enumCode);

    // Obtém código do enum pela descrição
    public static string GetCodeEnumByDescription<T>(this string value);
}
```

**Exemplo de uso**:
```csharp
public enum StatusPedido
{
    [Description("Aguardando Pagamento")]
    AguardandoPagamento = 1,

    [Description("Em Preparação")]
    EmPreparacao = 2
}

// Obter descrição
var status = StatusPedido.AguardandoPagamento;
string descricao = status.GetDescription(); // "Aguardando Pagamento"

// Converter string para enum
int numero = "AguardandoPagamento".ToEnumNumero<StatusPedido>(); // 1

// Validar enum
bool isValid = "StatusInvalido".IsEnum<StatusPedido>(out int codigo);
// isValid = false, codigo = int.MaxValue
```

#### DateTimeExtensions

Métodos de extensão para DateTime e DateTimeOffset.

```csharp
public static class DateTimeExtensions
{
    // Formata para padrão REST (yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ss)
    public static string GetFormatRest(this DateTime dateTime, bool timeToo = false);

    // Formata DateTimeOffset para padrão REST
    public static string GetFormatRest(this DateTimeOffset dateTimeOffset);

    // Obtém o primeiro dia útil do mês
    public static DateTime GetFirstWorkingDay(this DateTime data);

    // Obtém o primeiro dia útil do mês (DateTimeOffset)
    public static DateTimeOffset GetFirstWorkingDay(this DateTimeOffset data);
}
```

**Exemplo de uso**:
```csharp
var hoje = DateTime.Now;

// Formato REST
string dataRest = hoje.GetFormatRest();           // "2025-10-28"
string dataHoraRest = hoje.GetFormatRest(true);  // "2025-10-28T14:30:00"

// Primeiro dia útil
var primeiroDiaUtil = hoje.GetFirstWorkingDay();
```

#### DictionaryExtension

Métodos de extensão para dicionários.

```csharp
public static class DictionaryExtension
{
    // Adiciona ou substitui valor no dicionário
    public static void AddForce(this IDictionary<string, object> dictionary, string key, object value);
}
```

**Exemplo de uso**:
```csharp
var dict = new Dictionary<string, object>
{
    { "nome", "João" }
};

// Adiciona ou atualiza sem verificar se existe
dict.AddForce("nome", "Maria");   // Substitui
dict.AddForce("idade", 30);        // Adiciona
```

#### ObjectExtension

Métodos de extensão para objetos.

```csharp
public static class ObjectExtension
{
    // Compara objetos com proteção contra null
    public static bool EqualsObjectNotNull(this object obj, object obj1);
}
```

**Exemplo de uso**:
```csharp
object obj1 = null;
object obj2 = "teste";

// Comparação segura
bool iguais = obj1.EqualsObjectNotNull(obj2); // false (sem NullReferenceException)
```

---

### Validation & Notification

#### NotifiableR

Classe base para implementação do Notification Pattern.

```csharp
public class NotifiableR
{
    // Propriedades
    public IReadOnlyCollection<NotificationR> Notifications { get; }

    // Métodos
    public bool IsValid();
    public void AddNotification(string property, string message, string aggregateId = null);
    public void AddNotifications(IList<NotificationR> notifications);
    public void RemoveNotification(string property);
    public void RemoveNotifications(bool removeAll = true);
    public IList<NotificationR> LoggerNotifications(ILogger logger, string logDescription);
}
```

**Exemplo de uso**:
```csharp
public class ProdutoService : NotifiableR
{
    public void ValidarProduto(string nome, decimal preco)
    {
        if (string.IsNullOrEmpty(nome))
            AddNotification("Nome", "Nome é obrigatório");

        if (preco <= 0)
            AddNotification("Preco", "Preço deve ser maior que zero");

        if (!IsValid())
        {
            // Processar erros
            foreach (var error in Notifications)
                Console.WriteLine($"{error.Property}: {error.Message}");
        }
    }
}
```

#### NotificationR

Estrutura de notificação individual.

```csharp
public class NotificationR
{
    public string Property { get; set; }
    public string Message { get; set; }
    public string AggregatorId { get; set; }
    public DateTimeOffset DateOccurrence { get; set; }
}
```

#### DataAnnotationExtension

Extensões para integração com Data Annotations.

```csharp
public static class DataAnnotationExtension
{
    // Obtém notificações de validação
    public static IList<NotificationR> GetNotifications(this IDataAnnotationCustom model);

    // Valida modelo usando Data Annotations
    public static bool DataAnnotationsIsValid(this IDataAnnotationCustom model);
}
```

**Exemplo de uso**:
```csharp
public class ProdutoModel : IDataAnnotationCustom
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Nome { get; set; }

    [Range(1, 99999, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Preco { get; set; }
}

var produto = new ProdutoModel { Nome = "", Preco = 0 };

// Validar
bool isValid = produto.DataAnnotationsIsValid();

// Obter erros
var erros = produto.GetNotifications();
```

#### ValidatedNotNullExtensionMethods

Métodos para validação de null.

```csharp
public static class ValidatedNotNullExtensionMethods
{
    // Verifica se objeto não é null
    public static bool NotNull<T>(this T value) where T : class;

    // Verifica se coleção não é null ou vazia
    public static bool NotNullOrZero<T>(this T value) where T : IEnumerable<object>;

    // Verifica se enumerável não é null ou vazio
    public static bool NotNullOrZero(this System.Collections.IEnumerable value);
}
```

**Exemplo de uso**:
```csharp
string texto = "teste";
List<int> lista = new List<int> { 1, 2, 3 };

// Validações
bool textoValido = texto.NotNull();        // true
bool listaValida = lista.NotNullOrZero();  // true

string textoNulo = null;
bool nuloInvalido = textoNulo.NotNull();   // false
```

---

### Advanced Extensions

#### ExpressionExtension

Métodos para manipulação de expressões LINQ.

```csharp
public static class ExpressionExtension
{
    // Combina expressões com operador AND ou OR
    public static Expression<Func<T, bool>> CombineExpressions<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2,
        bool useOrOperator = false);
}
```

**Exemplo de uso**:
```csharp
Expression<Func<Produto, bool>> expr1 = p => p.Ativo;
Expression<Func<Produto, bool>> expr2 = p => p.Preco > 100;

// Combinar com AND
var combinadaAnd = expr1.CombineExpressions(expr2);
// Resultado: p => p.Ativo && p.Preco > 100

// Combinar com OR
var combinadaOr = expr1.CombineExpressions(expr2, useOrOperator: true);
// Resultado: p => p.Ativo || p.Preco > 100

// Usar em query
var produtos = dbContext.Produtos.Where(combinadaAnd).ToList();
```

#### ReflectionConstantsExtension

Métodos para obter constantes via reflexão.

```csharp
public static class ReflectionConstantsExtension
{
    // Obtém todas as constantes públicas de um tipo
    public static IEnumerable<FieldInfo> GetPublicConstants(this Type type);
}
```

**Exemplo de uso**:
```csharp
public static class StatusCodes
{
    public const int OK = 200;
    public const int NotFound = 404;
    public const int ServerError = 500;
}

// Obter todas as constantes
var constantes = typeof(StatusCodes).GetPublicConstants();

foreach (var constante in constantes)
{
    var nome = constante.Name;
    var valor = constante.GetValue(null);
    Console.WriteLine($"{nome} = {valor}");
}
// Output:
// OK = 200
// NotFound = 404
// ServerError = 500
```

#### DistinctExtension

Métodos para Distinct customizado.

```csharp
public static class DistinctExtension
{
    // Distinct usando propriedade específica
    public static IEnumerable<TSource> Distinct<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, object> keySelector);
}
```

**Exemplo de uso**:
```csharp
var produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Produto A", Categoria = "Cat1" },
    new Produto { Id = 2, Nome = "Produto B", Categoria = "Cat1" },
    new Produto { Id = 3, Nome = "Produto C", Categoria = "Cat2" }
};

// Distinct por categoria
var categorias = produtos.Distinct(p => p.Categoria);
// Resultado: 2 produtos (Cat1 e Cat2)
```

---

### JSON & Converters

#### JsonTypesExtensions

Métodos para conversão de tipos JSON.

```csharp
public static class JsonTypesExtensions
{
    // Converte tipo JSON customizado
    public static object ConvertJsonTypeCustom(
        this ref Utf8JsonReader reader,
        Type propertyType);
}
```

**Exemplo de uso**:
```csharp
// Usado internamente pelos conversores JSON customizados
// para converter tipos de forma inteligente durante deserialização
```

#### NullToDefaultValueConverter

Conversor JSON que transforma null em valores padrão.

```csharp
public class NullToDefaultValueConverter<T> : JsonConverter<T>
    where T : struct, IConvertible
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
}
```

**Exemplo de uso**:
```csharp
var options = new JsonSerializerOptions
{
    Converters = { new NullToDefaultValueConverter<int>() }
};

string json = "{\"idade\": null}";
var obj = JsonSerializer.Deserialize<Pessoa>(json, options);
// obj.Idade = 0 (valor padrão ao invés de exception)
```

#### JsonDateTimeConverters

Conversores para DateTime e DateTimeOffset.

```csharp
// Converte DateTime para tipos inferidos
public class JsonDateTimeToInferredTypesConverter : JsonConverter<DateTime>

// Converte DateTimeOffset para tipos inferidos
public class JsonDateTimeOffsetToInferredTypesConverter : JsonConverter<DateTimeOffset>

// Converte objetos para tipos inferidos
public class JsonObjectToInferredTypesConverter : JsonConverter<object>
```

**Exemplo de uso**:
```csharp
var options = new JsonSerializerOptions
{
    Converters =
    {
        new JsonDateTimeToInferredTypesConverter(),
        new JsonDateTimeOffsetToInferredTypesConverter()
    }
};

// Deserialização inteligente de datas
string json = "{\"data\": \"2025-10-28T14:30:00\"}";
var obj = JsonSerializer.Deserialize<MyObject>(json, options);
```

---

### Logging

#### NuuvifyLogFormatter

Formatador customizado de logs com cores.

```csharp
public class NuuvifyLogFormatter : ConsoleFormatter
{
    public NuuvifyLogFormatter(IOptionsMonitor<NuuvifyLogFormatterOptions> options);

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider scopeProvider,
        TextWriter textWriter);
}
```

#### NuuvifyLogSetupExtensions

Extensões para configuração de logging customizado.

```csharp
public static class NuuvifyLogSetupExtensions
{
    // Adiciona console formatter customizado
    public static ILoggingBuilder AddNuuvifyConsoleFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configure = null);
}
```

**Exemplo de uso**:
```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "nuuvify";
});
builder.Logging.AddNuuvifyConsoleFormatter(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
```

---

### Helpers & Utilities

#### DomainEntity

Classe base para entidades de domínio.

```csharp
public class DomainEntity
{
    public string Id { get; protected set; }
    public virtual DateTimeOffset DataCadastro { get; protected set; }
    public virtual string UsuarioCadastro { get; set; }
    public virtual DateTimeOffset? DataAlteracao { get; set; }
    public virtual string UsuarioAlteracao { get; set; }

    public override bool Equals(object obj);
    public override int GetHashCode();
}
```

**Exemplo de uso**:
```csharp
public class Produto : DomainEntity
{
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }

    public Produto(string nome, decimal preco)
    {
        // Id é gerado automaticamente
        Nome = nome;
        Preco = preco;
        // DataCadastro é preenchido automaticamente
    }
}
```

#### CustomGenericComparer

Comparador genérico customizado.

```csharp
public class CustomGenericComparer<T> : IEqualityComparer<T>
{
    public Func<T, object> KeySelector { get; set; }

    public bool Equals(T x, T y);
    public int GetHashCode(T obj);
}
```

**Exemplo de uso**:
```csharp
var comparer = new CustomGenericComparer<Produto>
{
    KeySelector = p => p.Nome
};

var lista = produtos.Distinct(comparer).ToList();
```

#### CacheTimeService

Serviço para gerenciamento de cache baseado em tempo.

```csharp
public class CacheTimeService
{
    // Implementação específica para cache temporal
}
```

#### Constants

Constantes do sistema.

```csharp
public static class Constants
{
    // Header de correlação
    public static string CorrelationHeader { get; }

    // Header de claim do usuário
    public static string UserClaimHeader { get; }

    // Header de validação do usuário
    public static string UserIsValidToApplication { get; }
}
```

**Exemplo de uso**:
```csharp
// Em um middleware ou controller
var correlationId = HttpContext.Request.Headers[Constants.CorrelationHeader];
var userClaim = HttpContext.Request.Headers[Constants.UserClaimHeader];
```

#### FileData

Classe para manipulação de dados de arquivo.

```csharp
public class FileData
{
    // Propriedades e métodos para manipulação de arquivos
}
```

#### WebProxyConfigureMethod

Configuração de proxy web.

```csharp
public class WebProxyConfigureMethod
{
    // Métodos para configurar proxy HTTP
}
```

---

### Interfaces

#### IDataAnnotationCustom

Interface para modelos com validação via Data Annotations.

```csharp
public interface IDataAnnotationCustom
{
    // Implementar para habilitar extensões de validação
}
```

#### INotPersistingAsTable

Marca uma entidade que não deve ser persistida como tabela.

```csharp
public interface INotPersistingAsTable
{
    // Interface de marcação (marker interface)
}
```

#### IRepositoryValidation

Interface para validações em repositórios.

```csharp
public interface IRepositoryValidation
{
    // Métodos de validação customizados para repositórios
}
```

---

### Métodos Legados (Notification Pattern)

#### NotifiableR (Continuação)
- `IReadOnlyCollection<NotificationR> Notifications`: Lista de notificações
- `bool IsValid()`: Verifica se não há notificações
- `void AddNotification(string property, string message, string aggregateId = null)`: Adiciona notificação
- `void AddNotifications(IList<NotificationR> notifications)`: Adiciona múltiplas notificações
- `void RemoveNotification(string property)`: Remove notificações de uma propriedade
- `void RemoveNotifications(bool removeAll = true)`: Remove todas as notificações
- `IList<NotificationR> LoggerNotifications(ILogger logger, string logDescription)`: Log automático das notificações

#### NotificationR (Continuação)
- `string Property`: Nome da propriedade
- `string Message`: Mensagem de erro
- `string AggregatorId`: ID do agregado
- `DateTimeOffset DateOccurrence`: Data/hora da notificação

### String Extensions (Continuação)

#### StringExtensionMethods (Continuação)
- `string RemoveSpecialChars(this string text)`: Remove caracteres especiais ASCII
- `string GetLettersAndNumbersOnly(this string text)`: Mantém apenas letras e números
- `string GetNumbers(this string text)`: Extrai apenas números
- `string RemoveAccent(this string text)`: Remove acentos
- `string ToTitleCase(this string text)`: Converte para Title Case
- `string ToUpperNotNull(this string value)`: ToUpper seguro (null-safe)
- `string ToLowerNotNull(this string value)`: ToLower seguro
- `string TrimNotNull(this string value)`: Trim seguro

### Enum Extensions (Continuação)

#### EnumExtensionMethods (Continuação)
- `string GetDescription(this Enum GenericEnum)`: Obtém descrição do enum
- `int ToEnumNumero<T>(this string value)`: Converte string para número do enum
- `string ToEnumTexto<T>(this int numero)`: Converte número para texto do enum
- `bool IsEnum<T>(this string value, out int result)`: Verifica se string é enum válido

### Domain Entity (Continuação)

#### DomainEntity (Continuação)
- `string Id`: Identificador único (GUID)
- `DateTimeOffset DataCadastro`: Data de cadastro
- `string UsuarioCadastro`: Usuário que cadastrou
- `DateTimeOffset? DataAlteracao`: Data da última alteração
- `string UsuarioAlteracao`: Usuário que alterou
- `override bool Equals(object obj)`: Comparação por Id
- `override int GetHashCode()`: Hash baseado no tipo e Id

### JSON Extensions (Continuação)

#### NullToDefaultValueConverter (Continuação)
- Converte valores null para valores padrão durante serialização JSON

## Troubleshooting

### Problemas Comuns

#### NullReferenceException com Strings
**Problema**: Erro ao chamar métodos em strings nulas

**Solução**: Usar extensões null-safe
```csharp
// ❌ Incorreto
string texto = null;
string upper = texto.ToUpper(); // NullReferenceException

// ✅ Correto
string upper = texto.ToUpperNotNull(); // Retorna null
```

#### Notificações não aparecendo
**Problema**: Notificações adicionadas mas não aparecem

**Causa**: Verificar se está herdando de NotifiableR

**Solução**: Herdar corretamente da classe base
```csharp
// ✅ Correto
public class MeuService : NotifiableR
{
    public void MinhaValidacao()
    {
        AddNotification("Campo", "Mensagem");
        bool temErros = !IsValid(); // Funciona corretamente
    }
}
```

#### Enum conversions falhando
**Problema**: `ToEnumNumero()` retorna int.MaxValue

**Causa**: String não corresponde a um valor válido do enum

**Solução**: Verificar se o valor existe no enum
```csharp
public enum MeuEnum { Valor1 = 1, Valor2 = 2 }

// ✅ Verificar antes de converter
string valorTexto = "Valor1";
if (valorTexto.IsEnum<MeuEnum>(out int numero))
{
    // Conversão foi bem-sucedida
    Console.WriteLine($"Número: {numero}");
}
```

### Logs e Debugging

Para debugging das notificações:

```csharp
public class DebugService : NotifiableR
{
    private readonly ILogger<DebugService> _logger;

    public void DebugarNotificacoes()
    {
        AddNotification("Campo1", "Erro 1");
        AddNotification("Campo2", "Erro 2");

        // Debug manual
        foreach (var notification in Notifications)
        {
            Console.WriteLine($"[{notification.DateOccurrence}] {notification.Property}: {notification.Message}");
        }

        // Log automático com detalhes
        LoggerNotifications(_logger, "DebugService.DebugarNotificacoes");
    }
}
```

## Changelog

Ver arquivo [CHANGELOG.md](CHANGELOG.md) para histórico detalhado de alterações.

---

## 📞 Suporte

Para dúvidas, issues ou contribuições:
- 🐛 **Issues**: [GitHub Issues](https://github.com/nuuvify/CommonPack/issues)
- 📧 **Email**: [suporte@zocate.li](mailto:suporte@zocate.li)
- 📖 **Documentação**: [Wiki do Projeto](https://github.com/nuuvify/CommonPack/wiki)

---
**Nuuvify CommonPack** - Construindo soluções robustas para .NET 🚀
