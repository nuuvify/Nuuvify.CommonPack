using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.Middleware.Filters;

public sealed partial class ValidateModelStateCustomAttribute
{
    private sealed partial class ValidateModelAttributeCustom : IActionFilter
    {

        private readonly ILogger<ValidateModelAttributeCustom> _logger;

        public ValidateModelAttributeCustom(ILogger<ValidateModelAttributeCustom> logger)
        {
            _logger = logger;
        }

        private static ObjectResult GetRetornoPadronizado(StatusCodeResult codigoRetornoModelStateComErro, IEnumerable<ModelStateErro> result)
        {
            var objectResultError = new ObjectResult(new RetornoPadraoComErrosModelState
            {
                Sucesso = false,
                Erros = result
            })
            {
                StatusCode = codigoRetornoModelStateComErro.StatusCode
            };

            return objectResultError;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

            if (!context.ModelState.IsValid)
            {
                var errosModel = NotificarErroModelInvalida(context);
                context.Result = Response(new StatusCodeResult(StatusCodes.Status417ExpectationFailed), errosModel);
            }
        }

        protected IActionResult Response(StatusCodeResult codigoRetornoModelStateComErro,
                                         IEnumerable<ModelStateErro> result)
        {

            var retornoPadronizado = GetRetornoPadronizado(codigoRetornoModelStateComErro, result);

            _logger.LogError("{erro}", retornoPadronizado);
            return retornoPadronizado;

        }

        protected static IEnumerable<ModelStateErro> NotificarErroModelInvalida(ActionExecutingContext context)
        {
            var errosModelState = new List<ModelStateErro>();

            var erros = context.ModelState.Values.SelectMany(v => v.Errors);
            foreach (var erro in erros)
            {
                var erroModelState = new ModelStateErro
                {
                    ErroHost = context.HttpContext.Request.Host.Value,
                    ErroPath = context.HttpContext.Request.Path.Value,
                    ErroMensagem = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message
                };

                errosModelState.Add(erroModelState);
            }

            return errosModelState;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogTrace("ValidateModelStateCustomAttribute On Executed");
        }
    }

}
