using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    public class BinarySet
    {
        public IReadOnlyCollection<Binary> Binaries { get; init; } = Array.Empty<Binary>();
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;

        public static async Task<BinarySet> Deserialise(string archivePath)
        {
            var archive = System.IO.Compression.ZipFile.OpenRead(archivePath);
            var set = await System.Text.Json.JsonSerializer.DeserializeAsync<BinarySet>(archive.GetEntry("set.json")?.Open() ?? throw new ArgumentException("Zip file did not contain expected entry `set.json`"));
            return set;
        }
    }
}
