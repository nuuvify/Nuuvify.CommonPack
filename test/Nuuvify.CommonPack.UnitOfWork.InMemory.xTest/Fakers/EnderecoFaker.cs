using Bogus;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

public static class EnderecoFaker
{
    private static readonly Faker<Endereco> s_faker = new Faker<Endereco>()
        .CustomInstantiator(f => new Endereco(
            logradouro: f.Address.StreetName(),
            numero: f.Address.BuildingNumber(),
            cidade: f.Address.City(),
            cep: f.Address.ZipCode()));

    public static Endereco Generate() => s_faker.Generate();

    public static IList<Endereco> Generate(int count) => s_faker.Generate(count);
}
