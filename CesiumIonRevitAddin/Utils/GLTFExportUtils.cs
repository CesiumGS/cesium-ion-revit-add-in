using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Model;
using System.Collections.Generic;
using System.Linq;

namespace CesiumIonRevitAddin.Utils
{
    internal class GltfExportUtils
    {
        const int DEF_COLOR = 250;
        const string DEF_MATERIAL_NAME = "default";
        public static GltfMaterial GetGLTFMaterial(List<GltfMaterial> gltfMaterials, Material material, bool doubleSided)
        {
            // search for an already existing material
            var m = gltfMaterials.FirstOrDefault(x =>
            x.PbrMetallicRoughness.BaseColorFactor[0] == material.Color.Red &&
            x.PbrMetallicRoughness.BaseColorFactor[1] == material.Color.Green &&
            x.PbrMetallicRoughness.BaseColorFactor[2] == material.Color.Blue && x.DoubleSided == doubleSided);

            return m != null ? m : GltfExportUtils.CreateGltfMaterial(DEF_MATERIAL_NAME, 0, new Color(DEF_COLOR, DEF_COLOR, DEF_COLOR), doubleSided);
        }
        public static GltfBinaryData AddGeometryMeta(
            List<GltfBuffer> buffers,
            List<GltfAccessor> accessors,
            List<GltfBufferView> bufferViews,
            GeometryDataObject geomData,
            string name,
            int elementId,
            bool exportNormals)
        {
            int byteOffset = 0;

            // add a buffer
            GltfBuffer buffer = new GltfBuffer
            {
                Uri = name + BIN // Ignore IntelliSense
            };
            buffers.Add(buffer);
            int bufferIdx = buffers.Count - 1;
            GltfBinaryData bufferData = new GltfBinaryData
            {
                Name = buffer.Uri
            };

            byteOffset = GltfBinaryDataUtils.ExportVertices(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors, out int sizeOfVec3View, out int elementsPerVertex);

            if (exportNormals)
            {
                // TODO
                // byteOffset = GltfBinaryDataUtils.ExportNormals(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);
            }

            byteOffset = GltfBinaryDataUtils.ExportFaces(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);

            return bufferData;
        }

        static readonly string BIN = ".bin";

        public static void AddNormals(Preferences preferences, Autodesk.Revit.DB.Transform transform, PolymeshTopology polymesh, List<double> normals)
        {
            IList<XYZ> polymeshNormals = polymesh.GetNormals();

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                    {
                        foreach (PolymeshFacet facet in polymesh.GetFacets())
                        {
                            var normalPoints = new List<XYZ>
                            {
                                transform.OfVector(polymeshNormals[facet.V1]),
                                transform.OfVector(polymeshNormals[facet.V2]),
                                transform.OfVector(polymeshNormals[facet.V3])
                            };

                            foreach (var normalPoint in normalPoints)
                            {
                                XYZ newNormalPoint = normalPoint;

                                normals.Add(newNormalPoint.X);
                                normals.Add(newNormalPoint.Y);
                                normals.Add(newNormalPoint.Z);
                            }
                        }

                        break;
                    }
                case DistributionOfNormals.OnePerFace:
                    {
                        foreach (var facet in polymesh.GetFacets())
                        {
                            foreach (var normal in polymesh.GetNormals())
                            {
                                var newNormal = normal;

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
                            normals.Add(newNormal.X);
                            normals.Add(newNormal.Y);
                            normals.Add(newNormal.Z);
                        }

                        break;
                    }
            }
        }

        public static void AddOrUpdateCurrentItem(IndexedDictionary<GltfNode> nodes, IndexedDictionary<GeometryDataObject> geomDataObj,
            IndexedDictionary<VertexLookupIntObject> vertexIntObj, IndexedDictionary<GltfMaterial> materials)
        {
            // Add new "_current" entries if vertex_key is unique
            string vertex_key = nodes.CurrentKey + "_" + materials.CurrentKey;
            geomDataObj.AddOrUpdateCurrent(vertex_key, new GeometryDataObject());
            vertexIntObj.AddOrUpdateCurrent(vertex_key, new VertexLookupIntObject());
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
