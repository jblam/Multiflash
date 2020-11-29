using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    public interface ITool
    {
        bool CanHandle(Binary binary);
        ProcessStartInfo GetStartInfo(Binary binary, string comPort);
    }

    public class Avrdude : ITool
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
    }

    public class EspUploaderPyTool : ITool
    {
        private readonly string pythonPath;
        private readonly string uploadScriptPath;

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
        public bool CanHandle(Binary binary) => binary.Format == BinaryFormat.Bin;
        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            return new ProcessStartInfo(pythonPath)
            {
                ArgumentList =
                {
                    uploadScriptPath,
                    "--chip",
                    "esp8266",
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
    }

    public class DemoTool : ITool
    {
        public bool CanHandle(Binary binary) => true;
        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            var output = new ProcessStartInfo("cmd.exe");
            output.Arguments = "/c DIR";
            return output;
        }
    }
}
