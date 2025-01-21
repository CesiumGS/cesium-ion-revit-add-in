using System.Collections.Generic;

namespace CesiumIonRevitAddin.Model
{
    internal class VertexLookupIntObject : Dictionary<PointIntObject, int>
    {
        public int AddVertex(PointIntObject p)
        {
            if (this.ContainsKey(p))
            {
                return this[p];
            }
            else
            {
                int index = this.Count;
                this[p] = index;
                return index;
            }
        }

        /// <summary>
        /// Define equality for integer-based PointInt.
        /// </summary>
        public class PointIntEqualityComparer : IEqualityComparer<PointIntObject>
        {
            public bool Equals(PointIntObject p, PointIntObject q) => p.CompareTo(q) == 0;

            public int GetHashCode(PointIntObject p) => string.Concat(p.X.ToString(), ",", p.Y.ToString(), ",", p.Z.ToString()).GetHashCode();
        }
    }
}
