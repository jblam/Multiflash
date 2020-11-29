using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    interface ITool
    {
        bool CanHandle(Binary binary);
        ProcessStartInfo GetStartInfo(Binary binary, string comPort);
    }

    public class Avrdude : ITool
    {
        public bool CanHandle(Binary binary)
        {
            return binary.Format == BinaryFormat.Hex;
        }

        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            var output = new ProcessStartInfo(@"C:\Program Files (x86)\Arduino\hardware\tools\avr\bin\avrdude.exe");
            output.ArgumentList.Add(@"-C");
            output.ArgumentList.Add(@"C:\Program Files (x86)\Arduino\hardware\tools\avr\etc\avrdude.conf");
            output.ArgumentList.Add("-patmega328p");
            output.ArgumentList.Add("-carduino");
            output.ArgumentList.Add("-P");
            output.ArgumentList.Add(comPort);
            output.ArgumentList.Add("-b115200");
            output.ArgumentList.Add("-D");
            output.ArgumentList.Add($"-Uflash:w:\"{binary.Path}\":i");
            return output;
        }
    }

    public class EspUploaderPyTool : ITool
    {
        public bool CanHandle(Binary binary) => binary.Format == BinaryFormat.Bin;
        public ProcessStartInfo GetStartInfo(Binary binary, string comPort)
        {
            var pythonDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Arduino15\packages\esp8266\tools\python3\3.7.2-post1");
            var scriptDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Arduino15\packages\esp8266\hardware\esp8266\2.7.4\tools");
            return new ProcessStartInfo(Path.Combine(pythonDir, "python.exe"))
            {
                ArgumentList =
                {
                    Path.Combine(scriptDir, "upload.py"),
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
