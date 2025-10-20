# Nuuvify.CommonPack.Extensions

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Build Status - Main](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=main)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=qas)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=qas)

Biblioteca fundamental com implementação do Notification Pattern e extensões utilitárias essenciais. Fornece uma base sólida para aplicações .NET com tratamento padronizado de notificações, extensões para strings, coleções, enums e outros tipos comuns.

## 📋 Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Dependências](#dependências)
- [Configuração](#configuração)
- [Uso](#uso)
  - [Notification Pattern](#notification-pattern)
  - [String Extensions](#string-extensions)
  - [Enum Extensions](#enum-extensions)
  - [Domain Entity](#domain-entity)
  - [JSON Extensions](#json-extensions)
  - [DateTime Extensions](#datetime-extensions)
- [Exemplos Práticos](#exemplos-práticos)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

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

### Notification Pattern

#### NotifiableR
- `IReadOnlyCollection<NotificationR> Notifications`: Lista de notificações
- `bool IsValid()`: Verifica se não há notificações
- `void AddNotification(string property, string message, string aggregateId = null)`: Adiciona notificação
- `void AddNotifications(IList<NotificationR> notifications)`: Adiciona múltiplas notificações
- `void RemoveNotification(string property)`: Remove notificações de uma propriedade
- `void RemoveNotifications(bool removeAll = true)`: Remove todas as notificações
- `IList<NotificationR> LoggerNotifications(ILogger logger, string logDescription)`: Log automático das notificações

#### NotificationR
- `string Property`: Nome da propriedade
- `string Message`: Mensagem de erro
- `string AggregatorId`: ID do agregado
- `DateTimeOffset DateOccurrence`: Data/hora da notificação

### String Extensions

#### StringExtensionMethods
- `string RemoveSpecialChars(this string text)`: Remove caracteres especiais ASCII
- `string GetLettersAndNumbersOnly(this string text)`: Mantém apenas letras e números
- `string GetNumbers(this string text)`: Extrai apenas números
- `string RemoveAccent(this string text)`: Remove acentos
- `string ToTitleCase(this string text)`: Converte para Title Case
- `string ToUpperNotNull(this string value)`: ToUpper seguro (null-safe)
- `string ToLowerNotNull(this string value)`: ToLower seguro
- `string TrimNotNull(this string value)`: Trim seguro

### Enum Extensions

#### EnumExtensionMethods
- `string GetDescription(this Enum GenericEnum)`: Obtém descrição do enum
- `int ToEnumNumero<T>(this string value)`: Converte string para número do enum
- `string ToEnumTexto<T>(this int numero)`: Converte número para texto do enum
- `bool IsEnum<T>(this string value, out int result)`: Verifica se string é enum válido

### Domain Entity

#### DomainEntity
- `string Id`: Identificador único (GUID)
- `DateTimeOffset DataCadastro`: Data de cadastro
- `string UsuarioCadastro`: Usuário que cadastrou
- `DateTimeOffset? DataAlteracao`: Data da última alteração
- `string UsuarioAlteracao`: Usuário que alterou
- `override bool Equals(object obj)`: Comparação por Id
- `override int GetHashCode()`: Hash baseado no tipo e Id

### JSON Extensions

#### NullToDefaultValueConverter
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
