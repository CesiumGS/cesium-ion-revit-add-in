using System.Collections.Generic;

namespace CesiumIonRevitAddin.Model
{
    internal class GeometryDataObject
    {
        public List<double> Vertices { get; set; } = new List<double>();
        public List<int> Faces { get; set; } = new List<int>();
        public List<double> Normals { get; set; } = new List<double>();
        public List<double> TexCoords { get; set; } = new List<double>();
    }
}
