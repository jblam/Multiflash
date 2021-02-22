using JBlam.Multiflash.Tools;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JBlam.Multiflash
{
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
}
