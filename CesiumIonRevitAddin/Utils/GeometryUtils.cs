using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace CesiumIonRevitAddin.Utils
{
    internal class GeometryUtils
    {
        public static List<Mesh> GetMeshes(Document document, Element element)
        {
            GeometryElement geometryElement = GetGeometryElement(document, element);

            List<Mesh> meshes = new List<Mesh>();
            foreach (GeometryInstance geoObject in geometryElement.OfType<GeometryInstance>())
            {
                foreach (var mesh in geoObject.GetSymbolGeometry().OfType<Mesh>())
                {
                    meshes.Add(mesh);
                }
            }

            return meshes;
        }

        public static GeometryElement GetGeometryElement(Document document, Element element)
        {
            GeometryElement result;
            try
            {
                Options opt = new Options
                {
                    ComputeReferences = true,
                    View = document.ActiveView,
                };
                result = element.get_Geometry(opt);
            }
            catch
            {
                Options opt = new Options
                {
                    ComputeReferences = true,
                };
                result = element.get_Geometry(opt);
            }

            return result;
        }

        public static XYZ GetNormal(MeshTriangle triangle)
        {
            var vertex0 = triangle.get_Vertex(0);
            var side1 = triangle.get_Vertex(1) - vertex0;
            var side2 = triangle.get_Vertex(2) - vertex0;
            var normal = side1.CrossProduct(side2);
            return normal.Normalize();
        }
    }
}
