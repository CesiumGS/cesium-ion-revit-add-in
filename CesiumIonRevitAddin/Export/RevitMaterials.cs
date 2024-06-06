using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Utils;
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

        public static void Export(MaterialNode materialNode, 
            Document doc, 
            IndexedDictionary<GltfMaterial> materials,
            GltfExtStructuralMetadataExtensionSchema extStructuralMetadata)
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
                    var materialGltfName = Utils.Util.GetGltfName(material.Name);
                    gltfMaterial.Extensions.EXT_structural_metadata.Class = materialGltfName;

                    ParameterSetIterator paramIterator = material.Parameters.ForwardIterator();
                    while (paramIterator.MoveNext())
                    {
                        var parameter = (Parameter) paramIterator.Current;
                        var paramName = parameter.Definition.Name;
                        var paramValue = GetParameterValueAsString(parameter);
                        var paramGltfName = Utils.Util.GetGltfName(paramName);

                        if (!gltfMaterial.Extensions.EXT_structural_metadata.Properties.ContainsKey(paramGltfName))
                        {
                            gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add(paramGltfName, paramValue);
                        }
                    }
                }

                // TODO: get textures
                // note on getting textures: https://thebuildingcoder.typepad.com/blog/2017/10/material-texture-path.html
                // ExtractAppearancePropertiesFromMaterial(material, doc, gltfMaterial);

                ExtractPropertyStringsFromMaterial(material, doc, gltfMaterial, extStructuralMetadata);

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

        static void ExtractPropertyStringsFromMaterial(Autodesk.Revit.DB.Material material, Document doc, GltfMaterial gltfMaterial,
            GltfExtStructuralMetadataExtensionSchema extStructuralMetadataSchema)
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

            var materialGltfName = Utils.Util.GetGltfName(material.Name);
            var classes = extStructuralMetadataSchema.GetClasses();
            Dictionary<string, object> classSchema;
            Dictionary<string, object> classSchemaProperties;
            if (classes.ContainsKey(materialGltfName))
            {
                classSchema = extStructuralMetadataSchema.GetClass(materialGltfName);
                classSchemaProperties = (Dictionary<string, object>)classSchema["properties"];
            }
            else
            {
                classSchema = extStructuralMetadataSchema.AddClass(materialGltfName);
                classSchemaProperties = new Dictionary<string, object>();
                classSchema["properties"] = classSchemaProperties;
                classSchema["class"] = materialGltfName;
            }

            for (int i = 0; i < renderingAsset.Size; i++)
            {
                AssetProperty property = renderingAsset.Get(i);
                if (property is AssetPropertyString propertyString)
                {
                    string gltfPropertyName = Util.GetGltfName(propertyString.Name);

                    // DEBUG
                    if (!gltfMaterial.Extensions.EXT_structural_metadata.Properties.ContainsKey(gltfPropertyName)) {
                        gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add(gltfPropertyName, propertyString.Value);
                    } else
                    {
                        System.Diagnostics.Debug.WriteLine("Error: should not happen");
                    }

                    // add to schema
                    if (extStructuralMetadataSchema.ClassHasProperty(classSchema, gltfPropertyName)) continue;

                    if (!classSchemaProperties.ContainsKey(gltfPropertyName))
                    {
                        classSchemaProperties.Add(gltfPropertyName, new Dictionary<string, object>());
                        var schemaProperty = (Dictionary<string, object>) classSchemaProperties[gltfPropertyName];

                        // name
                        if (!schemaProperty.ContainsKey("name"))
                        {
                            schemaProperty.Add("name", propertyString.Name);
                        } else
                        {
                            System.Diagnostics.Debug.WriteLine("Error: should not happen");
                        }

                        // type
                        AssetPropertyType assetPropertyType = property.Type;
                        switch (assetPropertyType)
                        {
                            //case AssetPropertyType.None:
                            //    {
                            //        // TODO
                            //        schemaProperty.Add("type", "None");
                            //        break;
                            //    }
                            case AssetPropertyType.String:
                                schemaProperty.Add("type", "STRING");
                                break;
                            case AssetPropertyType.Integer:
                                schemaProperty.Add("type", "SCALAR");
                                schemaProperty.Add("componentType", "INT32");
                                break;
                            case AssetPropertyType.Float:
                            case AssetPropertyType.Double1:
                                schemaProperty.Add("type", "SCALAR");
                                schemaProperty.Add("componentType", "FLOAT32");
                                break;
                            default:
                                schemaProperty.Add("type", "TODO: OTHER");
                                break;
                        }

                        schemaProperty.Add("required", GltfExtStructuralMetadataExtensionSchema.IsRequired(propertyString.Name));
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("Finished adding properties");
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
