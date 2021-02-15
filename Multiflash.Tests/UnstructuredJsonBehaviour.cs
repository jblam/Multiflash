using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Tests
{
    [TestClass]
    public class UnstructuredJsonBehaviour
    {
        record UnstructuredContent(string U, object Root);
        [TestMethod]
        public void CanDeserialiseUnstructured()
        {
            var json = @"{""u"": ""v"", ""root"":{""x"": ""x""}}";
            var doc = JsonSerializer.Deserialize<UnstructuredContent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(doc.U, "v");
            var unstructured = doc.Root;
            Assert.IsInstanceOfType(unstructured, typeof(JsonElement));
            Assert.IsTrue(((JsonElement)unstructured).TryGetProperty("x", out var prop));
            Assert.AreEqual(JsonValueKind.String, prop.ValueKind);
            Assert.AreEqual("x", prop.GetString());
        }
    }
}
