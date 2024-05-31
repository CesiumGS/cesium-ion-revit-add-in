using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Gltf
{

    /// <summary>
    /// Magic numbers to differentiate scalar and vector array buffers
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views.
    /// </summary>
    public enum Targets
    {
        ARRAY_BUFFER = 34962, // signals vertex data
        ELEMENT_ARRAY_BUFFER = 34963, // signals index or face data
    }

    /// <summary>
    /// Magic numbers to differentiate array buffer component types
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#accessor-element-size.
    /// </summary>
    public enum ComponentType
    {
        BYTE = 5120,
        UNSIGNED_BYTE = 5121,
        SHORT = 5122,
        UNSIGNED_SHORT = 5123,
        UNSIGNED_INT = 5125,
        FLOAT = 5126,
    }

    internal struct Gltf
    {
        public List<string> extensionsUsed;
		public GltfVersion asset;
		public List<GltfScene> scenes;
		public List<GltfNode> nodes;
		public List<GltfMesh> meshes;
		public List<GltfBuffer> buffers;
		public List<GltfBufferView> bufferViews;
		public List<GltfAccessor> accessors;
		public List<GltfMaterial> materials;
		public Dictionary<string, GltfExtensionSchema> extensions;

        public Gltf(Dictionary<string, GltfExtensionSchema> extensions) : this()
        {
            this.extensions = extensions;
        }
    }
}
