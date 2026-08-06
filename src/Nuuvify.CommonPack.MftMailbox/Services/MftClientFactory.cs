using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Protocols;

namespace Nuuvify.CommonPack.MftMailbox.Services;

/// <summary>
/// Implementação de <see cref="IMftClientFactory"/> que resolve clientes concretos pelo protocolo.
/// </summary>
/// <remarks>
/// Resolve os clientes por protocolo usando keyed services do DI e mantém cache explícito por
/// <see cref="MftProtocol"/> para reutilização durante o ciclo de vida da factory.
/// Registrado como Singleton por <c>MftMailboxSetup.AddMftMailboxCore</c>.
/// </remarks>
public sealed class MftClientFactory : IMftClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<MftProtocol> _cachedProtocols;
    private readonly Dictionary<MftProtocol, IProtocolMftClient> _cachedClients = new();
    private readonly object _sync = new();

    /// <summary>
    /// Inicializa a fábrica para resolver clientes por protocolo no container DI.
    /// </summary>
    /// <param name="serviceProvider">Provider raiz para resolução de keyed services.</param>
    /// <param name="options">Opções globais do MFT, incluindo estratégia de cache por protocolo.</param>
    /// <exception cref="InvalidOperationException">
    /// Lançado quando não há cliente registrado para o protocolo solicitado ou há duplicidade para a mesma chave.
    /// </exception>
    public MftClientFactory(IServiceProvider serviceProvider, IOptions<MftMailboxOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(options);

        _cachedProtocols = options.Value.CachedProtocols is null
            ? new HashSet<MftProtocol>()
            : new HashSet<MftProtocol>(options.Value.CachedProtocols);
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
        if (!_cachedProtocols.Contains(protocol))
        {
            return ResolveProtocolClient(protocol);
        }

        lock (_sync)
        {
            if (_cachedClients.TryGetValue(protocol, out var cached))
            {
                return cached;
            }

            var resolved = ResolveProtocolClient(protocol);
            _cachedClients[protocol] = resolved;
            return resolved;
        }
    }

    private IProtocolMftClient ResolveProtocolClient(MftProtocol protocol)
    {
        var keyedClients = _serviceProvider.GetKeyedServices<IProtocolMftClient>(protocol).ToList();

        if (keyedClients.Count == 0)
        {
            throw new InvalidOperationException($"No MFT client registered for protocol '{protocol}'.");
        }

        if (keyedClients.Count > 1)
        {
            throw new InvalidOperationException($"Multiple MFT clients registered for protocol '{protocol}'. Keep only one implementation per protocol.");
        }

        return keyedClients[0];
    }
}
