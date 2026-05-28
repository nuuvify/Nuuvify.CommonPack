# Nuuvify.CommonPack.Security.JwtCredentials

Pacote para gerenciamento de credenciais JWK/JWKS usadas na assinatura e distribuição de chaves para JWT.

Ele fornece o builder principal para registrar serviços de geração e armazenamento de chaves, além de opções para cache de tokens e persistência do conjunto de chaves.

## O que o pacote oferece

- registro do builder via `AddJwksManager`
- configuração de opções em `JwksOptions`
- geração e gerenciamento de JWK/JWKS
- persistência de chaves em store em memória por padrão
- integração opcional com cache distribuído SQL Server para tokens
- suporte a múltiplos algoritmos de assinatura, incluindo HMAC, RSA, RSA-PSS e ECDSA

## Quando usar

Use este pacote quando a aplicação emissora de tokens precisar:

- gerenciar chaves de assinatura como JWK/JWKS
- expor ou rotacionar chaves de forma controlada
- centralizar as opções de algoritmo e cache do conjunto de chaves
- integrar armazenamento de token com cache distribuído

## Registro básico

O ponto de entrada principal é `AddJwksManager`.

```csharp
using Nuuvify.CommonPack.Security.JwtCredentials;

builder.Services
	.AddJwksManager(options =>
	{
		options.Algorithm = Algorithm.ES256;
		options.AlgorithmsToKeep = 2;
		options.CacheTime = TimeSpan.FromMinutes(15);
	})
	.PersistKeysInCache();
```

Esse fluxo registra serviços como `IJwkService`, `IJwkSetService` e um `IJwkStore` em memória.

## JwksOptions

As opções públicas observáveis incluem:

- `Algorithm`: algoritmo principal usado para geração/assinatura. O padrão é `Algorithm.ES256`.
- `AlgorithmsToKeep`: quantidade de versões/chaves mantidas.
- `CacheTime`: tempo de cache do conjunto de chaves.

## Algoritmos suportados

O pacote expõe algoritmos prontos por meio da classe `Algorithm`, incluindo combinações comuns como:

- `HS256`, `HS384`, `HS512`
- `RS256`, `RS384`, `RS512`
- `PS256`, `PS384`, `PS512`
- `ES256`, `ES384`, `ES512`

Escolha o algoritmo conforme o tipo de chave e o contrato esperado pelos consumidores do token.

## Cache de token com SQL Server

Para usar cache distribuído baseado em SQL Server, o builder expõe `CacheTokenSetup`.

```csharp
builder.Services
	.AddJwksManager()
	.CacheTokenSetup(
		connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
		schemaName: "cache",
		tableName: "Tokens");
```

Esse setup registra `IJwtSetService` e um `IDistributedCache` com chave nomeada `SqlServerCache`.

### Estrutura esperada da tabela de cache

Quando usar a persistência em SQL Server, a tabela de tokens deve existir previamente no banco, no formato esperado pelo `SqlServerCache`.

## Observações de uso

- este pacote é mais adequado para a aplicação que emite tokens ou gerencia chaves
- o store padrão é em memória; persistência mais robusta pode ser adicionada por pacote complementar
- mudanças em algoritmo, retenção de chaves e tempo de cache podem afetar compatibilidade operacional com consumidores

## Pacotes relacionados

- `Nuuvify.CommonPack.Security`: setup base de autenticação e autorização
- `Nuuvify.CommonPack.Security.JwtStore.Ef`: persistência de chaves e cache de tokens com Entity Framework

## Validação recomendada ao alterar este pacote

- algoritmo configurado corretamente
- geração e recuperação de JWK/JWKS
- retenção de chaves conforme `AlgorithmsToKeep`
- cache distribuído funcionando com a tabela esperada
- compatibilidade do conjunto de chaves com consumidores do token