using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using CesiumIonRevitAddin.Gltf;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Export
{

    public class MaterialCacheDTO
    {
        public MaterialCacheDTO(string materialName, string uniqueId)
        {
            MaterialName = materialName;
            UniqueId = uniqueId;
        }

        public string MaterialName { get; set; }
        public string UniqueId { get; set; }
    }

    internal class RevitMaterials
    {
        const string BLEND = "BLEND";
        const string OPAQUE = "OPAQUE";
        const int ONEINTVALUE = 1;

        /// <summary>
        /// Container for material names (Local cache to avoid Revit API I/O)
        /// </summary>
        static Dictionary<ElementId, MaterialCacheDTO> MaterialNameContainer = new Dictionary<ElementId, MaterialCacheDTO>();

        public static void Export(MaterialNode materialNode, Document doc, IndexedDictionary<GltfMaterial> materials)
        {
            ElementId id = materialNode.MaterialId;
            var gltfMaterial = new GltfMaterial();
            float opacity = ONEINTVALUE - (float) materialNode.Transparency;

            // Validate if the material is valid because for some reason there are
            // materials with invalid Ids
            if (id != ElementId.InvalidElementId)
            {
                var material = doc.GetElement(materialNode.MaterialId) as Material;
                if (material != null)
                {
                    ParameterSetIterator paramIterator = material.Parameters.ForwardIterator();
                    while (paramIterator.MoveNext())
                    {
                        var parameter = (Parameter) paramIterator.Current;
                        string paramName = parameter.Definition.Name;
                        string paramValue = GetParameterValueAsString(parameter);

                        if (!gltfMaterial.Extensions.EXT_structural_metadata.Properties.ContainsKey(paramName))
                        {
                            gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add(paramName, paramValue);
                        }
                    }
                }

                // TODO: get textures
                // note on getting textures: https://thebuildingcoder.typepad.com/blog/2017/10/material-texture-path.html
                // ExtractAppearancePropertiesFromMaterial(material, doc, gltfMaterial);

                ExtractPropertyStringsFromMaterial(material, doc, gltfMaterial);

                string uniqueId;
                if (!MaterialNameContainer.TryGetValue(materialNode.MaterialId, out MaterialCacheDTO materialElement))
                {
                    // construct a material from the node
                    var m = doc.GetElement(materialNode.MaterialId);
                    gltfMaterial.Name = m.Name;
                    uniqueId = m.UniqueId;
                    MaterialNameContainer.Add(materialNode.MaterialId, new MaterialCacheDTO(m.Name, m.UniqueId));
                }
                else
                {
                    MaterialCacheDTO elementData = MaterialNameContainer[materialNode.MaterialId];
                    gltfMaterial.Name = elementData.MaterialName;
                    uniqueId = elementData.UniqueId;
                }

                var pbr = new GltfPbr();
                SetMaterialsProperties(materialNode, opacity, ref pbr, ref gltfMaterial);

                materials.AddOrUpdateCurrentMaterial(uniqueId, gltfMaterial, false);
            }
        }

        static string GetParameterValueAsString(Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsDouble().ToString("F2");
                case StorageType.Integer:
                    return parameter.AsInteger().ToString();
                case StorageType.String:
                    return parameter.AsString();
                case StorageType.ElementId:
                    return parameter.AsElementId().IntegerValue.ToString();
                default:
                    return "Unsupported type";
            }
        }

        static void ExtractPropertyStringsFromMaterial(Autodesk.Revit.DB.Material material, Document doc, GltfMaterial gltfMaterial)
        {
            if (!(doc.GetElement(material.AppearanceAssetId) is AppearanceAssetElement assetElement))
            {
                gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add("AppearanceAsset", "None");
                return;
            }

            Asset renderingAsset = assetElement.GetRenderingAsset();
            if (renderingAsset == null)
            {
                gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add("RenderingAsset", "None");
                return;
            }

            for (int i = 0; i < renderingAsset.Size; i++)
            {
                AssetProperty property = renderingAsset.Get(i);
                if (property is AssetPropertyString propertyString)
                {
                    gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add(propertyString.Name, propertyString.Value);
                }
            }
        }

        static void SetMaterialsProperties(MaterialNode node, float opacity, ref GltfPbr pbr, ref GltfMaterial gltfMaterial)
        {
            pbr.BaseColorFactor = new List<float>(4) { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
            pbr.MetallicFactor = 0f;
            pbr.RoughnessFactor = opacity != 1 ? 0.5f : 1f;
            gltfMaterial.PbrMetallicRoughness = pbr;

            // TODO: Implement MASK alphamode for elements like leaves or wire fences
            gltfMaterial.AlphaMode = opacity != 1 ? BLEND : OPAQUE;
            gltfMaterial.AlphaCutoff = null;
        }
    }
}
