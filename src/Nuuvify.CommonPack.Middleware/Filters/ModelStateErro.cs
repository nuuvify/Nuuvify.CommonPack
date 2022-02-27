namespace Nuuvify.CommonPack.Middleware.Filters
{
    public class ModelStateErro
    {
        public string ErroHost
        {
            get
            {
                return ErrorHost;
            }
            set
            {
                ErrorHost = value;
            }
        }
        public string ErroPath
        {
            get
            {
                return ErrorPath;
            }
            set
            {
                ErrorPath = value;
            }
        }
        public object ErroMensagem
        {
            get
            {
                return ErrorMessage;
            }
            set
            {
                ErrorMessage = value;
            }
        }

        public string ErrorHost { get; set; }
        public string ErrorPath { get; set; }
        public object ErrorMessage { get; set; }

    }
}
