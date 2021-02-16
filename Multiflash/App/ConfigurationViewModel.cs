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
        public ConfigurationViewModel(string comPort)
        {
            Connection = SerialConnection.Open(comPort, 115200);
            GetStatus = Command.Create(async () => StatusValue = await Connection.Prompt("?", true));
        }

        string? statusValue;

        public event PropertyChangedEventHandler? PropertyChanged;

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
