using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Trait("Category", "Unit")]
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

    [Fact]
    public void Deserializa_Retorno_Com_Propriedade_Success()
    {

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = "{\r\n  \"success\": true,\r\n  \"data\": [\r\n    {\r\n      \"pfJ_CODIGO\": \"Q141606\",\r\n      \"razaO_SOCIAL\": \"COMPANHIA DE GAS DE SAO PAULO COMGAS\",\r\n      \"cpF_CGC\": \"61.856.571/0006-21\",\r\n      \"inD_FISICA_JURIDICA\": \"J\",\r\n      \"inD_NACIONAL_ESTRANGEIRA\": \"N\",\r\n      \"dT_INICIO\": \"2005-04-27T00:00:00\"\r\n    }\r\n  ]\r\n}"
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp", 1);

        Assert.True(returnClass.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(returnClass.FirstOrDefault().PFJ_CODIGO));
    }

    [Fact]
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

        _ = mockBaseHttp.Setup(
            x => x.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp", 1))
        .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnList<PessoaVigenteCommandResult>(httpReturnResult, "urlHttp", 1);
        var actualReturn = mockBaseHttp.Object.IsValid();

        Assert.True(returnClass.Count == 0);
        Assert.False(actualReturn);

    }

    [Fact]
    public void Deserializa_Retorno_Generic_Com_Propriedade_Success()
    {

        var httpReturnResult = "{\r\n  \"Fornecedores\": [\r\n    {\r\n      \"pfJ_CODIGO\": \"Q141606\",\r\n      \"razaO_SOCIAL\": \"COMPANHIA DE GAS DE SAO PAULO COMGAS\",\r\n      \"cpF_CGC\": \"61.856.571/0006-21\",\r\n      \"inD_FISICA_JURIDICA\": \"J\",\r\n      \"inD_NACIONAL_ESTRANGEIRA\": \"N\",\r\n      \"dT_INICIO\": \"2005-04-27T00:00:00\"\r\n    }\r\n  ]\r\n}";

        _ = mockBaseHttp.Setup(x => x.ReturnGenericClass<RetornoPessoGenericoFornecedores>(httpReturnResult, "urlHttp", true))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnGenericClass<RetornoPessoGenericoFornecedores>(httpReturnResult, "urlHttp", true);

        Assert.True(returnClass.Fornecedores.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(returnClass.Fornecedores.FirstOrDefault().PFJ_CODIGO));
    }

    [Fact]
    public void Deserializa_Retorno_GenericList_Com_Propriedade_Success()
    {

        var httpReturnResult = "{\"@odata.context\":\"https://was-p.bcnet.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata$metadata#_CotacaoDolarDia\",\"value\":[{\"cotacaoCompra\":5.30690,\"cotacaoVenda\":5.30750,\"dataHoraCotacao\":\"2020-08-03 13:06:35.557\"}]}";
        var cotacaoCompraExpected = 5.30690M;

        _ = mockBaseHttp.Setup(x => x.ReturnGenericClass<CotacaoDolarDia>(httpReturnResult, "urlHttp", true))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnGenericClass<CotacaoDolarDia>(httpReturnResult, "urlHttp", true);

        Assert.True(returnClass.Value.Count > 0);
        Assert.Equal(returnClass.Value.FirstOrDefault().CotacaoCompra, cotacaoCompraExpected);

    }

    [Fact]
    public void Deserializa_Retorno_Com_Diversos_Tipos_ComSucesso()
    {

        var fakeResult = new FakeReturnNotNull
        {
            PropBool = true,
            PropChar = '%',
            PropDatetime = new DateTime(2021, 12, 01, 14, 45, 08, DateTimeKind.Local),
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

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

    [Fact]
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

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

    [Fact]
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp", 1);

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

    [Fact]
    public void Deserializa_Retorno_Com_Diversos_Tipos_Nulaveis_ComValor_ComSucesso()
    {

        var dateManual = new DateTime(2020, 08, 03, 13, 06, 35, 557, DateTimeKind.Local);
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNull>(httpReturnResult, "urlHttp", 1);

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

    [Fact]
    public void Deserializa_RetornoLista_Com_Diversos_Tipos_Nulaveis_ComValor_ComSucesso()
    {

        var fakeListResult = new List<FakeReturnNull>
        {
            new FakeReturnNull
            {
                PropBool = true,
                PropChar = '#',
                PropDateTimeOffset = DateTimeOffset.Now,
                PropDatetime = new DateTime(2020, 08, 03, 13, 06, 35, 557, DateTimeKind.Local),
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

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNull>(httpReturnResult, "urlHttp", 1);

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

    #region Testes para Navegação em Profundidade (jsonDataDepth)

    [Fact]
    public void DeserializeObject_Com_Profundidade_1_ComSucesso()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropBool = true,
            PropInt = 42,
            PropString = "Teste profundidade 1"
        };

        // JSON com uma propriedade "Data" aninhada
        var nestedJson = new
        {
            Data = fakeResult
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(nestedJson)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeObject_Com_Profundidade_2_ComSucesso()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropBool = false,
            PropInt = 123,
            PropString = "Teste profundidade 2"
        };

        // JSON com duas propriedades "Data" aninhadas
        var doubleNestedJson = new
        {
            Data = new
            {
                Data = fakeResult
            }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(doubleNestedJson)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropInt, actual: returnClass.PropInt);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeObject_Com_Profundidade_3_ComSucesso()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropDecimal = 999.99M,
            PropString = "Teste profundidade 3"
        };

        // JSON com três propriedades "Data" aninhadas
        var tripleNestedJson = new
        {
            Data = new
            {
                Data = new
                {
                    Data = fakeResult
                }
            }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(tripleNestedJson)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 3))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 3);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropDecimal, actual: returnClass.PropDecimal);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeList_Com_Profundidade_1_ComSucesso()
    {
        // Arrange
        var fakeList = new List<FakeReturnNotNull>
        {
            new FakeReturnNotNull { PropInt = 1, PropString = "Item 1" },
            new FakeReturnNotNull { PropInt = 2, PropString = "Item 2" }
        };

        // JSON com uma propriedade "Data" aninhada contendo lista
        var nestedJson = new
        {
            Data = fakeList
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(nestedJson)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(2, returnClass.Count);
        Assert.Equal(expected: fakeList[0].PropInt, actual: returnClass[0].PropInt);
        Assert.Equal(expected: fakeList[0].PropString, actual: returnClass[0].PropString);
        Assert.Equal(expected: fakeList[1].PropInt, actual: returnClass[1].PropInt);
        Assert.Equal(expected: fakeList[1].PropString, actual: returnClass[1].PropString);
    }

    [Fact]
    public void DeserializeList_Com_Profundidade_2_ComSucesso()
    {
        // Arrange
        var fakeList = new List<FakeReturnNotNull>
        {
            new FakeReturnNotNull { PropInt = 10, PropString = "Deep Item 1" },
            new FakeReturnNotNull { PropInt = 20, PropString = "Deep Item 2" },
            new FakeReturnNotNull { PropInt = 30, PropString = "Deep Item 3" }
        };

        // JSON com duas propriedades "Data" aninhadas
        var doubleNestedJson = new
        {
            Data = new
            {
                Data = fakeList
            }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(doubleNestedJson)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(3, returnClass.Count);
        Assert.Equal(expected: fakeList[0].PropInt, actual: returnClass[0].PropInt);
        Assert.Equal(expected: fakeList[2].PropString, actual: returnClass[2].PropString);
    }

    [Fact]
    public void DeserializeObject_Com_Profundidade_Case_Insensitive_ComSucesso()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropBool = true,
            PropString = "Teste case insensitive"
        };

        // JSON com propriedade "data" (lowercase)
        var nestedJsonLowercase = new
        {
            data = fakeResult
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(nestedJsonLowercase)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropBool, actual: returnClass.PropBool);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeObject_Com_Profundidade_Inexistente_Fallback_ComSucesso()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropString = "Teste fallback"
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 5))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 5);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeList_Com_Profundidade_Inexistente_Fallback_ComSucesso()
    {
        // Arrange
        var fakeList = new List<FakeReturnNotNull>
        {
            new FakeReturnNotNull { PropString = "Fallback Item" }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(fakeList)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 3))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 3);

        // Assert
        Assert.NotNull(returnClass);
        Assert.True(returnClass.Count == 1);
        Assert.Equal(expected: fakeList[0].PropString, actual: returnClass[0].PropString);
    }

    [Fact]
    public void DeserializeObject_Com_JSON_Invalido_E_Profundidade_Retorna_Null()
    {
        // Arrange
        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = "{ invalid json }"
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 1);

        // Assert
        Assert.Null(returnClass);
        Assert.False(mockBaseHttp.Object.IsValid());
    }

    [Fact]
    public void DeserializeList_Com_JSON_Invalido_E_Profundidade_Retorna_Lista_Vazia()
    {
        // Arrange
        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = "{ invalid json structure }"
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Empty(returnClass);
        Assert.False(mockBaseHttp.Object.IsValid());
    }

    [Fact]
    public void DeserializeObject_Com_Profundidade_0_Usa_Comportamento_Original()
    {
        // Arrange
        var fakeResult = new FakeReturnNotNull
        {
            PropString = "Teste profundidade 0"
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

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 0))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 0);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(expected: fakeResult.PropString, actual: returnClass.PropString);
    }

    [Fact]
    public void DeserializeObject_Cenario_Complexo_Api_Real_ComSucesso()
    {
        // Arrange - Simula resposta de API real com múltiplas camadas
        var apiResponse = new
        {
            Status = "Success",
            Data = new
            {
                Data = new FakeReturnNotNull
                {
                    PropInt = 999,
                    PropString = "Dados do usuário",
                    PropBool = true,
                    PropDecimal = 1500.75M
                }
            }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(apiResponse)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnClass<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(999, returnClass.PropInt);
        Assert.Equal("Dados do usuário", returnClass.PropString);
        Assert.True(returnClass.PropBool);
        Assert.Equal(1500.75M, returnClass.PropDecimal);
    }

    [Fact]
    public void DeserializeList_Cenario_Complexo_Api_Paginada_ComSucesso()
    {
        // Arrange - Simula API com paginação e dados aninhados
        var usersList = new List<FakeReturnNotNull>
        {
            new FakeReturnNotNull { PropInt = 1, PropString = "Usuário 1", PropBool = true },
            new FakeReturnNotNull { PropInt = 2, PropString = "Usuário 2", PropBool = false },
            new FakeReturnNotNull { PropInt = 3, PropString = "Usuário 3", PropBool = true }
        };

        var paginatedResponse = new
        {
            Success = true,
            Data = new
            {
                Pagination = new { Page = 1, PageSize = 10, TotalItems = 3 },
                Data = usersList
            }
        };

        var httpReturnResult = new HttpStandardReturn
        {
            Success = true,
            ReturnCode = "200",
            ReturnMessage = JsonSerializer.Serialize(paginatedResponse)
        };

        _ = mockBaseHttp.Setup(x => x.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2))
                    .CallBase();

        // Act
        var returnClass = mockBaseHttp.Object.ReturnList<FakeReturnNotNull>(httpReturnResult, "urlHttp", 2);

        // Assert
        Assert.NotNull(returnClass);
        Assert.Equal(3, returnClass.Count);
        Assert.Equal("Usuário 1", returnClass[0].PropString);
        Assert.Equal("Usuário 2", returnClass[1].PropString);
        Assert.Equal("Usuário 3", returnClass[2].PropString);
        Assert.True(returnClass[0].PropBool);
        Assert.False(returnClass[1].PropBool);
        Assert.True(returnClass[2].PropBool);
    }

    #endregion

}
