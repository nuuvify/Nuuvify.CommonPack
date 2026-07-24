using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Services;
using Nuuvify.CommonPack.MftMailbox.Sftp;
using Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public sealed class SftpMftMailboxClientAckNackTests
{
    [Fact]
    public async Task AckOrNackAsync_ModoNone_DeveRetornarSucessoSemConexaoRemota()
    {
        var options = new SftpMftMailboxOptions
        {
            Host = "127.0.0.1",
            Port = 22,
            Username = "integration",
            Password = "integration",
            AckNackMode = SftpAckNackMode.None
        };

        var client = new SftpMftMailboxClient(
            Options.Create(options),
            Options.Create(new MftMailboxOptions()),
            new InMemoryMftIdempotencyStore(),
            new NullTransferAuditSink(),
            NullLogger<SftpMftMailboxClient>.Instance);

        var command = new AckNackCommand
        {
            IntegrationKey = "mf-a",
            CorrelationId = Guid.NewGuid().ToString("N"),
            ItemId = "item-100",
            FileName = "retorno.dat",
            Protocol = MftProtocol.Sftp,
            Decision = AckNackType.Ack
        };

        var result = await client.AckOrNackAsync(command);

        Assert.Equal(TransferState.Succeeded, result.State);
        Assert.Equal("item-100", result.ItemId);
        Assert.Equal("retorno.dat", result.FileName);
    }
}
