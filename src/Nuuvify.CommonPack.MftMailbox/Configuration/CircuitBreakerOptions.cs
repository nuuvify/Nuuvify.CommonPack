namespace Nuuvify.CommonPack.MftMailbox.Configuration;

/// <summary>
/// Parâmetros do circuit breaker integrado ao cliente MFT.
/// </summary>
/// <remarks>
/// O circuit breaker abre após <see cref="FailureThreshold"/> falhas consecutivas e permanece aberto
/// por <see cref="BreakDuration"/>. Durante esse período, todas as chamadas falham imediatamente
/// com <see cref="InvalidOperationException"/> sem tentar conectar ao servidor MFT,
/// protegendo o servidor de sobrecarga e dando tempo para recuperação.
/// Configurado dentro de <see cref="MftMailboxOptions.CircuitBreaker"/>.
/// </remarks>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Número de falhas consecutivas que abre o circuit breaker.
    /// Padrão: 5 falhas.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duração do estado aberto (open) do circuit breaker após atingir <see cref="FailureThreshold"/>.
    /// Após esse período, o circuit testa automaticamente uma nova chamada (half-open implícito).
    /// Padrão: 60 segundos.
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(60);
}
