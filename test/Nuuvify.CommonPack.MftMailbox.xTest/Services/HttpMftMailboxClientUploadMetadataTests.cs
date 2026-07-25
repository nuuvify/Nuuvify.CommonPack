using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Http;
using Nuuvify.CommonPack.MftMailbox.Http.Configuration;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public sealed class HttpMftMailboxClientUploadMetadataTests
{
    [Fact]
    public async Task SendSingleAsync_DeveIncluirMetadadosComPrefixosConfiguradosNoMultipart()
    {
        using var responses = new DisposableHttpResponses();
        string? multipartBody = null;
        using var uploadContentStream = new MemoryStream(new byte[] { 1, 2, 3 });

        using var handler = new StubHttpMessageHandler(async request =>
        {
            if (request.Method == HttpMethod.Post && request.RequestUri?.AbsolutePath == "/mailbox/upload")
            {
                multipartBody = await request.Content!.ReadAsStringAsync();
                return responses.Track(new HttpResponseMessage(HttpStatusCode.OK));
            }

            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/mailbox/status")
            {
                return responses.Track(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new TransferStatus
                    {
                        IntegrationKey = "erp-http",
                        ItemId = "item-001",
                        State = TransferState.Succeeded
                    })
                });
            }

            return responses.Track(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        using var httpClient = new HttpClient(handler, disposeHandler: false);
        var client = new HttpMftMailboxClient(
            httpClient,
            Options.Create(new HttpMftMailboxOptions
            {
                BaseUrl = "http://localhost",
                UploadPath = "/mailbox/upload",
                StatusPath = "/mailbox/status",
                IncludeMetadataInUploadForm = true,
                EnvelopeMetadataFieldPrefix = "env-",
                ItemMetadataFieldPrefix = "item-"
            }),
            Options.Create(new MftMailboxOptions()),
            new InMemoryMftIdempotencyStore(),
            new NullTransferAuditSink(),
            NullLogger<HttpMftMailboxClient>.Instance);

        var result = await client.SendSingleAsync(new TransferEnvelope
        {
            IntegrationKey = "erp-http",
            CorrelationId = Guid.NewGuid().ToString("N"),
            Protocol = MftProtocol.Https,
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "sap",
                ["batch"] = "42"
            },
            Items =
                {
                    new TransferItem
                    {
                        ItemId = "item-001",
                        FileName = "orders.csv",
                        Metadata = new Dictionary<string, string>
                        {
                            ["priority"] = "high"
                        },
                        ContentFactory = _ =>
                        {
                            uploadContentStream.Position = 0;
                            return Task.FromResult<Stream>(uploadContentStream);
                        }
                    }
                }
        });

        Assert.Equal(TransferState.Succeeded, result.State);
        Assert.NotNull(multipartBody);
        Assert.Contains("orders.csv", multipartBody, StringComparison.Ordinal);
        Assert.Contains("env-", multipartBody, StringComparison.Ordinal);
        Assert.Contains("sap", multipartBody, StringComparison.Ordinal);
        Assert.Contains("env-", multipartBody, StringComparison.Ordinal);
        Assert.Contains("42", multipartBody, StringComparison.Ordinal);
        Assert.Contains("item-", multipartBody, StringComparison.Ordinal);
        Assert.Contains("high", multipartBody, StringComparison.Ordinal);
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

    private sealed class DisposableHttpResponses : IDisposable
    {
        private readonly List<HttpResponseMessage> _responses = [];

        public HttpResponseMessage Track(HttpResponseMessage response)
        {
            _responses.Add(response);
            return response;
        }

        public void Dispose()
        {
            foreach (var response in _responses)
            {
                response.Dispose();
            }
        }
    }
}
