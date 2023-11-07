using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nuuvify.CommonPack.Extensions.Implementation;
using Microsoft.AspNetCore.Mvc;

namespace Nuuvify.CommonPack.Middleware
{
    /// <summary>
    /// Essa classe deve ser implementada por herança nas controllers, caso precise personalisar alguma funcionalidade
    /// você deve utilizar override nos metodos ou escrever seus proprios metodos
    /// </summary>
    public abstract class BaseCustomController : ControllerBase
    {


        protected virtual IDictionary<string, string> ObsoleteActionMessage()
        {
            var message = new Dictionary<string, string>();

            object[] obsoleteAction = ControllerContext.ActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            if (obsoleteAction.NotNullOrZero())
            {
                var obsoleteMessage = (ObsoleteAttribute)obsoleteAction?.FirstOrDefault();
                message.Add("x-obsolete-message", obsoleteMessage?.Message);
            }

            return message;
        }
        protected virtual Type StatusCodeProducesResponseTypeAction(StatusCodeResult statusCodeResult)
        {

            var typesAction = ControllerContext.ActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), true);

            if (typesAction.NotNullOrZero())
            {
                var producesResponse = typesAction.Where(x => x != null);

                foreach (var item in producesResponse)
                {
                    var producesType = (ProducesResponseTypeAttribute)item;
                    if (producesType.StatusCode == statusCodeResult.StatusCode)
                    {
                        return producesType.Type;
                    }
                }
            }

            throw new TypeAccessException(message: 
                $@"ProducesResponseType retorno com tipo informado e
                com status code {statusCodeResult.StatusCode} é obrigatorio na
                Controller {ControllerContext.ActionDescriptor.ControllerName}
                ActionResult {ControllerContext.ActionDescriptor.ActionName}");
        }


        protected virtual object GetInstanceResponse(Type tipoResponseObject, object result)
        {

            if (IsNull(tipoResponseObject))
            {
                throw new TypeAccessException(message: 
                $@"ProducesResponseType retorno com o tipo informado é obrigatorio na
                Controller {ControllerContext.ActionDescriptor.ControllerName} \r
                ActionResult {ControllerContext.ActionDescriptor.ActionName}");
            }


            PropertyInfo successProperty = tipoResponseObject.GetProperties()
                .FirstOrDefault(p => p.Name == "Sucesso" || p.Name == "Success");
            PropertyInfo warningProperty = tipoResponseObject.GetProperties()
                .FirstOrDefault(p => p.Name == "Aviso" || p.Name == "Warning");
            PropertyInfo dataProperty = tipoResponseObject.GetProperties()
                .FirstOrDefault(p => p.Name == "Dados" || p.Name == "Data");

            if (IsNull(successProperty))
            {
                throw new TypeAccessException(
                    $@"ProducesResponseType retorno com tipo informado é obrigatorio na 
                    Controller {ControllerContext.ActionDescriptor.ControllerName} 
                    ActionResult {ControllerContext.ActionDescriptor.ActionName}");
            }


            var instanceType = Activator.CreateInstance(tipoResponseObject);

            successProperty.SetValue(instanceType, true);
            dataProperty.SetValue(instanceType, result);

            var obsoleteMessage = ObsoleteActionMessage();
            if (obsoleteMessage.Count > 0)
                warningProperty.SetValue(instanceType, obsoleteMessage);


            return instanceType;
        }

        protected static bool IsNull(object data)
        {
            if (data is null)
                return true;

            if (data.GetType().FullName.Contains("Enumerable") ||
                data.GetType().FullName.Contains("System.Collections.Generic.List"))
                return IsNull((IEnumerable)data);
            else
                return data is null;
        }

        protected static bool IsNull(IEnumerable data)
        {
            return !data.NotNullOrZero();

        }
    }
}
