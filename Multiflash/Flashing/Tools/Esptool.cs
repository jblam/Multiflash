using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JBlam.Multiflash.Tools
{
    using Binaries = IReadOnlyCollection<Binary>;

    class Esptool : CliSetTool
    {
        // TODO: this misrepresents compatibility between the ESP32 and ESP8266 SDKs.
        // - esptool.py (ESP32)
        // - uploader.py (ESP8266)
        // seems like the major incompatibility is the uploader.py does not support
        // multiple binaries.
        //
        // Decisions/research needed:
        // - do we *really need* multiple binaries? might be a slightly better UX to see
        //   each file being flashed
        // - do we want to support PIO's toolset in and of itself?
        // - are PIO's and Arduino's toolsets mutually compatible for the *same chip*?
        // - are the 8266 and 32 toolsets mutually compatible, just with different capabilities?

        public override string Name => "ESPTool";

        private static readonly IReadOnlyCollection<string> validTargetPlatforms = new[]
        {
            "esp8266",
            "esp32"
        };
        public override void AppendCliArgs(ICollection<string> processArgs, string? targetPlatform, Binaries binaries, string comPort)
        {
            processArgs.Add("--chip");
            processArgs.Add(targetPlatform!.ToLowerInvariant());
            processArgs.Add("--port");
            processArgs.Add(comPort);
            processArgs.Add("--baud");
            processArgs.Add("460800"); // TODO: PIO's value is different to arduino's
            processArgs.Add("write_flash");
            foreach (var binary in binaries)
            {
                if (!CanHandle(binary))
                    throw new InvalidOperationException("ProcessStartInfo requested for an incompatible binary");
                processArgs.Add($"0x{binary.StartAddress:X}");
                processArgs.Add(binary.Path);
            }
        }

        static bool CanHandlePlatform(string? targetPlatform) => targetPlatform is string platform
            && validTargetPlatforms.Contains(platform.ToLowerInvariant());
        static bool CanHandle(Binary binary) => binary.Format == BinaryFormat.Bin;
        public override (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries)
        {
            if (!CanHandlePlatform(targetPlatform))
                return (Array.Empty<Binary>(), binaries);
            var handled = binaries.TakeWhile(CanHandle).ToList();
            var unhandled = binaries.Skip(handled.Count).ToList();
            return (handled, unhandled);
        }
    }
}
