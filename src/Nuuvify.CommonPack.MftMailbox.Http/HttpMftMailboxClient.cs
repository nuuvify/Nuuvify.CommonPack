using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Http.Configuration;
using Nuuvify.CommonPack.MftMailbox.Http.Contracts;
using Nuuvify.CommonPack.MftMailbox.Protocols;
using Nuuvify.CommonPack.MftMailbox.Utilities;

namespace Nuuvify.CommonPack.MftMailbox.Http;

/// <summary>
/// Cliente MFT via HTTPS com API REST, implementando envio, recepção,
/// consulta de status e ACK/NACK de arquivos.
/// </summary>
/// <remarks>
/// Registrado via <c>HttpMftMailboxSetup.AddMftMailboxHttp</c> e resolvido
/// pela <c>MftClientFactory</c> para o protocolo <see cref="MftProtocol.Https"/>.
/// <para>
/// Recursos integrados:
/// <list type="bullet">
/// <item>Idempotência por chave composta via <c>IMftIdempotencyStore</c>.</item>
/// <item>Checksum SHA-256 calculado localmente após upload e download.</item>
/// <item>Upload via <c>multipart/form-data</c> com polling de confirmação de status no servidor.</item>
/// <item>Retry com backoff exponencial e jitter configurado em <c>MftMailboxOptions.Retry</c>.</item>
/// <item>Circuit breaker configurado em <c>MftMailboxOptions.CircuitBreaker</c>.</item>
/// <item>Suporte a Bearer Token (<c>HttpMftMailboxOptions.BearerToken</c>) e mutual TLS.</item>
/// <item>Download de arquivos via streaming para arquivo temporário com exclusão automática.</item>
/// <item>Auditoria de cada operação via <c>ITransferAuditSink</c>.</item>
/// </list>
/// </para>
/// <para>
/// O <see cref="HttpClient"/> é gerenciado pelo <c>IHttpClientFactory</c> (registrado via
/// <c>services.AddHttpClient&lt;HttpMftMailboxClient&gt;()</c>) e a URL base é configurada
/// automaticamente a partir de <c>HttpMftMailboxOptions.BaseUrl</c>.
/// </para>
/// </remarks>
public sealed class HttpMftMailboxClient : IProtocolMftClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpMftMailboxOptions _options;
    private readonly MftMailboxOptions _baseOptions;
    private readonly IMftIdempotencyStore _idempotencyStore;
    private readonly ITransferAuditSink _auditSink;
    private readonly ILogger<HttpMftMailboxClient> _logger;
    private readonly ResilienceGate _resilienceGate;
    private readonly ConcurrentDictionary<string, TransferStatus> _status = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Inicializa o cliente HTTP com as dependências injetadas e configura o <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">Instância gerenciada pelo <c>IHttpClientFactory</c>.</param>
    /// <param name="options">Opções HTTP específicas (URL base, caminhos de endpoint, token, polling).</param>
    /// <param name="baseOptions">Opções globais (batch size, timeouts, retry, circuit breaker).</param>
    /// <param name="idempotencyStore">Store de idempotência para evitar reenvio de arquivos já processados.</param>
    /// <param name="auditSink">Sink de auditoria para registrar cada operação.</param>
    /// <param name="logger">Logger para diagnóstico de erros e rastreamento de operações.</param>
    public HttpMftMailboxClient(
        HttpClient httpClient,
        IOptions<HttpMftMailboxOptions> options,
        IOptions<MftMailboxOptions> baseOptions,
        IMftIdempotencyStore idempotencyStore,
        ITransferAuditSink auditSink,
        ILogger<HttpMftMailboxClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _baseOptions = baseOptions.Value;
        _idempotencyStore = idempotencyStore;
        _auditSink = auditSink;
        _logger = logger;
        _resilienceGate = new ResilienceGate(_baseOptions.CircuitBreaker);

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
        }

        if (!string.IsNullOrWhiteSpace(_options.BearerToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        }
    }

    /// <inheritdoc />
    public MftProtocol Protocol => MftProtocol.Https;

    /// <inheritdoc />
    public async Task<TransferItemResult> SendSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ValidateEnvelope(envelope);
        if (envelope.Items.Count != 1)
        {
            throw new ArgumentException("SendSingleAsync requires exactly one item in envelope.", nameof(envelope));
        }

        return await SendItemAsync(envelope, envelope.Items[0], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TransferBatchResult> SendBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ValidateEnvelope(envelope);
        if (envelope.Items.Count > _baseOptions.MaxBatchSize)
        {
            throw new InvalidOperationException($"Batch size {envelope.Items.Count} exceeds configured limit {_baseOptions.MaxBatchSize}.");
        }

        var result = new TransferBatchResult
        {
            StartedAtUtc = DateTimeOffset.UtcNow
        };

        foreach (var item in envelope.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Items.Add(await SendItemAsync(envelope, item, cancellationToken).ConfigureAwait(false));
        }

        result.FinishedAtUtc = DateTimeOffset.UtcNow;
        return result;
    }

    /// <inheritdoc />
    public async Task<InboundTransferItem?> ReceiveSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ValidateEnvelope(envelope);
        var batch = await ReceiveBatchAsync(envelope, cancellationToken).ConfigureAwait(false);
        return batch.FirstOrDefault();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Lista os arquivos disponíveis via GET em <c>HttpMftMailboxOptions.ListPath</c>,
    /// faz o download de cada item até <c>MftMailboxOptions.MaxBatchSize</c> e retorna
    /// os streams de conteúdo. O circuit breaker é consultado antes da listagem.
    /// Cada arquivo é baixado com streaming para arquivo temporário (DeleteOnClose).
    /// </remarks>
    public async Task<IReadOnlyCollection<InboundTransferItem>> ReceiveBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ValidateEnvelope(envelope);
        _resilienceGate.EnsureCanExecute();

        var list = await RetryExecutor.ExecuteAsync(
            async () =>
            {
                using var response = await _httpClient.GetAsync(_options.ListPath, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<MailboxListResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
                return payload?.Items ?? new List<MailboxListItem>();
            },
            IsTransient,
            _baseOptions.Retry,
            cancellationToken).ConfigureAwait(false);

        var inbound = new List<InboundTransferItem>();
        foreach (var candidate in list.Take(_baseOptions.MaxBatchSize))
        {
            var item = await DownloadInboundAsync(candidate, cancellationToken).ConfigureAwait(false);
            inbound.Add(item);
        }

        _resilienceGate.RegisterSuccess();
        return inbound;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Verifica primeiro o cache em memória da instância atual. Se não encontrado,
    /// consulta o endpoint remoto em <c>HttpMftMailboxOptions.StatusPath</c> via GET
    /// com parâmetros <c>integrationKey</c> e <c>itemId</c>.
    /// Retorna <see langword="null"/> quando o servidor responde 404.
    /// </remarks>
    public async Task<TransferStatus?> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var localKey = BuildStatusKey(integrationKey, itemId);
        if (_status.TryGetValue(localKey, out var existing))
        {
            return existing;
        }

        var path = $"{_options.StatusPath.TrimEnd('/')}?integrationKey={Uri.EscapeDataString(integrationKey)}&itemId={Uri.EscapeDataString(itemId)}";
        using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TransferStatus>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Envia o comando como JSON via POST para <c>HttpMftMailboxOptions.AckNackPath</c>.
    /// A operação é protegida por retry para erros transitórios. A auditoria é gravada ao final.
    /// </remarks>
    public async Task<TransferItemResult> AckOrNackAsync(AckNackCommand command, CancellationToken cancellationToken = default)
    {
        var response = await RetryExecutor.ExecuteAsync(
            async () =>
            {
                var message = await _httpClient.PostAsJsonAsync(_options.AckNackPath, command, cancellationToken).ConfigureAwait(false);
                message.EnsureSuccessStatusCode();
                return message;
            },
            IsTransient,
            _baseOptions.Retry,
            cancellationToken).ConfigureAwait(false);

        response.Dispose();

        var result = new TransferItemResult
        {
            ItemId = command.ItemId,
            FileName = command.FileName,
            State = TransferState.Succeeded,
            StatusCode = "ACKNACK_OK",
            Message = command.Decision.ToString()
        };

        await _auditSink.WriteAsync(new TransferAuditEntry
        {
            CorrelationId = command.CorrelationId,
            FileName = command.FileName,
            IntegrationKey = command.IntegrationKey,
            ItemId = command.ItemId,
            Protocol = Protocol,
            State = TransferState.Succeeded,
            Message = result.Message
        }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    private async Task<TransferItemResult> SendItemAsync(TransferEnvelope envelope, TransferItem item, CancellationToken cancellationToken)
    {
        if (item.ContentFactory is null)
        {
            throw new InvalidOperationException($"Transfer item '{item.ItemId}' requires a ContentFactory.");
        }

        _resilienceGate.EnsureCanExecute();

        var idempotencyKey = IdempotencyKeyBuilder.Build(envelope, item);
        var statusKey = BuildStatusKey(envelope.IntegrationKey, item.ItemId);

        var result = new TransferItemResult
        {
            ItemId = item.ItemId,
            FileName = item.FileName,
            State = TransferState.Pending
        };
        ExceptionDispatchInfo? capturedException = null;

        if (!await _idempotencyStore.TryStartAsync(idempotencyKey, cancellationToken).ConfigureAwait(false))
        {
            result.State = TransferState.Skipped;
            result.StatusCode = "IDEMPOTENT_SKIP";
            result.Message = "Transfer already processed.";
            _status[statusKey] = BuildStatus(envelope, item, idempotencyKey, result);
            return result;
        }

        try
        {
            var (checksum, length) = await RetryExecutor.ExecuteAsync(
                async () =>
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(_baseOptions.FileTimeout);

                    await using var stream = await item.ContentFactory(timeoutCts.Token).ConfigureAwait(false);
                    ValidateFileSize(stream, item);

                    var checksum = await ChecksumCalculator.Sha256Async(stream, timeoutCts.Token).ConfigureAwait(false);

                    using var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(streamContent, "file", item.FileName);
                    content.Add(new StringContent(item.ItemId), "itemId");
                    content.Add(new StringContent(envelope.IntegrationKey), "integrationKey");
                    content.Add(new StringContent(envelope.CorrelationId), "correlationId");

                    using var response = await _httpClient.PostAsync(_options.UploadPath, content, timeoutCts.Token).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    await PollStatusUntilFinalAsync(envelope.IntegrationKey, item.ItemId, timeoutCts.Token).ConfigureAwait(false);

                    var size = stream.CanSeek ? stream.Length : item.SizeBytes;
                    return (checksum, size);
                },
                IsTransient,
                _baseOptions.Retry,
                cancellationToken).ConfigureAwait(false);

            result.State = TransferState.Succeeded;
            result.StatusCode = "UPLOADED";
            result.Message = "File uploaded successfully.";
            result.ChecksumSha256 = checksum;
            result.SizeBytes = length;

            await _idempotencyStore.MarkCompletedAsync(idempotencyKey, cancellationToken).ConfigureAwait(false);
            _resilienceGate.RegisterSuccess();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _resilienceGate.RegisterFailure();
            throw;
        }
        catch (Exception ex)
        {
            _resilienceGate.RegisterFailure();

            result.State = TransferState.Failed;
            result.StatusCode = "UPLOAD_FAILED";
            result.Message = ex.Message;
            result.IsTransientFailure = IsTransient(ex);

            await _idempotencyStore.MarkFailedAsync(idempotencyKey, ex.Message, cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "MFT HTTP transfer failed for integration {IntegrationKey} item {ItemId}", envelope.IntegrationKey, item.ItemId);

            if (!IsTransient(ex))
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }
        }

        _status[statusKey] = BuildStatus(envelope, item, idempotencyKey, result);

        await _auditSink.WriteAsync(new TransferAuditEntry
        {
            CorrelationId = envelope.CorrelationId,
            FileName = item.FileName,
            IntegrationKey = envelope.IntegrationKey,
            ItemId = item.ItemId,
            Protocol = Protocol,
            State = result.State,
            Message = result.Message
        }, cancellationToken).ConfigureAwait(false);

        capturedException?.Throw();

        return result;
    }

    private async Task<InboundTransferItem> DownloadInboundAsync(MailboxListItem item, CancellationToken cancellationToken)
    {
        var url = $"{_options.DownloadPath.TrimEnd('/')}?remotePath={Uri.EscapeDataString(item.RemotePath)}";

        using var response = await RetryExecutor.ExecuteAsync(
            () => _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken),
            IsTransient,
            _baseOptions.Retry,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var tempPath = Path.GetTempFileName();
        await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
        await using (var write = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await source.CopyToAsync(write, cancellationToken).ConfigureAwait(false);
        }

        var readStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
        var checksum = await ChecksumCalculator.Sha256Async(readStream, cancellationToken).ConfigureAwait(false);

        return new InboundTransferItem
        {
            ItemId = item.ItemId,
            FileName = item.FileName,
            RemotePath = item.RemotePath,
            SizeBytes = readStream.Length,
            ChecksumSha256 = checksum,
            Content = readStream,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["remotePath"] = item.RemotePath
            }
        };
    }

    private async Task PollStatusUntilFinalAsync(string integrationKey, string itemId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < _options.StatusPollingMaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await GetStatusAsync(integrationKey, itemId, cancellationToken).ConfigureAwait(false);
            if (status is null)
            {
                await Task.Delay(BuildBackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (status.State is TransferState.Succeeded or TransferState.Failed)
            {
                if (status.State == TransferState.Failed)
                {
                    throw new InvalidOperationException(status.Error ?? "Remote transfer failed.");
                }

                return;
            }

            await Task.Delay(BuildBackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
        }
    }

    private TimeSpan BuildBackoffDelay(int attempt)
    {
        var multiplier = Math.Pow(2, attempt);
        var delay = _options.StatusPollingBaseDelay.TotalMilliseconds * multiplier;
        return TimeSpan.FromMilliseconds(Math.Min(delay, 30000));
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException or TaskCanceledException or TimeoutException;
    }

    private TransferStatus BuildStatus(TransferEnvelope envelope, TransferItem item, string idempotencyKey, TransferItemResult result)
    {
        return new TransferStatus
        {
            IntegrationKey = envelope.IntegrationKey,
            ItemId = item.ItemId,
            IdempotencyKey = idempotencyKey,
            State = result.State,
            ExternalStatus = result.StatusCode,
            ChecksumSha256 = result.ChecksumSha256,
            SizeBytes = result.SizeBytes,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Error = result.State == TransferState.Failed ? result.Message : null
        };
    }

    private static string BuildStatusKey(string integrationKey, string itemId)
    {
        return $"{integrationKey}|{itemId}".ToLowerInvariant();
    }

    private static void ValidateEnvelope(TransferEnvelope envelope)
    {
        if (envelope is null)
        {
            throw new ArgumentNullException(nameof(envelope));
        }

        if (string.IsNullOrWhiteSpace(envelope.IntegrationKey))
        {
            throw new ArgumentException("IntegrationKey is required.", nameof(envelope));
        }
    }

    private void ValidateFileSize(Stream stream, TransferItem item)
    {
        var size = stream.CanSeek ? stream.Length : item.SizeBytes;
        if (!size.HasValue)
        {
            return;
        }

        if (size.Value > _baseOptions.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File '{item.FileName}' exceeds max size {_baseOptions.MaxFileSizeBytes} bytes.");
        }
    }
}
