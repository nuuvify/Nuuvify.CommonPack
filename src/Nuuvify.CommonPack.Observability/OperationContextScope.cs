using System;
using Nuuvify.CommonPack.Observability.Abstraction;

namespace Nuuvify.CommonPack.Observability;

/// <summary>
/// Escopo para o contexto de operação e restauração do valor anterior ao sair do uso.
/// </summary>
public sealed class OperationContextScope : IDisposable
{
    private readonly IOperationContextAccessor _accessor;
    private readonly OperationContext _previous;
    private bool _disposed;

    /// <summary>
    /// Cria um novo escopo de contexto de operação.
    /// </summary>
    /// <param name="accessor">Accessor do contexto corrente.</param>
    /// <param name="context">Novo contexto de operação.</param>
    public OperationContextScope(IOperationContextAccessor accessor, OperationContext context)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _previous = accessor.Current;
        accessor.Current = context ?? OperationContext.Empty;
    }

    /// <summary>
    /// Libera o escopo e restaura o contexto anterior.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _accessor.Current = _previous;
        _disposed = true;
    }
}
