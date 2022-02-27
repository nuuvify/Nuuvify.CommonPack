using System.ComponentModel;
using MediatR;

namespace Nuuvify.CommonPack.Domain
{
    /// <summary>
    /// Use essa interface quando deseja criar um Command para ser enviado a um Handler,
    /// utilizando _mediator.Send(SeuCommand)
    /// </summary>
    public interface ICommandR : IRequest<ICommandResultR>
    {

        /// <summary>
        /// Utilizado em um Handle, para chamar ou não o SaveChanges do Respository
        /// util em casos de processamento dentro de um laço, onde a gravação deve 
        /// ocorrer apenas no final, ou mesmo, em um processo que envolva varias 
        /// alterações no banco de dados, por diversas classes diferentes, mas o 
        /// SaveChanges deve ocorrer apenas uma vez, se esse for seu caso, deixe
        /// esse valor como false, e dentro do Handle utilize:
        /// <example>
        /// <code>
        ///     await _repository.SaveChangesAsync(toSave: request.SaveChanges);
        /// </code>
        /// </example>
        /// Onde request, é uma classe (FornecedorAdicionarCommand) que implementa
        /// ICommandR
        /// </summary>
        /// <value>true</value>
        [DefaultValue(true)]
        bool SaveChanges { get; set; }

        /// <summary>
        /// Em um processamento dentro de um laço, você pode querer remover as notificações
        /// em cada registro, caso faça sentido, atribua true, e inclua um codigo como esse
        /// em seu Handle:
        /// <example>
        /// <code>
        ///     RemoveNotifications(removeAll: request.RemoveNotificationsBeginning);
        /// </code>
        /// </example>
        /// </summary>
        /// <value></value>
        [DefaultValue(false)]
        bool RemoveNotificationsBeginning { get; set; }
    }

}