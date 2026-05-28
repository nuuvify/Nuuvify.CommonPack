# Arquitetura

Este documento resume a organização arquitetural do Nuuvify.CommonPack para facilitar manutenção e contribuição.

## Visão geral

O repositório agrupa bibliotecas .NET independentes, mas coerentes entre si, publicadas como pacotes reutilizáveis. O desenho favorece:

- separação de responsabilidades
- APIs pequenas e orientadas a domínio
- abstrações por pacote quando necessário
- documentação e changelog por pacote

## Blocos principais

### Base de domínio e persistência

- `Nuuvify.CommonPack.Domain`
- `Nuuvify.CommonPack.UnitOfWork`
- `Nuuvify.CommonPack.UnitOfWork.Abstraction`
- `Nuuvify.CommonPack.AutoHistory`

Esses pacotes concentram padrões de domínio, persistência, filtros dinâmicos e suporte a cenários com Entity Framework.

### Integração e infraestrutura

- `Nuuvify.CommonPack.AzureServiceBus`
- `Nuuvify.CommonPack.AzureServiceBus.Abstraction`
- `Nuuvify.CommonPack.AzureStorage`
- `Nuuvify.CommonPack.AzureStorage.Abstraction`
- `Nuuvify.CommonPack.BackgroundService`
- `Nuuvify.CommonPack.StandardHttpClient`
- `Nuuvify.CommonPack.Email`
- `Nuuvify.CommonPack.Email.Abstraction`

Esses pacotes encapsulam integrações com mensageria, storage, envio de e-mail, comunicação HTTP resiliente e workers.

### Segurança, APIs e extensões

- `Nuuvify.CommonPack.Security`
- `Nuuvify.CommonPack.Security.Abstraction`
- `Nuuvify.CommonPack.Security.JwtCredentials`
- `Nuuvify.CommonPack.Security.JwtStore.Ef`
- `Nuuvify.CommonPack.Middleware`
- `Nuuvify.CommonPack.Middleware.Abstraction`
- `Nuuvify.CommonPack.OpenApi`
- `Nuuvify.CommonPack.HealthCheck`
- `Nuuvify.CommonPack.Extensions`

Esses pacotes concentram autenticação, middlewares, OpenAPI, health checks e utilidades compartilhadas.

### Tratamento de exceções por provedor

- `Nuuvify.CommonPack.EF.Exceptions.Common`
- `Nuuvify.CommonPack.EF.Exceptions.Db2`
- `Nuuvify.CommonPack.EF.Exceptions.Oracle`

Esses pacotes especializam comportamento por tecnologia sem contaminar os módulos centrais.

## Convenções estruturais

- Cada pacote pode ter `README.md` e `CHANGELOG.md` próprios.
- Metadados comuns e empacotamento ficam em `src/Directory.Build.props`.
- Testes compartilham dependências comuns em `test/Directory.Build.props`.
- O target framework padrão é `net8.0`, com possibilidade de override local.

## Princípios de evolução

- Favorecer compatibilidade pública entre versões.
- Isolar integrações externas atrás de abstrações quando fizer sentido.
- Evitar acoplamento desnecessário entre pacotes independentes.
- Preferir mudanças incrementais com documentação e teste junto da implementação.

## Onde olhar primeiro ao contribuir

- `Readme.md` para visão geral pública
- `src/<Pacote>/README.md` para uso e comportamento do pacote
- `src/<Pacote>/CHANGELOG.md` para histórico local
- `test/` para convenções de validação
