using Xunit;

namespace Nuuvify.CommonPack.Email.xTest.Fixtures
{

    [CollectionDefinition(nameof(DataCollection))]
    public class DataCollection :
        ICollectionFixture<EmailConfigFixture>
    {

    }

}
