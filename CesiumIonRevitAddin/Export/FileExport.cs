using System.IO;
using System.Collections.Generic;
using CesiumIonRevitAddin.Export;

namespace CesiumIonRevitAddin.Gltf
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
            // TODO: remove placeholder Preferences object
            var preferences = new Preferences();

            // TODO: needed? create extensions schema

            BufferConfig.Run(bufferViews, buffers);
            string binFileName = preferences.path + ".bin";
            BinFile.Create(binFileName, binaryFileData, preferences.Normals, false);

            string gltfJson = GltfJson.Get(scenes, nodes.List, meshes.List, materials.List,
                buffers, bufferViews, accessors, extensionsUsed, extensions, preferences, asset);

            string gltfFileName = preferences.path + ".gltf";
            File.WriteAllText(gltfFileName, gltfJson);
        }
    }
}
