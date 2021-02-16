using JBlam.Multiflash.Serial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.App
{
    class ConfigurationViewModel
    {
        public ConfigurationViewModel(string comPort)
        {
            Connection = SerialConnection.Open(comPort, 115200);
        }

        SerialConnection Connection { get; }

        public ObservableCollection<Message> SerialContent => Connection.Output;
    }
}
