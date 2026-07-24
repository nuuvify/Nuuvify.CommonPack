using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Protocols;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public class MftClientFactoryTests
{
    [Fact]
    public void DeveResolverClientesPorProtocolo()
    {
        var sftp = new FakeProtocolClient(MftProtocol.Sftp);
        var https = new FakeProtocolClient(MftProtocol.Https);

        IMftClientFactory factory = new MftClientFactory(new[] { sftp, https });

        var transfer = factory.CreateTransferClient(MftProtocol.Sftp);
        var inbound = factory.CreateInboundClient(MftProtocol.Https);

        Assert.Same(sftp, transfer);
        Assert.Same(https, inbound);
    }

    [Fact]
    public void DeveFalharQuandoNaoExisteProtocoloRegistrado()
    {
        IMftClientFactory factory = new MftClientFactory(Array.Empty<IProtocolMftClient>());

        Assert.Throws<InvalidOperationException>(() => factory.CreateTransferClient(MftProtocol.Sftp));
    }

    [Fact]
    public void DeveFalharQuandoExisteDuplicidadeDeProtocolo()
    {
        var first = new FakeProtocolClient(MftProtocol.Sftp);
        var second = new FakeProtocolClient(MftProtocol.Sftp);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _ = new MftClientFactory(new[] { first, second }));

        Assert.Contains("Multiple MFT clients registered", exception.Message, StringComparison.Ordinal);
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
