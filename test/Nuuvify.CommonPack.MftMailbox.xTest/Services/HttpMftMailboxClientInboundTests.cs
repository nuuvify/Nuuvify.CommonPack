using System.Net;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Http;
using Nuuvify.CommonPack.MftMailbox.Http.Configuration;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public sealed class HttpMftMailboxClientInboundTests
{
    private static readonly byte[] DownloadPayload = new byte[] { 9, 9, 9 };

    [Fact]
    public async Task ReceiveBatchAsync_ComEnvelopeItens_NaoDeveChamarListagemRemota()
    {
        using var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/mailbox/list")
            {
                throw new InvalidOperationException("List endpoint should not be called when envelope has explicit items.");
            }

            if (request.RequestUri?.AbsolutePath == "/mailbox/download")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        using var httpClient = new HttpClient(handler, disposeHandler: false);

        var client = CreateClient(httpClient, new HttpMftMailboxOptions
        {
            BaseUrl = "http://localhost",
            DownloadPath = "/mailbox/download",
            ListPath = "/mailbox/list"
        });

        var envelope = new TransferEnvelope
        {
            IntegrationKey = "mainframe-x",
            CorrelationId = Guid.NewGuid().ToString("N"),
            Protocol = MftProtocol.Https,
            Items =
            {
                new TransferItem
                {
                    ItemId = "item-001",
                    FileName = "arquivo-c.dat",
                    RemotePath = "/inbound/arquivo-c.dat"
                }
            }
        };

        var result = await client.ReceiveBatchAsync(envelope);

        Assert.Single(result);
        Assert.Equal("item-001", result.First().ItemId);
        Assert.Equal("arquivo-c.dat", result.First().FileName);

        foreach (var item in result)
        {
            await item.DisposeAsync();
        }
    }

    [Fact]
    public async Task ReceiveBatchAsync_ListaRemota_DeveRespeitarOrdenacaoPorNome()
    {
        var payload = new
        {
            items = new[]
            {
                new { itemId = "b", fileName = "B-file.txt", remotePath = "/inbound/B-file.txt" },
                new { itemId = "a", fileName = "A-file.txt", remotePath = "/inbound/A-file.txt" }
            }
        };

        using var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/mailbox/list")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                });
            }

            if (request.RequestUri?.AbsolutePath == "/mailbox/download")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(DownloadPayload)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        using var httpClient = new HttpClient(handler, disposeHandler: false);

        var client = CreateClient(httpClient, new HttpMftMailboxOptions
        {
            BaseUrl = "http://localhost",
            DownloadPath = "/mailbox/download",
            ListPath = "/mailbox/list",
            InboundFileOrdering = InboundFileOrdering.FileNameAscending
        });

        var envelope = new TransferEnvelope
        {
            IntegrationKey = "mainframe-x",
            CorrelationId = Guid.NewGuid().ToString("N"),
            Protocol = MftProtocol.Https
        };

        var result = await client.ReceiveBatchAsync(envelope);
        var orderedNames = result.Select(x => x.FileName).ToArray();

        Assert.Equal(new[] { "A-file.txt", "B-file.txt" }, orderedNames);

        foreach (var item in result)
        {
            await item.DisposeAsync();
        }
    }

    private static HttpMftMailboxClient CreateClient(HttpClient httpClient, HttpMftMailboxOptions httpOptions)
    {
        var mailboxOptions = new MftMailboxOptions();

        return new HttpMftMailboxClient(
            httpClient,
            Options.Create(httpOptions),
            Options.Create(mailboxOptions),
            new InMemoryMftIdempotencyStore(),
            new NullTransferAuditSink(),
            NullLogger<HttpMftMailboxClient>.Instance);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handle;

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handle)
        {
            _handle = handle;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handle(request);
        }
    }
}
