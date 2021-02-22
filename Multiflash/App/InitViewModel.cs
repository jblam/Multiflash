using JBlam.Multiflash.Helpers;
using JBlam.Multiflash.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JBlam.Multiflash.App
{
    class InitViewModel : IContinuableViewModel<ProcessSetViewModel>, INotifyPropertyChanged
    {
        public InitViewModel(Toolset toolset)
        {
            Toolset = toolset ?? throw new ArgumentNullException(nameof(toolset));

            // We explicitly don't want to "run this command at startup" because it leads to a wonky
            // UX if the user hasn't yet connected the device. It *looks* like we're providing "live
            // updates" but we're not.
            RefreshPorts = Command.Create(() =>
            {
                Ports = SerialPort.GetPortNames();
                if (!Ports.Contains(SelectedPort))
                    SelectedPort = null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ports)));
            });
            StartTools = Command.Create(async () =>
            {
                try
                {
                    NextViewModel = new ProcessSetViewModel(toolset);
                    var (extractedLocation, extractedSet) = await BinarySet.Extract(BinarySetViewModel.BinarySetPath!);
                    await NextViewModel.SetBinaries(extractedSet!, SelectedPort!, extractedLocation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show(Application.Current.MainWindow, "Failed to extract data.\r\n\r\n" + ex.ToString(), "Flashing failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                }
            }, () => SelectedPort != null && Toolset.IsInstalled && (BinarySetViewModel.BinarySetTask?.IsCompletedSuccessfully ?? false));
            BinarySetViewModel.PropertyChanged += (_, args) =>
            {
                if (args.IsFor(nameof(BinarySetViewModel.EffectiveViewModel)))
                    StartTools.RaiseCanExecuteChanged();
            };
        }

        private bool? isDragDropValid;
        private string? selectedPort;
        private ProcessSetViewModel? nextViewModel;

        public BinarySetViewModel BinarySetViewModel { get; } = new BinarySetViewModel();
        public ICommand RefreshPorts { get; }
        public ICommand StartTools { get; }
        public IReadOnlyCollection<string>? Ports { get; private set; }
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

        public Toolset Toolset { get; }

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
        public void OnDrop(DragEventArgs args)
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
            BinarySetViewModel.BinarySetPath = paths[0];
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
