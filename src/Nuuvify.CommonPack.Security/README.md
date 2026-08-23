# Nuuvify.CommonPack.Security

Biblioteca de segurança para aplicações ASP.NET Core que centraliza setup de autenticação, autorização e acesso às claims do usuário autenticado.

O pacote principal reúne utilitários para cenários com JWT e OpenID, além de contratos usados pelos pacotes complementares `Nuuvify.CommonPack.Security.JwtCredentials` e `Nuuvify.CommonPack.Security.JwtStore.Ef`.

## O que o pacote oferece

- setup de autenticação JWT via `AddSecuritySetup`
- setup complementar para fluxos OpenID via `AddOpenIdSecuritySetup`
- handlers de autorização para políticas e validação por claims
- helper `IUserAuthenticated` para leitura do usuário autenticado, claims e papéis
- opções de token centralizadas em `JwtTokenOptions`
- autenticação por API key via esquema `ApiKey`

## Quando usar

Use este pacote quando a aplicação precisar:

- validar tokens JWT emitidos por uma autoridade conhecida
- configurar autenticação e autorização de forma padronizada no container de DI
- acessar claims e informações do usuário atual sem espalhar dependência de `HttpContext`
- integrar fluxos baseados em OpenID e transformação adicional de claims

## Configuração JWT

O ponto de entrada principal para JWT é a extensão `AddSecuritySetup`.

```csharp
using Nuuvify.CommonPack.Security.Jwt;

builder.Services.AddSecuritySetup(builder.Configuration);
```

Por padrão, o método lê a seção `JwtTokenOptions`, registra `IUserAuthenticated`, `IHttpContextAccessor` e configura `JwtBearer` com validação de emissor, audiência, chave de assinatura e expiração.

### Exemplo de configuração

```json
{
 "JwtTokenOptions": {
  "Issuer": "nuuvify-auth",
  "Audience": "nuuvify-api",
  "SecretKey": "uma-chave-com-pelo-menos-32-caracteres-seguros"
 }
}
```

## Configuração OpenID

Para cenários OpenID, o pacote expõe `AddOpenIdSecuritySetup`, que registra os componentes necessários para autorização, transformação de claims e acesso ao usuário autenticado.

```csharp
using Nuuvify.CommonPack.Security.JwtOpenId;

builder.Services.AddOpenIdSecuritySetup(builder.Configuration);
```

Esse setup complementa a infraestrutura de autenticação já existente na aplicação e adiciona os serviços auxiliares usados pelos handlers do pacote.

## Configuração de API key

O esquema `ApiKey` pode ser registrado quando a aplicação precisa validar uma
credencial em um header HTTP dedicado:

```csharp
using Nuuvify.CommonPack.Security;

builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
 .AddApiKeyAuthentication(options =>
 {
  options.HeaderName = "X-API-Key";
  options.ValidKeys = new[] { "valor-carregado-de-um-secret-manager" };
 });
```

O handler retorna `NoResult` quando o header não está presente e falha com uma
mensagem genérica quando a credencial é inválida. A claim emitida identifica o
header utilizado; o valor secreto nunca é copiado para claims, logs ou respostas.

## Migração da API Legada

### De Attribute (`[ApiKey]`) para Scheme (`AddApiKeyAuthentication`)

A implementação legada de atributo é mantida para compatibilidade, mas será removida
em uma versão futura. As aplicações devem migrar para o novo esquema de autenticação
baseado em ASP.NET Core `AuthenticationScheme`.

#### Código legado (deprecado)

```csharp
[ApiKey(KeyName = new[] { "MyApiKey" })]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}

app.UseHttpRequestKeyVerifyMiddleware("x-api-key", StatusCodes.Status401Unauthorized);
```

#### Novo código (canônico)

```csharp
builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
    .AddApiKeyAuthentication(options =>
    {
        options.HeaderName = "X-API-Key";
        options.ValidKeys = new[] { "secret-key-from-vault" };
    });

builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();

[Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
```

#### Período de transição

Durante a transição, o `ApiKeyFilter` (legado) registra dois claims ao mesmo tempo:

- `ApiKeyInfo` (legado)
- `urn:nuuvify:security:api-key` (canônico)

Isso permite que consumidores migrem gradualmente sem perder funcionalidade. A estratégia
será removida em uma versão futura.

### Integração com OpenAPI / Swagger

Para documentar endpoints protegidos por API key no Swagger/OpenAPI, registre o esquema
na configuração de `SwaggerGen` da sua aplicação:

```csharp
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Autenticação por chave de API"
    };

    options.AddSecurityDefinition("ApiKey", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { ApiKeyAuthenticationDefaults.AuthenticationScheme } }
    });
});
```

A biblioteca Security não adiciona dependência de Swashbuckle para preservar a separação
de responsabilidades: a biblioteca fornece o padrão de autenticação, enquanto a aplicação
consumidora é responsável pela integração OpenAPI quando necessário.

## Acesso ao usuário autenticado

O contrato `IUserAuthenticated` permite consultar o usuário atual, autenticação, claims e papéis sem espalhar leitura direta de `HttpContext`.

Exemplos comuns:

- verificar se o usuário está autenticado
- recuperar o login atual
- ler uma claim específica
- verificar pertença a papel ou grupo

## JwtTokenOptions

`JwtTokenOptions` centraliza as opções usadas na validação e emissão de tokens. Entre os campos mais relevantes estão:

- `Issuer`
- `Audience`
- `SecretKey`
- `NotBefore`
- `ValidFor`
- `Expiration`

O pacote exige chave simétrica válida e trata tempo de expiração com `ClockSkew` zerado no setup JWT padrão.

## Observações de segurança

- mantenha `SecretKey` fora do código-fonte e prefira secret manager, vault ou configuração segura do ambiente
- trate mudanças em emissor, audiência, claims obrigatórias e expiração como mudanças de contrato para consumidores
- não enfraqueça validações de token sem teste explícito e análise de impacto
- evite expor detalhes sensíveis de autenticação em logs e mensagens de erro

## Pacotes relacionados

- `Nuuvify.CommonPack.Security.JwtCredentials`: suporte complementar para credenciais JWT
- `Nuuvify.CommonPack.Security.JwtStore.Ef`: persistência de dados de JWT com Entity Framework

## Validação recomendada ao alterar este pacote

- cenários de token válido e inválido
- expiração e audiência incorreta
- claims esperadas e autorização negada
- ausência de vazamento de segredo ou detalhe sensível

