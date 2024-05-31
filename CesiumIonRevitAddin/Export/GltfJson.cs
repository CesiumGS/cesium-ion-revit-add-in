using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CesiumIonRevitAddin.Export
{
    internal class GltfJson
    {
        public static string Get(
    List<GltfScene> scenes,
    List<GltfNode> nodes,
    List<GltfMesh> meshes,
    List<GltfMaterial> materials,
    List<GltfBuffer> buffers,
    List<GltfBufferView> bufferViews,
    List<GltfAccessor> accessors,
    List<string> extensionsUsed,
    Dictionary<string, GltfExtensionSchema> extensions,
    Preferences preferences,
    GltfVersion asset)
        {
            // Package the properties into a serializable container
            var model = new CesiumIonRevitAddin.Gltf.Gltf();
            model.extensionsUsed = extensionsUsed;
            model.asset = asset;
            model.scenes = scenes;
            model.nodes = nodes;
            model.meshes = meshes;

            if (materials.Count > 0)
            {
                model.materials = materials;
            }

            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            model.extensions = extensions;

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string serializedModel = JsonConvert.SerializeObject(model, settings);

            if (!preferences.Normals)
            {
                serializedModel = serializedModel.Replace(",\"NORMAL\":0", string.Empty);
            }

            return serializedModel;
        }
    }
}
