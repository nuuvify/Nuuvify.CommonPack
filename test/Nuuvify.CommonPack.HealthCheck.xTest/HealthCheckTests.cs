using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest
{
    public class HealthCheckTests
    {

        private readonly Mock<IHttpClientFactory> mockFactory;
        private readonly Mock<IHealthCheck> mockHcContext;
        private readonly HealthCheckContext _context;

        public HealthCheckTests()
        {
            mockFactory = new Mock<IHttpClientFactory>();
            mockHcContext = new Mock<IHealthCheck>();

            _context = new HealthCheckContext();

        }



        [Fact]
        [Trait("Nuuvify.CommonPack.HealthCheck", "HealthCheckTests")]
        public async Task TestName()
        {

            var hcReportCustom = new HealthReportCustom
            {
                Status = HealthStatus.Healthy.ToString(),
                Entries = new List<HealthReportEntryCustom>()
                {
                    new HealthReportEntryCustom
                    {
                        Name = "Teste",
                        Description = "Testando HealthCheck",
                        Status = HealthStatus.Healthy.ToString()
                    }
                }
            };
            var listHcReportCustom = new List<HealthReportCustom>
            {
                hcReportCustom
            };

            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(listHcReportCustom), Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(clientHandlerStub, true)
            {
                BaseAddress = new Uri("https://meuteste/")
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");


            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);


            mockHcContext.Setup(x => x.CheckHealthAsync(It.IsAny<HealthCheckContext>(), default))
                .Returns(Task.FromResult(HealthCheckResult.Healthy()));



            var hc = new HttpCwsApiHealthCheck(mockFactory.Object);
            var hcResult = await hc.CheckHealthAsync(_context);


            Assert.Equal(HealthStatus.Healthy, hcResult.Status);

        }

    }
}