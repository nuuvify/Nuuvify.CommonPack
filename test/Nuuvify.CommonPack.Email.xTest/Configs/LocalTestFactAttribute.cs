using System;
using Xunit;

namespace Nuuvify.CommonPack.Email.xTest.Configs
{
    public sealed class LocalTestFactAttribute : FactAttribute
    {
        public LocalTestFactAttribute()
        {
            if (!IsLocalMachine())
            {
                Skip = "Ignore test in Server";
            }
        }

        private static bool IsLocalMachine()
        {
            var machineName = Environment.MachineName;
            return machineName.StartsWith("B8", StringComparison.OrdinalIgnoreCase);
        }
    }
}
