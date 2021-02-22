﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Tools
{
    using Binaries = IReadOnlyCollection<Binary>;
    public abstract class Toolset
    {
        internal static FlashPlan GetPlan(BinarySet set, IReadOnlyCollection<ISetTool> tools)
        {
            (ISetTool tool, Binaries handled, Binaries remaining)? GetNextTool(string? targetPlatform, Binaries binaries) =>
                tools.Select(tool =>
                {
                    var (handled, remaining) = tool.CanHandle(targetPlatform, binaries);
                    return (tool, handled, remaining);
                }).SkipWhile(t => !t.handled.Any())
                  .Take(1)
                  .Cast<(ISetTool, Binaries, Binaries)?>()
                  .FirstOrDefault();

            List<(ISetTool, Binaries)> output = new();
            Binaries remaining = set.Binaries;
            while (remaining.Any())
            {
                if (GetNextTool(set.TargetPlatform, remaining) is (ISetTool, Binaries, Binaries) value && value.handled.Any())
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
}
