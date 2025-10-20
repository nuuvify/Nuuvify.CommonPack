using Bogus;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

public static class FaturaFaker
{
    private static readonly Faker<Fatura> _faker = new Faker<Fatura>()
        .CustomInstantiator(f => new Fatura(
            numeroFatura: f.Random.Int(1000, 9999),
            enderecoFatura: EnderecoFaker.Generate(),
            enderecoEntrega: EnderecoFaker.Generate()));

    public static Fatura Generate() => _faker.Generate();

    public static IList<Fatura> Generate(int count) => _faker.Generate(count);
}
