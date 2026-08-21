# Nuuvify.CommonPack.Observability

Implementação do contexto de operação para execução assíncrona com isolamento por escopo.

## Uso

```csharp
var accessor = new OperationContextAccessor();
var context = new OperationContext("corr-123", "trace-456", "op-789");

using (new OperationContextScope(accessor, context))
{
    var current = accessor.Current;
}
```
