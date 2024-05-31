using System.Collections.Generic;
using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfScene
    {
        [JsonProperty("nodes")]
        public List<int> Nodes { get; set; } = new List<int>();
    }
}
