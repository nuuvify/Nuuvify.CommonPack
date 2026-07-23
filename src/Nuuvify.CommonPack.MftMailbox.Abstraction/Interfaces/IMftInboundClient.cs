using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Contrato para recepção de arquivos MFT (inbound).
/// </summary>
/// <remarks>
/// Implementações concretas conectam ao servidor MFT via SFTP ou HTTP e baixam os arquivos
/// disponíveis no diretório inbound configurado. Cada <see cref="InboundTransferItem"/> retornado
/// expõe um <see cref="Stream"/> do conteúdo e implementa <see cref="IAsyncDisposable"/>;
/// o consumidor deve descartar o item após processar o conteúdo.
/// Após o processamento, confirme via <see cref="IAckNackClient.AckOrNackAsync"/>.
/// </remarks>
public interface IMftInboundClient
{
    /// <summary>
    /// Recebe o primeiro arquivo disponível no servidor MFT.
    /// </summary>
    /// <param name="envelope">Envelope com metadados da integração. Quando <see cref="TransferEnvelope.Items"/> está vazio,
    /// o cliente lista os arquivos disponíveis e retorna o primeiro encontrado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// O primeiro <see cref="InboundTransferItem"/> disponível, ou <see langword="null"/> quando não há arquivos.
    /// </returns>
    Task<InboundTransferItem?> ReceiveSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recebe múltiplos arquivos disponíveis no servidor MFT em uma única operação.
    /// </summary>
    /// <param name="envelope">Envelope com metadados da integração. Quando <see cref="TransferEnvelope.Items"/> está preenchido,
    /// baixa apenas os arquivos explicitamente listados; caso contrário, lista e baixa todos os disponíveis
    /// respeitando o limite <c>MaxBatchSize</c>.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Coleção de itens recebidos. Pode ser vazia quando não há arquivos disponíveis.</returns>
    Task<IReadOnlyCollection<InboundTransferItem>> ReceiveBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default);
}
