using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Nuuvify.CommonPack.StandardHttpClient.xTest")]
namespace Nuuvify.CommonPack.StandardHttpClient.Results;

internal class DeserializeObjectSuccess<ClasseCommandResult> where ClasseCommandResult : class
{
    public bool Success { get; set; }
    public IDictionary<string, string> Warnings { get; set; }
    public ClasseCommandResult Data { get; set; }
}
