﻿using Newtonsoft.Json;
using System.Collections.Generic;

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

        [JsonProperty("texcoordsBuffer")]
        public List<float> TexCoordBuffer { get; set; } = new List<float>();

        [JsonProperty("vertexAccessorIndex")]
        public int VertexAccessorIndex { get; set; }

        [JsonProperty("indexAccessorIndex")]
        public int IndexAccessorIndex { get; set; }

        [JsonProperty("normalsAccessorIndex")]
        public int NormalsAccessorIndex { get; set; }

        [JsonProperty("texcoordsAccessorIndex")]
        public int TexCoordAccessorIndex { get; set; }
    }
}
