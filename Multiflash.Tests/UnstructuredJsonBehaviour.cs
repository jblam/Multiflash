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
        record UnstructuredContent(string Fixed, object Template);
        [TestMethod]
        public void CanDeserialiseUnstructured()
        {
            var json = @"{""fixed"": ""v"", ""template"":{""x"": ""x""}}";
            var doc = JsonSerializer.Deserialize<UnstructuredContent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.AreEqual(doc.Fixed, "v");
            var unstructured = doc.Template;
            Assert.IsInstanceOfType(unstructured, typeof(JsonElement));
            Assert.IsTrue(((JsonElement)unstructured).TryGetProperty("x", out var prop));
            Assert.AreEqual(JsonValueKind.String, prop.ValueKind);
            Assert.AreEqual("x", prop.GetString());
        }
        [TestMethod]
        public void CanMutateJsonDocument()
        {
            var json = @"{""fixed"": ""1234"", ""template"": {""parameter"": ""{{PARAMETER-NAME}}""}}";
            var doc = JsonSerializer.Deserialize<UnstructuredContent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var output = JsonSerializer.Serialize(doc.Template).Replace("{{PARAMETER-NAME}}", "value");
            Assert.AreEqual(@"{""parameter"":""value""}", output);
        }
    }
}
