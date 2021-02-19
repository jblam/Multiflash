using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    using Binaries = IReadOnlyCollection<Binary>;

    public interface ISetTool
    {
        (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries);
        ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort);
    }

    public class Avrdude : ISetTool
    {
        private readonly string exePath;
        private readonly string configPath;
        private static readonly IReadOnlyCollection<string> validTargetPlatforms = new[]
        {
            "atmega328p"
        };

        public Avrdude(string exePath, string configPath)
        {
            if (string.IsNullOrEmpty(exePath))
            {
                throw new ArgumentException($"'{nameof(exePath)}' cannot be null or empty", nameof(exePath));
            }

            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentException($"'{nameof(configPath)}' cannot be null or empty", nameof(configPath));
            }

            this.exePath = exePath;
            this.configPath = configPath;
        }

        static bool CanHandle(string? targetPlatform, Binary binary)
        {
            return binary.Format == BinaryFormat.Hex && validTargetPlatforms.Contains(targetPlatform);
        }

        public (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries)
        {
            var first = binaries.First();
            if (CanHandle(targetPlatform, first))
            {
                return (new[] { first }, binaries.Skip(1).ToList());
            }
            else
            {
                return (Array.Empty<Binary>(), binaries);
            }
        }

        ProcessStartInfo GetStartInfo(string? targetPlatform, Binary binary, string comPort)
        {
            var output = new ProcessStartInfo(exePath);
            output.ArgumentList.Add(@"-C");
            output.ArgumentList.Add(configPath);
            output.ArgumentList.Add("-p" + targetPlatform);
            output.ArgumentList.Add("-carduino");
            output.ArgumentList.Add("-P");
            output.ArgumentList.Add(comPort);
            output.ArgumentList.Add("-b115200");
            output.ArgumentList.Add("-D");
            // Note that we don't quote the path here, because ArgumentList will inappropriately escape the quotes.
            output.ArgumentList.Add($"-Uflash:w:{binary.Path}:i");
            return output;
        }

        public ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort)
        {
            if (binaries.Count != 1)
                throw new InvalidOperationException("AVRDUDE cannot compose multiple binaries");
            return GetStartInfo(targetPlatform, binaries.First(), comPort);
        }
    }

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

        /* Sensor, ESP32, PIO build
         * ".platformio\penv\scripts\python.exe" ".platformio\packages\tool-esptoolpy\esptool.py" --chip esp32 --port "COM4" --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 40m --flash_size detect 0x1000 .platformio\packages\framework-arduinoespressif32\tools\sdk\bin\bootloader_dio_40m.bin 
0x8000 "partitions.bin" 0xe000 .platformio\packages\framework-arduinoespressif32\tools\partitions\boot_app0.bin 0x10000 firmware.bin
         */

        /* Hub application, ESP32, PIO build
         * ".platformio\penv\scripts\python.exe" ".platformio\packages\tool-esptoolpy\esptool.py" --chip esp32 --port "COM4" --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 40m --flash_size detect 0x1000 .platformio\packages\framework-arduinoespressif32\tools\sdk\bin\bootloader_dio_40m.bin 
0x8000 "partitions.bin" 0xe000 .platformio\packages\framework-arduinoespressif32\tools\partitions\boot_app0.bin 0x10000 firmware.bin
        */

        /* Hub filesystem, ESP32, PIO build
         * ".platformio\penv\scripts\python.exe" ".platformio\packages\tool-esptoolpy\esptool.py" --chip esp32 --port "COM4" --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_size detect 
2686976 spiffs.bin
        */


    public class DemoTool : ISetTool
    {
        public (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries)
        {
            return (binaries, Array.Empty<Binary>());
        }

        public ProcessStartInfo GetStartInfo(string? targetPlatform, Binaries binaries, string comPort) => new ProcessStartInfo(@"Multiflash.DemoTool.exe")
        {
            Arguments = @"\r"
        };
    }
}
