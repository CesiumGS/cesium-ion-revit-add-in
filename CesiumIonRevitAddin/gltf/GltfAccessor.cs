using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfAccessor
    {
        public GltfAccessor(int bufferView, int byteOffset, ComponentType componentType, int count, string type, List<float> max, List<float> min, string name)
        {
            this.bufferView = bufferView;
            this.byteOffset = byteOffset;
            this.componentType = componentType;
            this.count = count;
            this.type = type;
            this.max = max;
            this.min = min;
            this.name = name;
        }

        // TODO: casing
        public int bufferView { get; set; }
        public int byteOffset { get; set; }
        public ComponentType componentType { get; set; }
        public int count { get; set; }
        public string type { get; set; }
        public List<float> max { get; set; }
        public List<float> min { get; set; }
        public string name { get; set; }
    }
}
