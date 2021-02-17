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
    public interface ITool
    {
        bool CanHandle(Binary binary);
        ProcessStartInfo GetStartInfo(Binary binary, string comPort);
    }

    public interface ISetTool
    {
        (Binaries handled, Binaries remaining) CanHandle(Binaries binaries);
        ProcessStartInfo GetStartInfo(Binaries binaries, string comPort);
    }

    public class Avrdude : ITool, ISetTool
    {
        private readonly string exePath;
        private readonly string configPath;

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

        public bool CanHandle(Binary binary)
        {
            return binary.Format == BinaryFormat.Hex;
        }

        public (Binaries handled, Binaries remaining) CanHandle(Binaries binaries)
        {
            var first = binaries.First();
            if (CanHandle(first))
            {
                return (new[] { first }, binaries.Skip(1).ToList());
            }
            else
            {
                return (Array.Empty<Binary>(), binaries);
            }
        }

        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            var output = new ProcessStartInfo(exePath);
            output.ArgumentList.Add(@"-C");
            output.ArgumentList.Add(configPath);
            output.ArgumentList.Add("-patmega328p");
            output.ArgumentList.Add("-carduino");
            output.ArgumentList.Add("-P");
            output.ArgumentList.Add(comPort);
            output.ArgumentList.Add("-b115200");
            output.ArgumentList.Add("-D");
            // Note that we don't quote the path here, because ArgumentList will inappropriately escape the quotes.
            output.ArgumentList.Add($"-Uflash:w:{binary.Path}:i");
            return output;
        }

        public ProcessStartInfo GetStartInfo(Binaries binaries, string comPort)
        {
            if (binaries.Count != 1)
                throw new InvalidOperationException("AVRDUDE cannot compose multiple binaries");
            return GetStartInfo(binaries.First(), comPort);
        }
    }

    public class EspUploaderPyTool : ITool, ISetTool
    {
        private readonly string pythonPath;
        private readonly string uploadScriptPath;
        private static readonly IReadOnlyCollection<string> validTargetPlatforms = new[]
        {
            "esp8266",
            "esp32"
        };

        public EspUploaderPyTool(string pythonPath, string uploadScriptPath)
        {
            if (string.IsNullOrEmpty(pythonPath))
            {
                throw new ArgumentException($"'{nameof(pythonPath)}' cannot be null or empty", nameof(pythonPath));
            }

            if (string.IsNullOrEmpty(uploadScriptPath))
            {
                throw new ArgumentException($"'{nameof(uploadScriptPath)}' cannot be null or empty", nameof(uploadScriptPath));
            }

            this.pythonPath = pythonPath;
            this.uploadScriptPath = uploadScriptPath;
        }
        public bool CanHandle(Binary binary) => binary.Format == BinaryFormat.Bin
            && binary.TargetPlatform is string platform
            && validTargetPlatforms.Contains(platform.ToLowerInvariant());


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

        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            return new ProcessStartInfo(pythonPath)
            {
                ArgumentList =
                {
                    uploadScriptPath,
                    "--chip",
                    binary.TargetPlatform!.ToLowerInvariant(),
                    "--port",
                    comPort,
                    "--baud",
                    "115200",
                    "write_flash",
                    binary.StartAddress.ToString("X"),
                    binary.Path
                }
            };
        }

        public (Binaries handled, Binaries remaining) CanHandle(Binaries binaries)
        {
            // JB 2021-02-16:
            // For now, let's just assume all .bin files can be uploaded.
            // If anything bad happens, we'll blame the person who produced the set
            var handled = binaries.TakeWhile(CanHandle).ToList();
            var unhandled = binaries.Skip(handled.Count).ToList();
            return (handled, unhandled);
        }

        public ProcessStartInfo GetStartInfo(Binaries binaries, string comPort)
        {
            var output = new ProcessStartInfo(pythonPath)
            {
                ArgumentList =
                {
                    uploadScriptPath,
                    "--chip",
                    // TODO: targetplatform should go on the set I guess
                    "--port",
                    comPort,
                    "--baud",
                    "460800", // TODO: PIO's value is different to arduino's?
                    "write_flash"
                }
            };
            foreach (var binary in binaries)
            {
                if (!CanHandle(binary))
                    throw new InvalidOperationException("ProcessStartInfo requested for an incompatible binary");
                output.ArgumentList.Add(binary.StartAddress.ToString("X"));
                output.ArgumentList.Add(binary.Path);
            }
            return output;
        }
    }

    public class DemoTool : ITool, ISetTool
    {
        public bool CanHandle(Binary binary) => true;

        public (Binaries handled, Binaries remaining) CanHandle(Binaries binaries)
        {
            return (binaries, Array.Empty<Binary>());
        }

        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            var output = new ProcessStartInfo(@"Multiflash.DemoTool.exe")
            {
                Arguments = @"\r"
            };
            return output;
        }

        public ProcessStartInfo GetStartInfo(Binaries binaries, string comPort) => new ProcessStartInfo(@"Multiflash.DemoTool.exe")
        {
            Arguments = @"\r"
        };
    }
}
