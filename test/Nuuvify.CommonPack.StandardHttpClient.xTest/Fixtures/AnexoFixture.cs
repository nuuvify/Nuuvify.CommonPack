namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Fixtures;

public class AnexoFixture
{
    public AnexoFixture(string aggregateId, string tipoAnexo)
    {
        AggregateId = aggregateId;
        TipoAnexo = tipoAnexo;
    }

    public string AggregateId { get; set; }
    public string TipoAnexo { get; set; }

}
