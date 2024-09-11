using Newtonsoft.Json;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfAccessor
    {
        public GltfAccessor(int bufferView, int byteOffset, ComponentType componentType, int count, string type, List<float> max, List<float> min, string name)
        {
            BufferView = bufferView;
            ByteOffset = byteOffset;
            ComponentType = componentType;
            Count = count;
            Type = type;
            Max = max;
            Min = min;
            Name = name;
        }

        [JsonProperty("bufferView")]
        public int BufferView { get; set; }
        [JsonProperty("byteOffset")]
        public int ByteOffset { get; set; }
        [JsonProperty("componentType")]
        public ComponentType ComponentType { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("max")]
        public List<float> Max { get; set; }
        [JsonProperty("min")]
        public List<float> Min { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
