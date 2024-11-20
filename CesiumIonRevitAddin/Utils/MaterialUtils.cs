using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Gltf;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CesiumIonRevitAddin.Utils
{
    internal class MaterialUtils
    {
        public static Material GetMeshMaterial(Document document, Mesh mesh)
        {
            ElementId materialId = mesh.MaterialElementId;
            if (materialId != null)
            {
                return document.GetElement(materialId) as Material;
            }
            else
            {
                return null;
            }
        }

        public static void SetMaterial(Document doc, Preferences preferences, Mesh mesh, IndexedDictionary<GltfMaterial> materials, bool doubleSided)
        {
            Material material = MaterialUtils.GetMeshMaterial(doc, mesh);

            if (preferences.Materials)
            {
                if (material == null)
                {
                    material = Collectors.GetRandomMaterial(doc);
                }

                GltfMaterial gltfMaterial = GltfExportUtils.GetGLTFMaterial(materials.List, material, doubleSided);

                materials.AddOrUpdateCurrentMaterial(material.UniqueId, gltfMaterial, doubleSided);
            }
        }

        public static void SaveDownsampledTexture(string inputPath, string outputPath, int maxWidth, int maxHeight)
        {
            using (var sourceImage = Image.FromFile(inputPath))
            {
                int originalWidth = sourceImage.Width;
                int originalHeight = sourceImage.Height;

                // Check if downsampling is needed
                if (originalWidth <= maxWidth && originalHeight <= maxHeight)
                {
                    // If the image is smaller or equal to the target size, save it as is
                    sourceImage.Save(outputPath);
                    return;
                }

                // Calculate the new dimensions while maintaining aspect ratio
                float widthRatio = (float)maxWidth / originalWidth;
                float heightRatio = (float)maxHeight / originalHeight;
                float ratio = Math.Min(widthRatio, heightRatio);

                int newWidth = (int)(originalWidth * ratio);
                int newHeight = (int)(originalHeight * ratio);

                // Create a new bitmap for the downsampled image
                var destRect = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
                var destImage = new Bitmap(newWidth, newHeight);

                // Use high-quality settings for downsampling
                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(sourceImage, destRect, 0, 0, originalWidth, originalHeight, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                // Save the downsampled image in the original format
                destImage.Save(outputPath, sourceImage.RawFormat);
            }
        }
    }
}
