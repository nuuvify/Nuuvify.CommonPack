using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;


[CollectionDefinition(nameof(DataCollection))]
public class DataCollection : ICollectionFixture<AppDbContextFixture>,
    ICollectionFixture<DataFixture>,
    ICollectionFixture<SeedDbFixture>
{

}
