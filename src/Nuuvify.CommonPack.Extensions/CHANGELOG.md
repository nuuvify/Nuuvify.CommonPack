# Changelog - Nuuvify.CommonPack.Extensions

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado
- Geração automática de Id em `DomainEntity` atualizada para UUID orientado a banco de dados (UUID v7), visando melhor ordenação e redução de page split em cenários persistidos.
- Inclusão da dependência `UUIDNext` para suportar a nova estratégia de geração de Id.
- Ampliação da cobertura de testes do módulo de logging (`NuuvifyLogColor`, `NuuvifyLogColorConfiguration` e `TextWriterExtensions`) sem alteração de API pública.
- `AddCustomFormatter` passou a oferecer um overload aditivo para customizar `ConsoleLoggerOptions` sem substituir o formatter do pacote.

### Corrigido
- `NuuvifyLogFormatter` passou a descartar corretamente os dois tokens de recarga observados pelos `IOptionsMonitor`, evitando retenção indevida de callbacks em cenários de bootstrap e reload de configuração.

### Removido

### Segurança

## [Sem versão registrada] - 2026-05-29

### Adicionado
- Estrutura inicial do changelog padronizada para este pacote.
