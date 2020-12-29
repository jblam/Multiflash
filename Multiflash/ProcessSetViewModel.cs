using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    class ProcessSetViewModel
    {
        private readonly IToolset toolset;

        public ProcessSetViewModel(IToolset toolset)
        {
            this.toolset = toolset ?? throw new ArgumentNullException(nameof(toolset));
        }

        public BinarySet? BinarySet { get; private set; }
        public IReadOnlyList<StreamingConsoleViewModel> Consoles { get; private set; } = Array.Empty<StreamingConsoleViewModel>();

        public async Task SetBinaries(BinarySet binarySet, string comPort, string? workingDir = null)
        {
            if (BinarySet != null)
                throw new InvalidOperationException("Binaries have already been set");
            BinarySet = binarySet ?? throw new ArgumentNullException(nameof(binarySet));
            Consoles = BinarySet.Binaries.Select(binary =>
            {
                var tool = toolset.GetToolForBinary(binary) ?? throw new InvalidOperationException("Couldn't get a tool");
                var s = tool.GetStartInfo(binary, comPort ?? throw new InvalidOperationException("Couldn't get the port"));
                s.RedirectStandardOutput = true;
                s.RedirectStandardError = true;
                s.RedirectStandardInput = true;
                s.CreateNoWindow = true;
                s.WorkingDirectory = workingDir ?? s.WorkingDirectory;
                return new StreamingConsoleViewModel(binary, s);
            }).ToList();
            foreach (var vm in Consoles)
            {
                await vm.Start().WaitForExitAsync();
            }
        }
    }
}
