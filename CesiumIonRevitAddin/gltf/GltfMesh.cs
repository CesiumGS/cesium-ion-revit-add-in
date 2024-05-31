using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfMesh
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("primitives")]
        public List<GltfMeshPrimitive> Primitives { get; set; }
    }
}
