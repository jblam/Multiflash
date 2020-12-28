using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JBlam.Multiflash.Helpers
{
    class ConditionalValueConverter : IValueConverter
    {
        public object? TrueValue { get; set; }
        public object? FalseValue { get; set; }
        public object? NullValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b
                ? (b ? TrueValue : FalseValue)
                : NullValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
