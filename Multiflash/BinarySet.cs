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
        public IReadOnlyCollection<Binary> Binaries { get; init; } = Array.Empty<Binary>();
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;

        public static async Task<(string extractLocation, BinarySet? contents)> Extract(string archivePath)
        {
            var tempName = Path.Combine(Path.GetTempPath(), "multiflash", Path.GetRandomFileName());
            if (Directory.Exists(tempName))
                Directory.Delete(tempName, true);
            var dir = Directory.CreateDirectory(tempName);
            ZipFile.ExtractToDirectory(archivePath, tempName);
            var json = File.OpenRead(Path.Combine(dir.FullName, "set.json"));
            var set = await JsonSerializer.DeserializeAsync<BinarySet>(json);
            return (tempName, set);
        }
        public static async Task<BinarySet?> ReadSetAsync(string archivePath)
        {
            var stream = ZipFile.OpenRead(archivePath).GetEntry("set.json")?.Open();
            return stream is null
                ? null
                : await JsonSerializer.DeserializeAsync<BinarySet>(stream);
        }
    }
}
