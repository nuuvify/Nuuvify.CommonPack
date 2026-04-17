using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Domain.xTest.Fakers;

namespace Nuuvify.CommonPack.Domain.xTest.Fixtures;

public class CnpjFixture
{
    public Cnpj CnpjNumericoValido { get; }
    public Cnpj CnpjAlfanumericoValido { get; }
    public string CnpjNumericoString { get; }
    public string CnpjAlfanumericoString { get; }

    public CnpjFixture()
    {
        CnpjNumericoString = CnpjFaker.GerarCnpjNumericoValido();
        CnpjAlfanumericoString = CnpjFaker.GerarCnpjAlfanumericoValido();
        CnpjNumericoValido = new Cnpj(CnpjNumericoString);
        CnpjAlfanumericoValido = new Cnpj(CnpjAlfanumericoString);
    }
}
