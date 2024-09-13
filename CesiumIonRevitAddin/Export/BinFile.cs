using CesiumIonRevitAddin.Gltf;
using System.Collections.Generic;
using System.IO;

namespace CesiumIonRevitAddin.Export
{
    internal static class BinFile
    {
        public static void Create(string filename, List<GltfBinaryData> binaryFileData, bool exportNormals, bool exportBatchId)
        {
            using (FileStream f = File.Create(filename))
            {
                using (var writer = new BinaryWriter(new BufferedStream(f), System.Text.Encoding.Default))
                {
                    foreach (GltfBinaryData binaryData in binaryFileData)
                    {
                        for (int i = 0; i < binaryData.VertexBuffer.Count; i++)
                        {
                            writer.Write(binaryData.VertexBuffer[i]);
                        }

                        if (exportNormals)
                        {
                            for (int i = 0; i < binaryData.NormalBuffer.Count; i++)
                            {
                                writer.Write(binaryData.NormalBuffer[i]);
                            }
                        }

                        for (int i = 0; i < binaryData.TexCoordBuffer.Count; i++)
                        {
                            writer.Write(binaryData.TexCoordBuffer[i]);
                        }

                        for (int i = 0; i < binaryData.IndexBuffer.Count; i++)
                        {
                            writer.Write(binaryData.IndexBuffer[i]);
                        }
                    }

                    writer.Flush();
                }
            }
        }
    }
}
