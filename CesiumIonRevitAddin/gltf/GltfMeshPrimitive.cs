using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfMeshPrimitive
    {
        [JsonProperty("attributes")]
        public GltfAttribute Attributes { get; set; } = new GltfAttribute();

        [JsonProperty("indices")]
        public int Indices { get; set; }

        [JsonProperty("material")]
        public int? Material { get; set; } = null;
    }
}
