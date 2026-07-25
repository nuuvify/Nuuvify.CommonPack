using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Protocols;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public class MftClientFactoryTests
{
    [Fact]
    public void DeveResolverClientesPorProtocolo()
    {
        var services = new ServiceCollection();
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Sftp, (_, _) => new FakeProtocolClient(MftProtocol.Sftp));
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Https, (_, _) => new FakeProtocolClient(MftProtocol.Https));

        var provider = services.BuildServiceProvider();
        IMftClientFactory factory = new MftClientFactory(provider, Options.Create(new MftMailboxOptions()));

        var transfer = factory.CreateTransferClient(MftProtocol.Sftp);
        var inbound = factory.CreateInboundClient(MftProtocol.Https);

        Assert.Equal(MftProtocol.Sftp, ((IProtocolMftClient)transfer).Protocol);
        Assert.Equal(MftProtocol.Https, ((IProtocolMftClient)inbound).Protocol);
    }

    [Fact]
    public void DeveFalharQuandoNaoExisteProtocoloRegistrado()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        IMftClientFactory factory = new MftClientFactory(provider, Options.Create(new MftMailboxOptions()));

        Assert.Throws<InvalidOperationException>(() => factory.CreateTransferClient(MftProtocol.Sftp));
    }

    [Fact]
    public void DeveFalharQuandoExisteDuplicidadeDeProtocolo()
    {
        var services = new ServiceCollection();
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Sftp, (_, _) => new FakeProtocolClient(MftProtocol.Sftp));
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Sftp, (_, _) => new FakeProtocolClient(MftProtocol.Sftp));
        var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            IMftClientFactory factory = new MftClientFactory(provider, Options.Create(new MftMailboxOptions()));
            _ = factory.CreateTransferClient(MftProtocol.Sftp);
        });

        Assert.Contains("Multiple MFT clients registered", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DeveCachearInstanciaPorProtocoloMesmoComRegistroTransiente()
    {
        var services = new ServiceCollection();
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Sftp, (_, _) => new FakeProtocolClient(MftProtocol.Sftp));

        var provider = services.BuildServiceProvider();
        IMftClientFactory factory = new MftClientFactory(provider, Options.Create(new MftMailboxOptions()));

        var first = factory.CreateTransferClient(MftProtocol.Sftp);
        var second = factory.CreateInboundClient(MftProtocol.Sftp);

        Assert.Same(first, second);
    }

    [Fact]
    public void NaoDeveCachearQuandoProtocoloNaoEstaConfiguradoParaCache()
    {
        var services = new ServiceCollection();
        _ = services.AddKeyedTransient<IProtocolMftClient>(MftProtocol.Https, (_, _) => new FakeProtocolClient(MftProtocol.Https));

        var provider = services.BuildServiceProvider();
        var options = new MftMailboxOptions
        {
            CachedProtocols = [MftProtocol.Sftp]
        };

        IMftClientFactory factory = new MftClientFactory(provider, Options.Create(options));

        var first = factory.CreateTransferClient(MftProtocol.Https);
        var second = factory.CreateInboundClient(MftProtocol.Https);

        Assert.NotSame(first, second);
    }

    private sealed class FakeProtocolClient : IProtocolMftClient
    {
        public FakeProtocolClient(MftProtocol protocol)
        {
            Protocol = protocol;
        }

        public MftProtocol Protocol { get; }

        public Task<TransferItemResult> AckOrNackAsync(AckNackCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(new TransferItemResult { ItemId = command.ItemId, FileName = command.FileName, State = TransferState.Succeeded });

        public Task<TransferStatus> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default)
            => Task.FromResult<TransferStatus>(null);

        public Task<IReadOnlyCollection<InboundTransferItem>> ReceiveBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<InboundTransferItem>>(Array.Empty<InboundTransferItem>());

        public Task<InboundTransferItem> ReceiveSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.FromResult<InboundTransferItem>(null);

        public Task<TransferBatchResult> SendBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.FromResult(new TransferBatchResult());

        public Task<TransferItemResult> SendSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
            => Task.FromResult(new TransferItemResult { State = TransferState.Succeeded });
    }
}
