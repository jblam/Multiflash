using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JBlam.Multiflash
{
    public record Binary(string Path)
    {
        public BinaryFormat Format => System.IO.Path.GetExtension(Path).ToLowerInvariant() switch
        {
            ".hex" => BinaryFormat.Hex,
            ".bin" => BinaryFormat.Bin,
            _ => BinaryFormat.Unknown
        };

        [JsonConverter(typeof(HexStringJsonConverter))]
        public long StartAddress { get; init; }
    }
    public enum BinaryFormat
    {
        Unknown,
        Bin,
        Hex
    }

    class HexStringJsonConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt64();
            var x = reader.GetString();
            // You might expect that long.Parse(blah, HexadecimalNumber) might be the go here.
            // However, for whatever reason AllowHexSpecifier does not, well, allow hex specifiers.
            // ¯\_(ツ)_/¯
            return Convert.ToInt64(x ?? throw new JsonException(), 16);
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"0x{value:X}");
        }
    }
}
