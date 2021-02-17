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
            Parameters = binarySet.ConfigTemplate.Parameters?.Select(p => new ParameterViewModel(p)).ToList() as IReadOnlyCollection<ParameterViewModel> ?? Array.Empty<ParameterViewModel>();
        }

        string? statusValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public IReadOnlyCollection<VerificationViewModel> Verifications { get; }
        public IReadOnlyCollection<ParameterViewModel> Parameters { get; }

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
    }
}
