# Nuuvify.CommonPack.StandardHttpClient

[![Build Status - QAS](https://dev.azure.com/nuuvify/Nuuvify.CommonPack/_apis/build/status/Nuuvify.CommonPack?branchName=qas)](https://dev.azure.com/nuuvify/Nuuvify.CommonPack/_build/latest?definitionId=1&branchName=qas)
[![Build Status - Main](https://dev.azure.com/nuuvify/Nuuvify.CommonPack/_apis/build/status/Nuuvify.CommonPack?branchName=main)](https://dev.azure.com/nuuvify/Nuuvify.CommonPack/_build/latest?definitionId=1&branchName=main)

Biblioteca para padronização de comunicação HTTP com APIs externas, incluindo gerenciamento de tokens, retry policies, e serialização padronizada.

## Índice

- [Funcionalidades](#funcionalidades)
- [Dependências](#dependências)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Uso Básico](#uso-básico)
  - [IStandardHttpClient](#istandardhttpclient)
  - [BaseStandardHttpClient](#basestandardhttpclient)
  - [TokenService](#tokenservice)
  - [StandardWebService](#standardwebservice)
- [Exemplos Práticos](#exemplos-práticos)
- [Configurações Avançadas](#configurações-avançadas)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

## Funcionalidades

- ✅ **Comunicação HTTP padronizada** com APIs REST
- ✅ **Gerenciamento automático de tokens** com renovação inteligente
- ✅ **Retry policies com Polly** para resiliência (retry, circuit breaker, fallback)
- ✅ **Renovação automática de tokens expirados** em caso de HTTP 401
- ✅ **Circuit Breaker pattern** para proteção contra cascata de falhas
- ✅ **Serialização JSON configurável** com conversores customizados
- ✅ **Suporte a SOAP** para web services legados
- ✅ **Logging detalhado** para debugging e monitoramento
- ✅ **Correlation IDs** para rastreamento de requisições
- ✅ **Suporte a CancellationToken** para cancelamento gracioso
- ✅ **Headers customizáveis** e autenticação flexível
- ✅ **QueryString helper** com encoding automático
- ✅ **Upload de arquivos (multipart)** com progress tracking

## Dependências

### Framework
- **.NET 8.0**
- **Microsoft.AspNetCore.App** (FrameworkReference)

### Pacotes NuGet
- **Microsoft.Extensions.Http.Polly** (8.0.16) - Para retry policies e circuit breaker patterns
- **Nuuvify.CommonPack.Security.Abstraction** - Para gerenciamento de credenciais

> **ℹ️ Sobre a integração com Polly:** O TokenService utiliza a biblioteca Polly para implementar padrões de resiliência em comunicações HTTP, incluindo retry policies, circuit breaker e fallback handlers. Isso garante que as requisições de token sejam resilientes a falhas temporárias de rede.

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.StandardHttpClient" Version="x.x.x" />
```

## Configuração

### 1. Dependency Injection

```csharp
// Program.cs ou Startup.cs
using Nuuvify.CommonPack.StandardHttpClient;

// Configuração básica
builder.Services.AddStandardHttpClientSetup(builder.Configuration);

// OU configuração sem registro de credencial
builder.Services.AddStandardHttpClientSetup(builder.Configuration, registerCredential: false);

// OU como Singleton
builder.Services.AddStandardHttpClientSetupSingleton(builder.Configuration);

// OU como Transient
builder.Services.AddStandardHttpClientSetupAddTransient(builder.Configuration);

// Configuração com Polly para resiliência (exemplo avançado)
builder.Services.AddHttpClient<IStandardHttpClient, BaseStandardHttpClient>()
    .AddPolicyWithTokenHandlers<TokenService>(
        retryCount: 3,
        circuitBreakerExceptionsThreshold: 5,
        circuitBreakerDurationInSeconds: 30
    );
```

### 2. Configuração do appsettings.json

```json
{
  "AppConfig": {
    "AppURLs": {
      "UrlLoginApi": "https://api.exemplo.com",
      "UrlLoginApiToken": "/auth/token"
    }
  },
  "ApisCredentials": {
    "Username": "seu-usuario",
    "Password": "sua-senha"
  },
  "AzureAdOpenID": {
    "cc": {
      "ClientId": "seu-client-id",
      "ClientSecret": "seu-client-secret"
    }
  }
}
```

### 3. Configuração de HttpClient com Proxy (Opcional)

```csharp
services.AddServiceCredentialRegister(configuration, "CredentialApi")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        Proxy = new WebProxy("http://proxy:8080"),
        UseProxy = true
    });
```

## Uso Básico

### IStandardHttpClient

Interface principal para comunicação HTTP direta:

```csharp
public class ExemploService
{
    private readonly IStandardHttpClient _httpClient;

    public ExemploService(IStandardHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpStandardReturn> ChamarApiAsync(CancellationToken cancellationToken = default)
    {
        _httpClient.CreateClient("MeuClient");

        var dados = new { Nome = "João", Idade = 30 };

        var resultado = await _httpClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithAuthorization("Bearer", "meu-token")
            .WithCurrelationHeader(Guid.NewGuid().ToString())
            .WithQueryString("page", 1)
            .Post("api/usuarios", dados, cancellationToken);

        return resultado;
    }
}
```

### BaseStandardHttpClient

Classe base para implementação de clientes HTTP específicos com serialização automática:

```csharp
public class MeuClienteApi : BaseStandardHttpClient
{
    public MeuClienteApi(IStandardHttpClient standardHttpClient, ITokenService tokenService)
        : base(standardHttpClient, tokenService)
    {
    }

    public async Task<Usuario> ObterUsuarioAsync(int id, CancellationToken cancellationToken = default)
    {
        var token = await ObterTokenAsync(cancellationToken);

        var resultado = await ExecuteWithTokenAsync<Usuario>(
            httpClient => httpClient
                .WithAuthorization("Bearer", token)
                .Get($"api/usuarios/{id}", cancellationToken)
        );

        return resultado.Data;
    }

    public async Task<List<Usuario>> ListarUsuariosAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        var token = await ObterTokenAsync(cancellationToken);

        var resultado = await ExecuteWithTokenAsync<List<Usuario>>(
            httpClient => httpClient
                .WithAuthorization("Bearer", token)
                .WithQueryString("page", page)
                .WithQueryString("limit", 10)
                .Get("api/usuarios", cancellationToken)
        );

        return resultado.Data;
    }
}
```

### TokenService

Gerenciamento automático de tokens de autenticação:

```csharp
public class MinhaApiService
{
    private readonly ITokenService _tokenService;

    public MinhaApiService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<string> ObterTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetToken(
            login: "meu-usuario",
            password: "minha-senha",
            userClaim: "usuario-atual",
            cancellationToken: cancellationToken
        );

        return token?.Token;
    }

    public async Task<bool> RenovarTokenAsync(CancellationToken cancellationToken = default)
    {
        return await _tokenService.GetNewToken(
            "https://api.exemplo.com/auth/token",
            "meu-usuario",
            "minha-senha",
            "usuario-atual",
            cancellationToken
        );
    }
}
```

### Resiliência com Polly

O **TokenService** integra-se nativamente com a biblioteca **Polly** para garantir comunicações HTTP resilientes. As políticas implementadas incluem:

#### 🔄 Retry Policies
- **Retry básico**: Tenta novamente em caso de falha temporária
- **Retry com renovação de token**: Renova automaticamente tokens expirados (HTTP 401)
- **Backoff exponencial**: Aumenta progressivamente o tempo entre tentativas

#### 🔌 Circuit Breaker
- **Proteção contra cascata de falhas**: Interrompe chamadas para serviços indisponíveis
- **Recuperação automática**: Testa periodicamente se o serviço voltou ao normal
- **Logs detalhados**: Registra quando o circuito abre/fecha

#### 🛡️ Fallback Handlers
- **Respostas alternativas**: Retorna dados cached ou padrão em caso de falha
- **Degradação graciosa**: Mantém funcionalidade básica mesmo com serviços indisponíveis

```csharp
// Configuração automática com Polly no HttpClient
services.AddServiceCredentialRegister(configuration, "CredentialApi")
    .AddPolicyWithTokenHandlers(services, retryTotal: 3, breakDurationMilliSeconds: 5000);

// OU para APIs externas (sem token)
services.AddHttpClient("ApiExterna", client =>
{
    client.BaseAddress = new Uri("https://api.externa.com");
})
.AddPolicyHandlers(services, retryTotal: 2, breakDurationMilliSeconds: 3000);
```

#### Cenários de Retry Automático

| Cenário                          | Ação da Política                                 |
| -------------------------------- | ------------------------------------------------ |
| **HTTP 401 (Unauthorized)**      | Renova token automaticamente e repete requisição |
| **HTTP 429 (Too Many Requests)** | Aguarda tempo recomendado e repete               |
| **HTTP 5xx (Server Error)**      | Retry com backoff exponencial                    |
| **Timeout de rede**              | Retry com intervalo crescente                    |
| **Circuit Breaker aberto**       | Fallback ou erro controlado                      |

#### Logs de Resiliência

```csharp
// O TokenService produz logs detalhados sobre retry policies:
// - "GetHttpResponseRetryPolicyWithToken Request with token failed with StatusCode: Unauthorized"
// - "Before ITokenService.GetToken" / "After ITokenService.GetToken"
// - "Service shutdown during: 00:00:05 after: 3 failed retries"
```

### StandardWebService

Para comunicação com web services SOAP:

```csharp
public class SoapService
{
    private readonly IStandardWebService _webService;

    public SoapService(IStandardWebService webService)
    {
        _webService = webService;
    }

    public async Task<HttpStandardXmlReturn> ChamarWebServiceAsync()
    {
        var soapEnvelope = new XmlDocument();
        soapEnvelope.LoadXml(@"
            <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                <soap:Body>
                    <MinhaOperacao>
                        <Parametro>Valor</Parametro>
                    </MinhaOperacao>
                </soap:Body>
            </soap:Envelope>
        ");

        _webService.CreateClient("SoapClient");

        var resultado = await _webService
            .WithHeader("SOAPAction", "MinhaOperacao")
            .WithCurrelationHeader(Guid.NewGuid().ToString())
            .RequestSoap("WebService.asmx", soapEnvelope);

        return resultado;
    }
}
```

## Exemplos Práticos

### GET com QueryString

```csharp
var resultado = await _httpClient
    .WithQueryString("nome", "João")
    .WithQueryString("idade", 30)
    .WithQueryString("ativo", true)
    .Get("api/usuarios", cancellationToken);
// URL: api/usuarios?nome=João&idade=30&ativo=true
```

### POST com JSON

```csharp
var usuario = new Usuario { Nome = "João", Email = "joao@exemplo.com" };

var resultado = await _httpClient
    .WithHeader("Content-Type", "application/json")
    .Post("api/usuarios", usuario, cancellationToken);
```

### PUT com autenticação

```csharp
var dadosAtualizacao = new { Nome = "João Silva" };

var resultado = await _httpClient
    .WithAuthorization("Bearer", token)
    .Put($"api/usuarios/{id}", dadosAtualizacao, cancellationToken);
```

### PATCH parcial

```csharp
var patch = new { Status = "Ativo" };

var resultado = await _httpClient
    .WithAuthorization("Bearer", token)
    .Patch($"api/usuarios/{id}", patch, cancellationToken);
```

### DELETE

```csharp
var resultado = await _httpClient
    .WithAuthorization("Bearer", token)
    .Delete($"api/usuarios/{id}", cancellationToken);
```

### Upload de arquivo

```csharp
using var fileContent = new StreamContent(fileStream);
using var multipartContent = new MultipartFormDataContent();
multipartContent.Add(fileContent, "arquivo", "documento.pdf");
multipartContent.Add(new StringContent("Descrição do arquivo"), "descricao");

var resultado = await _httpClient
    .WithAuthorization("Bearer", token)
    .Post("api/upload", multipartContent, "multipart/form-data", cancellationToken);
```

### Download de arquivo

```csharp
var stream = await _httpClient
    .WithAuthorization("Bearer", token)
    .GetStream("api/download/arquivo.pdf", cancellationToken);

await using var fileStream = File.Create("arquivo-local.pdf");
await stream.Data.CopyToAsync(fileStream, cancellationToken);
```

### Autenticação Basic

```csharp
var username = "usuario";
var password = "senha";
var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

var resultado = await _httpClient
    .WithAuthorization("Basic", credentials)
    .Get("api/dados", cancellationToken);
```

## Configurações Avançadas

### Retry Policy Personalizada

```csharp
// No Startup.cs
StandardHttpClientSetup.RetryTotal = 5;
StandardHttpClientSetup.BreakDurationMilliSeconds = 3000;

builder.Services.AddStandardHttpClientSetup(builder.Configuration);
```

### JsonSerializerOptions Customizado

```csharp
public class MeuClienteCustomizado : BaseStandardHttpClient
{
    public MeuClienteCustomizado(IStandardHttpClient standardHttpClient, ITokenService tokenService)
        : base(standardHttpClient, tokenService)
    {
        JsonSettings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
```

### Logging Detalhado

```csharp
_httpClient.LogRequest = true; // Ativa logging detalhado

var resultado = await _httpClient
    .WithCurrelationHeader("REQ-001") // Para rastreamento
    .Get("api/dados", cancellationToken);

Console.WriteLine($"Correlation ID: {_httpClient.CorrelationId}");
Console.WriteLine($"URL completa: {_httpClient.FullUrl}");
```

### Timeout Customizado

```csharp
_httpClient.CreateClient("ClienteCustom");
_httpClient.Configure(
    timeOut: TimeSpan.FromMinutes(5),
    maxResponseContentBufferSize: 1024 * 1024, // 1MB
    httpCompletionOption: HttpCompletionOption.ResponseContentRead
);
```

## API Reference

### IStandardHttpClient

| Método                                      | Descrição                     |
| ------------------------------------------- | ----------------------------- |
| `CreateClient(string)`                      | Cria instância do HttpClient  |
| `ResetStandardHttpClient()`                 | Limpa headers e configurações |
| `WithHeader(string, object)`                | Adiciona header customizado   |
| `WithAuthorization(string, string, string)` | Configura autenticação        |
| `WithQueryString(string, object)`           | Adiciona parâmetro à URL      |
| `WithCurrelationHeader(string)`             | Define ID de correlação       |
| `Get(string, CancellationToken)`            | Requisição GET                |
| `Post(string, object, CancellationToken)`   | Requisição POST               |
| `Put(string, object, CancellationToken)`    | Requisição PUT                |
| `Patch(string, object, CancellationToken)`  | Requisição PATCH              |
| `Delete(string, CancellationToken)`         | Requisição DELETE             |
| `GetStream(string, CancellationToken)`      | Download de arquivo           |

### ITokenService

| Método                                                           | Descrição                    |
| ---------------------------------------------------------------- | ---------------------------- |
| `GetToken(string, string, string, CancellationToken)`            | Obtém token de acesso        |
| `GetNewToken(string, string, string, string, CancellationToken)` | Força renovação do token     |
| `GetActualToken()`                                               | Retorna token atual          |
| `GetTokenAcessor()`                                              | Obtém token do contexto HTTP |
| `HttpClientTokenName(string)`                                    | Define nome do HttpClient    |

### HttpStandardReturn

| Propriedade                   | Tipo     | Descrição                      |
| ----------------------------- | -------- | ------------------------------ |
| `Success`                     | `bool`   | Indica sucesso da operação     |
| `ReturnCode`                  | `string` | Código de retorno HTTP         |
| `ReturnMessage`               | `string` | Conteúdo da resposta           |
| `GetReturnMessageWithoutRn()` | `string` | Limpa quebras de linha do JSON |

## Troubleshooting

### Problema: Token não é renovado automaticamente
**Solução:** Verifique se as configurações `AppConfig:AppURLs:UrlLoginApi` e `AppConfig:AppURLs:UrlLoginApiToken` estão corretas no appsettings.json.

### Problema: Erro de SSL/TLS
**Solução:** Configure o HttpClientHandler com as certificações apropriadas:
```csharp
services.AddServiceCredentialRegister(configuration)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true
    });
```

### Problema: Timeout em requisições longas
**Solução:** Configure timeout personalizado:
```csharp
_httpClient.Configure(TimeSpan.FromMinutes(10));
```

### Problema: Problema de encoding em QueryString
**Solução:** Use `WithQueryString` que faz encoding automático:
```csharp
.WithQueryString("busca", "João & Maria") // Automaticamente encoded
```

### Problema: Headers não estão sendo enviados
**Solução:** Use `WithHeader` antes da chamada HTTP e certifique-se de não chamar `ResetStandardHttpClient()` depois:
```csharp
_httpClient.CreateClient();
// NÃO chame ResetStandardHttpClient() aqui
_httpClient.WithHeader("Custom-Header", "valor");
var result = await _httpClient.Get("api/dados");
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
