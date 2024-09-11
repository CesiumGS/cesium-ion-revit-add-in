using CesiumIonRevitAddin.Export;
using System.Collections.Generic;
using System.IO;

namespace CesiumIonRevitAddin.Gltf
{
    internal class FileExport
    {
        public static void Run(
            Preferences preferences,
            List<GltfBufferView> bufferViews,
            List<GltfBuffer> buffers,
            List<GltfBinaryData> binaryData,
            List<GltfScene> scenes,
            IndexedDictionary<GltfNode> nodes,
            IndexedDictionary<GltfMesh> meshes,
            IndexedDictionary<GltfMaterial> materials,
            List<GltfAccessor> accessors,
            List<string> extensionsUsed,
            Dictionary<string, GltfExtensionSchema> extensions,
            GltfVersion asset,
            IndexedDictionary<GltfImage> images,
            IndexedDictionary<GltfTexture> textures,
            IndexedDictionary<GltfSampler> samplers)
        {
            BufferConfig.Run(bufferViews, buffers, Path.GetFileName(preferences.BinPath));
            BinFile.Create(preferences.BinPath, binaryData, preferences.Normals, false);

            string gltfJson = GltfJson.Get(scenes, nodes.List, meshes.List, materials.List,
                buffers, bufferViews, accessors, extensionsUsed, extensions, preferences, asset, images.List, textures.List, samplers.List);

            File.WriteAllText(preferences.GltfPath, gltfJson);
        }
    }
}
