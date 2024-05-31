using Autodesk.Revit.DB;
using System.Linq;

namespace CesiumIonRevitAddin.Utils
{
    internal class Collectors
    {
        public static Material GetRandomMaterial(Document document)
        {
            using (var collector = new FilteredElementCollector(document))
            {
                return collector.OfCategory(BuiltInCategory.OST_Materials)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Material>()
                .FirstOrDefault();
            }
        }
    }
}
