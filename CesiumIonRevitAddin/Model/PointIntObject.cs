using Autodesk.Revit.DB;
using System;

namespace CesiumIonRevitAddin.Model
{
    internal class PointIntObject : IComparable<PointIntObject>
    {
        public PointIntObject(XYZ p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public int CompareTo(PointIntObject a)
        {
            double d = this.X - a.X;
            if (d == 0)
            {
                d = this.Y - a.Y;
                if (d == 0)
                {
                    d = this.Z - a.Z;
                }
            }

            return (d == 0) ? 0 : ((d > 0) ? 1 : -1);
        }
    }
}
