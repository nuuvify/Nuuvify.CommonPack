using Nuuvify.CommonPack.MftMailbox.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.Utilities;

/// <summary>
/// Executor genérico de retry com backoff exponencial e jitter opcional.
/// </summary>
/// <remarks>
/// Utilizado internamente pelos clientes MFT para envolver operações de rede.
/// O comportamento de retry é controlado por <see cref="RetryOptions"/>.
/// Apenas exceções identificadas por <c>transientPredicate</c> como transitórias são retentadas;
/// erros permanentes propagam imediatamente.
/// </remarks>
public static class RetryExecutor
{
    /// <summary>
    /// Executa a operação assincronamente, retentando em caso de falhas transitórias
    /// com backoff exponencial conforme as opções fornecidas.
    /// </summary>
    /// <typeparam name="T">Tipo do resultado da operação.</typeparam>
    /// <param name="operation">Delegate assíncrono que representa a operação a ser executada.</param>
    /// <param name="transientPredicate">
    /// Função que avalia se uma exceção é transitória e deve desencadear um retry.
    /// Retorne <see langword="true"/> para falhas de rede, timeouts e indisponibilidade temporária;
    /// <see langword="false"/> para erros permanentes (ex.: autenticação inválida).
    /// </param>
    /// <param name="options">Parâmetros de retry (número máximo, atraso base, atraso máximo e jitter).</param>
    /// <param name="cancellationToken">Token de cancelamento propagado para cada tentativa e para os atrasos.</param>
    /// <returns>Resultado da operação quando executada com êxito.</returns>
    /// <exception cref="ArgumentNullException">Lançado quando <paramref name="operation"/>, <paramref name="transientPredicate"/> ou <paramref name="options"/> são nulos.</exception>
    /// <exception cref="OperationCanceledException">Lançado quando o token de cancelamento é acionado durante uma tentativa ou atraso.</exception>
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<Exception, bool> transientPredicate, RetryOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(transientPredicate);
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxRetries cannot be negative.");
        }

        if (options.BaseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "BaseDelay cannot be negative.");
        }

        if (options.MaxDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxDelay cannot be negative.");
        }

        if (options.MaxDelay < options.BaseDelay)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "MaxDelay must be greater than or equal to BaseDelay.");
        }

        var random = options.UseJitter ? Random.Shared : null;
        var attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < options.MaxRetries && transientPredicate(ex))
            {
                var exponent = Math.Pow(2, attempt);
                var delayMs = options.BaseDelay.TotalMilliseconds * exponent;
                delayMs = Math.Min(delayMs, options.MaxDelay.TotalMilliseconds);
                if (random is not null)
                {
                    delayMs += random.Next(25, 250);
                }

                delayMs = Math.Min(delayMs, options.MaxDelay.TotalMilliseconds);

                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken).ConfigureAwait(false);
                attempt++;
            }
        }
    }
}
