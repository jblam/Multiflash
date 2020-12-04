using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Helpers
{
    public class StringComposer
    {
        public static string Compose(string first, string second)
        {
            var backspaceCount = second
                .Take(first.Length)
                .TakeWhile(c => c == '\b')
                .Count();
            if (backspaceCount == 0)
            {
                return first + second;
            }
            return first[0..^backspaceCount] + second[backspaceCount..];
        }
    }
}
