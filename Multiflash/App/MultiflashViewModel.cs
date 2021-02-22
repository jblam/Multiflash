using JBlam.Multiflash.Helpers;
using JBlam.Multiflash.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.App
{
    class MultiflashViewModel : INotifyPropertyChanged
    {
        public MultiflashViewModel() : this(new ArduinoToolset()) { }
        public MultiflashViewModel(Toolset toolset)
        {
            InitViewModel = new(toolset);
            viewModels.Push(InitViewModel);
            InitViewModel.PropertyChanged += ContinuableViewModel_PropertyChanged;
        }

        readonly Stack<object> viewModels = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? CurrentViewModel => viewModels.TryPeek(out var vm) ? vm : null;

        public InitViewModel InitViewModel { get; }
        public ProcessSetViewModel? ProcessSet => InitViewModel.NextViewModel;
        public ConfigurationViewModel? Configuration => ProcessSet?.NextViewModel;

        void ContinuableViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            var current = (sender as IContinuableViewModel<object>);
            if (args.IsFor(nameof(IContinuableViewModel<object>.NextViewModel)) && current?.NextViewModel is object next)
            {
                viewModels.Push(next);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
                current.PropertyChanged -= ContinuableViewModel_PropertyChanged;
                if (next is IContinuableViewModel<object> continuable)
                {
                    continuable.PropertyChanged += ContinuableViewModel_PropertyChanged;
                }
            }
        }
    }
}
