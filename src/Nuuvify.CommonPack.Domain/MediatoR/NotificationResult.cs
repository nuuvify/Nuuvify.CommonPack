using System;
using Nuuvify.CommonPack.Extensions.Interfaces;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    
    /// <summary>
    /// Essa clase deve ser utilizada para manipular informações entre camadas "projetos", 
    /// que não tenha o intuito de Erro, ou inconsistencia de dados, apenas mensagens para avisar
    /// o client ou utilizar como log
    /// Se sua necessidade é manipular informações entre camadas, para retornar erro ao client,
    /// utilize NotificationR
    /// </summary>
    public class NotificationResult : NotificationR, ICommandResultR, INotPersistingAsTable
    {
    
        public NotificationResult(string property, string message) 
            : base(property, message)
        {
        }

        public NotificationResult(string property, string message, string aggregatorId) 
            : base(property, message, aggregatorId)
        {
        }

        public NotificationResult(string property, string message, string aggregatorId, string type, Type originNotification) 
            : base(property, message, aggregatorId, type, originNotification)
        {
        }
    
    }

}