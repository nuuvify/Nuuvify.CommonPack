using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Protocols;
using Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;
using Nuuvify.CommonPack.MftMailbox.Utilities;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Nuuvify.CommonPack.MftMailbox.Sftp;

/// <summary>
/// Cliente MFT via SFTP (SSH File Transfer Protocol), implementando envio, recepção,
/// consulta de status e ACK/NACK de arquivos.
/// </summary>
/// <remarks>
/// Registrado como Singleton via <c>SftpMftMailboxSetup.AddMftMailboxSftp</c> e resolvido
/// pela <c>MftClientFactory</c> para o protocolo <see cref="MftProtocol.Sftp"/>.
/// <para>
/// Recursos integrados:
/// <list type="bullet">
/// <item>Idempotência por chave composta via <c>IMftIdempotencyStore</c>.</item>
/// <item>Checksum SHA-256 calculado após upload e download.</item>
/// <item>Retry com backoff exponencial e jitter configurável em <c>MftMailboxOptions.Retry</c>.</item>
/// <item>Circuit breaker configurável em <c>MftMailboxOptions.CircuitBreaker</c>.</item>
/// <item>Upload atômico usando arquivo temporário (.tmp) renomeado após envio completo.</item>
/// <item>Download para arquivo temporário local com exclusão automática no descarte.</item>
/// <item>Auditoria de cada operação via <c>ITransferAuditSink</c>.</item>
/// </list>
/// </para>
/// <para>
/// A autenticação suporta senha (<c>SftpMftMailboxOptions.Password</c>) e chave privada
/// (<c>SftpMftMailboxOptions.PrivateKeyPath</c>). Configure <c>HostKeyFingerprint</c>
/// para validar a identidade do servidor e evitar ataques man-in-the-middle.
/// </para>
/// </remarks>
public sealed class SftpMftMailboxClient : IProtocolMftClient
{
    private readonly SftpMftMailboxOptions _options;
    private readonly MftMailboxOptions _baseOptions;
    private readonly IMftIdempotencyStore _idempotencyStore;
    private readonly ITransferAuditSink _auditSink;
    private readonly ILogger<SftpMftMailboxClient> _logger;
    private readonly ResilienceGate _resilienceGate;
    private readonly ConcurrentDictionary<string, TransferStatus> _status = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Inicializa o cliente SFTP com as opções e dependências injetadas.
    /// </summary>
    /// <param name="options">Opções específicas do SFTP (host, porta, credenciais, diretórios).</param>
    /// <param name="baseOptions">Opções globais (batch size, timeouts, retry, circuit breaker).</param>
    /// <param name="idempotencyStore">Store de idempotência para evitar reenvio de arquivos já processados.</param>
    /// <param name="auditSink">Sink de auditoria para registrar cada operação.</param>
    /// <param name="logger">Logger para diagnóstico de erros e rastreamento de operações.</param>
    public SftpMftMailboxClient(
        IOptions<SftpMftMailboxOptions> options,
        IOptions<MftMailboxOptions> baseOptions,
        IMftIdempotencyStore idempotencyStore,
        ITransferAuditSink auditSink,
        ILogger<SftpMftMailboxClient> logger)
    {
        _options = options.Value;
        _baseOptions = baseOptions.Value;
        _idempotencyStore = idempotencyStore;
        _auditSink = auditSink;
        _logger = logger;
        _resilienceGate = new ResilienceGate(_baseOptions.CircuitBreaker);
    }

    /// <inheritdoc />
    public MftProtocol Protocol => MftProtocol.Sftp;

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

        var items = await ReceiveBatchAsync(envelope, cancellationToken).ConfigureAwait(false);
        return items.FirstOrDefault();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Quando <see cref="TransferEnvelope.Items"/> está vazio, lista todos os arquivos disponíveis
    /// em <c>SftpMftMailboxOptions.InboundDirectory</c> e baixa até <c>MftMailboxOptions.MaxBatchSize</c>.
    /// Cada arquivo é baixado para um arquivo temporário local (FileOptions.DeleteOnClose)
    /// e exposto como stream no <see cref="InboundTransferItem.Content"/>.
    /// O circuit breaker é consultado antes da listagem e confirmado ao final do lote.
    /// </remarks>
    public async Task<IReadOnlyCollection<InboundTransferItem>> ReceiveBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ValidateEnvelope(envelope);

        _resilienceGate.EnsureCanExecute();

        var paths = envelope.Items.Count > 0
            ? envelope.Items.Select(x => ResolveInboundPath(x)).ToList()
            : await ListInboundFilesAsync(cancellationToken).ConfigureAwait(false);

        if (paths.Count > _baseOptions.MaxBatchSize)
        {
            paths = paths.Take(_baseOptions.MaxBatchSize).ToList();
        }

        var items = new List<InboundTransferItem>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = await RetryExecutor.ExecuteAsync(
                () => DownloadInboundFileAsync(path, cancellationToken),
                IsTransient,
                _baseOptions.Retry,
                cancellationToken).ConfigureAwait(false);

            items.Add(item);
        }

        _resilienceGate.RegisterSuccess();
        return items;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Consulta o cache em memória populado durante as operações de envio desta instância.
    /// Não realiza chamada de rede. Para status persistente entre instâncias, use a implementação HTTP.
    /// </remarks>
    public Task<TransferStatus?> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildStatusKey(integrationKey, itemId);
        _ = _status.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    /// <remarks>
    /// O comportamento depende de <see cref="SftpMftMailboxOptions.AckNackMode"/>:
    /// <list type="bullet">
    /// <item><see cref="SftpAckNackMode.Metadata"/>: move o arquivo de <c>InboundDirectory</c>
    /// para <c>ArchiveSuccessDirectory</c> (ACK) ou <c>ArchiveErrorDirectory</c> (NACK).</item>
    /// <item><see cref="SftpAckNackMode.MarkerFile"/>: cria um arquivo <c>&lt;filename&gt;.ack</c>
    /// ou <c>&lt;filename&gt;.nack</c> em <c>AckMarkerDirectory</c>.</item>
    /// </list>
    /// A operação é protegida por retry. A auditoria é gravada ao final.
    /// </remarks>
    public async Task<TransferItemResult> AckOrNackAsync(AckNackCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ItemId))
        {
            throw new ArgumentException("ItemId is required.", nameof(command));
        }

        var fileName = string.IsNullOrWhiteSpace(command.FileName) ? command.ItemId : command.FileName;

        await RetryExecutor.ExecuteAsync(
            async () =>
            {
                await Task.Run(() =>
                {
                    using var sftp = CreateClient();
                    sftp.Connect();

                    if (_options.AckNackMode == SftpAckNackMode.Metadata)
                    {
                        var source = CombinePath(_options.InboundDirectory, fileName);
                        var target = command.Decision == AckNackType.Ack
                            ? CombinePath(_options.ArchiveSuccessDirectory, fileName)
                            : CombinePath(_options.ArchiveErrorDirectory, fileName);

                        EnsureDirectory(sftp, Path.GetDirectoryName(target)?.Replace("\\", "/") ?? "/");
                        if (sftp.Exists(source))
                        {
                            sftp.RenameFile(source, target);
                        }
                    }
                    else
                    {
                        EnsureDirectory(sftp, _options.AckMarkerDirectory);
                        var suffix = command.Decision == AckNackType.Ack ? "ack" : "nack";
                        var markerPath = CombinePath(_options.AckMarkerDirectory, $"{fileName}.{suffix}");
                        using var markerStream = sftp.Create(markerPath);
                        var marker = System.Text.Encoding.UTF8.GetBytes(command.Reason ?? suffix);
                        markerStream.Write(marker, 0, marker.Length);
                    }

                    sftp.Disconnect();
                }, cancellationToken).ConfigureAwait(false);

                return true;
            },
            IsTransient,
            _baseOptions.Retry,
            cancellationToken).ConfigureAwait(false);

        var result = new TransferItemResult
        {
            ItemId = command.ItemId,
            FileName = fileName,
            State = TransferState.Succeeded,
            StatusCode = "ACKNACK_OK",
            Message = command.Decision.ToString()
        };

        await _auditSink.WriteAsync(new TransferAuditEntry
        {
            CorrelationId = command.CorrelationId,
            FileName = fileName,
            IntegrationKey = command.IntegrationKey,
            ItemId = command.ItemId,
            Protocol = Protocol,
            State = TransferState.Succeeded,
            Message = $"ACK/NACK processed as {command.Decision}."
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

        var result = new TransferItemResult
        {
            ItemId = item.ItemId,
            FileName = item.FileName,
            State = TransferState.Pending
        };

        var idempotencyKey = IdempotencyKeyBuilder.Build(envelope, item);
        var statusKey = BuildStatusKey(envelope.IntegrationKey, item.ItemId);

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

                    await Task.Run(() =>
                    {
                        using var sftp = CreateClient();
                        sftp.Connect();

                        EnsureDirectory(sftp, _options.OutboundDirectory);

                        var remotePath = ResolveOutboundPath(item);
                        var tempPath = $"{remotePath}.tmp-{Guid.NewGuid():N}";

                        sftp.UploadFile(stream, tempPath, true);

                        if (sftp.Exists(remotePath))
                        {
                            sftp.DeleteFile(remotePath);
                        }

                        sftp.RenameFile(tempPath, remotePath);
                        sftp.Disconnect();
                    }, timeoutCts.Token).ConfigureAwait(false);

                    var length = stream.CanSeek ? stream.Length : item.SizeBytes;
                    return (checksum, length);
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
        catch (Exception ex)
        {
            _resilienceGate.RegisterFailure();

            result.State = TransferState.Failed;
            result.StatusCode = "UPLOAD_FAILED";
            result.Message = ex.Message;
            result.IsTransientFailure = IsTransient(ex);

            await _idempotencyStore.MarkFailedAsync(idempotencyKey, ex.Message, cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "MFT SFTP transfer failed for integration {IntegrationKey} item {ItemId}", envelope.IntegrationKey, item.ItemId);
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

        return result;
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

    private SftpClient CreateClient()
    {
        var connectionInfo = BuildConnectionInfo();
        var client = new SftpClient(connectionInfo);

        if (!string.IsNullOrWhiteSpace(_options.HostKeyFingerprint))
        {
            client.HostKeyReceived += (_, args) =>
            {
                var fingerprint = Convert.ToHexString(args.FingerPrint).ToLowerInvariant();
                var expected = _options.HostKeyFingerprint.Replace(":", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
                args.CanTrust = string.Equals(fingerprint, expected, StringComparison.OrdinalIgnoreCase);
            };
        }

        return client;
    }

    private ConnectionInfo BuildConnectionInfo()
    {
        if (!string.IsNullOrWhiteSpace(_options.PrivateKeyPath))
        {
            var keyFile = string.IsNullOrWhiteSpace(_options.PrivateKeyPassphrase)
                ? new PrivateKeyFile(_options.PrivateKeyPath)
                : new PrivateKeyFile(_options.PrivateKeyPath, _options.PrivateKeyPassphrase);

            return new ConnectionInfo(_options.Host, _options.Port, _options.Username, new PrivateKeyAuthenticationMethod(_options.Username, keyFile));
        }

        if (!string.IsNullOrWhiteSpace(_options.Password))
        {
            return new PasswordConnectionInfo(_options.Host, _options.Port, _options.Username, _options.Password);
        }

        throw new InvalidOperationException("SFTP credentials are not configured. Configure Password or PrivateKeyPath.");
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is SshConnectionException
            or SshOperationTimeoutException
            or SocketException
            or IOException
            or TimeoutException;
    }

    private string ResolveOutboundPath(TransferItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.RemotePath))
        {
            return item.RemotePath;
        }

        return CombinePath(_options.OutboundDirectory, item.FileName);
    }

    private string ResolveInboundPath(TransferItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.RemotePath))
        {
            return item.RemotePath;
        }

        return CombinePath(_options.InboundDirectory, item.FileName);
    }

    private async Task<List<string>> ListInboundFilesAsync(CancellationToken cancellationToken)
    {
        return await RetryExecutor.ExecuteAsync(
            async () =>
            {
                return await Task.Run(() =>
                {
                    using var sftp = CreateClient();
                    sftp.Connect();

                    var files = sftp.ListDirectory(_options.InboundDirectory)
                        .Where(x => !x.IsDirectory && !x.IsSymbolicLink)
                        .Select(x => x.FullName)
                        .ToList();

                    sftp.Disconnect();
                    return files;
                }, cancellationToken).ConfigureAwait(false);
            },
            IsTransient,
            _baseOptions.Retry,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<InboundTransferItem> DownloadInboundFileAsync(string remotePath, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_baseOptions.FileTimeout);

        return await Task.Run(async () =>
        {
            var tempPath = Path.GetTempFileName();
            var fileName = Path.GetFileName(remotePath);

            using (var sftp = CreateClient())
            {
                sftp.Connect();
                await using var writeStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                sftp.DownloadFile(remotePath, writeStream);
                await writeStream.FlushAsync(timeoutCts.Token).ConfigureAwait(false);
                sftp.Disconnect();
            }

            var readStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
            var checksum = await ChecksumCalculator.Sha256Async(readStream, timeoutCts.Token).ConfigureAwait(false);

            return new InboundTransferItem
            {
                ItemId = fileName,
                FileName = fileName,
                RemotePath = remotePath,
                SizeBytes = readStream.Length,
                ChecksumSha256 = checksum,
                Content = readStream,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["remotePath"] = remotePath
                }
            };
        }, timeoutCts.Token).ConfigureAwait(false);
    }

    private static string CombinePath(string folder, string file)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return file;
        }

        return $"{folder.TrimEnd('/')}/{file.TrimStart('/')}";
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

    private static void EnsureDirectory(SftpClient sftp, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath) || fullPath == "/")
        {
            return;
        }

        var segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = "/";

        foreach (var segment in segments)
        {
            current = current.EndsWith('/') ? current + segment : current + "/" + segment;
            if (!sftp.Exists(current))
            {
                sftp.CreateDirectory(current);
            }
        }
    }
}
