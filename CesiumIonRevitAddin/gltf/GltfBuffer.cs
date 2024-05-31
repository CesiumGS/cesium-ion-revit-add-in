using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfBuffer
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }
    }
}
