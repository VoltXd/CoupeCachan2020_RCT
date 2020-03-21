using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidar;
using MathNet.Numerics;

namespace LidarProcessor
{
    public class LidarDataProcessor
    {
        //Traitement des points, réception sur l'évent du Lidar
        public void OnPointsAvailableReceived(object sender, LidarPointsReadyEventArgs e)
        {
            //ProcessLidarSimple(e);
            ProcessLidarStatistic(e);
        }

        private void ProcessLidarSimple(LidarPointsReadyEventArgs e)
        {
            //On récupère les données brutes du Lidar
            List<LidarPoint> lidarPointList = e.LidarPoints;

            List<double> distanceList = lidarPointList.Select(r => r.Distance).ToList();
            List<double> angleList = lidarPointList.Select(r => (double)r.Angle).ToList();

            //Filtrage des points avec une distance incohérente (pics)
            List<double> distanceListFiltered = new List<double>();
            distanceListFiltered.Add(distanceList[0]);
            for (int i = 1; i < distanceList.Count - 1; i++)
            {
                double ecartPasse = Math.Abs(distanceList[i] - distanceList[i - 1]);
                double ecartFutur = Math.Abs(distanceList[i] - distanceList[i + 1]);

                if (ecartFutur >= 0.04 || ecartPasse >= 0.04)
                {
                    distanceListFiltered.Add(distanceListFiltered[i - 1]);
                }
                else
                {
                    distanceListFiltered.Add(distanceList[i]);
                }
            }
            distanceListFiltered.Add(distanceList[distanceList.Count - 1]);

            //Les coordonnées (x;y) correspondent aux données non filtré
            //Les coordonnées (x2;y2) correspondent aux données filtré (plus de pics)
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y2 = new List<double>();
            for (int i = 0; i < lidarPointList.Count; i++)
            {
                x.Add(distanceList[i] * Math.Cos(angleList[i]));
                y.Add(distanceList[i] * Math.Sin(angleList[i]));
                x2.Add(distanceListFiltered[i] * Math.Cos(angleList[i]));
                y2.Add(distanceListFiltered[i] * Math.Sin(angleList[i]));
            }
            
            //Calcul des données spatiales de chaque objet            

            OnProcessedData(x.ToArray(), y.ToArray(), x2.ToArray(), y2.ToArray(), null);
        }

        private void ProcessLidarStatistic(LidarPointsReadyEventArgs e)
        {
            //On récupère les données brutes du Lidar
            List<LidarPoint> lidarPointList = e.LidarPoints;

            List<double> distanceList = lidarPointList.Select(r => r.Distance).ToList();
            List<double> angleList = lidarPointList.Select(r => (double)r.Angle).ToList();

            //Filtrage des points avec une distance incohérente (pics)
            List<double> distanceListFiltered = new List<double>();
            distanceListFiltered.Add(distanceList[0]);
            for (int i = 1; i < distanceList.Count - 1; i++)
            {
                double ecartPasse = Math.Abs(distanceList[i] - distanceList[i - 1]);
                double ecartFutur = Math.Abs(distanceList[i] - distanceList[i + 1]);

                if (ecartFutur >= 0.04 || ecartPasse >= 0.04)
                {
                    distanceListFiltered.Add(distanceListFiltered[i - 1]);
                }
                else
                {
                    distanceListFiltered.Add(distanceList[i]);
                }
            }
            distanceListFiltered.Add(distanceList[distanceList.Count - 1]);

            //Les coordonnées (x;y) correspondent aux données non filtré
            //Les coordonnées (x2;y2) correspondent aux données filtré (plus de pics)
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y2 = new List<double>();
            for (int i = 0; i < lidarPointList.Count; i++)
            {
                x.Add(distanceList[i] * Math.Cos(angleList[i]));
                y.Add(distanceList[i] * Math.Sin(angleList[i]));
                x2.Add(distanceListFiltered[i] * Math.Cos(angleList[i]));
                y2.Add(distanceListFiltered[i] * Math.Sin(angleList[i]));
            }

            List<CoherentObject> coherentObjectList = new List<CoherentObject>();
            CoherentObject currentObject = new CoherentObject();
            double[] xdata = x2.ToArray();
            double[] ydata = y2.ToArray();

            int pointToRecreateLine = 4;
            double distPtLine = 0;
            double distThreshold = 0.045;
            bool isNewPointOk = false;

            //Trie des différents objets perçus (par regression lineaire)
            for (int i = 0; i < xdata.Count(); i++)
            {
                PointD currentPoint = new PointD(xdata[i], ydata[i]);
                //Si l'objet testé contient 4 points, on test l'ecart entre un nouveau point et la droite formé par l'objet
                if (currentObject.xPointList.Count() > pointToRecreateLine)
                {
                    //List<double> xTestList = new List<double>() { currentObject.xPointList.Last(), xdata[i] };
                    //List<double> yTestList = new List<double>() { currentObject.yPointList.Last(), ydata[i] };
                    distPtLine = DistanceFromPointToLine(currentPoint, new PointD(0, currentObject.lastA), new PointD(1, currentObject.lastA + 1 * currentObject.lastB));
                }

                //Si la distance entre le point et la ligne est trop grande, on fini l'objet et on en crée un nouveau
                if (distPtLine > distThreshold || i >= xdata.Count() - 1) //currentObject.fitCoeff * 3 || i >= xdata.Count() - 1) //0.05 || i >= xdata.Count() - 1)
                {
                    if (currentObject.xPointList.Count >= 2)
                    {
                        isNewPointOk = NewLinearRegression(currentObject, currentPoint, distThreshold);
                        currentObject.n++;
                    }

                    if (!isNewPointOk)
                    {
                        coherentObjectList.Add(currentObject);
                        currentObject = new CoherentObject();
                        distPtLine = 0;
                    }
                }

                currentObject.xPointList.Add(xdata[i]);
                currentObject.yPointList.Add(ydata[i]);
                //Si l'objet testé contient 2^n points(CI: n = 2), on en calcule la droite par régression linéaire
                if (currentObject.xPointList.Count() == pointToRecreateLine)
                {
                    NewLinearRegression(currentObject, new PointD(currentObject.xPointList.Last(), currentObject.yPointList.Last()), 10);
                    currentObject.n++;
                }
            }

            //Calcul des données spatiales de chaque objet
            foreach (CoherentObject co in coherentObjectList)
            {
                co.CalculateSpatialData();
            }

            OnProcessedData(x.ToArray(), y.ToArray(), xdata, ydata, coherentObjectList);
        }

        public event EventHandler<ProcessedLidarDataEventArgs> OnProcessedDataEvent;
        private void OnProcessedData(double[] x1Array, double[] y1Array, double[] x2Array, double[] y2Array, List<CoherentObject> coherentObjectList)
        {
            if (OnProcessedDataEvent != null)
                OnProcessedDataEvent(this, new ProcessedLidarDataEventArgs { xArrayRaw = x1Array, yArrayRaw = y1Array, xArrayProcessed = x2Array, yArrayProcessed = y2Array, coherentObjects = coherentObjectList });
        }

        private bool NewLinearRegression(CoherentObject currentObject, PointD point, double threshold)
        {
            var xSegmentArray = currentObject.xPointList.ToArray();
            var ySegmentArray = currentObject.yPointList.ToArray();
            Tuple<double, double> p = Fit.Line(xSegmentArray, ySegmentArray);
            //p = Fit.Line(new double[] { 1.01, 1.015, 1.02, 1.025, 1.03 }, new double[] { 1, 2, 3, 4, 5 });
            double a = p.Item1; // == 10; intercept
            currentObject.lastA = a;
            double b = p.Item2; // == 0.5; slope
            currentObject.lastB = b;

            currentObject.fitCoeff = GoodnessOfFit.PopulationStandardError(xSegmentArray.Select(f => a + b * f), ySegmentArray); // == 1.0

            double distPointToNewLine = DistanceFromPointToLine(point, new PointD(0, currentObject.lastA), new PointD(1, currentObject.lastA + 1 * currentObject.lastB));
            if (distPointToNewLine > threshold)
                return false;
            else
                return true;
        }

        private double DistanceFromPointToLine(PointD point, PointD linePt1, PointD linePt2)
        {
            // given a line based on two points, and a point away from the line,
            // find the perpendicular distance from the point to the line.
            // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // for explanation and defination.

            return Math.Abs((linePt2.X - linePt1.X) * (linePt1.Y - point.Y) - (linePt1.X - point.X) * (linePt2.Y - linePt1.Y)) /
                    Math.Sqrt(Math.Pow(linePt2.X - linePt1.X, 2) + Math.Pow(linePt2.Y - linePt1.Y, 2));
        }
    }

    public class ProcessedLidarDataEventArgs : EventArgs
    {
        public double[] xArrayRaw { get; set; }
        public double[] yArrayRaw { get; set; }
        public double[] xArrayProcessed { get; set; }
        public double[] yArrayProcessed { get; set; }
        public List<CoherentObject> coherentObjects { get; set; }
    }
}
