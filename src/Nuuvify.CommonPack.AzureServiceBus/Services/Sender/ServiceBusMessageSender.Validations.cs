namespace Nuuvify.CommonPack.AzureServiceBus.Services.Sender;

/// <summary>
/// Implementação do Azure Service Bus - Métodos de Validação
/// </summary>
public partial class ServiceBusMessageSender
{
    #region Métodos de Validação

    /// <summary>
    /// Verifica se o objeto foi disposed e lança exceção caso tenha sido
    /// </summary>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <remarks>
    /// <para>Deve ser chamado no início de todos os métodos públicos para garantir que:</para>
    /// <list type="bullet">
    /// <item><description>Operações não sejam executadas em objetos disposed</description></item>
    /// <item><description>Resources cleanup seja respeitado</description></item>
    /// <item><description>Comportamento determinístico após Dispose()</description></item>
    /// </list>
    /// <para>Implementa o padrão Dispose corretamente conforme diretrizes .NET.</para>
    /// </remarks>
    private void ValidateDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceBusMessageSender));
        }
    }

    /// <summary>
    /// Valida parâmetros de entrada dos métodos públicos de forma genérica
    /// </summary>
    /// <typeparam name="T">Tipo do parâmetro a ser validado</typeparam>
    /// <param name="parameter">Valor do parâmetro a ser validado</param>
    /// <param name="parameterName">Nome do parâmetro para mensagens de erro</param>
    /// <exception cref="ArgumentNullException">Quando o parâmetro é nulo</exception>
    /// <exception cref="ArgumentException">Quando string é vazia ou contém apenas espaços em branco</exception>
    /// <remarks>
    /// <para>Validações realizadas por tipo:</para>
    /// <list type="bullet">
    /// <item><description>Qualquer tipo: Verifica se não é o valor padrão (null para reference types)</description></item>
    /// <item><description>String: Verifica se não é null, vazia ou apenas whitespace</description></item>
    /// </list>
    /// <para>Método estático para permitir reutilização e melhor performance.</para>
    /// <para>Centraliza lógica de validação evitando duplicação de código.</para>
    /// </remarks>
    private static void ValidateParameter<T>(T parameter, string parameterName)
    {
        if (object.Equals(parameter, default(T)))
        {
            throw new ArgumentNullException(parameterName);
        }

        if (parameter is string str && string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException($"{parameterName} não pode ser nulo ou vazio", parameterName);
        }
    }

    #endregion
}
