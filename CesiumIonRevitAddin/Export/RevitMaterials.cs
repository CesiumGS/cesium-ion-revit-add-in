﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Utils;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
// using System.Windows.Media.Media3D;

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

        enum GltfBitmapType
        {
            baseColorTexture,
            metallicRoughnessTexture,
            normalTexture,
            occlusionTexture,
            emissiveTexture
        }

        struct BitmapInfo
        {
            readonly public string AbsolutePath;
            readonly public GltfBitmapType GltfBitmapType;

            public BitmapInfo(string absolutePath, GltfBitmapType gltfBitmapType)
            {
                AbsolutePath = absolutePath;
                GltfBitmapType = gltfBitmapType;
            }
        }

        public static void Export(MaterialNode materialNode,
            Document doc,
            IndexedDictionary<GltfMaterial> materials,
            GltfExtStructuralMetadataExtensionSchema extStructuralMetadata,
            IndexedDictionary<GltfSampler> samplers,
            IndexedDictionary<GltfImage> images,
            IndexedDictionary<GltfTexture> textures)
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

                var bitmapInfoCollection = GetBitmapInfo(doc, material);
                if (bitmapInfoCollection.Any())
                {
                    if (!samplers.List.Any())
                    {
                        samplers.AddOrUpdateCurrent("defaultSampler", new GltfSampler());
                    }

                    foreach (BitmapInfo bitmapInfo in bitmapInfoCollection)
                    {
                        if (bitmapInfo.GltfBitmapType == GltfBitmapType.baseColorTexture)
                        {

                            // addOrUpdate to images IndexedDir
                            // TODO: handle file-name collision
                            var fileName = Path.GetFileName(bitmapInfo.AbsolutePath);
                            bool addtexture = false;
                            if (!images.Contains(fileName))
                            {
                                var preferences = new Preferences(); // TODO: preferences
                                var copiedFilePath = Path.Combine(preferences.OutputDirectory, fileName);
                                File.Copy(bitmapInfo.AbsolutePath, copiedFilePath, true);
                                var gltfImage = new GltfImage
                                {
                                    Uri = fileName
                                };
                                images.AddOrUpdateCurrent(fileName, gltfImage);
                                addtexture = true;
                            }

                            // addorUpdateTextures
                            // TODO: assuming one-to-one mapping between glTF images and texture arrays
                            if (addtexture)
                            {
                                var imageIndex = images.GetIndexFromUuid(fileName);
                                var gltfTexture = new GltfTexture
                                {
                                    Sampler = 0,
                                    Source = imageIndex
                                };
                                textures.AddOrUpdateCurrent(imageIndex.ToString(), gltfTexture);
                            }
                        }
                        // switch {
                        //    // TODO: add non-baseColor
                        //}
                    }
                }

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

        // see https://thebuildingcoder.typepad.com/blog/2017/10/material-texture-path.html
        // and https://forums.autodesk.com/t5/revit-api-forum/parsing-assetproperty-to-unifiedbitmap/td-p/10406464
        //static string GetMaterialBitmapPath(Document doc, Material material)
        //{
        //    string result = null;

        //    // Find materials
        //    FilteredElementCollector fec
        //      = new FilteredElementCollector(doc);

        //    fec.OfClass(typeof(Material));

        //    IEnumerable<Material> targetMaterials
        //      = fec.Cast<Material>().Where<Material>(mtl =>
        //        mtl.Name == material.Name);

        //    var targetMaterial = targetMaterials.FirstOrDefault();
        //    if (targetMaterial != null)
        //    {                
        //        // Get appearance asset for read
        //        ElementId appearanceAssetId = targetMaterial.AppearanceAssetId;

        //        AppearanceAssetElement appearanceAssetElem = doc.GetElement(appearanceAssetId) as AppearanceAssetElement;
        //        Asset asset = appearanceAssetElem.GetRenderingAsset();

        //        // Walk through all first level assets to find 
        //        // connected Bitmap properties.  Note: it is 
        //        // possible to have multilevel connected 
        //        // properties with Bitmaps in the leaf nodes.  
        //        // So this would need to be recursive.
        //        int size = asset.Size;
        //        for (int assetIdx = 0; assetIdx < size; assetIdx++)
        //        {
        //            AssetProperty aProperty = asset[assetIdx];

        //            if (aProperty.NumberOfConnectedProperties < 1)
        //                continue;

        //            // Find first connected property.  
        //            // Should work for all current (2018) schemas.  
        //            // Safer code would loop through all connected
        //            // properties based on the number provided.

        //            Asset connectedAsset = aProperty
        //              .GetConnectedProperty(0) as Asset;

        //            // We are only checking for bitmap connected assets. 
        //            if (connectedAsset.Name == "UnifiedBitmapSchema")
        //            {
        //                // This line is 2018.1 & up because of the 
        //                // property reference to UnifiedBitmap
        //                // .UnifiedbitmapBitmap.  In earlier versions,
        //                // you can still reference the string name 
        //                // instead: "unifiedbitmap_Bitmap"

        //                // AssetPropertyString path = connectedAsset[UnifiedBitmap.UnifiedbitmapBitmap] as AssetPropertyString;
        //                AssetPropertyString path = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;

        //                // This will be a relative path to the built -in materials folder, additional 
        //                // render appearance folder, or an absolute path.

        //                result = System.String.Format("{0} from {2}: {1}", aProperty.Name, path.Value, connectedAsset.LibraryName);
        //                result = GetAbsoluteMaterialPath(result);
        //            }
        //        }
        //    }

        //    return result;
        //}

        static List<BitmapInfo> GetBitmapInfo(Document document, Material material)
        {
            var attachedBitmapInfo = new List<BitmapInfo>();

            // Find materials
            FilteredElementCollector filteredElementCollector
              = new FilteredElementCollector(document);
            filteredElementCollector.OfClass(typeof(Material));
            IEnumerable<Material> targetMaterials
              = filteredElementCollector.Cast<Material>().Where<Material>(mtl =>
                mtl.Name == material.Name);

            var targetMaterial = targetMaterials.FirstOrDefault();
            if (targetMaterial != null)
            {
                ElementId appearanceAssetId = targetMaterial.AppearanceAssetId;

                AppearanceAssetElement appearanceAssetElem = document.GetElement(appearanceAssetId) as AppearanceAssetElement;
                Asset asset = appearanceAssetElem.GetRenderingAsset();

                // it seems asset.Name is typically a schema
                var schema = asset.Name;

                // schema list at https://help.autodesk.com/view/RVT/2025/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Revit_Geometric_Elements_Material_General_Material_Information_html
                // apparently not exhaustive: missing "PrismOpaqueSchema"
                // nevermind. "PrismOpaqueSchema" seems to have become "AdvancedOpaque"
                switch (schema)
                {
                    case "PrismOpaqueSchema":
                    case "AdvancedOpaque":
                        attachedBitmapInfo = ParseSchemaPrismOpaqueSchema(asset);
                        break;
                    case "HardwoodSchema":
                        attachedBitmapInfo = ParseSchemaHardwoodSchema(asset);
                        break;
                    default:
                        throw new System.Exception("unknown material schema type: " + schema);
                }
            }

            return attachedBitmapInfo;
        }

        // https://help.autodesk.com/view/RVT/2022/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Revit_Geometric_Elements_Material_Material_Schema_Prism_Schema_Opaque_html
        // first try via looping
        //static List<BitmapInfo> ParseSchemaPrismOpaque(Asset renderingAsset)
        //{
        //    var bitmapInfoCollection = new List<BitmapInfo>();

        //    // TODO: Walk through all first level assets to find 
        //    // connected Bitmap properties.  Note: it is 
        //    // possible to have multilevel connected 
        //    // properties with Bitmaps in the leaf nodes.  
        //    // So this would need to be recursive.
        //    int size = renderingAsset.Size;
        //    for (int assetIdx = 0; assetIdx < size; assetIdx++)
        //    {
        //        AssetProperty aProperty = renderingAsset[assetIdx];

        //        if (aProperty.NumberOfConnectedProperties < 1)
        //            continue;

        //        // Find first connected property.  
        //        // Should work for all current (2018) schemas.  
        //        // Safer code would loop through all connected
        //        // properties based on the number provided.

        //        Asset connectedAsset = aProperty
        //          .GetConnectedProperty(0) as Asset;

        //        // We are only checking for bitmap connected assets. 
        //        // TODO: other connected assets to handle?
        //        if (connectedAsset.Name == "UnifiedBitmapSchema")
        //        {
        //            // This line is 2018.1 & up because of the 
        //            // property reference to UnifiedBitmap
        //            // .UnifiedbitmapBitmap.  In earlier versions,
        //            // you can still reference the string name 
        //            // instead: "unifiedbitmap_Bitmap"

        //            // AssetPropertyString path = connectedAsset[UnifiedBitmap.UnifiedbitmapBitmap] as AssetPropertyString;
        //            AssetPropertyString path = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;

        //            // This will be a relative path to the built-in materials folder, additional 
        //            // render appearance folder, or an absolute path.

        //            // TODO: clean up
        //            var absPath = System.String.Format("{0} from {2}: {1}", aProperty.Name, path.Value, connectedAsset.LibraryName);
        //            absPath = GetAbsoluteMaterialPath(absPath);
        //            var gltfBitmapType = GetGltfBitmapType(aProperty);
        //            if (gltfBitmapType != null)
        //            {
        //                var bitMapInfo = new BitmapInfo(absPath, gltfBitmapType.Value);
        //                bitmapInfoCollection.Add(bitMapInfo);
        //            }
        //        }
        //    }

        //    return bitmapInfoCollection;
        //}

        // https://help.autodesk.com/view/RVT/2022/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Revit_Geometric_Elements_Material_Material_Schema_Prism_Schema_Opaque_html
        static List<BitmapInfo> ParseSchemaPrismOpaqueSchema(Asset renderingAsset)
        {
            var bitmapInfoCollection = new List<BitmapInfo>();

            // AssetProperty baseColorProperty = renderingAsset.FindByName("opaque_albedo");
            AssetProperty baseColorProperty = renderingAsset.FindByName(AdvancedOpaque.OpaqueAlbedo);
            if (baseColorProperty.NumberOfConnectedProperties < 1) return bitmapInfoCollection;

            var connectedProperty = baseColorProperty.GetConnectedProperty(0) as Asset;
            AssetPropertyString path = connectedProperty.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;
            var absolutePath = GetAbsoluteMaterialPath(path.Value);
            BitmapInfo baseColor = new BitmapInfo(absolutePath, GltfBitmapType.baseColorTexture);
            bitmapInfoCollection.Add(baseColor);

            // TODO: add normal maps, roughness, etc.

            return bitmapInfoCollection;
        }

        // https://help.autodesk.com/view/RVT/2025/ENU/?guid=Revit_API_Revit_API_Developers_Guide_Revit_Geometric_Elements_Material_Material_Schema_Protein_Hardwood_Schema_html
        static List<BitmapInfo> ParseSchemaHardwoodSchema(Asset renderingAsset)
        {
            var bitmapInfoCollection = new List<BitmapInfo>();

            // AssetProperty baseColorProperty = renderingAsset.FindByName("opaque_albedo");
            AssetProperty baseColorProperty = renderingAsset.FindByName(Hardwood.HardwoodColor);
            if (baseColorProperty.NumberOfConnectedProperties < 1) return bitmapInfoCollection;

            var connectedProperty = baseColorProperty.GetConnectedProperty(0) as Asset;
            AssetPropertyString path = connectedProperty.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;
            var absolutePath = GetAbsoluteMaterialPath(path.Value);
            BitmapInfo baseColor = new BitmapInfo(absolutePath, GltfBitmapType.baseColorTexture);
            bitmapInfoCollection.Add(baseColor);

            // TODO: add normal maps, roughness, etc.

            return bitmapInfoCollection;
        }

        static GltfBitmapType? GetGltfBitmapType(AssetProperty assetProperty)
        {
            if (assetProperty.Name == "surface_albedo")
            {
                return GltfBitmapType.baseColorTexture;
            }

            if (assetProperty.Name == "surface_roughness")
            {
                return GltfBitmapType.metallicRoughnessTexture;
            }

            return null;

            // TODO: handle opacity
            //if (assetProperty.Name == "opaque_albedo")
            //{
            //    return GltfBitmapType.???;
            //}

            // TODO: track down other strings for other glTF types:
            // baseColorTexture
            // metallicRoughnessTexture
            // normalTexture
            // occlusionTexture
            // emissiveTexture

        }

        static string GetAbsoluteMaterialPath(string relativeOrAbsolutePath)
        {

            string[] allPaths = relativeOrAbsolutePath.Split('|');
            relativeOrAbsolutePath = allPaths[allPaths.Length - 1];

            if (Path.IsPathRooted(relativeOrAbsolutePath) && File.Exists(relativeOrAbsolutePath))
            {
                return relativeOrAbsolutePath;
            }

            string[] possibleBasePaths = {
                @"C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures",
                @"C:\Program Files\Common Files\Autodesk Shared\Materials\Textures",
                GetAdditionalRenderAppearancePath()  // Paths from Revit.ini
            };

            foreach (var basePath in possibleBasePaths)
            {
                if (!string.IsNullOrEmpty(basePath))
                {
                    string fullPath = Path.Combine(basePath, relativeOrAbsolutePath);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }

        static string GetAdditionalRenderAppearancePath()
        {
            // Path to Revit.ini file - adjust based on actual location and Revit version
            string revitIniPath = @"C:\Users\[Username]\AppData\Roaming\Autodesk\Revit\Autodesk Revit 2021\Revit.ini";
            if (System.IO.File.Exists(revitIniPath))
            {
                // Read the .ini file and extract the path
                var lines = System.IO.File.ReadAllLines(revitIniPath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("AdditionalRenderAppearancePath="))
                    {
                        return line.Split('=')[1].Trim();
                    }
                }
            }
            return null;
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
            Dictionary<string, object> classPropertiesSchema;
            if (classes.ContainsKey(materialGltfName))
            {
                classSchema = extStructuralMetadataSchema.GetClass(materialGltfName);
                classPropertiesSchema = (Dictionary<string, object>)classSchema["properties"];
            }
            else
            {
                classSchema = extStructuralMetadataSchema.AddClass(material.Name);
                classPropertiesSchema = new Dictionary<string, object>();
                classSchema["properties"] = classPropertiesSchema;
            }

            for (int i = 0; i < renderingAsset.Size; i++)
            {
                AssetProperty property = renderingAsset.Get(i);
                if (property is AssetPropertyString propertyString)
                {
                    string gltfPropertyName = Util.GetGltfName(propertyString.Name);

                    // TODO: DEBUG
                    if (!gltfMaterial.Extensions.EXT_structural_metadata.Properties.ContainsKey(gltfPropertyName)) {
                        gltfMaterial.Extensions.EXT_structural_metadata.Properties.Add(gltfPropertyName, propertyString.Value);
                    } else
                    {
                        // TODO: why does this fire?
                        System.Diagnostics.Debug.WriteLine("Error: should not happen");
                    }

                    // add to schema
                    if (!classPropertiesSchema.ContainsKey(gltfPropertyName))
                    {
                        classPropertiesSchema.Add(gltfPropertyName, new Dictionary<string, object>());
                        var schemaProperty = (Dictionary<string, object>) classPropertiesSchema[gltfPropertyName];

                        if (!schemaProperty.ContainsKey("name"))
                        {
                            schemaProperty.Add("name", propertyString.Name);
                        }

                        // TODO: more deeply investigate this way of handling the type
                        AssetPropertyType assetPropertyType = property.Type;
                        switch (assetPropertyType)
                        {
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
