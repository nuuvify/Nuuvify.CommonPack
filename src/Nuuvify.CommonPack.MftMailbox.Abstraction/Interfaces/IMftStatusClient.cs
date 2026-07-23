using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Contrato para consulta de status de uma transferência MFT.
/// </summary>
/// <remarks>
/// Permite ao consumidor verificar o estado atual de um arquivo enviado ou recebido.
/// A implementação SFTP mantém um cache em memória populado durante as operações de envio;
/// a implementação HTTP consulta o endpoint remoto configurado em <c>HttpMftMailboxOptions.StatusPath</c>.
/// </remarks>
public interface IMftStatusClient
{
    /// <summary>
    /// Obtém o status atual de uma transferência identificada pela chave de integração e pelo ID do item.
    /// </summary>
    /// <param name="integrationKey">Chave de integração que agrupa a transferência (ex.: código do sistema parceiro).</param>
    /// <param name="itemId">Identificador único do item dentro da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <see cref="TransferStatus"/> com o estado atual, ou <see langword="null"/> quando o item não é encontrado.
    /// </returns>
    Task<TransferStatus?> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default);
}
