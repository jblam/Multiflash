using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    class MultiflashViewModel
    {
        public ComPortSelectorViewModel ComPortSelector { get; } = new();
    }
}
