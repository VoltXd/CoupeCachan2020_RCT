using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidarProcessor
{
    public class PointD
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
