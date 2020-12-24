using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Helpers
{
    public class StringComposer
    {
        enum ComposeMode
        {
            Start,
            Push,
            Pop
        }
        public static string[] Split(string input) => input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string Compose(string first, string second)
        {
            var printables = BuildParts(first, second);
            var output = new char[printables.Sum(m => m.Length)];
            var index = 0;
            foreach (var item in printables)
            {
                item.CopyTo(output.AsMemory(index));
                index += item.Length;
            }
            return new string(output);
        }
        static List<ReadOnlyMemory<char>> BuildParts(string first, string second)
        {
            // assume that `first` is all-printable
            List<ReadOnlyMemory<char>> printables = new();
            Range activeRange = new Range(0, ^0);
            var mode = ComposeMode.Start;
            int index = 0;
            foreach (var c in second)
            {
                if (c == '\b')
                {
                    if (mode == ComposeMode.Push)
                        mode = ComposeMode.Pop;
                    activeRange = new Range(activeRange.Start, Decrement(activeRange.End));
                }
                else if (c == '\r')
                {
                    printables.Clear();
                    activeRange = default;
                    mode = ComposeMode.Pop;
                }
                else if (mode == ComposeMode.Push)
                {
                    Debug.Assert(!activeRange.End.IsFromEnd);
                    activeRange = new Range(activeRange.Start, activeRange.End.Value + 1);
                }
                else
                {
                    printables.Add(ActiveMemory());
                    activeRange = new(index, index + 1);
                    mode = ComposeMode.Push;
                }
                index += 1;
            }
            printables.Add(ActiveMemory());
            return printables;


            static Index Decrement(Index i)
            {
                if (i.IsFromEnd)
                    return new Index(i.Value + 1, true);
                else
                    return new Index(Math.Max(0, i.Value - 1), false);
            }
            ReadOnlyMemory<char> ActiveMemory()
            {
                var target = mode == ComposeMode.Start ? first : second;
                if (activeRange.End.IsFromEnd && activeRange.End.Value >= target.Length)
                    return ReadOnlyMemory<char>.Empty;
                if (activeRange.End.Value < activeRange.Start.Value)
                    return ReadOnlyMemory<char>.Empty;
                return target.AsMemory(activeRange);
            }
        }
    }
}
