using CesiumIonRevitAddin.Gltf;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CesiumIonRevitAddin.Export
{
    internal static class GltfJson
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
            List<string> extensionsRequired,
            Dictionary<string, GltfExtensionSchema> extensions,
            Preferences preferences,
            GltfVersion asset,
            List<GltfImage> images,
            List<GltfTexture> textures,
            List<GltfSampler> samplers)
        {
            // Package the properties into a serializable container
            var model = new CesiumIonRevitAddin.Gltf.Gltf
            {
                extensionsUsed = extensionsUsed,
                extensionsRequired = extensionsRequired,
                asset = asset,
                scenes = scenes,
                nodes = nodes,
                meshes = meshes,
                buffers = buffers,
                bufferViews = bufferViews,
                accessors = accessors
            };

            if (images.Count > 0)
            {
                model.images = images;
                model.textures = textures;
                model.samplers = samplers;
            }

            if (materials.Count > 0)
            {
                model.materials = materials;
            }

            model.extensions = extensions;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string serializedModel = JsonConvert.SerializeObject(model, settings);

            if (!preferences.Normals)
            {
                serializedModel = serializedModel.Replace(",\"NORMAL\":0", string.Empty);
                var pattern = @"\s*,\s*""NORMAL""\s*:\s*0";
                serializedModel = Regex.Replace(serializedModel, pattern, string.Empty);
            }

            return serializedModel;
        }
    }
}
