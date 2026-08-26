namespace Nuuvify.CommonPack.Observability.Abstraction;

/// <summary>
/// Define o acesso ao contexto de operação atual em uma execução.
/// </summary>
public interface IOperationContextAccessor
{
    /// <summary>
    /// Obtém ou define o contexto de operação atual da execução atual.
    /// </summary>
    OperationContext Current { get; set; }
}
