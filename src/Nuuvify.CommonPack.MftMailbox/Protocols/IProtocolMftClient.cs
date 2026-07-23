using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Protocols;

/// <summary>
/// Interface interna que combina todos os contratos operacionais MFT em um único ponto de implementação.
/// </summary>
/// <remarks>
/// Implementada pelos clientes concretos (<c>SftpMftMailboxClient</c> e <c>HttpMftMailboxClient</c>).
/// Registrado no container DI como <c>IProtocolMftClient</c> (singleton) e resolvido pela
/// <c>MftClientFactory</c> por meio de <see cref="Protocol"/>.
/// Consumidores externos não devem depender diretamente desta interface; use os contratos
/// individuais (<see cref="IMftTransferClient"/>, <see cref="IMftInboundClient"/>, etc.)
/// obtidos via <see cref="IMftClientFactory"/>.
/// </remarks>
public interface IProtocolMftClient : IMftTransferClient, IMftInboundClient, IMftStatusClient, IAckNackClient
{
    /// <summary>Protocolo de rede suportado por esta implementação.</summary>
    MftProtocol Protocol { get; }
}
