using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace JBlam.Multiflash.Tools
{
    public class PlatformIoToolset : Toolset
    {
        static readonly string ExpectedAppDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @".platformio");
        readonly ISetTool espUploaderPyTool = new PythonTool(
            Path.Combine(ExpectedAppDataRoot, @"penv\scripts\python.exe"),
            Path.Combine(ExpectedAppDataRoot, @"packages\tool-esptoolpy\esptool.py"),
            new Esptool());

        public override FlashPlan GetPlan(BinarySet set) => GetPlan(set, new ISetTool[] { espUploaderPyTool });

        public override IEnumerable<ISetTool> MissingTools
        {
            get
            {
                if (!espUploaderPyTool.IsInstalled())
                    yield return espUploaderPyTool;
            }
        }

        public override string ToolsetName => "PlatformIO tools";
    }
}
