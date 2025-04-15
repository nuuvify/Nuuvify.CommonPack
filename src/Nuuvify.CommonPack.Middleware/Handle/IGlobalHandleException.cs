using Microsoft.AspNetCore.Http;

namespace Nuuvify.CommonPack.Middleware.Handle;

/// <summary>
/// Essa interface necessita de Microsoft.AspNetCore.Http 
/// por esse motivo deve ser mantida nessa DLL e não em Abstraction
/// </summary>
public interface IGlobalHandleException
{

    Task HandleException(Exception ex, HttpContext context);
}
