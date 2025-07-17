using System.Text.Json;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Order(1)]
public class BaseStandardHttpClientTests
{

    private readonly Mock<IStandardHttpClient> mockStandardHttpClient;
    private readonly Mock<ITokenService> mockTokenService;
    private readonly Mock<BaseStandardHttpClient> mockBaseHttp;

    public BaseStandardHttpClientTests()
    {
        mockStandardHttpClient = new Mock<IStandardHttpClient>();
        mockTokenService = new Mock<ITokenService>();

        mockBaseHttp = new Mock<BaseStandardHttpClient>(mockStandardHttpClient.Object, mockTokenService.Object);

    }

    [Fact, Order(1)]
    public void Deserializa_Retorno_Com_Propriedade_Success()
    {

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = "{\r\n  \"success\": true,\r\n  \"data\": [\r\n    {\r\n      \"pfJ_CODIGO\": \"Q141606\",\r\n      \"razaO_SOCIAL\": \"COMPANHIA DE GAS DE SAO PAULO COMGAS\",\r\n      \"cpF_CGC\": \"61.856.571/0006-21\",\r\n      \"inD_FISICA_JURIDICA\": \"J\",\r\n      \"inD_NACIONAL_ESTRANGEIRA\": \"N\",\r\n      \"dT_INICIO\": \"2005-04-27T00:00:00\"\r\n    }\r\n  ]\r\n}"
        };

        mockBaseHttp.Setup(x => x.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp");



        Assert.True(returnClass.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(returnClass.FirstOrDefault().PFJ_CODIGO));
    }

    [Fact, Order(2)]
    public void Deserializa_Retorno_Com_Erro()
    {

        var errors = new List<NotificationR>
        {
            new NotificationR(
                property: "Teste-Nome Propriedade",
                message: "Teste-Mensagem",
                aggregatorId: "Teste-Aggregador",
                type: "Teste-Tipo",
                originNotification: typeof(BaseStandardHttpClientTests)),
            new NotificationR(
                property: "Teste1-Nome Propriedade",
                message: "Teste1-Mensagem",
                aggregatorId: "Teste1-Aggregador",
                type: "Teste1-Tipo",
                originNotification: typeof(BaseStandardHttpClientTests))
        };

        var httpReturnErrors = new ReturnStandardErrors
        {
            Success = false,
            Errors = errors
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = false,
            ReturnCode = "",
            ReturnMessage = JsonSerializer.Serialize(httpReturnErrors)
        };


        mockBaseHttp.Setup(
            x => x.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp"))
        .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp");
        var actualReturn = mockBaseHttp.Object.IsValid();


        Assert.True(returnClass.Count == 0);
        Assert.False(actualReturn);

    }

    [Fact, Order(3)]
    public void Deserializa_Retorno_Generic_Com_Propriedade_Success()
    {

        var httpReturnResult = "{\r\n  \"Fornecedores\": [\r\n    {\r\n      \"pfJ_CODIGO\": \"Q141606\",\r\n      \"razaO_SOCIAL\": \"COMPANHIA DE GAS DE SAO PAULO COMGAS\",\r\n      \"cpF_CGC\": \"61.856.571/0006-21\",\r\n      \"inD_FISICA_JURIDICA\": \"J\",\r\n      \"inD_NACIONAL_ESTRANGEIRA\": \"N\",\r\n      \"dT_INICIO\": \"2005-04-27T00:00:00\"\r\n    }\r\n  ]\r\n}";


        mockBaseHttp.Setup(x => x.ReturnGenericClass<RetornoPessoGenericoFornecedores>(httpReturnResult, "urlHttp", true))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnGenericClass<RetornoPessoGenericoFornecedores>(httpReturnResult, "urlHttp", true);


        Assert.True(returnClass.Fornecedores.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(returnClass.Fornecedores.FirstOrDefault().PFJ_CODIGO));
    }

    [Fact, Order(4)]
    public void Deserializa_Retorno_GenericList_Com_Propriedade_Success()
    {

        var httpReturnResult = "{\"@odata.context\":\"https://was-p.bcnet.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata$metadata#_CotacaoDolarDia\",\"value\":[{\"cotacaoCompra\":5.30690,\"cotacaoVenda\":5.30750,\"dataHoraCotacao\":\"2020-08-03 13:06:35.557\"}]}";
        var cotacaoCompraExpected = 5.30690M;


        mockBaseHttp.Setup(x => x.ReturnGenericClass<CotacaoDolarDia>(httpReturnResult, "urlHttp", true))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnGenericClass<CotacaoDolarDia>(httpReturnResult, "urlHttp", true);



        Assert.True(returnClass.Value.Count > 0);
        Assert.Equal(returnClass.Value.FirstOrDefault().CotacaoCompra, cotacaoCompraExpected);


    }

    [Fact, Order(4)]
    public void Deserializa_Retorno_Com_Diversos_Tipos_ComSucesso()
    {

        var fakeResult = new FakeReturnNotNull
        {
            PropBool = true,
            PropChar = '%',
            PropDatetime = new DateTime(2021, 12, 01, 14, 45, 08),
            PropDateTimeOffset = DateTimeOffset.Now,
            PropDecimal = 1234556789.16M,
            PropDouble = 1234567.8901,
            PropInt = 987654,
            PropLong = long.MinValue,
            PropString = "Isso é um teste de serialização"
        };

        var fakeSucess = new DeserializeObjectSuccess<FakeReturnNotNull>
        {
            Success = true,
            Data = fakeResult
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeSucess)

        };

        mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp");




        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropChar, actual: returnClass.PropChar);
        Assert.Equal(expected: fakeResult.PropDatetime, actual: returnClass.PropDatetime);
        Assert.Equal(expected: fakeResult.PropDateTimeOffset, actual: returnClass.PropDateTimeOffset);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClass.PropDecimal);
        Assert.Equal(expected: fakeResult.PropDouble, actual: returnClass.PropDouble);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropLong, actual: returnClass.PropLong);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);

    }

    [Fact, Order(4)]
    public void Deserializa_Retorno_Com_Diversos_Tipos_Null_ComSucesso()
    {

        var fakeResult = new FakeReturnNotNull();

        var fakeSucess = new DeserializeObjectSuccess<FakeReturnNotNull>
        {
            Success = true,
            Data = fakeResult
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeSucess)

        };

        mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp");


        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropChar, actual: returnClass.PropChar);
        Assert.Equal(expected: fakeResult.PropDatetime, actual: returnClass.PropDatetime);
        Assert.Equal(expected: fakeResult.PropDateTimeOffset, actual: returnClass.PropDateTimeOffset);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClass.PropDecimal);
        Assert.Equal(expected: fakeResult.PropDouble, actual: returnClass.PropDouble);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropLong, actual: returnClass.PropLong);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);

    }

    [Fact, Order(4)]
    public void Deserializa_Retorno_Com_Diversos_Tipos_Nulaveis_ComSucesso()
    {

        var fakeResult = new FakeReturnNull();

        var fakeSucess = new DeserializeObjectSuccess<FakeReturnNull>
        {
            Success = true,
            Data = fakeResult
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeSucess)

        };

        mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp");


        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropChar, actual: returnClass.PropChar);
        Assert.Equal(expected: fakeResult.PropDatetime, actual: returnClass.PropDatetime);
        Assert.Equal(expected: fakeResult.PropDateTimeOffset, actual: returnClass.PropDateTimeOffset);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClass.PropDecimal);
        Assert.Equal(expected: fakeResult.PropDouble, actual: returnClass.PropDouble);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropLong, actual: returnClass.PropLong);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);

    }

    [Fact, Order(4)]
    public void Deserializa_Retorno_Com_Diversos_Tipos_Nulaveis_ComValor_ComSucesso()
    {

        var dateManual = new DateTime(2020, 08, 03, 13, 06, 35, 557);
        var fakeResult = new FakeReturnNull
        {
            PropBool = true,
            PropChar = '#',
            PropDateTimeOffset = dateManual,
            PropDatetime = dateManual,
            PropDecimal = decimal.MaxValue,
            PropDouble = double.MaxValue,
            PropInt = int.MaxValue,
            PropLong = long.MaxValue,
            PropString = string.Empty
        };

        var fakeSucess = new DeserializeObjectSuccess<FakeReturnNull>
        {
            Success = true,
            Data = fakeResult
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeSucess)

        };

        mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp");


        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropChar, actual: returnClass.PropChar);
        Assert.Equal(expected: fakeResult.PropDatetime, actual: returnClass.PropDatetime);
        Assert.Equal(expected: fakeResult.PropDateTimeOffset, actual: returnClass.PropDateTimeOffset);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClass.PropDecimal);
        Assert.Equal(expected: fakeResult.PropDouble, actual: returnClass.PropDouble);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropLong, actual: returnClass.PropLong);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);

    }

    [Fact, Order(4)]
    public void Deserializa_RetornoLista_Com_Diversos_Tipos_Nulaveis_ComValor_ComSucesso()
    {

        var fakeListResult = new List<FakeReturnNull>
        {
            new FakeReturnNull
            {
                PropBool = true,
                PropChar = '#',
                PropDateTimeOffset = DateTimeOffset.Now,
                PropDatetime = new DateTime(2020, 08, 03, 13, 06, 35, 557),
                PropDecimal = decimal.MaxValue,
                PropDouble = double.MaxValue,
                PropInt = int.MaxValue,
                PropLong = long.MaxValue,
                PropString = string.Empty
            },
            new FakeReturnNull
            {
                PropBool = false,
                PropChar = '$',
                PropDateTimeOffset = DateTime.Now,
                PropDatetime = DateTime.Today.AddDays(-18),
                PropDecimal = decimal.MinValue,
                PropDouble = double.MinValue,
                PropInt = int.MinValue,
                PropLong = long.MinValue,
                PropString = "Registro 2"
            }
        };


        var fakeSucess = new DeserializeListSuccess<FakeReturnNull>
        {
            Success = true,
            Data = fakeListResult
        };


        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeSucess)

        };

        mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNull>(httpReturnResult, "urlHttp"))
                    .CallBase();


        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNull>(httpReturnResult, "urlHttp");

        var fakeResult = fakeListResult.FirstOrDefault();
        var returnClassEntity = returnClass.FirstOrDefault();

        Assert.True(returnClass.Count == 2);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClassEntity.PropBool);
        Assert.Equal(expected: fakeResult.PropChar, actual: returnClassEntity.PropChar);
        Assert.Equal(expected: fakeResult.PropDatetime, actual: returnClassEntity.PropDatetime);
        Assert.Equal(expected: fakeResult.PropDateTimeOffset, actual: returnClassEntity.PropDateTimeOffset);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClassEntity.PropDecimal);
        Assert.Equal(expected: fakeResult.PropDouble, actual: returnClassEntity.PropDouble);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClassEntity.PropInt);
        Assert.Equal(expected: fakeResult.PropLong, actual: returnClassEntity.PropLong);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClassEntity.PropString);

    }



}
