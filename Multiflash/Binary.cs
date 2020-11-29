using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.Multiflash
{
    public record Binary(string Path, long StartAddress = 0)
    {
        public BinaryFormat Format => System.IO.Path.GetExtension(Path).ToLowerInvariant() switch
        {
            ".hex" => BinaryFormat.Hex,
            ".bin" => BinaryFormat.Bin,
            _ => BinaryFormat.Unknown
        };
    }
    public enum BinaryFormat
    {
        Unknown,
        Bin,
        Hex
    }
}
