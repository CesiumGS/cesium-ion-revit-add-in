using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Model
{
    internal class GeometryDataObject
    {
        public List<double> Vertices { get; set; } = new List<double>();

        public List<int> Faces { get; set; } = new List<int>();
    }
}
