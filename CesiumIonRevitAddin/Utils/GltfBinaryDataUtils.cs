using CesiumIonRevitAddin.Gltf;
using CesiumIonRevitAddin.Model;
using System;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Utils
{
    internal static class GltfBinaryDataUtils
    {
        private const string VEC2_STR = "VEC2";
        private const string VEC3_STR = "VEC3";
        private const string POSITION_STR = "POSITION";
        private const string NORMAL_STR = "NORMALS";
        private const string TEXCOORD_STR = "TEXCOORD_0";
        private const string SCALAR_STR = "SCALAR";
        private const string FACE_STR = "FACE";

        public static ulong ExportVertices(int bufferIndex, ulong byteOffset, GeometryDataObject geometryDataObject,
            GltfBinaryData bufferData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            for (int i = 0; i < geometryDataObject.Vertices.Count; i++)
            {
                bufferData.VertexBuffer.Add(Convert.ToSingle(geometryDataObject.Vertices[i]));
            }

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.VertexBuffer);

            // Add a vec3 buffer view
            const int elementsPerVertex = 3;
            const int bytesPerElement = 4;
            const int bytesPerVertex = elementsPerVertex * bytesPerElement;
            int numVec3s = geometryDataObject.Vertices.Count / elementsPerVertex;
            ulong sizeOfVec3View = (ulong) (numVec3s * bytesPerVertex);

            var vec3View = new GltfBufferView(bufferIndex, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, "verts");
            bufferViews.Add(vec3View);
            int vec3ViewIndex = bufferViews.Count - 1;

            // add a position accessor
            int count = geometryDataObject.Vertices.Count / elementsPerVertex;
            var max = new List<float>(3) { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            var min = new List<float>(3) { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };

            var positionAccessor = new GltfAccessor(vec3ViewIndex, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, POSITION_STR);
            accessors.Add(positionAccessor);
            bufferData.VertexAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3View.ByteLength;
        }

        public static ulong ExportFaces(int bufferIndex, ulong byteOffset, GeometryDataObject geometryData, GltfBinaryData binaryData,
            List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            foreach (var index in geometryData.Faces)
            {
                binaryData.IndexBuffer.Add(index);
            }

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(binaryData.IndexBuffer);

            // Add a faces / indexes buffer view
            const int elementsPerIndex = 1;
            const int bytesPerIndexElement = 4;
            const int bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            var numIndexes = geometryData.Faces.Count;
            ulong sizeOfIndexView = (ulong) (numIndexes * bytesPerIndex);
            var facesView = new GltfBufferView(bufferIndex, byteOffset, sizeOfIndexView, Targets.ELEMENT_ARRAY_BUFFER, "faces");
            bufferViews.Add(facesView);
            var facesViewIndex = bufferViews.Count - 1;

            // add a face accessor
            var count = geometryData.Faces.Count / elementsPerIndex;
            var max = new List<float>(1) { faceMinMax[1] };
            var min = new List<float>(1) { faceMinMax[0] };
            var faceAccessor = new GltfAccessor(facesViewIndex, 0, ComponentType.UNSIGNED_INT, count, SCALAR_STR, max, min, FACE_STR);
            accessors.Add(faceAccessor);
            binaryData.IndexAccessorIndex = accessors.Count - 1;
            return byteOffset + facesView.ByteLength;
        }

        public static ulong ExportNormals(int bufferIndex, ulong byteOffset, GeometryDataObject geometryDataObject, GltfBinaryData binaryData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            foreach (double normal in geometryDataObject.Normals)
            {
                binaryData.NormalBuffer.Add(Convert.ToSingle(normal));
            }

            // Get max and min for normal data
            float[] normalMinMax = Util.GetVec3MinMax(binaryData.NormalBuffer);

            // Add a normals (vec3) buffer view
            const int elementsPerNormal = 3;
            const int bytesPerNormalElement = 4;
            const int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
            int normalsCount = geometryDataObject.Normals.Count;
            int numVec3Normals = normalsCount / elementsPerNormal;
            ulong sizeOfVec3ViewNormals = (ulong)numVec3Normals * (ulong)bytesPerNormal;
            GltfBufferView vec3ViewNormals = new GltfBufferView(bufferIndex, byteOffset, sizeOfVec3ViewNormals, Targets.ARRAY_BUFFER, "normals");
            bufferViews.Add(vec3ViewNormals);
            int vec3ViewNormalsIndex = bufferViews.Count - 1;

            // add a normals accessor
            var count = normalsCount / elementsPerNormal;
            var max = new List<float>(3) { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            var min = new List<float>(3) { normalMinMax[0], normalMinMax[2], normalMinMax[4] };

            var normalsAccessor = new GltfAccessor(vec3ViewNormalsIndex, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, NORMAL_STR);
            accessors.Add(normalsAccessor);
            binaryData.NormalsAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3ViewNormals.ByteLength;
        }
        public static ulong ExportTexCoords(int bufferIndex, ulong byteOffset, GeometryDataObject geometryDataObject, GltfBinaryData binaryData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            int texCoordsCount = geometryDataObject.TexCoords.Count;
            if (texCoordsCount == 0)
            {
                return byteOffset;
            }

            for (int i = 0; i < texCoordsCount; i++)
            {
                binaryData.TexCoordBuffer.Add(Convert.ToSingle(geometryDataObject.TexCoords[i]));
            }

            float[] maxMin = Util.GetVec2MinMax(binaryData.TexCoordBuffer);

            // add a vec2 buffer view
            const int elementsPerTexcoord = 2;
            const int bytesPerElement = 4;
            const int bytesPerTexcoord = elementsPerTexcoord * bytesPerElement;
            int numVec2TexCoords = texCoordsCount / elementsPerTexcoord;
            ulong sizeOfVec2ViewTexCoords = (ulong) numVec2TexCoords * bytesPerTexcoord;
            GltfBufferView vec2ViewTexCoords = new GltfBufferView(bufferIndex, byteOffset, sizeOfVec2ViewTexCoords, Targets.ARRAY_BUFFER, "texcoords_0");
            bufferViews.Add(vec2ViewTexCoords);
            int vec2ViewTexCoordsIndex = bufferViews.Count - 1;

            // add the accessor
            var count = texCoordsCount / elementsPerTexcoord;
            var max = new List<float>(2) { maxMin[1], maxMin[3] };
            var min = new List<float>(2) { maxMin[0], maxMin[2] };
            var accessor = new GltfAccessor(vec2ViewTexCoordsIndex, 0, ComponentType.FLOAT, count, VEC2_STR, max, min, TEXCOORD_STR);
            accessors.Add(accessor);
            binaryData.TexCoordAccessorIndex = accessors.Count - 1;
            return byteOffset + vec2ViewTexCoords.ByteLength;
        }
    }
}
