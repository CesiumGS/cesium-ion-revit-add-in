﻿using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Gltf
{
    internal class KhrTextureTransform
    {
        [JsonProperty("offset")]
        public double[] Offset { get; set; } = new double[] { 0, 0 };
        [JsonProperty("rotation")]
        public double? Rotation { get; set; }
        [JsonProperty("scale")]
        public double[] Scale { get; set; } = new double[] { 1.0, 1.0 };
    }
}
