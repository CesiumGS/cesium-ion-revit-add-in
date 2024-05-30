using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfBinaryData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vertexBuffer")]
        public List<float> VertexBuffer { get; set; } = new List<float>();

        [JsonProperty("vertexAccessorIndex")]
        public int VertexAccessorIndex { get; set; }

        [JsonProperty("indexBuffer")]
        public List<int> IndexBuffer { get; set; } = new List<int>();

        [JsonProperty("indexAccessorIndex")]
        public int IndexAccessorIndex { get; set; }

        [JsonProperty("normalsAccessorIndex")]
        public int NormalsAccessorIndex { get; set; }
    }
}
