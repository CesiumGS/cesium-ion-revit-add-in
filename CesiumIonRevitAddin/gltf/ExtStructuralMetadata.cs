using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    // appended to nodes
    internal class ExtStructuralMetadata
    {
        [JsonProperty("class")]
        public String Class;

        Dictionary<String, Object> properties;

        public void AddProperty(String key, Object value)
        {
            properties = properties ?? new Dictionary<String, Object>();
            properties.Add(key, value);
        }

        public bool HasProperty(String key) {
            if (properties == null) return false;

            return properties.ContainsKey(key);
        }
    }
}
