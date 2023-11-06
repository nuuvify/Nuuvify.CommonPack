using System.Collections.Generic;

namespace Nuuvify.CommonPack.StandardHttpClient.Results
{
    internal class DeserializeListSuccess<ClasseCommandResult> where ClasseCommandResult : class
    {
        public bool Success { get; set; }
        public IEnumerable<ClasseCommandResult> Data { get; set; }
    }
}
