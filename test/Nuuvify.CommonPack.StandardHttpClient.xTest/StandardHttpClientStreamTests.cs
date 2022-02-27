using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest
{

    public class StandardHttpClientStreamTests
    {

        private readonly Mock<IHttpClientFactory> mockFactory;
        private readonly IConfiguration Config;

        public StandardHttpClientStreamTests()
        {
            mockFactory = new Mock<IHttpClientFactory>();

            Config = AppSettingsConfig.GetConfig();
        }


 

        [Fact]
        public async Task EnviaArquivoPorStreamComPost()
        {
            string[] FilesToPost = {"ubuntu3d--dark-blue.jpg", "Excel Example File.xlsx"};

            var api = "http://localhost:5001/";
            var pathName = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];


            var tokenFactory = new TokenFactory();
            var tokenResult = tokenFactory.ObtemTokenValido(
                Config.GetSection("ApisCredentials:Username")?.Value,
                Config.GetSection("ApisCredentials:Password")?.Value)
                .Result;

            var uri = new Uri(api);
            // var handler = new HttpClientHandler();
            // var client = new HttpClient(handler, true)
            // {
            //     BaseAddress = uri
            // };

            var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Testando retorno")
            });
            var client = new HttpClient(clientHandlerStub, true)
            {
                BaseAddress = uri
            };


            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(client);




            var url = $"{api}v1/Anexos";

            var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
            standardClient.CreateClient();
            standardClient.ResetStandardHttpClient();


            var anexo = new AnexoFixture("df33819a-1b97-4a9e-b948-e8972ef5990a", "ConsultaAtivos");


            StringContent stringContent;
            ContentDispositionHeaderValue stringHeader;
            HttpStandardReturn result = null;
            ByteArrayContent fileContent;
            string fullPathFileNamePost;



            using (var multipartContent = new MultipartFormDataContent())
            {

                foreach (var file in FilesToPost)
                {
                    fullPathFileNamePost = Path.Combine(pathName, "Arrange", file);

                    using (var fs = File.OpenRead(fullPathFileNamePost))
                    {
                        using (var streamContent = new StreamContent(fs))
                        {
                            fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());

                            stringHeader = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = nameof(anexo.AggregateId)
                            };
                            stringContent = new StringContent(anexo.AggregateId);
                            stringContent.Headers.ContentDisposition = stringHeader;
                            multipartContent.Add(stringContent);

                            stringHeader = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = nameof(anexo.TipoAnexo)
                            };
                            stringContent = new StringContent(anexo.TipoAnexo);
                            stringContent.Headers.ContentDisposition = stringHeader;
                            multipartContent.Add(stringContent);


                            stringHeader = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "Anexos",
                                FileName = file
                            };
                            fileContent.Headers.ContentDisposition = stringHeader;
                            multipartContent.Add(fileContent);

                        }
                    }

                }


                result = await standardClient
                    .WithAuthorization("bearer", tokenResult)
                    .Post(url, multipartContent);

            }


            Assert.True(result.Success);

        }

    }


}
