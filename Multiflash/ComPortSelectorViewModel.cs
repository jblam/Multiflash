using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JBlam.Multiflash
{
    class ComPortSelectorViewModel : INotifyPropertyChanged
    {
        public ComPortSelectorViewModel()
        {
            Refresh = Command.Create(() =>
            {
                SerialPort.GetPortNames();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ports)));
            });
            Ports = SerialPort.GetPortNames();
        }

        private string? selectedPort;

        public ICommand Refresh { get; }
        public IReadOnlyCollection<string> Ports { get; private set; }
        public string? SelectedPort
        {
            get => selectedPort; set
            {
                selectedPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(selectedPort)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
