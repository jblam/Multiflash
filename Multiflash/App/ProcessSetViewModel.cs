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
        private string? comPort;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProcessSetViewModel(IToolset toolset)
        {
            this.toolset = toolset ?? throw new ArgumentNullException(nameof(toolset));
            Next = Command.Create(() =>
            {
                NextViewModel = new ConfigurationViewModel(BinarySet!, comPort!);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextViewModel)));
            }, () => Consoles.All(c => c.IsSuccess == true) && comPort != null);
        }

        public BinarySet? BinarySet { get; private set; }
        public IReadOnlyList<StreamingConsoleViewModel> Consoles { get; private set; } = Array.Empty<StreamingConsoleViewModel>();

        public async Task SetBinaries(BinarySet binarySet, string comPort, string? workingDir = null)
        {
            if (BinarySet != null)
                throw new InvalidOperationException("Binaries have already been set");
            BinarySet = binarySet ?? throw new ArgumentNullException(nameof(binarySet));
            this.comPort = comPort;
            var plan = toolset.GetPlan(BinarySet);
            if (plan.IsSuccess(out var details))
            {
                Consoles = details.Select((part, index) =>
                {
                    var s = part.Item1.GetStartInfo(BinarySet.TargetPlatform, part.Item2, comPort ?? throw new InvalidOperationException("Couldn't get the port"));
                    s.RedirectStandardOutput = true;
                    s.RedirectStandardError = true;
                    s.RedirectStandardInput = true;
                    s.CreateNoWindow = true;
                    s.WorkingDirectory = workingDir ?? s.WorkingDirectory;
                    return new StreamingConsoleViewModel($"{BinarySet.Name} ({index + 1} of {details.Count})", s);
                }).ToList();
            }
            else
            {
                throw new InvalidOperationException($"Couldn't get a tool for binary {plan.UnflashableBinary?.Path ?? "(unknown)"}");
            }
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
