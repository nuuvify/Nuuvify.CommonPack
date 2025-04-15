using System.ComponentModel;

namespace Nuuvify.CommonPack.Middleware.Abstraction.Results;

public class ReturnStandardSuccess<T>
{
    public bool Success { get; set; }
    [Description("List of important warnings about the method, such as obsolescence.")]
    public IDictionary<string, string> Warnings { get; set; }
    public T Data { get; set; }
}
