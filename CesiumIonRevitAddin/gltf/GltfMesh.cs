﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CesiumIonRevitAddin.gltf;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfMesh
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("primitives")]
        public List<GltfMeshPrimitive> Primitives { get; set; }
    }
}