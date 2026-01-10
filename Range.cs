using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.General
{
    public struct Range<T>
    {
        public Range() {}

        public Range(T max, T min)
        {
            Max = max;
            Min = min;
        }
        public T Max { set; get; }
        public T Min { set; get; }
    }
}
