using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfPbr
    {
        [JsonProperty("baseColorFactor")]
        public List<float> BaseColorFactor { get; set; }

        [JsonProperty("metallicFactor")]
        public float MetallicFactor { get; set; }

        [JsonProperty("roughnessFactor")]
        public float RoughnessFactor { get; set; }
    }
}
