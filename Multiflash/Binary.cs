using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.Multiflash
{
    public record Binary(BinaryFormat Format, string Path);
    public enum BinaryFormat
    {
        Unknown,
        Bin,
        Hex
    }
}
