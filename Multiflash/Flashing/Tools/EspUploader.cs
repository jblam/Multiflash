using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;

namespace JBlam.Multiflash.Tools
{
    using Binaries = IReadOnlyCollection<Binary>;

    class EspUploader : CliSetTool
    {
        public override void AppendCliArgs(ICollection<string> processArgs, string? targetPlatform, Binaries binaries, string comPort)
        {
            throw new NotImplementedException("ESP8266 upload.py");
        }

        public override (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries)
        {
            throw new NotImplementedException("ESP8266 upload.py");
        }
    }
}
