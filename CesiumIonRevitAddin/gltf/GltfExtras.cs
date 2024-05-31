using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfExtras
    {
        public string uniqueId;

        // TODO: handle grids
        // RevitGridParametersObject gridParameters;

        public Dictionary<string, string> parameters;
        public int elementId;
        public string elementCategory;
    }
}
