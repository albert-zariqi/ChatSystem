using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.CachingKeys
{
    public static class ShiftCachingKeys
    {
        public static string CapacityByShift(Guid shiftId) => $"shift-capacity:{shiftId}";
    }
}
