using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfVersion
    {
        public string version = "2.0";
        public string generator = "Cesium ion Revit generator";
        public string copyright = "unsure"; // TODO
        public Dictionary<string, System.Object> extras = new Dictionary<string, System.Object>();
    }
}
