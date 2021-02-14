using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Helpers
{
    static class PropertyChangeExtensions
    {
        public static bool IsFor(this PropertyChangedEventArgs args, string propertyName) =>
            string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == propertyName;
    }
}
