using Nuuvify.CommonPack.MftMailbox.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.Utilities;

/// <summary>
/// Implementação thread-safe de circuit breaker para proteger operações MFT.
/// </summary>
/// <remarks>
/// O circuit abre (estado aberto) após atingir <see cref="CircuitBreakerOptions.FailureThreshold"/>
/// falhas consecutivas e permanece aberto por <see cref="CircuitBreakerOptions.BreakDuration"/>.
/// Após a duração, o estado retorna automaticamente para fechado (closed) na próxima chamada
/// a <see cref="EnsureCanExecute"/>. Não há estado half-open explícito; a primeira chamada
/// após o período de break sempre tenta executar.
/// </remarks>
public sealed class ResilienceGate
{
    private readonly object _sync = new();
    private readonly CircuitBreakerOptions _options;
    private int _failureCount;
    private DateTimeOffset? _openUntilUtc;

    /// <summary>
    /// Inicializa o circuit breaker com as opções fornecidas.
    /// </summary>
    /// <param name="options">Parâmetros de threshold e duração do break.</param>
    /// <exception cref="ArgumentNullException">Lançado quando <paramref name="options"/> é <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Lançado quando o threshold de falha ou a duração do break são inválidos.</exception>
    public ResilienceGate(CircuitBreakerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.FailureThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "FailureThreshold must be greater than zero.");
        }

        if (options.BreakDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "BreakDuration must be greater than zero.");
        }

        _options = options;
    }

    /// <summary>
    /// Verifica se o circuit está fechado e a operação pode ser executada.
    /// Redefine automaticamente o estado para fechado quando o período de break expirou.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançado quando o circuit está aberto. A mensagem inclui o instante de reabertura.
    /// </exception>
    public void EnsureCanExecute()
    {
        lock (_sync)
        {
            if (_openUntilUtc.HasValue && _openUntilUtc.Value > DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException($"Circuit breaker open until {_openUntilUtc.Value:O}.");
            }

            if (_openUntilUtc.HasValue && _openUntilUtc.Value <= DateTimeOffset.UtcNow)
            {
                _openUntilUtc = null;
                _failureCount = 0;
            }
        }
    }

    /// <summary>
    /// Registra uma execução bem-sucedida, reiniciando o contador de falhas e fechando o circuit.
    /// </summary>
    public void RegisterSuccess()
    {
        lock (_sync)
        {
            _failureCount = 0;
            _openUntilUtc = null;
        }
    }

    /// <summary>
    /// Registra uma falha. Quando o número de falhas consecutivas atinge
    /// <see cref="CircuitBreakerOptions.FailureThreshold"/>, o circuit é aberto
    /// por <see cref="CircuitBreakerOptions.BreakDuration"/>.
    /// </summary>
    public void RegisterFailure()
    {
        lock (_sync)
        {
            _failureCount++;
            if (_failureCount >= _options.FailureThreshold)
            {
                _openUntilUtc = DateTimeOffset.UtcNow.Add(_options.BreakDuration);
            }
        }
    }
}
