using System;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest
{
    public sealed class InMemoryTestFactAttribute : FactAttribute
    {
        public InMemoryTestFactAttribute()
        {
            if (!IsInMemoryContext())
            {
                Skip = "Ignore test Database InMemory";
            }
        }

        private static bool IsInMemoryContext()
        {
            var config = AppSettingsConfig.GetConfig();
            var inMemory = config.GetSection("TestOptions:DataBaseTesteInMemory")?.Value;

            return inMemory.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
