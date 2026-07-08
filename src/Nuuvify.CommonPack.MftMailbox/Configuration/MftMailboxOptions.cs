namespace Nuuvify.CommonPack.MftMailbox.Configuration;

/// <summary>
/// Opções globais do pacote MftMailbox, compartilhadas por todos os protocolos.
/// </summary>
/// <remarks>
/// Configurado via <c>services.AddMftMailboxCore(options => { ... })</c> ou pelo arquivo
/// de configuração usando a seção <c>MftMailbox</c>. Os valores padrão cobrem a maioria
/// dos casos de uso; ajuste apenas o que a integração exigir.
/// </remarks>
public sealed class MftMailboxOptions
{
    /// <summary>Tamanho máximo padrão de um lote: 500 itens.</summary>
    public const int DefaultMaxBatchSize = 500;

    /// <summary>Tamanho máximo padrão de arquivo: 1 GiB (1.073.741.824 bytes).</summary>
    public const long DefaultMaxFileSizeBytes = 1_073_741_824;

    /// <summary>
    /// Número máximo de itens por lote. Exceder este limite gera <see cref="InvalidOperationException"/>.
    /// Padrão: <see cref="DefaultMaxBatchSize"/> (500).
    /// </summary>
    public int MaxBatchSize { get; set; } = DefaultMaxBatchSize;

    /// <summary>
    /// Tamanho máximo aceito por arquivo individual, em bytes.
    /// Padrão: <see cref="DefaultMaxFileSizeBytes"/> (1 GiB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = DefaultMaxFileSizeBytes;

    /// <summary>
    /// Grau máximo de paralelismo usado em operações de lote.
    /// Padrão: 4 tarefas simultâneas.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;

    /// <summary>
    /// Timeout para a transferência de um único arquivo.
    /// Padrão: 5 minutos.
    /// </summary>
    public TimeSpan FileTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout total para a execução de um lote completo.
    /// Padrão: 30 minutos.
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>Parâmetros da política de retry com backoff exponencial.</summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>Parâmetros do circuit breaker que protege o servidor MFT de sobrecarga.</summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
}
