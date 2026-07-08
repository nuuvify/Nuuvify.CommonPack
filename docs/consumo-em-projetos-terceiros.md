# Guia Central - Consumo do Nuuvify.CommonPack em Projetos de Terceiros

Este guia centraliza o passo a passo para adotar os pacotes do Nuuvify.CommonPack em aplicações externas, com foco em integração segura, previsível e de baixo acoplamento.

## Objetivo

- Padronizar a adoção em projetos terceiros.
- Evitar configuração incompleta de DI, credenciais e resiliência.
- Reduzir tempo de setup com um fluxo único de onboarding.

## Pré-requisitos

- .NET 8.0 SDK instalado.
- Projeto terceiro com suporte a DI via Microsoft.Extensions.DependencyInjection.
- Acesso ao feed NuGet onde os pacotes estão publicados (nuget.org ou feed privado).
- Credenciais e variáveis de ambiente definidas para os serviços externos consumidos.

## Fluxo Recomendado de Adoção

1. Escolher apenas os pacotes necessários para o caso de uso.
2. Instalar pacotes no projeto de entrada (API, Worker ou Application).
3. Registrar serviços no Program.cs com métodos de setup do pacote.
4. Configurar appsettings e segredos externos.
5. Validar healthcheck/build/test local antes de promover para ambiente superior.

## Matriz de Pacotes por Cenário

### Comunicação HTTP resiliente

- Pacote principal: Nuuvify.CommonPack.StandardHttpClient
- Quando usar: integrações REST/SOAP, chamadas externas com retry e token.

### Envio de e-mail

- Pacotes: Nuuvify.CommonPack.Email e Nuuvify.CommonPack.Email.Abstraction
- Quando usar: envio SMTP com anexos e múltiplos destinatários.

### Mensageria Azure Service Bus

- Pacotes: Nuuvify.CommonPack.AzureServiceBus, Nuuvify.CommonPack.AzureServiceBus.Abstraction
- Quando usar: publicação e consumo em queue/topic com políticas de resiliência.

### Processamento em Background

- Pacote: Nuuvify.CommonPack.BackgroundService
- Quando usar: workers com controle de concorrência, retry e dead-letter.

### Persistência e consultas

- Pacotes: Nuuvify.CommonPack.UnitOfWork e Nuuvify.CommonPack.UnitOfWork.Abstraction
- Quando usar: composição de queries e persistência com padrão Unit of Work.

### Segurança

- Pacotes: Nuuvify.CommonPack.Security, Nuuvify.CommonPack.Security.Abstraction, Nuuvify.CommonPack.Security.JwtCredentials, Nuuvify.CommonPack.Security.JwtStore.Ef
- Quando usar: autenticação/autorização e armazenamento de contexto de segurança.

### MFT Mailbox (SFTP/HTTPS)

- Pacotes: Nuuvify.CommonPack.MftMailbox.Abstraction, Nuuvify.CommonPack.MftMailbox, Nuuvify.CommonPack.MftMailbox.Sftp, Nuuvify.CommonPack.MftMailbox.Http
- Quando usar: transferência de arquivos com streaming, idempotência, status e ACK/NACK.

## Instalação

Exemplo de instalação para um cenário de API com HTTP resiliente e segurança:

```bash
dotnet add package Nuuvify.CommonPack.StandardHttpClient
dotnet add package Nuuvify.CommonPack.Security.Abstraction
```

Exemplo de instalação para cenário de Worker com MFT Mailbox:

```bash
dotnet add package Nuuvify.CommonPack.MftMailbox.Abstraction
dotnet add package Nuuvify.CommonPack.MftMailbox
dotnet add package Nuuvify.CommonPack.MftMailbox.Sftp
dotnet add package Nuuvify.CommonPack.MftMailbox.Http
```

## Configuração Base no Program.cs

## Padrão de Registro

```csharp
using Nuuvify.CommonPack.StandardHttpClient;
using Nuuvify.CommonPack.MftMailbox;
using Nuuvify.CommonPack.MftMailbox.Sftp;
using Nuuvify.CommonPack.MftMailbox.Http;

var builder = WebApplication.CreateBuilder(args);

// Exemplo: HTTP resiliente
builder.Services.AddStandardHttpClientSetup(builder.Configuration);

// Exemplo: Núcleo MFT + adapters
builder.Services.AddMftMailboxCore(options =>
{
    options.MaxBatchSize = 500;
    options.MaxFileSizeBytes = 1_073_741_824;
});

builder.Services.AddMftMailboxSftp(options =>
{
    options.Host = "sftp.host.local";
    options.Port = 22;
    options.Username = "user";
    options.PrivateKeyPath = "/secrets/id_rsa";
    options.OutboundDirectory = "/outbound";
    options.InboundDirectory = "/inbound";
});

builder.Services.AddMftMailboxHttp(options =>
{
    options.BaseUrl = "https://mft.example.com";
    options.UploadPath = "/mailbox/upload";
    options.DownloadPath = "/mailbox/download";
    options.StatusPath = "/mailbox/status";
    options.AckNackPath = "/mailbox/acknack";
});

var app = builder.Build();
```

## Configuração de AppSettings

Use placeholders seguros e mantenha segredos fora do código.

```json
{
  "AppConfig": {
    "AppURLs": {
      "UrlLoginApi": "https://api.exemplo.com",
      "UrlLoginApiToken": "/auth/token"
    }
  },
  "ApisCredentials": {
    "Username": "${API_USERNAME}",
    "Password": "${API_PASSWORD}"
  },
  "AzureAdOpenID": {
    "cc": {
      "ClientId": "${CLIENT_ID}",
      "ClientSecret": "${CLIENT_SECRET}"
    }
  }
}
```

## Padrão de Segredos

- Não versionar secrets em appsettings.
- Preferir variáveis de ambiente, Key Vault ou secret manager do ambiente.
- Em pipelines, injetar valores no deploy e não em tempo de build.

## Exemplo de Uso - StandardHttpClient

```csharp
public sealed class CustomerGateway
{
    private readonly IStandardHttpClient _httpClient;

    public CustomerGateway(IStandardHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpStandardReturn> CreateAsync(object payload, CancellationToken ct)
    {
        _httpClient.CreateClient("CustomerApi");

        return await _httpClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithQueryString("source", "third-party")
            .Post("api/customers", payload, ct);
    }
}
```

## Exemplo de Uso - MFT com Factory por Protocolo

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class MftOutboundService
{
    private readonly IMftClientFactory _factory;

    public MftOutboundService(IMftClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<TransferItemResult> SendAsync(Stream content, CancellationToken ct)
    {
        var client = _factory.CreateTransferClient(MftProtocol.Sftp);

        var envelope = new TransferEnvelope
        {
            IntegrationKey = "erp-files",
            CorrelationId = Guid.NewGuid().ToString("N"),
            Protocol = MftProtocol.Sftp,
            Items = new List<TransferItem>
            {
                new()
                {
                    ItemId = Guid.NewGuid().ToString("N"),
                    FileName = "orders.csv",
                    ContentFactory = _ => Task.FromResult<Stream>(content)
                }
            }
        };

        return await client.SendSingleAsync(envelope, ct);
    }
}
```

## Boas Práticas para Projetos Terceiros

- Referenciar abstrações no domínio/aplicação e implementações no entrypoint.
- Propagar CancellationToken em toda a cadeia.
- Usar logs estruturados e correlationId em integrações externas.
- Tratar resultado por item em lote para evitar perda de rastreabilidade.
- Manter limites explícitos de concorrência, timeout e tamanho de arquivo.

## Checklist de Go-Live

- Build e testes passando no projeto terceiro.
- Credenciais e segredos vindos de provider seguro.
- Retry, timeout e circuit breaker revisados para o SLA da integração.
- Observabilidade ativa (logs, métricas, rastreio de correlationId).
- Plano de rollback definido para mudanças de versão de pacote.

## Compatibilidade e Versionamento

- O repositório segue Semantic Versioning.
- Atualizações de major version devem ser revisadas com janela de migração.
- Para upgrades, ler CHANGELOG raiz e CHANGELOG de cada pacote usado.

## Referências

- Readme raiz do repositório: ../Readme.md
- Exemplo de pacote HTTP: ../src/Nuuvify.CommonPack.StandardHttpClient/README.md
- Exemplo de pacote MFT core: ../src/Nuuvify.CommonPack.MftMailbox/README.md
- Exemplo de pacote MFT SFTP: ../src/Nuuvify.CommonPack.MftMailbox.Sftp/README.md
- Exemplo de pacote MFT HTTP: ../src/Nuuvify.CommonPack.MftMailbox.Http/README.md
