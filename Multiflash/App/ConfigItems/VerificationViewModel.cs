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
            Send = Command.Create(async () => Response = await serialConnection.Prompt(Verification.Prompt, true));
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
