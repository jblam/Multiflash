using JBlam.Multiflash;
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

        [TestMethod]
        public void CanDeserialiseConfigTemplate()
        {
            var json = @"{""template"":{""fixed"":1234,""variable"":""{{PARAMETER-ONE}}""}, ""parameters"":[{""identifier"":""PARAMETER-ONE"", ""label"":""One""}]}";
            var doc = JsonSerializer.Deserialize<ConfigTemplate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            CollectionAssert.AreEqual(new[] { new Parameter("PARAMETER-ONE", "One") }, (System.Collections.ICollection)doc.Parameters);
            Assert.IsInstanceOfType(doc.Template, typeof(JsonElement));
        }

        [TestMethod]
        public void CanBuildConfig()
        {
            var json = @"{""template"":{""fixed"":1234,""variable"":""{{PARAMETER-ONE}}""}, ""parameters"":[{""identifier"":""PARAMETER-ONE"", ""label"":""One""}]}";
            var doc = JsonSerializer.Deserialize<ConfigTemplate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var completed = doc.Build(p => p.Label == "One" ? "one" : null);
            Assert.AreEqual(@"{""fixed"":1234,""variable"":""one""}", completed);
        }

        [TestMethod]
        public void ConfigThrowsIfParameterMissing()
        {
            var json = @"{""template"":{""fixed"":1234,""variable"":""{{PARAMETER-ONE}}""}, ""parameters"":[{""identifier"":""PARAMETER-ONE"", ""label"":""One""}]}";
            var doc = JsonSerializer.Deserialize<ConfigTemplate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.ThrowsException<ArgumentException>(() => doc.Build(_ => null));
        }
    }
}
