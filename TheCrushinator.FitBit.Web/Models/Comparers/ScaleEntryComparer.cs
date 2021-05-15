using System.Collections.Generic;

namespace TheCrushinator.FitBit.Web.Models.Comparers
{
    public class ScaleEntryComparer : IEqualityComparer<ScaleEntry>
    {
        public bool Equals(ScaleEntry x, ScaleEntry y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.EntryId == y.EntryId;
        }

        public int GetHashCode(ScaleEntry obj)
        {
            return (obj.EntryId != null ? obj.EntryId.GetHashCode() : 0);
        }
    }
}
