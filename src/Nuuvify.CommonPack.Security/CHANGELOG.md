# Changelog - Nuuvify.CommonPack.Security

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Alterado

- Dependências atualizadas na linha compatível com .NET 8: `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.32, `Microsoft.AspNetCore.Authentication.OpenIdConnect` 8.0.32, `Microsoft.AspNetCore.Authorization` 8.0.32, `Microsoft.Extensions.Caching.SqlServer` 8.0.32, `System.Text.Json` 8.0.6.

## [2.9.0] - 2026-08-26

### Adicionado

- Validação de startup para header e credenciais do esquema de API key.
- Esquema de autenticação por API key com comparação em tempo constante.
- Registro de API key via `AddApiKeyAuthentication`.
- Claim canônica `urn:nuuvify:security:api-key` para autorização por esquema.

### Alterado

- A integração OpenAPI permanece responsabilidade da aplicação consumidora, sem dependência de Swashbuckle no pacote Security.

### Corrigido

- Testes de autenticação passaram a usar comparação explícita e configuração determinística.

### Segurança

- impedir que a credencial validada seja materializada em claims

## [Sem versão registrada] - 2026-05-29

### Histórico

- Estrutura inicial do changelog padronizada para este pacote.
