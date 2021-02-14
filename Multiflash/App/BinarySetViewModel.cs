using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JBlam.Multiflash.App
{
    class BinarySetViewModel : INotifyPropertyChanged
    {
        public BinarySetViewModel()
        {
            ClearDroppedSet = Command.Create(() => BinarySetPath = null, () => BinarySetPath != null);
        }
        string? binarySetPath;
        Task<BinarySet>? binarySetTask;

        public string? BinarySetPath
        {
            get => binarySetPath;
            set
            {
                binarySetPath = value;
                if (binarySetPath is string path)
                    BinarySetTask = BinarySet.ReadSetAsync(path);
                else
                    BinarySetTask = null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BinarySetPath)));
                ClearDroppedSet.RaiseCanExecuteChanged();
            }
        }

        public Task<BinarySet>? BinarySetTask
        {
            get => binarySetTask;
            private set {
                binarySetTask = value;
                if (binarySetTask?.Status < TaskStatus.RanToCompletion)
                    binarySetTask.ContinueWith(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffectiveViewModel))), TaskScheduler.FromCurrentSynchronizationContext());
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BinarySetTask)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EffectiveViewModel)));
            }
        }
        public object? EffectiveViewModel
        {
            get
            {
                if (binarySetTask is null)
                    return null;
                if (binarySetTask.IsCompletedSuccessfully)
                    return binarySetTask.Result;
                if (binarySetTask.IsFaulted)
                    return new Error { Exception = binarySetTask.Exception };
                if (binarySetTask.IsCanceled)
                    return new Error { Exception = new TaskCanceledException() };
                return null;
            }
        }
        public BinarySet? ExtractedSet => EffectiveViewModel as BinarySet;

        public ICommand ClearDroppedSet { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    struct Error
    {
        public Exception? Exception { get; init; }
        public string Message => (Exception is AggregateException agg ? agg.InnerException?.Message : Exception?.Message) ?? "Unspecified error";
    }
}
