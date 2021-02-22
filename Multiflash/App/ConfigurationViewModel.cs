using JBlam.Multiflash.App.ConfigItems;
using JBlam.Multiflash.Helpers;
using JBlam.Multiflash.Serial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JBlam.Multiflash.App
{
    class ConfigurationViewModel : INotifyPropertyChanged
    {
        public ConfigurationViewModel(BinarySet binarySet, string comPort)
        {
            Connection = SerialConnection.Open(comPort, 115200);
            GetStatus = Command.Create(async () => StatusValue = await Connection.Prompt("?", true));
            Verifications = binarySet.Verifications.Select(v => new VerificationViewModel(v, Connection)).ToList();
            Parameters = binarySet.ConfigTemplate?.Parameters.Select(p => new ParameterViewModel(p)).ToList() as IReadOnlyCollection<ParameterViewModel>
                ?? Array.Empty<ParameterViewModel>();
            CommitConfig = Command.Create(
                () => Connection.Prompt($"{binarySet.ConfigTemplate!.SerialCommand} {binarySet.ConfigTemplate!.Build(GetValue)}"),
                () => (binarySet.ConfigTemplate?.SerialCommand) != null && Parameters.All(p => !p.IsRequired || p.Value != null));

            foreach (var parameter in Parameters)
            {
                parameter.PropertyChanged += (sender, args) => CommitConfig.RaiseCanExecuteChanged();
            }
        }

        string? statusValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        // TODO: this will display like arse if there are no verifications or no parameters.

        public IReadOnlyCollection<VerificationViewModel> Verifications { get; }
        public IReadOnlyCollection<ParameterViewModel> Parameters { get; }
        public ICommand CommitConfig { get; }

        SerialConnection Connection { get; }
        public ICommand GetStatus { get; }
        public string? StatusValue
        {
            get => statusValue;
            private set
            {
                statusValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(statusValue)));
            }
        }

        public ObservableCollection<Message> SerialContent => Connection.Output;

        internal string? GetValue(Parameter p)
        {
            var vm = Parameters.FirstOrDefault(vm => vm.Parameter.Identifier == p.Identifier);
            return vm?.Value ?? vm?.Parameter.Fallback;
        }
    }
}
