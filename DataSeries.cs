using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.General
{
    public struct DataSeriesItem
    {
        public DateTime Time { set; get; }
        public double Value { set; get; }

        public override string ToString()
        {
            return $"{Time};{Value}";
        }

    }

    public struct TimeSeriesItem
    {
        public DateTime Time { set; get; }
        public double Value { set; get; }

        public override string ToString()
        {
            return $"{Time};{Value}";
        }
    }

    public struct TimeSeriesElement
    {
        public DateTime Time { set; get; }
        public double Value { set; get; }

        public override string ToString()
        {
            return $"{Time};{Value}";
        }
    }

}
