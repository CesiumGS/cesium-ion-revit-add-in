using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Model;
using System.Collections.Generic;
using System.Linq;

namespace CesiumIonRevitAddin.Utils
{
    internal static class GltfExportUtils
    {
        private const int DEF_COLOR = 250;
        private const string DEF_MATERIAL_NAME = "default";
        private const string BIN = ".bin";

        public static GltfMaterial GetGLTFMaterial(List<GltfMaterial> gltfMaterials, Material material, bool doubleSided)
        {
            // search for an already existing material
            GltfMaterial m = gltfMaterials.FirstOrDefault(x =>
            x.PbrMetallicRoughness != null &&
            x.PbrMetallicRoughness.BaseColorFactor[0] == material.Color.Red &&
            x.PbrMetallicRoughness.BaseColorFactor[1] == material.Color.Green &&
            x.PbrMetallicRoughness.BaseColorFactor[2] == material.Color.Blue && x.DoubleSided == doubleSided);

            return m ?? GltfExportUtils.CreateGltfMaterial(DEF_MATERIAL_NAME, 0, new Color(DEF_COLOR, DEF_COLOR, DEF_COLOR), doubleSided);
        }
        public static GltfBinaryData AddGeometryMeta(
            List<GltfBuffer> buffers,
            List<GltfAccessor> accessors,
            List<GltfBufferView> bufferViews,
            GeometryDataObject geometryDataObject,
            string name,
            int elementId,
            bool exportNormals)
        {
            ulong byteOffset = 0;

            var buffer = new GltfBuffer
            {
                Uri = name + BIN
            };
            buffers.Add(buffer);
            int bufferIndex = buffers.Count - 1;
            GltfBinaryData bufferData = new GltfBinaryData
            {
                Name = buffer.Uri
            };

            byteOffset = GltfBinaryDataUtils.ExportVertices(bufferIndex, byteOffset, geometryDataObject, bufferData, bufferViews, accessors);

            if (exportNormals)
            {
                byteOffset = GltfBinaryDataUtils.ExportNormals(bufferIndex, byteOffset, geometryDataObject, bufferData, bufferViews, accessors);
            }

            byteOffset = GltfBinaryDataUtils.ExportTexCoords(bufferIndex, byteOffset, geometryDataObject, bufferData, bufferViews, accessors);

            GltfBinaryDataUtils.ExportFaces(bufferIndex, byteOffset, geometryDataObject, bufferData, bufferViews, accessors);

            return bufferData;
        }

        public static void AddNormals(Autodesk.Revit.DB.Transform transform, PolymeshTopology polymeshTopology, List<double> normals)
        {
            IList<XYZ> polymeshNormals = polymeshTopology.GetNormals();

            switch (polymeshTopology.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                    {
                        foreach (PolymeshFacet facet in polymeshTopology.GetFacets())
                        {
                            var normalPoints = new List<XYZ>
                            {
                                transform.OfVector(polymeshNormals[facet.V1]),
                                transform.OfVector(polymeshNormals[facet.V2]),
                                transform.OfVector(polymeshNormals[facet.V3])
                            };

                            foreach (var normalPoint in normalPoints)
                            {
                                var newNormalPoint = normalPoint;
                                newNormalPoint = newNormalPoint.Normalize();

                                normals.Add(newNormalPoint.X);
                                normals.Add(newNormalPoint.Y);
                                normals.Add(newNormalPoint.Z);
                            }
                        }

                        break;
                    }
                case DistributionOfNormals.OnePerFace:
                    {
                        foreach (var facet in polymeshTopology.GetFacets())
                        {
                            foreach (var normal in polymeshTopology.GetNormals())
                            {
                                var newNormal = normal;
                                newNormal = newNormal.Normalize();

                                for (int j = 0; j < 3; j++)
                                {
                                    normals.Add(newNormal.X);
                                    normals.Add(newNormal.Y);
                                    normals.Add(newNormal.Z);
                                }
                            }
                        }

                        break;
                    }

                case DistributionOfNormals.OnEachFacet:
                    {
                        foreach (var normal in polymeshNormals)
                        {
                            var newNormal = transform.OfVector(normal);
                            newNormal = newNormal.Normalize();

                            // Add a normal for each vertex of the facet
                            for (int j = 0; j < 3; j++)
                            {
                                normals.Add(newNormal.X);
                                normals.Add(newNormal.Y);
                                normals.Add(newNormal.Z);
                            }
                        }

                        break;
                    }
            }
        }

        public static void AddTexCoords(PolymeshTopology polymeshTopology, List<double> uvs)
        {
            IList<UV> polyMeshUvs = polymeshTopology.GetUVs();

            foreach (PolymeshFacet facet in polymeshTopology.GetFacets())
            {
                var facetVertIndex = facet.V1;
                uvs.Add(polyMeshUvs[facetVertIndex].U);
                uvs.Add(polyMeshUvs[facetVertIndex].V);

                facetVertIndex = facet.V2;
                uvs.Add(polyMeshUvs[facetVertIndex].U);
                uvs.Add(polyMeshUvs[facetVertIndex].V);

                facetVertIndex = facet.V3;
                uvs.Add(polyMeshUvs[facetVertIndex].U);
                uvs.Add(polyMeshUvs[facetVertIndex].V);
            }
        }

        public static void AddOrUpdateCurrentItem(IndexedDictionary<GltfNode> nodes, IndexedDictionary<GeometryDataObject> geometryDataObjects,
            IndexedDictionary<VertexLookupIntObject> vertexLookupIntObjects, IndexedDictionary<GltfMaterial> materials)
        {
            // Add new "_current" entries if vertex_key is unique
            string vertexKey = nodes.CurrentKey + "_" + materials.CurrentKey;
            Logger.Instance.Log($"vertex_key: {vertexKey}");
            geometryDataObjects.AddOrUpdateCurrent(vertexKey, new GeometryDataObject());
            vertexLookupIntObjects.AddOrUpdateCurrent(vertexKey, new VertexLookupIntObject());
        }

        public static void AddRPCNormals(Preferences preferences, MeshTriangle triangle, GeometryDataObject geomDataObj)
        {
            XYZ normal = GeometryUtils.GetNormal(triangle);

            for (int j = 0; j < 3; j++)
            {
                geomDataObj.Normals.Add(normal.X);
                geomDataObj.Normals.Add(normal.Y);
                geomDataObj.Normals.Add(normal.Z);
            }
        }

        public static void AddVerticesAndFaces(VertexLookupIntObject vertex, GeometryDataObject geometryDataObject, List<XYZ> pts)
        {
            var idx = vertex.AddVertex(new PointIntObject(pts[0]));
            geometryDataObject.Faces.Add(idx);

            var idx1 = vertex.AddVertex(new PointIntObject(pts[1]));
            geometryDataObject.Faces.Add(idx1);

            var idx2 = vertex.AddVertex(new PointIntObject(pts[2]));
            geometryDataObject.Faces.Add(idx2);
        }

        public static GltfMaterial CreateGltfMaterial(string materialName, int materialOpacity, Color color, bool doubleSided)
        {
            var gltfMaterial = new GltfMaterial();
            gltfMaterial.DoubleSided = doubleSided;
            float opacity = 1 - (float)materialOpacity;
            gltfMaterial.Name = materialName;
            var pbr = new GltfPbr
            {
                BaseColorFactor = new List<float>(4) { color.Red / 255f, color.Green / 255f, color.Blue / 255f, opacity },
                MetallicFactor = 0f,
                RoughnessFactor = 1f
            };
            gltfMaterial.PbrMetallicRoughness = pbr;

            return gltfMaterial;
        }
    }
}
