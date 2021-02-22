using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    public class ConfigTemplate
    {
        /// <summary>
        /// The serial command which instructs the device to update its configuration
        /// </summary>
        public string? SerialCommand { get; init; }
        /// <summary>
        /// The configuration template
        /// </summary>
        /// <remarks>
        /// For serialisation reasons this is public, but should not be treated as usable
        /// outside the API
        /// </remarks>
        [JsonInclude]
        public JsonElement? Template { get; internal init; }
        /// <summary>
        /// Gets the set of settable configuration parameters declared by the template
        /// </summary>
        public IReadOnlyCollection<Parameter> Parameters { get; init; } = Array.Empty<Parameter>();
        /// <summary>
        /// Constructs the final JSON by direct string replacement of the parameter values into the
        /// original JSON string defined by the template.
        /// </summary>
        /// <param name="lookup">
        /// A function which returns a string value for the specified parameter. A <see langword="null"/>
        /// return will cause the <see cref="Build(Func{Parameter, string?})"/> to throw.
        /// </param>
        /// <returns>The constructed JSON</returns>
        /// <remarks>
        /// Only JSON-string parameter values are supported. Neither the supplied values nor the constructed
        /// output are checked for syntax or structure.
        /// </remarks>
        public string Build(Func<Parameter, string?> lookup)
        {
            var templateString = JsonSerializer.Serialize(Template);
            foreach (var parameter in Parameters)
            {
                templateString = templateString.Replace(
                    $"{{{{{parameter.Identifier}}}}}",
                    lookup(parameter) ?? throw new ArgumentException($"Failed to find a replacement for parameter `{parameter.Identifier}`"));
            }
            return templateString;
        }
    }
}
