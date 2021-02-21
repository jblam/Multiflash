using JBlam.Multiflash.Helpers;
using JBlam.Multiflash.Serial;
using System.ComponentModel;
using System.Windows.Input;

namespace JBlam.Multiflash.App.ConfigItems
{
    public class VerificationViewModel : INotifyPropertyChanged
    {

        internal VerificationViewModel(Verification verification, SerialConnection serialConnection)
        {
            Verification = verification;
            if (verification.Prompt is string prompt)
            {
                Send = Command.Create(
                    async () => Response = await serialConnection.Prompt(prompt, true));
            }
            else
            {
                Send = Command.Create(() => {}, () => false);
            }
            if (verification.ResponsePrefix is string prefix)
            {
                serialConnection.Output.CollectionChanged += (sender, e) =>
                {
                    var newItem = serialConnection.Output[e.NewStartingIndex];
                    if (newItem.IsTerminated && newItem.Direction == MessageDirection.FromRemote && newItem.Content.StartsWith(prefix))
                    {
                        Response = newItem.Content;
                    }
                };
            }
        }

        public Verification Verification { get; }
        public ICommand Send { get; }
        private string? response;
        public string? Response
        {
            get => response;
            private set
            {
                response = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Response)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
