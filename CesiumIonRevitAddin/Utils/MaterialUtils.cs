using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Gltf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Utils
{
    internal class MaterialUtils
    {
        public static Material GetMeshMaterial(Document document, Mesh mesh)
        {
            ElementId materialId = mesh.MaterialElementId;
            if (materialId != null)
            {
                return document.GetElement(materialId) as Material;
            }
            else
            {
                return null;
            }
        }

        public static void SetMaterial(Document doc, Preferences preferences, Mesh mesh, IndexedDictionary<GltfMaterial> materials, bool doubleSided)
        {
            GltfMaterial gltfMaterial = new GltfMaterial();

            Material material = MaterialUtils.GetMeshMaterial(doc, mesh);

            if (preferences.Materials)
            {
                if (material == null)
                {
                    material = Collectors.GetRandomMaterial(doc);
                }

                gltfMaterial = GltfExportUtils.GetGLTFMaterial(materials.List, material, doubleSided);

                materials.AddOrUpdateCurrentMaterial(material.UniqueId, gltfMaterial, doubleSided);
            }
        }
    }
}
