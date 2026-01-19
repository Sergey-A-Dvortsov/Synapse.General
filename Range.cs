using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.General
{
    public struct Range<T> where T : INumber<T>
    {
        public Range() {}

        public Range(T max, T min)
        {
            Max = max;
            Min = min;
        }

        /// <summary>
        /// Specifies a range using an absolute or relative offset from a given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="isper">if true, then relative offset</param>
        public Range(T value, T offset, bool isper)
        {
            Max = isper ? value + (value * offset) : value + offset;
            Min = isper ? value - (value * offset) : value - offset;
        }

        public T Max { set; get; }
        public T Min { set; get; }

        /// <summary>
        /// Determines whether a value is within a range.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if a value is within a range</returns>
        public bool Contains(T value)
        {
            return value >= Min && value <= Max;
        }

    }
}
