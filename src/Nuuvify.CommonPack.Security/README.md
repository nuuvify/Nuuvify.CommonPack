# Nuuvify.CommonPack.Security

Biblioteca de segurança para aplicações ASP.NET Core que centraliza setup de autenticação, autorização e acesso às claims do usuário autenticado.

O pacote principal reúne utilitários para cenários com JWT e OpenID, além de contratos usados pelos pacotes complementares `Nuuvify.CommonPack.Security.JwtCredentials` e `Nuuvify.CommonPack.Security.JwtStore.Ef`.

## O que o pacote oferece

- setup de autenticação JWT via `AddSecuritySetup`
- setup complementar para fluxos OpenID via `AddOpenIdSecuritySetup`
- handlers de autorização para políticas e validação por claims
- helper `IUserAuthenticated` para leitura do usuário autenticado, claims e papéis
- opções de token centralizadas em `JwtTokenOptions`

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