using System.Collections.Generic;

namespace JBlam.Multiflash.CommandLine
{
    using Binaries = IReadOnlyCollection<Binary>;

    abstract class CliSetTool
    {
        public abstract (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries);
        public abstract void AppendCliArgs(ICollection<string> processArgs, string? targetPlatform, Binaries binaries, string comPort);
    }
}
