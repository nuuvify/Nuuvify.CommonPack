using Nuuvify.CommonPack.Extensions.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

public class Endereco : INotPersistingAsTable
{
    protected Endereco() { }

    public Endereco(string logradouro, string numero, string cidade, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Cidade = cidade;
        Cep = cep;
    }

    public string Logradouro { get; private set; }
    public string Numero { get; private set; }
    public string Cidade { get; private set; }
    public string Cep { get; private set; }
}
