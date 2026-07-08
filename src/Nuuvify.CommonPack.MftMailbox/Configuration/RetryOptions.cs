namespace Nuuvify.CommonPack.MftMailbox.Configuration;

/// <summary>
/// Parâmetros da política de retry com backoff exponencial aplicada pelos clientes MFT.
/// </summary>
/// <remarks>
/// O atraso de cada tentativa é calculado como <c>BaseDelay * 2^tentativa</c>, limitado a <see cref="MaxDelay"/>.
/// Quando <see cref="UseJitter"/> está ativo, um valor aleatório entre 25 ms e 250 ms é adicionado
/// para evitar thundering herd em cenários com múltiplos workers.
/// Configurado dentro de <see cref="MftMailboxOptions.Retry"/>.
/// </remarks>
public sealed class RetryOptions
{
    /// <summary>
    /// Número máximo de tentativas após a falha inicial (não inclui a primeira tentativa).
    /// Padrão: 3 tentativas.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Atraso base para o cálculo de backoff exponencial.
    /// Padrão: 2 segundos.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Atraso máximo permitido entre tentativas, independente do expoente calculado.
    /// Padrão: 30 segundos.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Quando <see langword="true"/>, adiciona um valor aleatório (jitter) ao atraso calculado
    /// para evitar sincronismo de retries em cenários de alta concorrência.
    /// Padrão: <see langword="true"/>.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}
