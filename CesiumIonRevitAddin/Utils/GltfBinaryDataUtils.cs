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

        public static ulong ExportVertices(int bufferIdx, ulong byteOffset, GeometryDataObject geomData,
            GltfBinaryData bufferData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors,
            out ulong sizeOfVec3View, out int elementsPerVertex)
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
            sizeOfVec3View = (ulong) (numVec3 * bytesPerVertex);

            var vec3View = new GltfBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, "verts");
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

        public static ulong ExportFaces(int bufferIdx, ulong byteOffset, GeometryDataObject geometryData, GltfBinaryData binaryData,
            List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            foreach (var index in geometryData.Faces)
            {
                binaryData.IndexBuffer.Add(index);
            }

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(binaryData.IndexBuffer);

            // Add a faces / indexes buffer view
            var elementsPerIndex = 1;
            var bytesPerIndexElement = 4;
            var bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            var numIndexes = geometryData.Faces.Count;
            ulong sizeOfIndexView = (ulong) (numIndexes * bytesPerIndex);
            var facesView = new GltfBufferView(bufferIdx, byteOffset, sizeOfIndexView, Targets.ELEMENT_ARRAY_BUFFER, "faces");
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

        public static ulong ExportNormals(int bufferIdx, ulong byteOffset, GeometryDataObject geomData, GltfBinaryData binaryData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
        {
            for (int i = 0; i < geomData.Normals.Count; i++)
            {
                binaryData.NormalBuffer.Add(Convert.ToSingle(geomData.Normals[i]));
            }

            // Get max and min for normal data
            float[] normalMinMax = Util.GetVec3MinMax(binaryData.NormalBuffer);

            // Add a normals (vec3) buffer view
            int elementsPerNormal = 3;
            int bytesPerNormalElement = 4;
            int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
            int normalsCount = geomData.Normals.Count;
            int numVec3Normals = normalsCount / elementsPerNormal;
            ulong sizeOfVec3ViewNormals = (ulong)numVec3Normals * (ulong)bytesPerNormal;
            GltfBufferView vec3ViewNormals = new GltfBufferView(bufferIdx, byteOffset, sizeOfVec3ViewNormals, Targets.ARRAY_BUFFER, "normals");
            bufferViews.Add(vec3ViewNormals);
            int vec3ViewNormalsIdx = bufferViews.Count - 1;

            // add a normals accessor
            var count = normalsCount / elementsPerNormal;
            var max = new List<float>(3) { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            var min = new List<float>(3) { normalMinMax[0], normalMinMax[2], normalMinMax[4] };

            var normalsAccessor = new GltfAccessor(vec3ViewNormalsIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, NORMAL_STR);
            accessors.Add(normalsAccessor);
            binaryData.NormalsAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3ViewNormals.ByteLength;
        }
        public static ulong ExportTexCoords(int bufferIdx, ulong byteOffset, GeometryDataObject geometryDataObject, GltfBinaryData binaryData, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors)
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
            int elementsPerTexcoord = 2;
            int bytesPerElement = 4;
            int bytesPerTexcoord = elementsPerTexcoord * bytesPerElement;
            int numVec2TexCoords = texCoordsCount / elementsPerTexcoord;
            ulong sizeOfVec2ViewTexCoords = (ulong) numVec2TexCoords * (ulong) bytesPerTexcoord;
            GltfBufferView vec2ViewTexCoords = new GltfBufferView(bufferIdx, byteOffset, sizeOfVec2ViewTexCoords, Targets.ARRAY_BUFFER, "texcoords_0");
            bufferViews.Add(vec2ViewTexCoords);
            int vec2ViewTexCoordsIdx = bufferViews.Count - 1;

            // add the accessor
            var count = texCoordsCount / elementsPerTexcoord;
            var max = new List<float>(2) { maxMin[1], maxMin[3] };
            var min = new List<float>(2) { maxMin[0], maxMin[2] };
            var accessor = new GltfAccessor(vec2ViewTexCoordsIdx, 0, ComponentType.FLOAT, count, VEC2_STR, max, min, TEXCOORD_STR);
            accessors.Add(accessor);
            binaryData.TexCoordAccessorIndex = accessors.Count - 1;
            return byteOffset + vec2ViewTexCoords.ByteLength;
        }
    }
}
