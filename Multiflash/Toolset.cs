using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    public interface IToolset
    {
        ITool? GetToolForBinary(Binary binary);
    }

    public class ArduinoToolset : IToolset
    {
        static readonly string ExpectedProgramFilesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Arduino");
        static readonly string ExpectedAppDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Arduino15");

        readonly Avrdude avrdude = new(
            Path.Combine(ExpectedProgramFilesRoot, @"hardware\tools\avr\bin\avrdude.exe"),
            Path.Combine(ExpectedProgramFilesRoot, @"hardware\tools\avr\etc\avrdude.conf"));
        readonly EspUploaderPyTool espUploaderPyTool = new(
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\tools\python3\3.7.2-post1\python.exe"),
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\hardware\esp8266\2.7.4\tools\upload.py"));

        public ITool? GetToolForBinary(Binary binary)
        {
            if (espUploaderPyTool.CanHandle(binary))
                return espUploaderPyTool;
            if (avrdude.CanHandle(binary))
                return avrdude;
            return null;
        }
    }
}
