using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JBlam.Multiflash.App
{
    class ProcessSetViewModel : IContinuableViewModel<ConfigurationViewModel>, INotifyPropertyChanged
    {
        private readonly IToolset toolset;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProcessSetViewModel(IToolset toolset)
        {
            this.toolset = toolset ?? throw new ArgumentNullException(nameof(toolset));
            Next = Command.Create(() =>
            {
                NextViewModel = new ConfigurationViewModel();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextViewModel)));
            }, () => Consoles.All(c => c.IsSuccess == true));
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
            Next.RaiseCanExecuteChanged();
        }

        public ICommand Next { get; }

        public ConfigurationViewModel? NextViewModel { get; private set; }
    }
}
