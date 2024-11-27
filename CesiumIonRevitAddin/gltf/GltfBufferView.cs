using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfBufferView
    {
        [JsonProperty("buffer")]
        public int Buffer { get; set; }

        [JsonProperty("byteOffset")]
        public ulong ByteOffset { get; set; }

        [JsonProperty("byteLength")]
        public ulong ByteLength { get; set; }

        /// <summary>
        [JsonProperty("target")]
        public Targets Target { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public GltfBufferView(int buffer, ulong byteOffset, ulong byteLength, Targets target, string name)
        {
            Buffer = buffer;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
            Target = target;
            Name = name;
        }
    }
}
