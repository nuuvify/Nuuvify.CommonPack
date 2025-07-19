using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Essa classe implementa resiliencia para o uso do HttpClient, bem como tenta obter um token com as credenciais de CWS contidas no 
        /// appsettings ou na classe <see cref="CredentialToken"/>, os seguintes patterns estão sendo implementados:
        /// Retry Pattern = Caso ocorra um erro 5xx ou 408, será tentado novamente conforme os parametros informados
        /// Circuit Breaker Patern = Caso as tentativas anteriores tenham falhado, o Circuit ira tentar mais 1 vez, caso falhe novamente,
        ///     será lançado uma exception do tipo BrokenCircuitBreaker, e o circuito será aberto pelo tempo contido no parametro {breakDurationMilliSeconds},
        ///     durante esse tempo, se houver nova request, a request já é retornada com a exception, evitando consumo de recurso desnecessario.
        ///     Após esse tempo, o circuito é fechado novamente e o ciclo se reinicia. 
        /// FallBack =  Para que o usuario (client) não receba uma exception, o fallback intercepta a exception BrokenCircuitBreaker, e devolve uma mensagem 
        ///             customizada, mais amigavel.
        /// </summary>
        /// <remarks>
        /// Importante: A Ordem de configurações das classes importam, pois, como o FallBack esta subistituindo uma exception do CircuitBreaker, o FallBack deve 
        /// ser injetada antes do CircuitBreaker, e após o retry
        /// </remarks>
        /// <param name="httpClientBuilder"></param>
        /// <param name="services"></param>
        /// <param name="retryTotal">Numero de vezes que será feito nova tentativa</param>
        /// <param name="breakDurationMilliSeconds">Tempo em que o circuito ficara aberto caso o serviço não responda</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPolicyWithTokenHandlers(this IHttpClientBuilder httpClientBuilder,
            IServiceCollection services,
            int retryTotal = 2,
            int breakDurationMilliSeconds = 5000)
        {

            var sp = services.BuildServiceProvider();
            var logger = sp.GetService<ILogger<IHttpClientBuilder>>();
            var tokenService = sp.GetService<ITokenService>();

            if (tokenService is null)
            {
                throw new TypeLoadException("A interface ITokenService não foi injetada no conteiner DI, a injeção dessa interface deve ocorrer antes da chamada desse metodo, veja o metodo AddStandardHttpClientSetup");
            }


            var policyConfig = new PolicyConfig
            {
                RetryCount = retryTotal,
                BreakDurationMilliSeconds = breakDurationMilliSeconds
            };


            var circuitBreakerPolicyConfig = (ICircuitBreakerPolicyConfig)policyConfig;
            var retryPolicyConfig = (IRetryPolicyConfig)policyConfig;



            return httpClientBuilder.AddRetryPolicyHandler(logger, tokenService, retryPolicyConfig)
                .AddCircuitBreakerFallBackHandler(logger)
                .AddCircuitBreakerHandler(logger, circuitBreakerPolicyConfig);
        }

        /// <summary cref="AddPolicyWithTokenHandlers">
        /// Faz a mesma implementação, mas sem a obtenção de token, essa implementacão é interessante para acesso a
        /// apis de terceiros
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="services"></param>
        /// <param name="retryTotal">Numero de vezes que será feito nova tentativa</param>
        /// <param name="breakDurationMilliSeconds">Tempo em que o circuito ficara aberto caso o serviço não responda</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPolicyHandlers(this IHttpClientBuilder httpClientBuilder,
            IServiceCollection services,
            int retryTotal = 2,
            int breakDurationMilliSeconds = 5000)
        {

            var sp = services.BuildServiceProvider();
            var logger = sp.GetService<ILogger<IHttpClientBuilder>>();

            var policyConfig = new PolicyConfig
            {
                RetryCount = retryTotal,
                BreakDurationMilliSeconds = breakDurationMilliSeconds
            };


            var circuitBreakerPolicyConfig = (ICircuitBreakerPolicyConfig)policyConfig;
            var retryPolicyConfig = (IRetryPolicyConfig)policyConfig;



            return httpClientBuilder.AddRetryPolicyHandler(logger, retryPolicyConfig)
                .AddCircuitBreakerFallBackHandler(logger)
                .AddCircuitBreakerHandler(logger, circuitBreakerPolicyConfig);

        }


        public static IHttpClientBuilder AddRetryPolicyHandler(this IHttpClientBuilder httpClientBuilder, ILogger logger, IRetryPolicyConfig retryPolicyConfig)
        {
            return httpClientBuilder.AddPolicyHandler(request => HttpRetryPolicies.GetHttpResponseRetryPolicy(request, logger, retryPolicyConfig));
        }
        public static IHttpClientBuilder AddRetryPolicyHandler(this IHttpClientBuilder httpClientBuilder, ILogger logger, ITokenService tokenService, IRetryPolicyConfig retryPolicyConfig)
        {
            return httpClientBuilder.AddPolicyHandler(request => HttpRetryWithTokenPolicies.GetHttpResponseRetryPolicyWithToken(request, logger, tokenService, retryPolicyConfig));
        }
        public static IHttpClientBuilder AddCircuitBreakerHandler(this IHttpClientBuilder httpClientBuilder, ILogger logger, ICircuitBreakerPolicyConfig circuitBreakerPolicyConfig)
        {
            return httpClientBuilder.AddPolicyHandler(HttpCircuitBreakerPolicies.GetHttpCircuitBreakerPolicy(logger, circuitBreakerPolicyConfig));
        }
        public static IHttpClientBuilder AddCircuitBreakerFallBackHandler(this IHttpClientBuilder httpClientBuilder, ILogger logger)
        {
            return httpClientBuilder.AddPolicyHandler(request => HttpCircuitBreakerFallBackPolicies.GetHttpFallBackPolicy(request, logger));
        }

    }
}