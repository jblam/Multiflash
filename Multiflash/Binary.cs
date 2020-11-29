using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.Multiflash
{
    public record Binary(BinaryFormat Format, string Path, long StartAddress = 0);
    public enum BinaryFormat
    {
        Unknown,
        Bin,
        Hex
    }
}
