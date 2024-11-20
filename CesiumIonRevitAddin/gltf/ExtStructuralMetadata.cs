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

        [JsonProperty("properties")]
        public Dictionary<String, Object> Properties = new Dictionary<String, Object>();
    }
}
