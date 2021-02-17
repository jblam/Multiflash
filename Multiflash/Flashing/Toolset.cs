using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    using Binaries = IReadOnlyCollection<Binary>;
    using Plan = IReadOnlyCollection<(ISetTool, IReadOnlyCollection<Binary>)>;

    public class FlashPlan
    {
        public static FlashPlan Success(Plan plan) => new FlashPlan { Plan = plan };
        public static FlashPlan Failure(Binary unflashableBinary, Plan? partialPlan = null) => new FlashPlan
        {
            Plan = partialPlan,
            UnflashableBinary = unflashableBinary
        };
        public bool IsSuccess([NotNullWhen(true)] out Plan? plan)
        {
            plan = Plan;
            return plan is not null && UnflashableBinary is null;
        }
        public bool IsError([NotNullWhen(true)] out Binary? unflashableBinary)
        {
            unflashableBinary = UnflashableBinary;
            return unflashableBinary is not null;
        }
        public Plan? Plan { get; private init; }
        public Binary? UnflashableBinary { get; private init; }
    }
    public interface IToolset
    {
        FlashPlan GetPlan(BinarySet set);
    }

    public class DummyToolset : IToolset
    {
        public FlashPlan GetPlan(BinarySet set) => FlashPlan.Success(new[] { ((ISetTool)new DemoTool(), set.Binaries) });
    }

    public abstract class Toolset : IToolset
    {
        internal static FlashPlan GetPlan(BinarySet set, IReadOnlyCollection<ISetTool> tools)
        {
            (ISetTool tool, Binaries handled, Binaries remaining)? GetNextTool(Binaries binaries)
            {
                return tools.Select(tool =>
                {
                    var (handled, remaining) = tool.CanHandle(binaries);
                    return (tool, handled, remaining);
                }).SkipWhile(t => !t.handled.Any()).Take(1).Cast<(ISetTool, Binaries, Binaries)?>().FirstOrDefault();
            }

            List<(ISetTool, Binaries)> output = new();
            Binaries remaining = set.Binaries;
            while (remaining.Any())
            {
                if (GetNextTool(remaining) is (ISetTool, Binaries, Binaries) value && value.handled.Any())
                {
                    output.Add((value.tool, value.handled));
                    remaining = value.remaining;
                }
                else
                {
                    return FlashPlan.Failure(remaining.First());
                }
            }
            return FlashPlan.Success(output);
        }

        public abstract FlashPlan GetPlan(BinarySet set);
    }

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
        readonly EspUploaderPyTool espUploaderPyTool = new(
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\tools\python3\3.7.2-post1\python.exe"),
            Path.Combine(ExpectedAppDataRoot, @"packages\esp8266\hardware\esp8266\2.7.4\tools\upload.py"));

        public override FlashPlan GetPlan(BinarySet set) => GetPlan(set, new ISetTool[] { espUploaderPyTool });
    }
}
