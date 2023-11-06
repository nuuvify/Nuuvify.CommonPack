using System.Collections.Generic;

namespace Nuuvify.CommonPack.Middleware.Abstraction.Results
{
    public class ReturnStandardErrorsModelState
    {
        public bool Success { get; set; }
        public IEnumerable<ModelStateError> Errors { get; set; }
    }
}
