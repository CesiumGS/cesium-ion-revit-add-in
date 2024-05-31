using CesiumIonRevitAddin.Gltf;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Export
{
    internal class BufferConfig
    {
        public static void Run(List<GltfBufferView> bufferViews, List<GltfBuffer> buffers)
        {
            int bytePosition = 0;
            int currentBuffer = 0;

            foreach (var bufferView in bufferViews)

            {
                if (bufferView.Buffer == 0)
                {
                    bytePosition += bufferView.ByteLength;
                    continue;
                }

                if (bufferView.Buffer != currentBuffer)
                {
                    bufferView.Buffer = 0;
                    bufferView.ByteOffset = bytePosition;
                    bytePosition += bufferView.ByteLength;
                }
            }

            var buffer = new GltfBuffer();

            // TODO: add back preferences
            var preferences = new Preferences();
            {
                var bufferUri = preferences.FileName + ".bin";
                buffer.Uri = bufferUri;
            }

            buffer.ByteLength = bytePosition;
            buffers.Clear();
            buffers.Add(buffer);
        }
    }
}
