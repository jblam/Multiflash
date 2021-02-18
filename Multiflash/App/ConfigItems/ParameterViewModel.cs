using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.App.ConfigItems
{
    public class ParameterViewModel : INotifyPropertyChanged
    {
        private string? value;

        public ParameterViewModel(Parameter parameter)
        {
            Parameter = parameter;
        }
        public Parameter Parameter { get; }
        public string? Value
        {
            get => value;
            set
            {
                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
