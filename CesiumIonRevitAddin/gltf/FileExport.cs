using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.gltf
{
    internal class FileExport
    {
        public static void Run(
            List<GltfBufferView> bufferViews,
            List<GltfBuffer> buffers,
            List<GltfBinaryData> binaryFileData,
            List<GltfScene> scenes,
            IndexedDictionary<GltfNode> nodes,
            IndexedDictionary<GltfMesh> meshes,
            IndexedDictionary<GltfMaterial> materials,
            List<GltfAccessor> accessors,
            List<string> extensionsUsed,
            Dictionary<string, GltfExtensionSchema> extensions,
            GltfVersion asset)
        {
            throw new NotImplementedException();
        }
    }
}
