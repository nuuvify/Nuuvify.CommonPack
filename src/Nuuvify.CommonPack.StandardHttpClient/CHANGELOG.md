# Nuuvify.CommonPack - Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

## [2.1.0] - 2025-09-29

### 🚀 Performance & Resource Management

#### ✨ Recursos Adicionados
- **ConfigureAwait(false)**: Implementado em todos os métodos `await` para otimização de performance em bibliotecas
- **Resource Management**: Padrão `using var` aplicado a `HttpRequestMessage`, `StringContent` e recursos `IDisposable`
- **Memory Leak Prevention**: Sistema automático de disposal para prevenção de vazamentos de memória
- **Enhanced IDisposable**: Pattern aprimorado na classe `StandardHttpClient` para cleanup completo
- **CA2000 Compliance**: Conformidade total com regras de análise estática para gerenciamento de recursos

#### 🔧 Melhorias de Performance
- **Async Optimization**: Eliminação de context switching desnecessário com `ConfigureAwait(false)`
- **Thread Pool Efficiency**: Melhor utilização de threads em cenários de alta concorrência
- **Scalability**: Desempenho superior em aplicações server-side (ASP.NET Core, Web APIs)
- **Deadlock Prevention**: Prevenção de deadlocks em código que mistura sync/async

#### 🛠️ Componentes Modificados
- **HTTP Verbs**: Todos os métodos (GET, POST, PUT, PATCH, DELETE) otimizados
- **StandardHttpClientService**: Enhanced disposal pattern implementado
- **TokenService**: ConfigureAwait(false) aplicado em operações de token
- **Polly Policies**: Retry e Circuit Breaker policies otimizadas
- **StandardWebServicePrivate**: StringContent com proper disposal

#### 📊 Impacto Técnico
- **Memory Usage**: Redução significativa no consumo de memória
- **Response Times**: Tempos de resposta mais rápidos devido à otimização de contexto
- **Stability**: Maior estabilidade através de proper resource management
- **Code Quality**: Conformidade com best practices para Class Libraries .NET

## [2.0.0] - 2025-01-01

## [1.0.0] - 2025-07-09

