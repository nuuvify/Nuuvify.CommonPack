using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures;


[CollectionDefinition(nameof(DataCollection))]
public class DataCollection : ICollectionFixture<AppDbContextFixture>,
    ICollectionFixture<DataFixture>,
    ICollectionFixture<SeedDbFixture>
{

}
