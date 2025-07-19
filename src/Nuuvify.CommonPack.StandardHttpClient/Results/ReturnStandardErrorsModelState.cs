using System.Collections.Generic;

namespace Nuuvify.CommonPack.StandardHttpClient.Results
{
    internal class ReturnStandardErrorsModelState
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
        public IEnumerable<ModelStateError> Erros
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
        public IEnumerable<ModelStateError> Errors { get; set; }
    }
}
