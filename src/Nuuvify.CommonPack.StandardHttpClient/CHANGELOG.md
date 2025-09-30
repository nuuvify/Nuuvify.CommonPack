# Nuuvify.CommonPack - Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

## [2.1.0] - 2025-09-29

### StandardHttpClient - Performance & Resource Management

#### ✨ Adicionado
- **ConfigureAwait(false)**: Implementado em todas as chamadas `await` para otimização de performance
- **Resource Management**: Padrão `using var` para `HttpRequestMessage` e `StringContent`
- **Memory Leak Prevention**: Disposal automático de recursos `IDisposable`

#### 🔧 Melhorado
- **Performance**: Eliminação de context switching desnecessário
- **Escalabilidade**: Melhor utilização de threads em alta concorrência
- **Qualidade**: Compliance com regras de análise estática (CA2000)

#### 🛠️ Modificado
- Todos os HTTP Verbs (GET, POST, PUT, PATCH, DELETE)
- `StandardHttpClientService` com enhanced `IDisposable`
- `TokenService` e Polly policies com `ConfigureAwait(false)`

## [2.0.0] - 2025-01-01

## [1.0.0] - 2025-07-09

