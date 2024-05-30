using System.Collections.Generic;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfNode
    {
        public string name;
        public int? mesh;
        public List<double> rotation;
        public List<double> scale;
        public List<float> translation;
        public List<int> children;
        public GltfExtras extras;
        public GltfExtensions extensions = new GltfExtensions();
    }
}
