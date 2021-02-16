using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.App
{
    interface IContinuableViewModel<out T> : INotifyPropertyChanged
    {
        T? NextViewModel { get; }
    }
}
