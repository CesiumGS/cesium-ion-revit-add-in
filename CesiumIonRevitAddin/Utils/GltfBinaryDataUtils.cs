using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Utils
{
    internal class GltfBinaryDataUtils
    {
        const string VEC3_STR = "VEC3";
        const string POSITION_STR = "POSITION";
        const string SCALAR_STR = "SCALAR";
        const string FACE_STR = "FACE";
        const string BATCH_ID_STR = "BATCH_ID";

        public static int ExportVertices(int bufferIdx, int byteOffset, GeometryDataObject geomData,
            GltfBinaryData bufferData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors,
            out int sizeOfVec3View, out int elementsPerVertex)
        {
            for (int i = 0; i < geomData.Vertices.Count; i++)
            {
                bufferData.VertexBuffer.Add(Convert.ToSingle(geomData.Vertices[i]));
            }

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.VertexBuffer);

            // Add a vec3 buffer view
            elementsPerVertex = 3;
            int bytesPerElement = 4;
            int bytesPerVertex = elementsPerVertex * bytesPerElement;
            int numVec3 = geomData.Vertices.Count / elementsPerVertex;
            sizeOfVec3View = numVec3 * bytesPerVertex;

            var vec3View = new GltfBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(vec3View);
            int vec3ViewIdx = bufferViews.Count - 1;

            // add a position accessor
            int count = geomData.Vertices.Count / elementsPerVertex;
            var max = new List<float>(3) { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            var min = new List<float>(3) { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };

            var positionAccessor = new GltfAccessor(vec3ViewIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, POSITION_STR);
            accessors.Add(positionAccessor);
            bufferData.VertexAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3View.ByteLength;
        }

        public static int ExportFaces(int bufferIdx, int byteOffset, GeometryDataObject geometryData, GltfBinaryData binaryData,
            List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            foreach (var index in geometryData.Faces)
            {
                binaryData.IndexBuffer.Add(index);
            }

            // Get max and min for index data
            var faceMinMax = Util.GetScalarMinMax(binaryData.IndexBuffer);

            // Add a faces / indexes buffer view
            var elementsPerIndex = 1;
            var bytesPerIndexElement = 4;
            var bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            var numIndexes = geometryData.Faces.Count;
            var sizeOfIndexView = numIndexes * bytesPerIndex;
            var facesView = new GltfBufferView(bufferIdx, byteOffset, sizeOfIndexView, Targets.ELEMENT_ARRAY_BUFFER, string.Empty);
            bufferViews.Add(facesView);
            var facesViewIdx = bufferViews.Count - 1;

            // add a face accessor
            var count = geometryData.Faces.Count / elementsPerIndex;
            var max = new List<float>(1) { faceMinMax[1] };
            var min = new List<float>(1) { faceMinMax[0] };
            var faceAccessor = new GltfAccessor(facesViewIdx, 0, ComponentType.UNSIGNED_INT, count, SCALAR_STR, max, min, FACE_STR);
            accessors.Add(faceAccessor);
            binaryData.IndexAccessorIndex = accessors.Count - 1;
            return byteOffset + facesView.ByteLength;
        }
    }
}
