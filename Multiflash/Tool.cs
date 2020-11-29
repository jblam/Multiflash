using System;
using System.Collections.Generic;
using System.Diagnostics;
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
