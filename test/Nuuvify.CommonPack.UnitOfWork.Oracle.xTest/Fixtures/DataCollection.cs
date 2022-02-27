using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures
{

    [CollectionDefinition(nameof(DataCollection))]
    public class DataCollection : ICollectionFixture<AppDbContextFixture>, 
        ICollectionFixture<DataFixture>,
        ICollectionFixture<SeedDbFixture>
    {

    }

}
