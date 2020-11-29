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
    }
}
