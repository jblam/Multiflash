using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Helpers
{
    /// <summary>
    /// Defines methods for composing string according to common rules implemented by terminal consoles.
    /// </summary>
    /// <remarks>The composition rules are:
    /// <list type="bullet">
    /// <item>The newline <c>\n</c> always starts a new line</item>
    /// <item>A Windows-style newline <c>\r\n</c> starts a new line</item>
    /// <item>Backspace <c>\b</c> erases the previous character on the current line</item>
    /// <item>A carriage-return <c>\r</c> erases the current line unless it's immediately followed by a newline <c>\n</c></item>
    /// </list>
    /// </remarks>
    public static class StringComposer
    {
        static IEnumerable<(T, T?, int)> ZipPeek<T>(this IEnumerable<T> t)
        {
            T item = default!;
            int index = 0;
            bool didInit = false;
            foreach (var current in t)
            {
                if (didInit)
                {
                    yield return (item, current, index);
                    index += 1;
                }
                item = current;
                didInit = true;
            }
            if (didInit)
            {
                yield return (item, default(T?), index);
            }
        }

        /// <summary>
        /// Produces lines of input, following common composition rules of terminal consoles.
        /// </summary>
        /// <param name="first">The existing text on the current line. This must not contain any newlines.</param>
        /// <param name="second">The new text to append. This may contain any characters.</param>
        /// <returns>An enumerable of lines to be added to the output, where the first item replaces <paramref name="first"/></returns>
        public static IEnumerable<string> ToLines(string first, string second)
        {
            Debug.Assert(!first.Contains('\n'));

            List<Range> ranges = new()
            {
                new(0, first.Length),
                new(0, 0),
            };
            foreach (var (c, peek, index) in second.ZipPeek())
            {
                var nextIndex = index + 1;
                if (c == '\b')
                {
                    // Backspace erases a character, if there is one to erase, and skips the actual char.
                    PopChar(ranges);
                    ranges.Add(new(nextIndex, nextIndex));
                }
                else if (c == '\r' && peek != '\n')
                {
                    // Carriage Return, if not followed by a newline, will reset to the next char.
                    ranges.Clear();
                    ranges.Add(new(0, 0));
                    ranges.Add(new(nextIndex, nextIndex));
                }
                else if (c == '\n')
                {
                    // Newline will yield the current state, then reset to the next char.
                    yield return BuildString(ranges, first, second);
                    ranges.Clear();
                    ranges.Add(new(0, 0));
                    ranges.Add(new(nextIndex, nextIndex));
                }
                else if (c != '\r')
                {
                    // Otherwise, exclude the current char only if it's the CR of a CRLF.
                    ranges[^1] = new(ranges[^1].Start, nextIndex);
                }
            }
            yield return BuildString(ranges, first, second);

            static string BuildString(List<Range> ranges, string first, string second)
            {
                var rangeAndReference = ranges.Select((range, idx) => (range, reference: idx == 0 ? first : second));
                return string.Join(null, rangeAndReference.Select(t => t.reference[t.range]));
            }

            static void PopChar(List<Range> ranges)
            {
                while (ranges.Count > 0)
                {
                    var candidate = ranges[^1];
                    Debug.Assert(!candidate.End.IsFromEnd);
                    if (candidate.End.Value > candidate.Start.Value)
                    {
                        // if we can shorten this range, do so
                        ranges[^1] = new(candidate.Start, candidate.End.Value - 1);
                        return;
                    }
                    else if (ranges.Count == 1)
                    {
                        // If we cannot shorten the only range remaining, bail.
                        // The first range always points to the "first string",
                        // so we need to retain a range even if it's zero-length.
                        return;
                    }
                    else
                    {
                        // The candidate is empty; pop it and try the previous.
                        ranges.RemoveAt(ranges.Count - 1);
                    }
                }
            }
        }
    }
}
