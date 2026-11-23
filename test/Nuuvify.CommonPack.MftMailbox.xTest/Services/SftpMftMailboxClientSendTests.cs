using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Services;
using Nuuvify.CommonPack.MftMailbox.Sftp;
using Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public sealed class SftpMftMailboxClientSendTests
{
    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "/partner/03I30003.TXT")]
    [InlineData(true, "/partner/03I30003.TXT")]
    public async Task SendAsync_DeveCriarArquivoComNomeDefinitivoSemOutrasOperacoesRemotas(bool batch, string remotePath)
    {
        var payload = Encoding.UTF8.GetBytes("arquivo MFT");
        var envelope = CreateEnvelope(payload, remotePath);
        var expectedPath = string.IsNullOrEmpty(remotePath) ? "/outbound/03I30003.TXT" : remotePath;
        var sftp = new Mock<ISftpClient>(MockBehavior.Strict);
        var sequence = new MockSequence();
        sftp.InSequence(sequence).Setup(x => x.Connect());
        sftp.InSequence(sequence)
            .Setup(x => x.UploadFile(It.IsAny<Stream>(), expectedPath, true, null))
            .Callback<Stream, string, bool, Action<ulong>>((stream, _, _, _) =>
            {
                using var uploaded = new MemoryStream();
                stream.CopyTo(uploaded);
                Assert.Equal(payload, uploaded.ToArray());
            });
        sftp.InSequence(sequence).Setup(x => x.Disconnect());
        sftp.InSequence(sequence).Setup(x => x.Dispose());
        var audit = new Mock<ITransferAuditSink>(MockBehavior.Strict);
        audit.Setup(x => x.WriteAsync(It.Is<TransferAuditEntry>(entry =>
                entry.State == TransferState.Succeeded && entry.FileName == "03I30003.TXT"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var client = CreateClient(sftp.Object, audit.Object);

        var result = batch
            ? Assert.Single((await client.SendBatchAsync(envelope)).Items)
            : await client.SendSingleAsync(envelope);

        Assert.Equal(TransferState.Succeeded, result.State);
        Assert.Equal("UPLOADED", result.StatusCode);
        Assert.Equal("03I30003.TXT", result.FileName);
        Assert.Equal(payload.Length, result.SizeBytes);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant(), result.ChecksumSha256);
        var status = await client.GetStatusAsync(envelope.IntegrationKey, envelope.Items[0].ItemId);
        Assert.NotNull(status);
        Assert.Equal(TransferState.Succeeded, status.State);
        Assert.Equal(result.ChecksumSha256, status.ChecksumSha256);

        var repeated = batch
            ? Assert.Single((await client.SendBatchAsync(envelope)).Items)
            : await client.SendSingleAsync(envelope);
        Assert.Equal(TransferState.Skipped, repeated.State);
        sftp.Verify(x => x.Connect(), Times.Once);
        sftp.Verify(x => x.UploadFile(It.IsAny<Stream>(), expectedPath, true, null), Times.Once);
        sftp.Verify(x => x.Disconnect(), Times.Once);
        sftp.Verify(x => x.Dispose(), Times.Once);
        sftp.VerifyNoOtherCalls();
        audit.Verify(x => x.WriteAsync(It.IsAny<TransferAuditEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBatchAsync_UploadNegado_DeveRegistrarFalhaSemTentarExcluirOuRenomear()
    {
        var envelope = CreateEnvelope(Encoding.UTF8.GetBytes("arquivo MFT"), "");
        var failure = new SftpPermissionDeniedException("Permission denied.");
        var sftp = new Mock<ISftpClient>(MockBehavior.Strict);
        sftp.Setup(x => x.Connect());
        sftp.Setup(x => x.UploadFile(It.IsAny<Stream>(), "/outbound/03I30003.TXT", true, null))
            .Throws(failure);
        sftp.Setup(x => x.Dispose());
        var audit = new Mock<ITransferAuditSink>(MockBehavior.Strict);
        audit.Setup(x => x.WriteAsync(It.Is<TransferAuditEntry>(entry =>
                entry.State == TransferState.Failed && entry.Message == failure.Message), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var client = CreateClient(sftp.Object, audit.Object);

        var exception = await Assert.ThrowsAsync<SftpPermissionDeniedException>(() => client.SendBatchAsync(envelope));

        Assert.Same(failure, exception);
        var status = await client.GetStatusAsync(envelope.IntegrationKey, envelope.Items[0].ItemId);
        Assert.NotNull(status);
        Assert.Equal(TransferState.Failed, status.State);
        Assert.Equal("UPLOAD_FAILED", status.ExternalStatus);
        Assert.Equal(failure.Message, status.Error);
        sftp.Verify(x => x.Connect(), Times.Once);
        sftp.Verify(x => x.UploadFile(It.IsAny<Stream>(), "/outbound/03I30003.TXT", true, null), Times.Once);
        sftp.Verify(x => x.Dispose(), Times.Once);
        sftp.VerifyNoOtherCalls();
        audit.Verify(x => x.WriteAsync(It.IsAny<TransferAuditEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static TransferEnvelope CreateEnvelope(byte[] payload, string remotePath)
    {
        return new TransferEnvelope
        {
            IntegrationKey = "mft",
            CorrelationId = "correlation-1",
            Protocol = MftProtocol.Sftp,
            Items = new List<TransferItem>
            {
                new()
                {
                    ItemId = "item-1",
                    FileName = "03I30003.TXT",
                    RemotePath = remotePath,
                    ContentFactory = _ => Task.FromResult<Stream>(new MemoryStream(payload))
                }
            }
        };
    }

    private static SftpMftMailboxClient CreateClient(ISftpClient sftp, ITransferAuditSink audit)
    {
        return new SftpMftMailboxClient(
            Options.Create(new SftpMftMailboxOptions
            {
                Host = "sftp.example.local",
                Username = "integration",
                Password = "test-only",
                OutboundDirectory = "/outbound"
            }),
            Options.Create(new MftMailboxOptions()),
            new InMemoryMftIdempotencyStore(),
            audit,
            NullLogger<SftpMftMailboxClient>.Instance,
            _ => sftp);
    }
}
