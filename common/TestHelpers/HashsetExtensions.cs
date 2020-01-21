using System.Collections.Generic;

namespace Mmm.Platform.IoT.Common.TestHelpers
{
    public static class HashsetExtensions
    {
        public static bool SetEquals<T>(this HashSet<T> me, IEnumerable<T> other)
        {
            return me.IsSubsetOf(other) && me.IsSupersetOf(other);
        }
    }
}