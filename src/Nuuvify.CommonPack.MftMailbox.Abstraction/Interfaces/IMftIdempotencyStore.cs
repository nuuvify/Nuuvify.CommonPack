namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Armazenamento de estado de idempotência para transferências MFT.
/// </summary>
/// <remarks>
/// Garante que um arquivo não seja enviado ou processado mais de uma vez caso o processo
/// seja reiniciado após uma falha parcial. A chave de idempotência é composta por
/// <c>integrationKey|correlationId|itemId|fileName</c> (gerada por <c>IdempotencyKeyBuilder</c>).
/// A implementação padrão é <c>InMemoryMftIdempotencyStore</c>, adequada para processos
/// de vida curta. Para durabilidade entre reinicializações, implemente esta interface
/// usando um store persistente (banco de dados, Redis, etc.).
/// </remarks>
public interface IMftIdempotencyStore
{
    /// <summary>
    /// Tenta iniciar o processamento de uma chave de idempotência.
    /// </summary>
    /// <param name="idempotencyKey">Chave única da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <see langword="true"/> quando a chave foi registrada pela primeira vez e o processamento pode prosseguir;
    /// <see langword="false"/> quando a chave já existe (item já processado ou em andamento).
    /// </returns>
    Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca a chave como concluída com sucesso.
    /// </summary>
    /// <param name="idempotencyKey">Chave única da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task MarkCompletedAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca a chave como falha, preservando o motivo para diagnóstico.
    /// </summary>
    /// <param name="idempotencyKey">Chave única da transferência.</param>
    /// <param name="reason">Descrição do motivo da falha.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task MarkFailedAsync(string idempotencyKey, string reason, CancellationToken cancellationToken = default);
}
