using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfBinaryData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vertexBuffer")]
        public List<float> VertexBuffer { get; set; } = new List<float>();

        [JsonProperty("indexBuffer")]
        public List<int> IndexBuffer { get; set; } = new List<int>();

        [JsonProperty("normalBuffer")]
        public List<float> NormalBuffer { get; set; } = new List<float>();

        [JsonProperty("vertexAccessorIndex")]
        public int VertexAccessorIndex { get; set; }

        [JsonProperty("indexAccessorIndex")]
        public int IndexAccessorIndex { get; set; }

        [JsonProperty("normalsAccessorIndex")]
        public int NormalsAccessorIndex { get; set; }
    }
}
