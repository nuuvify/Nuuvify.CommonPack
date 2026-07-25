using System.Collections.Concurrent;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.MftMailbox.Services;

/// <summary>
/// Implementação em memória de <see cref="IMftIdempotencyStore"/> usando um dicionário concorrente.
/// </summary>
/// <remarks>
/// Adequada para processos de vida curta ou testes unitários. O estado é perdido ao reiniciar
/// o processo. Para durabilidade entre reinicializações, implemente <see cref="IMftIdempotencyStore"/>
/// com persistência em banco de dados ou cache distribuído (ex.: Redis, SQL Server).
/// Registrada como Singleton por <c>MftMailboxSetup.AddMftMailboxCore</c>.
/// </remarks>
public sealed class InMemoryMftIdempotencyStore : IMftIdempotencyStore
{
    private readonly ConcurrentDictionary<string, string> _state = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tenta registrar a chave como iniciada atomicamente.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta gerada por <c>IdempotencyKeyBuilder</c>.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    /// <see langword="true"/> se a chave foi inserida pela primeira vez (processamento pode prosseguir);
    /// <see langword="false"/> se já existia (item já processado ou em andamento).
    /// </returns>
    /// <exception cref="ArgumentException">Lançado quando <paramref name="idempotencyKey"/> é nula, vazia ou whitespace.</exception>
    public Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));
        }

        var started = _state.TryAdd(idempotencyKey, "started");
        return Task.FromResult(started);
    }

    /// <summary>
    /// Atualiza o valor da chave para <c>"completed"</c>, marcando a transferência como concluída.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <exception cref="ArgumentException">Lançado quando <paramref name="idempotencyKey"/> é nula, vazia ou whitespace.</exception>
    public Task MarkCompletedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));
        }

        _state[idempotencyKey] = "completed";
        return Task.CompletedTask;
    }

    /// <summary>
    /// Atualiza o valor da chave para <c>"failed:&lt;reason&gt;"</c>, preservando o motivo para diagnóstico.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="reason">Descrição do motivo da falha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <exception cref="ArgumentException">
    /// Lançado quando <paramref name="idempotencyKey"/> ou <paramref name="reason"/> são nulos, vazios ou whitespace.
    /// </exception>
    public Task MarkFailedAsync(string idempotencyKey, string reason, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Motivo da falha não pode ser nulo ou vazio.", nameof(reason));
        }

        _state[idempotencyKey] = $"failed:{reason}";
        return Task.CompletedTask;
    }
}
