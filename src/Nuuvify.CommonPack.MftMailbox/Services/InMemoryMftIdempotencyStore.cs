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
    public Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var started = _state.TryAdd(idempotencyKey, "started");
        return Task.FromResult(started);
    }

    /// <summary>
    /// Atualiza o valor da chave para <c>"completed"</c>, marcando a transferência como concluída.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task MarkCompletedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _state[idempotencyKey] = "completed";
        return Task.CompletedTask;
    }

    /// <summary>
    /// Atualiza o valor da chave para <c>"failed:&lt;reason&gt;"</c>, preservando o motivo para diagnóstico.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="reason">Descrição do motivo da falha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public Task MarkFailedAsync(string idempotencyKey, string reason, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _state[idempotencyKey] = $"failed:{reason}";
        return Task.CompletedTask;
    }
}
