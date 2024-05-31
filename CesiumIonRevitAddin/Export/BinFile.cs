using CesiumIonRevitAddin.Gltf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CesiumIonRevitAddin.Export
{
    internal class BinFile
    {
        public static void Create(string filename, List<GltfBinaryData> binaryFileData, bool exportNormals, bool exportBatchId)
        {
            FileStream f = File.Create(filename);
            var writer = new BinaryWriter(new BufferedStream(f), System.Text.Encoding.Default);
            {
                foreach (GltfBinaryData binaryData in binaryFileData)
                {
                    for (int i = 0; i < binaryData.VertexBuffer.Count; i++)
                    {
                        writer.Write((float)binaryData.VertexBuffer[i]);
                    }

                    if (exportNormals)
                    {
                        for (int i = 0; i < binaryData.NormalBuffer.Count; i++)
                        {
                            writer.Write((float)binaryData.NormalBuffer[i]);
                        }
                    }

                    for (int i = 0; i < binaryData.IndexBuffer.Count; i++)
                    {
                        writer.Write((int)binaryData.IndexBuffer[i]);
                    }
                }

                writer.Flush();
            }
        }
    }
}
