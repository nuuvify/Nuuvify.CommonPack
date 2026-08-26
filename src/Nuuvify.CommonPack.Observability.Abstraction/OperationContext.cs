using System.Collections.Generic;

namespace Nuuvify.CommonPack.Observability.Abstraction;

/// <summary>
/// Representa o contexto mínimo de uma operação para correlação e rastreio.
/// </summary>
public sealed class OperationContext
{
    /// <summary>
    /// Contexto vazio compartilhado para cenários sem operação inicializada.
    /// </summary>
    public static readonly OperationContext Empty = new OperationContext();

    /// <summary>
    /// Inicializa uma nova instância do contexto de operação.
    /// </summary>
    /// <param name="correlationId">Identificador de correlação da operação.</param>
    /// <param name="traceId">Identificador de trace.</param>
    /// <param name="operationId">Identificador da operação.</param>
    /// <param name="metadata">Metadados adicionais, sem conteúdo sensível.</param>
    public OperationContext(
        string correlationId = "",
        string traceId = "",
        string operationId = "",
        IReadOnlyDictionary<string, string> metadata = null)
    {
        CorrelationId = correlationId ?? string.Empty;
        TraceId = traceId ?? string.Empty;
        OperationId = operationId ?? string.Empty;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Identificador de correlação.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Identificador de trace.
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// Identificador da operação atual.
    /// </summary>
    public string OperationId { get; }

    /// <summary>
    /// Conjunto de metadados neutros da operação.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
