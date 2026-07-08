using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Ponto de extensão para auditoria de todas as operações de transferência MFT.
/// </summary>
/// <remarks>
/// A implementação padrão é <c>NullTransferAuditSink</c>, que descarta silenciosamente cada entrada.
/// Para habilitar auditoria, registre uma implementação customizada no container DI antes de chamar
/// <c>AddMftMailboxCore</c>, ou substitua após o registro:
/// <code>
/// services.AddSingleton&lt;ITransferAuditSink, MinhaAuditoria&gt;();
/// services.AddMftMailboxCore();
/// </code>
/// Cada operação (envio, recepção, ACK/NACK) grava uma entrada com estado e metadados.
/// </remarks>
public interface ITransferAuditSink
{
    /// <summary>
    /// Grava uma entrada de auditoria referente a uma operação de transferência.
    /// </summary>
    /// <param name="entry">Dados da operação, incluindo protocolo, estado, arquivo e metadados.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task WriteAsync(TransferAuditEntry entry, CancellationToken cancellationToken = default);
}
