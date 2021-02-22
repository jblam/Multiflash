using JBlam.Multiflash.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JBlam.Multiflash.CommandLine
{
    using Binaries = IReadOnlyCollection<Binary>;

    public class StandaloneExeTool : ISetTool
    {
        private readonly string exePath;
        private readonly CliSetTool inner;

        internal StandaloneExeTool(string exePath, CliSetTool inner)
        {
            this.exePath = exePath;
            this.inner = inner;
        }

        public (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries) =>
            inner.CanHandle(targetPlatform, binaries);

        public ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort)
        {
            var startInfo = new ProcessStartInfo(exePath);
            inner.AppendCliArgs(startInfo.ArgumentList, targetPlatform, binaries, comPort);
            return startInfo;
        }

        public bool IsInstalled() => File.Exists(exePath) && inner.IsInstalled();
    }
}
