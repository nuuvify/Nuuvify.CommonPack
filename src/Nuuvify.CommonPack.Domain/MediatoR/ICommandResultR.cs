using MediatR;

namespace Nuuvify.CommonPack.Domain;

/// <summary>
/// Use essa interface quando desejar retornar um objeto pelo Handler
/// <para>
/// Também pode ser utilizada para criar uma classe de publish do MediatR para
/// notificar algum evento, para uma ou mais classes que implementem:
/// </para>
/// <para>
/// public class ClienteObserve : INotificationHandler{SuaClasseNotification}
/// </para>
/// <para>
/// public class FornecedorObserve : INotificationHandler{SuaClasseNotification}
/// </para>
/// <para>
/// public class TransportadorObserve : INotificationHandler{SuaClasseNotification}
/// </para>
/// <para>
/// onde: SuaClasseNotification : ICommandResultR
/// </para>
/// <para>
/// nesse caso o gatilho seria _mediator.Publish(SuaClasseNotification)
/// </para>
/// <para>
/// Isso dispararia ao mesmo tempo as classes ClienteObserve, FornecedorObserve e TransportadorObserve
/// </para>
/// </summary>
public interface ICommandResultR : INotification
{

}
