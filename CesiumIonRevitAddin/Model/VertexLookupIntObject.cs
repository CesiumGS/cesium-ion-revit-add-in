using System.Collections.Generic;

namespace CesiumIonRevitAddin.Model
{
    internal class VertexLookupIntObject : Dictionary<PointIntObject, int>
    {
        public int AddVertex(PointIntObject p)
        {
            return this.ContainsKey(p)
              ? this[p]
              : this[p] = this.Count;
        }

        /// <summary>
        /// Define equality for integer-based PointInt.
        /// </summary>
        public class PointIntEqualityComparer : IEqualityComparer<PointIntObject>
        {
            public bool Equals(PointIntObject p, PointIntObject q)
            {
                return p.CompareTo(q) == 0;
            }

            public int GetHashCode(PointIntObject p)
            {
                return string.Concat(p.X.ToString(), ",", p.Y.ToString(), ",", p.Z.ToString()).GetHashCode();
            }
        }
    }
}
