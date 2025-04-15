namespace Nuuvify.CommonPack.Middleware.Abstraction.Results;

public class ModelStateError
{
    public string ErrorHost { get; set; }
    public string ErrorPath { get; set; }
    public object ErrorMessage { get; set; }
}
