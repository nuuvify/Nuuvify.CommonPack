namespace Nuuvify.CommonPack.HealthCheck
{
    public class HealthCheckCustomConfiguration
    {

        private string _urlHealthCheck;
        /// <summary>
        /// Endpoint configurado em appsettings
        /// </summary>
        /// <value>/hc</value>
        public string UrlHealthCheck
        {
            get
            {
                return _urlHealthCheck;
            }
            set
            {
                _urlHealthCheck = string.IsNullOrWhiteSpace(value) ? "/hc" : value;
            }
        }
        public bool EnableChecksStandard { get; set; }
        public int EvaluationTimeInSeconds { get; set; }
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
        public int MaximumHistoryEntriesPerEndpoint { get; set; }
        public int SetApiMaxActiveRequests { get; set; }

        /// <summary>
        /// Retorna o valor da propriedade EnableChecksStandard 
        /// dessa forma é possivel, por exemplo, não executar HealthCheck para o ambiente de Development
        /// apenas mudando o parametro HealthCheckCustomConfiguration:EnableChecksStandard no appsettings.Development.json
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return EnableChecksStandard;
        }

    }
}