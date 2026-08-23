using System.Threading;
using Nuuvify.CommonPack.Observability.Abstraction;

namespace Nuuvify.CommonPack.Observability;

/// <summary>
/// Acesso ao contexto de operação isolado por execução assíncrona.
/// </summary>
public sealed class OperationContextAccessor : IOperationContextAccessor
{
    private readonly AsyncLocal<OperationContext> _current = new AsyncLocal<OperationContext>();

    /// <inheritdoc />
    public OperationContext Current
    {
        get => _current.Value ?? OperationContext.Empty;
        set => _current.Value = value ?? OperationContext.Empty;
    }
}
