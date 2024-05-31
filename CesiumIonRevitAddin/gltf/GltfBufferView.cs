using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfBufferView
    {
        [JsonProperty("buffer")]
        public int Buffer { get; set; }

        [JsonProperty("byteOffset")]
        public int ByteOffset { get; set; }

        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }

        /// <summary>
        [JsonProperty("target")]
        public Targets Target { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public GltfBufferView(int buffer, int byteOffset, int byteLength, Targets target, string name)
        {
            Buffer = buffer;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
            Target = target;
            Name = name;
        }
    }
}
