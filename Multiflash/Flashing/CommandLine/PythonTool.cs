using JBlam.Multiflash.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JBlam.Multiflash.CommandLine
{
    using Binaries = IReadOnlyCollection<Binary>;

    public class PythonTool : ISetTool
    {
        private readonly string pythonPath;
        private readonly string scriptPath;
        private readonly CliSetTool inner;

        internal PythonTool(string pythonPath, string scriptPath, CliSetTool inner)
        {
            this.pythonPath = pythonPath;
            this.scriptPath = scriptPath;
            this.inner = inner;
        }

        public (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries) =>
            inner.CanHandle(targetPlatform, binaries);

        public ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort)
        {
            var startInfo = new ProcessStartInfo(pythonPath)
            {
                ArgumentList = { scriptPath }
            };
            inner.AppendCliArgs(startInfo.ArgumentList, targetPlatform, binaries, comPort);
            return startInfo;
        }

        public string Name => inner.Name;

        public bool IsInstalled() => File.Exists(scriptPath) && File.Exists(pythonPath);
    }
}
