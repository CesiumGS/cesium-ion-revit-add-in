using Autodesk.Revit.DB;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Autodesk.Revit.DB.SpecTypeId;

namespace CesiumIonRevitAddin.Utils
{
    internal class Util
    {
        // TODO: camel case. "Export to IFC" currently is "exporttoIFC"
        public static string GetGltfName(string input)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; ++i)
            {
                Char c = input[i];
                if (Char.IsLetterOrDigit(c))
                {
                    if (i == 0) c = Char.ToLower(c);
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static bool CanBeLockOrHidden(Element element, View view)
        {
            if (element.Category.CanAddSubcategory)
            {
                return true;
            }
            if (element.CanBeHidden(view))
            {
                return true;
            }

            return false;
        }

        public static string CreateClassName(string categoryName, string familyName)
        {
            return categoryName + ": " + familyName;
        }

        public static float[] GetVec3MinMax(IEnumerable<float> vec3)
        {
            var xvalues = vec3.Where((val, idx) => idx % 3 == 0);
            var yvalues = vec3.Where((val, idx) => idx % 3 == 1);
            var zvalues = vec3.Where((val, idx) => idx % 3 == 2);

            return new float[] { xvalues.Min(), xvalues.Max(), yvalues.Min(), yvalues.Max(), zvalues.Min(), zvalues.Max() };
        }

        public static float[] GetVec2MinMax(IEnumerable<float> vec2)
        {
            var xvalues = vec2.Where((val, idx) => idx % 2 == 0);
            var yvalues = vec2.Where((val, idx) => idx % 2 == 1);

            return new float[] { xvalues.Min(), xvalues.Max(), yvalues.Min(), yvalues.Max() };
        }

        public static int[] GetScalarMinMax(List<int> scalars)
        {
            if (scalars == null || scalars.Count == 0) return null;
            return new int[] { scalars.Min(), scalars.Max() };
        }

        public static string GetAddinUserDataFolder()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppDataPath, "Cesium", "CesiumIonRevitAddin");
        }
    }
}
