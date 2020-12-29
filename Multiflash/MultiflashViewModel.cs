using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    class MultiflashViewModel : INotifyPropertyChanged
    {
        public MultiflashViewModel() : this(new DummyToolset()) { }
        public MultiflashViewModel(IToolset toolset)
        {
            InitViewModel = new(toolset);
            viewModels.Push(InitViewModel);
            InitViewModel.PropertyChanged += InitViewModel_PropertyChanged;
        }

        readonly Stack<object> viewModels = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? CurrentViewModel => viewModels.TryPeek(out var vm) ? vm : null;

        public InitViewModel InitViewModel { get; }
        public ProcessSetViewModel? ProcessSet => InitViewModel.NextViewModel;

        void InitViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.IsFor(nameof(InitViewModel.NextViewModel)) && ProcessSet is not null)
            {
                viewModels.Push(ProcessSet);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
                InitViewModel.PropertyChanged -= InitViewModel_PropertyChanged;
            }
        }
    }
}
