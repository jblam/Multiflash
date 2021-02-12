using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JBlam.Multiflash
{
    class InitViewModel : INotifyPropertyChanged
    {
        public InitViewModel(IToolset toolset)
        {
            if (toolset is null)
                throw new ArgumentNullException(nameof(toolset));
            RefreshPorts = Command.Create(() =>
            {
                Ports = SerialPort.GetPortNames();
                if (!Ports.Contains(SelectedPort))
                    SelectedPort = null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ports)));
            });
            ClearDroppedSet = Command.Create(() => DroppedSet = null, () => DroppedSet != null);
            StartTools = Command.Create(() =>
            {
                NextViewModel = new ProcessSetViewModel(toolset);
                // TODO: actually extract the contents
                _ = NextViewModel.SetBinaries(DroppedSet!, SelectedPort!);
            }, () => SelectedPort != null && droppedPath != null && DroppedSet != null);
            Ports = SerialPort.GetPortNames();
        }

        private bool? isDragDropValid;
        private string? selectedPort;
        private BinarySet? droppedSet;
        private string? droppedPath;
        private ProcessSetViewModel? nextViewModel;

        public ICommand RefreshPorts { get; }
        public ICommand ClearDroppedSet { get; }
        public ICommand StartTools { get; }
        public IReadOnlyCollection<string> Ports { get; private set; }
        public string? SelectedPort
        {
            get => selectedPort;
            set
            {
                selectedPort = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPort)));
                StartTools.RaiseCanExecuteChanged();
            }
        }
        public BinarySet? DroppedSet
        {
            get => droppedSet;
            private set
            {
                droppedSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DroppedSet)));
                ClearDroppedSet.RaiseCanExecuteChanged();
                StartTools.RaiseCanExecuteChanged();
            }
        }
        public ProcessSetViewModel? NextViewModel
        {
            get => nextViewModel;
            private set
            {
                nextViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextViewModel)));
            }
        }

        public bool? IsDragDropValid
        {
            get => isDragDropValid; private set
            {
                isDragDropValid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDragDropValid)));
            }
        }

        public void OnDragEnter(DragEventArgs args)
        {
            IsDragDropValid = args.Data.GetDataPresent(DataFormats.FileDrop);
        }

        public void OnDragOver(DragEventArgs args)
        {
            args.Effects = args.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }
        public void OnDragLeave(DragEventArgs args)
        {
            IsDragDropValid = null;
        }
        public async void OnDrop(DragEventArgs args)
        {
            IsDragDropValid = null;
            var data = args.Data.GetData(DataFormats.FileDrop);
            if (data is not string[] paths)
            {
                throw new InvalidOperationException("Allowed drop which did not contain any files");
            }
            if (paths.Length != 1)
            {
                throw new NotSupportedException("Multiple files not supported");
            }
            droppedPath = paths[0];
            DroppedSet = await BinarySet.ReadSetAsync(droppedPath);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
