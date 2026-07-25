using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Contrato para confirmação (ACK) ou rejeição (NACK) de arquivos recebidos via MFT.
/// </summary>
/// <remarks>
/// Após receber e processar um arquivo com <see cref="IMftInboundClient"/>, o consumidor
/// deve sinalizar o resultado ao servidor MFT para que o arquivo seja arquivado em diretório
/// de sucesso (ACK) ou de erro (NACK), evitando reprocessamento desnecessário.
/// </remarks>
public interface IAckNackClient
{
    /// <summary>
    /// Confirma (ACK) ou rejeita (NACK) um arquivo recebido.
    /// </summary>
    /// <param name="command">Comando com identificadores do item e a decisão (<see cref="AckNackType.Ack"/> ou <see cref="AckNackType.Nack"/>).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <see cref="TransferItemResult"/> com <see cref="TransferState.Succeeded"/> quando o ACK/NACK
    /// é processado com sucesso, ou <see cref="TransferState.Failed"/> em caso de erro.
    /// </returns>
    /// <exception cref="ArgumentException">Lançado quando <paramref name="command"/> não contém um <c>ItemId</c> válido.</exception>
    Task<TransferItemResult> AckOrNackAsync(AckNackCommand command, CancellationToken cancellationToken = default);
}
