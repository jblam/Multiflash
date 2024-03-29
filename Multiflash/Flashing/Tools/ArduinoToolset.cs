﻿using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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


        static readonly ISetTool avrdude = new StandaloneExeTool(
            Path.Combine(ExpectedProgramFilesRoot, @"hardware\tools\avr\bin\avrdude.exe"),
            new Avrdude(Path.Combine(ExpectedProgramFilesRoot, @"hardware\tools\avr\etc\avrdude.conf")));

        // TODO: the CLI is not implemented due to outstanding questions about the difference between upload and esptool.
        // ESP8266 compat is not required for initial release.
        static readonly ISetTool espUploaderPyTool = new PythonTool(
            FindVersionedPath(Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\tools\python3"), "python.exe"),
            FindVersionedPath(Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\hardware\esp8266"), @"tools\upload.py"),
            new EspUploader());
        static readonly ISetTool esptool = new StandaloneExeTool(
            FindVersionedPath(Path.Combine(ExpectedAppDataRoot, @"packages\esp32\tools\esptool_py"), "esptool.exe"),
            new Esptool());

        readonly IReadOnlyCollection<ISetTool> tools = new[]
        {
            avrdude, esptool
        };
        public override FlashPlan GetPlan(BinarySet set) => GetPlan(set, tools);

        public override IEnumerable<ISetTool> MissingTools => tools.Where(t => !t.IsInstalled());

        public override string ToolsetName => "Arduino tools";
    }
}
