using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash.App.ConfigItems
{
    public class ParameterViewModel
    {
        public ParameterViewModel(Parameter parameter)
        {
            Parameter = parameter;
        }
        public Parameter Parameter { get; }
        public string? Value { get; set; }
    }
}
