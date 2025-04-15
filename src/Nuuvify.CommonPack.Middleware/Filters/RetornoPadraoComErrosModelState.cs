namespace Nuuvify.CommonPack.Middleware.Filters;

public class RetornoPadraoComErrosModelState
{
    public bool Sucesso
    {
        get
        {
            return Success;
        }
        set
        {
            Success = value;
        }
    }
    public IEnumerable<ModelStateErro> Erros
    {
        get
        {
            return Errors;
        }
        set
        {
            Errors = value;
        }
    }
    public bool Success { get; set; }
    public IEnumerable<ModelStateErro> Errors { get; set; }
}
