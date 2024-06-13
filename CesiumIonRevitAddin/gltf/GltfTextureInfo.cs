﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfTextureInfo
    {
        [JsonProperty("index")]
        public int Index { get; set; }  // Index to the texture in the glTF's "textures" array

        [JsonProperty("texCoord")]
        public int TexCoord { get; set; } = 0;  // Default to using the first set of UV coordinates, typically base color
    }
}