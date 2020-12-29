using System;
using System.Linq;
using System.Threading.Tasks;

namespace Multiflash.DemoTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("This is a demo tool for Multiflash.");
            Console.WriteLine();
            var method = GetMethodFromArgs(args);
            switch (method)
            {
                case ConsoleOverwriteMethod.Position:
                    Console.WriteLine("Using console position");
                    break;
                case ConsoleOverwriteMethod.Backspace:
                    Console.WriteLine("Using backspace");
                    break;
                case ConsoleOverwriteMethod.CarriageReturn:
                    Console.WriteLine("Using carriage-return");
                    break;
                default:
                    Console.WriteLine("No console overwrite behaviour specified. Valid options are 'position', 'backspace', 'return'.");
                    break;
            }
            var report = new ProgressReport(method, 10, "[", "]");
            for (double i = 0; i <= 1; i += 0.101)
            {
                if (i > 0)
                    await Task.Delay(1000);
                report.Report(i);
            }
            report.Report(1);
            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        class ProgressReport : IProgress<double>
        {
            private readonly ConsoleOverwriteMethod method;
            private readonly string leading;
            private readonly string trailing;
            private readonly int width;

            public ProgressReport(ConsoleOverwriteMethod method, int width, string leading = null, string trailing = null)
            {
                this.method = method;
                this.leading = leading ?? string.Empty;
                this.trailing = trailing ?? string.Empty;
                this.width = width;
            }
            public void Report(double value)
            {
                switch (method)
                {
                    case ConsoleOverwriteMethod.Position:
                        Console.CursorLeft = 0;
                        break;
                    case ConsoleOverwriteMethod.Backspace:
                        Console.Write(string.Join(null, Enumerable.Repeat('\b', leading.Length + trailing.Length + width)));
                        break;
                    case ConsoleOverwriteMethod.CarriageReturn:
                        Console.Write('\r');
                        break;
                    default:
                        Console.WriteLine();
                        break;
                }

                var progressCharCount = (int)(width * Math.Max(Math.Min(1, value), 0));
                Console.Write(leading);
                Console.Write(string.Join(null, Enumerable.Repeat('#', progressCharCount)));
                Console.Write(string.Join(null, Enumerable.Repeat(' ', width - progressCharCount)));
                Console.Write(trailing);
            }
        }

        static ConsoleOverwriteMethod GetMethodFromArgs(string[] args)
        {

            foreach (var arg in args)
            {
                var lowerArg = arg.ToLowerInvariant();
                switch (lowerArg)
                {
                    case "pos":
                    case "position":
                        return ConsoleOverwriteMethod.Position;
                    case "backspace":
                    case "\\b":
                    case "backsp":
                        return ConsoleOverwriteMethod.Backspace;
                    case "cr":
                    case "return":
                    case "\\r":
                        return ConsoleOverwriteMethod.CarriageReturn;
                    default:
                        break;
                }
            }
            return ConsoleOverwriteMethod.NotSpecified;
        }
        enum ConsoleOverwriteMethod
        {
            NotSpecified,
            Position,
            Backspace,
            CarriageReturn
        }
    }
}
