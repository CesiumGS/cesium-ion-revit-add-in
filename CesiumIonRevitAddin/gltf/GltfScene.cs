using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfScene
    {
        [JsonProperty("nodes")]
        public List<int> Nodes { get; set; } = new List<int>();
    }
}
