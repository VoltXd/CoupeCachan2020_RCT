using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LidarProcessor
{
    public class CoherentObject : IComparable
    {
        public List<double> xPointList = new List<double>();
        public List<double> yPointList = new List<double>();
        public double size { get; private set; }
        public double xPos { get; private set; }
        public double yPos { get; private set; }
        public double distance { get; private set; }
        public double angle { get; private set; }
        public double lastA { get; set; }
        public double lastB { get; set; }
        public double fitCoeff = 0;

        public int n = 0;

        public CoherentObject()
        {
            fitCoeff = 10;
        }

        /// <summary>
        /// Calcul la position selon la moyenne de chaques points en x et en y
        /// </summary>
        public void CalculateSpatialData()
        {
            if (xPointList.Count > 0 && yPointList.Count > 0)
            {
                int N = xPointList.Count;
                double xMin = xPointList[0];
                double yMin = yPointList[0];
                double xMax = xPointList[N - 1];
                double yMax = yPointList[N - 1];
                double angleMin = Math.Atan2(yMin, xMin);
                double angleMax = Math.Atan2(yMax, xMax);
                double deltaAngle = Math.Abs(angleMax - angleMin);
                xPos = xPointList.Average();
                yPos = yPointList.Average();
                distance = Math.Sqrt(xPos * xPos + yPos * yPos);
                angle = Math.Atan2(yPos, xPos);
                size = 2 * (distance * Math.Tan(deltaAngle / 2));
            }
        }

        public int CompareTo(object obj)
        {
            CoherentObject objet = obj as CoherentObject;
            if (this.distance > objet.distance)
                return 1;
            else
                return -1;
        }
    }
}
