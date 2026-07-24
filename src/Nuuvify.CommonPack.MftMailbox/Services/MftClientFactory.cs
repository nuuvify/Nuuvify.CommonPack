using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Protocols;

namespace Nuuvify.CommonPack.MftMailbox.Services;

/// <summary>
/// Implementação de <see cref="IMftClientFactory"/> que resolve clientes concretos pelo protocolo.
/// </summary>
/// <remarks>
/// Recebe todos os <see cref="IProtocolMftClient"/> registrados no container DI e os indexa por
/// <see cref="IProtocolMftClient.Protocol"/>. Registrado como Singleton por
/// <c>MftMailboxSetup.AddMftMailboxCore</c>. A resolução é O(1) após a construção do índice.
/// </remarks>
public sealed class MftClientFactory : IMftClientFactory
{
    private readonly IReadOnlyDictionary<MftProtocol, IProtocolMftClient> _clients;

    /// <summary>
    /// Inicializa a fábrica indexando os clientes fornecidos pelo protocolo.
    /// </summary>
    /// <param name="clients">Coleção de todos os <see cref="IProtocolMftClient"/> registrados no DI.</param>
    /// <exception cref="InvalidOperationException">
    /// Lançado quando há mais de um <see cref="IProtocolMftClient"/> registrado para o mesmo <see cref="MftProtocol"/>.
    /// </exception>
    public MftClientFactory(IEnumerable<IProtocolMftClient> clients)
    {
        var dictionary = new Dictionary<MftProtocol, IProtocolMftClient>();
        foreach (var client in clients)
        {
            if (dictionary.ContainsKey(client.Protocol))
            {
                throw new InvalidOperationException($"Multiple MFT clients registered for protocol '{client.Protocol}'. Keep only one implementation per protocol.");
            }

            dictionary[client.Protocol] = client;
        }

        _clients = dictionary;
    }

    /// <inheritdoc />
    public IMftTransferClient CreateTransferClient(MftProtocol protocol) => Resolve(protocol);

    /// <inheritdoc />
    public IMftInboundClient CreateInboundClient(MftProtocol protocol) => Resolve(protocol);

    /// <inheritdoc />
    public IMftStatusClient CreateStatusClient(MftProtocol protocol) => Resolve(protocol);

    /// <inheritdoc />
    public IAckNackClient CreateAckNackClient(MftProtocol protocol) => Resolve(protocol);

    private IProtocolMftClient Resolve(MftProtocol protocol)
    {
        if (_clients.TryGetValue(protocol, out var client))
        {
            return client;
        }

        throw new InvalidOperationException($"No MFT client registered for protocol '{protocol}'.");
    }
}
