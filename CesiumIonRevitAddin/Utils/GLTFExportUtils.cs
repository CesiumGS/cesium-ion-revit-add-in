using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Model;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CesiumIonRevitAddin.Utils
{
    internal class GLTFExportUtils
    {
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
    }
}
