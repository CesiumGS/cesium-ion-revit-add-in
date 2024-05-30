using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CesiumIonRevitAddin.gltf
{
    internal class ExtStructuralMetadata
    {
        [JsonProperty("class")]
        public String Class;

        [JsonProperty("name")]
        public String Name;

        [JsonProperty("properties")]
        public Dictionary<String, Object> Properties = new Dictionary<String, Object>();
    }
}
