namespace Nuuvify.CommonPack.BackgroundService.Models;

/// <summary>
/// Ação a ser aplicada para uma mensagem lida da Dead Letter Queue.
/// </summary>
public enum DeadLetterMessageAction
{
    /// <summary>
    /// Descarta a mensagem da Dead Letter Queue através de complete.
    /// </summary>
    Discard = 0,

    /// <summary>
    /// Reenvia a mensagem para a mesma origem (fila ou tópico) com nova identidade.
    /// </summary>
    RequeueToOrigin = 1
}
