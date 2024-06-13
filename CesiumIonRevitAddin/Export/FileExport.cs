using System.IO;
using System.Collections.Generic;
using CesiumIonRevitAddin.Export;
using Autodesk.Revit.DB.DirectContext3D;

namespace CesiumIonRevitAddin.Gltf
{
    internal class FileExport
    {
        public static void Run(
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
            // TODO: remove placeholder Preferences object
            var preferences = new Preferences();

            // TODO: needed? create extensions schema

            // DEBUG
            int bufferByteLength = buffers[0].ByteLength;

            BufferConfig.Run(bufferViews, buffers);
            string binFileName = preferences.path + ".bin";
            BinFile.Create(binFileName, binaryData, preferences.Normals, false);

            string gltfJson = GltfJson.Get(scenes, nodes.List, meshes.List, materials.List,
                buffers, bufferViews, accessors, extensionsUsed, extensions, preferences, asset, images.List, textures.List, samplers.List);

            string gltfFileName = preferences.path + ".gltf";
            File.WriteAllText(gltfFileName, gltfJson);
        }
    }
}
