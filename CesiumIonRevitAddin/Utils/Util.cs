using System;
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
    }
}
