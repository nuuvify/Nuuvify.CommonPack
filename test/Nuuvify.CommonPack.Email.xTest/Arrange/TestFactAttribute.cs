using System;
using Nuuvify.CommonPack.Email.xTest.Configs;
using Xunit;

namespace Nuuvify.CommonPack.Email.xTest.Arrange
{
    public sealed class TestFactAttribute : FactAttribute
    {
        public TestFactAttribute()
        {
            if (IsByPass())
            {
                Skip = "Bypass automated testing";
            }
        }

        private static bool IsByPass()
        {
            var config = AppSettingsConfig.GetConfig();
            var bypass = config.GetSection("TestOptions:BypassEmailSend")?.Value;

            return bypass.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
