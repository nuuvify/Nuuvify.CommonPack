using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Fábrica responsável por resolver o cliente MFT correto de acordo com o protocolo desejado.
/// </summary>
/// <remarks>
/// Cada protocolo registrado via <c>AddMftMailboxSftp</c> ou <c>AddMftMailboxHttp</c> expõe
/// todos os contratos operacionais (<see cref="IMftTransferClient"/>, <see cref="IMftInboundClient"/>,
/// <see cref="IMftStatusClient"/> e <see cref="IAckNackClient"/>). A fábrica elimina a necessidade
/// de injetar diretamente a implementação concreta, permitindo trocar o protocolo em tempo de execução.
/// </remarks>
public interface IMftClientFactory
{
    /// <summary>
    /// Retorna um cliente de transferência de saída (upload) para o protocolo informado.
    /// </summary>
    /// <param name="protocol">Protocolo desejado (<see cref="MftProtocol.Sftp"/> ou <see cref="MftProtocol.Https"/>).</param>
    /// <returns>Implementação de <see cref="IMftTransferClient"/> para o protocolo.</returns>
    /// <exception cref="InvalidOperationException">Lançado quando nenhum cliente está registrado para o protocolo.</exception>
    IMftTransferClient CreateTransferClient(MftProtocol protocol);

    /// <summary>
    /// Retorna um cliente de recepção de arquivos (download/inbound) para o protocolo informado.
    /// </summary>
    /// <param name="protocol">Protocolo desejado.</param>
    /// <returns>Implementação de <see cref="IMftInboundClient"/> para o protocolo.</returns>
    /// <exception cref="InvalidOperationException">Lançado quando nenhum cliente está registrado para o protocolo.</exception>
    IMftInboundClient CreateInboundClient(MftProtocol protocol);

    /// <summary>
    /// Retorna um cliente de consulta de status da transferência para o protocolo informado.
    /// </summary>
    /// <param name="protocol">Protocolo desejado.</param>
    /// <returns>Implementação de <see cref="IMftStatusClient"/> para o protocolo.</returns>
    /// <exception cref="InvalidOperationException">Lançado quando nenhum cliente está registrado para o protocolo.</exception>
    IMftStatusClient CreateStatusClient(MftProtocol protocol);

    /// <summary>
    /// Retorna um cliente de ACK/NACK para o protocolo informado.
    /// </summary>
    /// <param name="protocol">Protocolo desejado.</param>
    /// <returns>Implementação de <see cref="IAckNackClient"/> para o protocolo.</returns>
    /// <exception cref="InvalidOperationException">Lançado quando nenhum cliente está registrado para o protocolo.</exception>
    IAckNackClient CreateAckNackClient(MftProtocol protocol);
}
