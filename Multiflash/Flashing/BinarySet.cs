using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    public class BinarySet
    {
        public string? TargetPlatform { get; init; }
        public IReadOnlyCollection<Binary> Binaries { get; init; } = Array.Empty<Binary>();
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public IReadOnlyCollection<Verification> Verifications { get; init; } = Array.Empty<Verification>();
        public ConfigTemplate? ConfigTemplate { get; init; }

        public static async Task<(string extractLocation, BinarySet? contents)> Extract(string archivePath)
        {
            var tempName = Path.Combine(Path.GetTempPath(), "multiflash", Path.GetRandomFileName());
            if (Directory.Exists(tempName))
                Directory.Delete(tempName, true);
            var dir = Directory.CreateDirectory(tempName);
            ZipFile.ExtractToDirectory(archivePath, tempName);
            var json = File.OpenRead(Path.Combine(dir.FullName, "set.json"));
            var set = await JsonSerializer.DeserializeAsync<BinarySet>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return (tempName, set);
        }
        public static async Task<BinarySet> ReadSetAsync(string archivePath)
        {
            Stream zipStream;
            try
            {
                zipStream = ZipFile.OpenRead(archivePath).GetEntry("set.json")?.Open()!;
            }
            catch (Exception e)
            {
                throw new ZipFileException("Error opening path as Zip file", e);
            }
            if (zipStream is null)
            {
                throw new ZipFileException("Zip file did not contain the expected set.json file");
            }
            var maybeSet = await JsonSerializer.DeserializeAsync<BinarySet>(zipStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return maybeSet ?? throw new ArgumentException("The Zip archive content could not be read.");
        }
    }


    [Serializable]
    public class ZipFileException : Exception
    {
        public ZipFileException() { }
        public ZipFileException(string message) : base(message) { }
        public ZipFileException(string message, Exception inner) : base(message, inner) { }
        protected ZipFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
