using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace JBlam.Multiflash.Tools
{
    public class ArduinoToolset : Toolset
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
        readonly ISetTool espUploaderPyTool = new PythonTool(
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\tools\python3\3.7.2-post1\python.exe"),
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\hardware\esp8266\2.7.4\tools\upload.py"),
            new EspUploader());
        readonly ISetTool esptool = new StandaloneExeTool(
            Path.Combine(ExpectedAppDataRoot, @"packages\esp32\tools\esptool_py\2.6.1\esptool.exe"),
            new Esptool());

        public override FlashPlan GetPlan(BinarySet set) => GetPlan(set, new ISetTool[] { avrdude, esptool });
    }
}
