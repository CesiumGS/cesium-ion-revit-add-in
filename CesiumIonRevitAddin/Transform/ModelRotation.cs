using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.Transform
{
    internal class ModelRotation
    {
        public static List<double> Get(bool flipAxis)
        {
            if (flipAxis)
            {
                return new List<double> { 0.7071, 0, 0, -0.7071 };
            }
            else
            {
                return new List<double> { 0, 0, 0, 0 };
            }
        }
    }
}
