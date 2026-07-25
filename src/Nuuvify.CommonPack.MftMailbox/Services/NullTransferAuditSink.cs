using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Services;

/// <summary>
/// Implementação nula (no-op) de <see cref="ITransferAuditSink"/> que descarta silenciosamente
/// todas as entradas de auditoria.
/// </summary>
/// <remarks>
/// Registrada automaticamente por <c>MftMailboxSetup.AddMftMailboxCore</c>.
/// Para ativar auditoria real, registre sua própria implementação de <see cref="ITransferAuditSink"/>
/// antes ou após chamar <c>AddMftMailboxCore</c> e garanta que o último registro vence
/// (comportamento padrão do DI do .NET para registros repetidos do mesmo tipo).
/// </remarks>
public sealed class NullTransferAuditSink : ITransferAuditSink
{
    /// <summary>
    /// Descarta a entrada de auditoria sem realizar nenhuma operação.
    /// </summary>
    /// <param name="entry">Entrada a ser descartada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançado quando <paramref name="entry"/> é <see langword="null"/>.
    /// </exception>
    public Task WriteAsync(TransferAuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
