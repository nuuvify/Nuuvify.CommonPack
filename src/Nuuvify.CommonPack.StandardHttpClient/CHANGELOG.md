# Changelog - Nuuvify.CommonPack.StandardHttpClient

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

## [Sem versão registrada] - 2025-09-30

### Alterado
- Sistema de notificações implementado no BaseStandardHttpClient, incluindo coleta de notificações do TokenService.
- Otimizações de desempenho com ConfigureAwait(false) e melhorias de gerenciamento de recursos IDisposable.

### Corrigido
- Correções na infraestrutura de testes unitários com mock de HttpClient, melhorando estabilidade e previsibilidade da suíte.
