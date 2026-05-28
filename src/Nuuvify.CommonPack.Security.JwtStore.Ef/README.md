# Nuuvify.CommonPack.Security.JwtStore.Ef

Pacote de persistência para chaves e cache de tokens usando Entity Framework Core em conjunto com `Nuuvify.CommonPack.Security.JwtCredentials`.

Ele fornece extensões para trocar o store em memória por armazenamento em banco e para persistir o cache de tokens em um contexto EF compatível.

## O que o pacote oferece

- persistência de chaves JWK em banco de dados via `PersistKeysToDatabaseStore<TContext>`
- persistência de cache de tokens via `PersistCacheTokenToDatabaseStore<TContext>`
- contratos de contexto para chaves e tokens: `ISecurityKeyContext` e `IJwtCacheContext`
- integração com `IJwksBuilder` do pacote `JwtCredentials`

## Quando usar

Use este pacote quando a aplicação emissora de tokens precisar:

- persistir chaves privadas/públicas em banco via Entity Framework Core
- persistir tokens em cache de banco em vez de depender apenas de memória
- centralizar armazenamento de chaves e cache em um `DbContext` controlado pela aplicação

## Pré-requisito

Este pacote depende do fluxo iniciado por `AddJwksManager` do pacote `Nuuvify.CommonPack.Security.JwtCredentials`.

## Persistência de chaves no banco

Para persistir chaves, use `PersistKeysToDatabaseStore<TContext>` com um contexto que implemente `ISecurityKeyContext`.

```csharp
builder.Services
	.AddJwksManager()
	.PersistKeysToDatabaseStore<MySecurityDbContext>();
```

O contexto precisa expor:

```csharp
public class MySecurityDbContext : DbContext, ISecurityKeyContext
{
	public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
}
```

## Persistência do cache de tokens no banco

Para persistir tokens, use `PersistCacheTokenToDatabaseStore<TContext>` com um contexto que implemente `IJwtCacheContext`.

```csharp
builder.Services
	.AddJwksManager()
	.PersistCacheTokenToDatabaseStore<MyTokenDbContext>();
```

O contexto precisa expor:

```csharp
public class MyTokenDbContext : DbContext, IJwtCacheContext
{
	public DbSet<JwtCacheToken> Tokens { get; set; }
}
```

## Exemplo combinando os dois stores

Quando o mesmo contexto atender ambos os contratos, o builder pode ser encadeado:

```csharp
builder.Services
	.AddJwksManager(options =>
	{
		options.Algorithm = Algorithm.ES256;
	})
	.PersistKeysToDatabaseStore<MySecurityDbContext>()
	.PersistCacheTokenToDatabaseStore<MySecurityDbContext>();
```

## Comportamento observável do store de tokens

O store de tokens trabalha com operações de:

- leitura por usuário e tipo de cache
- gravação de token serializado com expiração absoluta
- remoção por usuário
- limpeza global do cache persistido

O pacote usa `IDistributedCache` resolvido por chave e aplica expiração baseada no campo `Expires` do token persistido.

## Observações de implementação

- este pacote é destinado principalmente à aplicação que gera e controla o ciclo de vida dos tokens
- os contextos EF devem mapear corretamente `SecurityKeyWithPrivate` e `JwtCacheToken`
- mudanças no schema dessas entidades devem ser tratadas como mudança operacional relevante
- a persistência substitui o store em memória do fluxo padrão quando registrada no container

## Pacotes relacionados

- `Nuuvify.CommonPack.Security.JwtCredentials`: builder e serviços centrais de JWK/JWKS
- `Nuuvify.CommonPack.Security`: setup base de autenticação e autorização

## Validação recomendada ao alterar este pacote

- resolução correta do store via DI
- persistência e leitura de chaves no contexto EF
- persistência, leitura e remoção de tokens por usuário
- expiração aplicada ao cache do token
- compatibilidade do contexto EF com os contratos `ISecurityKeyContext` e `IJwtCacheContext`