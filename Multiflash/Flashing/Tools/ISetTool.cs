using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Tools
{
    using Binaries = IReadOnlyCollection<Binary>;

    public interface ISetTool
    {
        string Name { get; }
        bool IsInstalled();
        (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries);
        ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort);
    }
}
