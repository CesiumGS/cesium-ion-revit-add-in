using CesiumIonRevitAddin.gltf;
using CesiumIonRevitAddin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
