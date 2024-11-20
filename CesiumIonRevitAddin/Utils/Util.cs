using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CesiumIonRevitAddin.Utils
{
    internal static class Util
    {
        public static string GetGltfName(string input)
        {
            // Revit enums usually start with "OST_". Remove it.
            if (input.StartsWith("OST_")) input = input.Substring(4);
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; ++i)
            {
                Char c = input[i];
                if (Char.IsLetterOrDigit(c))
                {
                    if (i == 0)
                    {
                        c = Char.ToLower(c);
                    }

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
            if (scalars == null || scalars.Count == 0)
            {
                return Array.Empty<int>();
            }

            return new int[] { scalars.Min(), scalars.Max() };
        }

        public static string GetAddinUserDataFolder()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppDataPath, "Cesium", "CesiumIonRevitAddin");
        }

        public static string GetAddinFolder()
        {
            // Get the location of the executing assembly (the add-in itself)
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;

            // Get the directory of the assembly
            string addinDirectory = Path.GetDirectoryName(assemblyLocation);

            return addinDirectory;
        }

        public static long GetElementIdAsLong(ElementId elementId)
        {
#if REVIT2022 || REVIT2023
            return (long)elementId.IntegerValue;
#else
            return elementId.Value;
#endif
        }
    }
}
