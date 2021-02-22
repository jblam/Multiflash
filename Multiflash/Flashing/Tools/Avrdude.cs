using JBlam.Multiflash.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JBlam.Multiflash.Tools
{
    using Binaries = IReadOnlyCollection<Binary>;

    class Avrdude : CliSetTool
    {
        public override string Name => "AVRDude";

        private readonly string configPath;
        private static readonly IReadOnlyCollection<string> validTargetPlatforms = new[]
        {
            "atmega328p"
        };

        public Avrdude(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentException($"'{nameof(configPath)}' cannot be null or empty", nameof(configPath));
            }
            this.configPath = configPath;
        }

        static bool CanHandle(string? targetPlatform, Binary binary)
        {
            return binary.Format == BinaryFormat.Hex && validTargetPlatforms.Contains(targetPlatform);
        }
        public override (Binaries handled, Binaries remaining) CanHandle(string? targetPlatform, Binaries binaries)
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
        public override void AppendCliArgs(ICollection<string> processArgs, string? targetPlatform, Binaries binaries, string comPort)
        {
            processArgs.Add(@"-C");
            processArgs.Add(configPath);
            processArgs.Add("-p" + targetPlatform);
            processArgs.Add("-carduino");
            processArgs.Add("-P");
            processArgs.Add(comPort);
            processArgs.Add("-b115200");
            processArgs.Add("-D");
            // Note that we don't quote the path here, because ArgumentList will inappropriately escape the quotes.
            processArgs.Add($"-Uflash:w:{binaries.Single().Path}:i");
        }
        internal override bool IsInstalled() => File.Exists(configPath);
    }
}
