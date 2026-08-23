# Changelog - Nuuvify.CommonPack.Security

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

- Validação de startup para header e credenciais do esquema de API key.
- Esquema de autenticação por API key com comparação em tempo constante.
- Registro de API key via `AddApiKeyAuthentication`.
- Claim canônica `urn:nuuvify:security:api-key` para autorização por esquema.

### Alterado

- A integração OpenAPI permanece responsabilidade da aplicação consumidora, sem dependência de Swashbuckle no pacote Security.

### Corrigido

### Removido

### Segurança

- impedir que a credencial validada seja materializada em claims

## [Sem versão registrada] - 2026-05-29

### Histórico

- Estrutura inicial do changelog padronizada para este pacote.
