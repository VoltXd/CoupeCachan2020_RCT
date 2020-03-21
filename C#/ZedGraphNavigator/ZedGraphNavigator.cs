using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph; 


namespace ZedGraphNavigatorDll
{

    public partial class ZedGraphNavigator : UserControl
    {
        public delegate void MouseDoubleClickEventHandler(object sender, DoubleClickEventArgs e);
        public event MouseDoubleClickEventHandler MouseDbleClick;

        List<RollingPointPairList> pointList = new List<RollingPointPairList>();
        Dictionary<int, Color> colorList = new Dictionary<int, Color>();
        public ZedGraphNavigator()
        {
            InitializeComponent();
            colorList.Add(0, Color.Green);
            colorList.Add(1, Color.Blue);
            colorList.Add(2, Color.Red);
            colorList.Add(3, Color.Violet);
            colorList.Add(4, Color.Orange);


            // Get a reference to the GraphPane
            GraphPane myPane = zedGraphControl.GraphPane;

            myPane.Title.IsVisible = false;
            myPane.XAxis.IsVisible = false;
            myPane.YAxis.IsVisible = false;
            myPane.Legend.IsVisible = false;


            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }
        

        public void SetUpField(double width, double height)
        {
            double ratio = (double)zedGraphControl.Height / (double)zedGraphControl.Width;
            if (height > ratio * width)
            {
                SetFixedXScale(-height / 2 / ratio, height / 2 / ratio);
                SetFixedYScale(-height / 2, height / 2);
            }
            else
            {
                SetFixedXScale(-width / 2, width / 2);
                SetFixedYScale(-width / 2 * ratio, width / 2 * ratio);
            }

            var poly = new ZedGraph.PolyObj
            {
                Points = new[]{
                    new ZedGraph.PointD(-width/2, -height/2),
                    new ZedGraph.PointD(width/2, -height/2),
                    new ZedGraph.PointD(width/2, height/2),
                    new ZedGraph.PointD(-width/2, height/2)
                },
                Fill = new ZedGraph.Fill(Color.LightGreen),
                ZOrder = ZedGraph.ZOrder.E_BehindCurves,
                Border = new Border(Color.Black, 4)
            };
            zedGraphControl.GraphPane.GraphObjList.Add(poly);

        }

        public void SetVisibleAxis(bool xAxisVisible, bool yAxisVisible, bool y2AxisVisible)
        {
            zedGraphControl.GraphPane.XAxis.IsVisible = xAxisVisible;
            zedGraphControl.GraphPane.YAxis.IsVisible = yAxisVisible;
            zedGraphControl.GraphPane.Y2Axis.IsVisible = y2AxisVisible;
        }

        public void PointCreate(string name, double x, double y, double theta, double radius, Color fillColor, Color borderColor)
        {
            //foreach (zedGraphControl.GraphPane.GraphObjList.)
            var robot = new ZedGraph.EllipseObj
            {
                Location = new Location(x, y, radius, radius, CoordType.AxisXYScale, AlignH.Center, AlignV.Center),
                ZOrder = ZedGraph.ZOrder.A_InFront,
                Fill = new Fill(fillColor),
                Border = new Border(borderColor, 2),
                Tag = name
            };
            zedGraphControl.GraphPane.GraphObjList.Add(robot);
        }

        public void PointUpdate(string name, double x, double y, double theta)
        {
            int objIndex = zedGraphControl.GraphPane.GraphObjList.IndexOfTag(name);

            if (objIndex > 0)
            {
                var obj = zedGraphControl.GraphPane.GraphObjList[objIndex];

                if (obj.GetType().Name == "EllipseObj")
                {
                    ((EllipseObj)obj).Location.X = x;
                    ((EllipseObj)obj).Location.Y = y;
                }
                zedGraphControl.Invalidate();
            }
        }

        public void PolyObjCreate(string name, PointD[] ptList, Color fillColor, Color borderColor, int borderWidth, ZedGraph.ZOrder zorder)
        {
            var poly = new PolyObject
            {
                originalPointList = ptList,
                Points = ptList,
                Fill = new ZedGraph.Fill(fillColor),
                ZOrder = zorder,
                Border = new Border(borderColor, borderWidth),
                Tag = name
            };
            //var poly = new ZedGraph.PolyObj
            //{
            //    Points = ptList,
            //    Fill = new ZedGraph.Fill(fillColor),
            //    ZOrder = zorder,
            //    Border = new Border(borderColor, borderWidth),
            //    Tag = name
            //};
            zedGraphControl.GraphPane.GraphObjList.Add(poly);
        }

        public void PolyObjUpdate(string name, PointD[] ptList)
        {
            int objIndex = zedGraphControl.GraphPane.GraphObjList.IndexOfTag(name);
            if (objIndex > 0)
            {
                var obj = zedGraphControl.GraphPane.GraphObjList[objIndex];
                if (obj.GetType().Name == "PolyObj")
                {
                    ((PolyObject)obj).Points = ptList;
                    ((PolyObject)obj).originalPointList = ptList;
                }
                zedGraphControl.Invalidate();
            }
        }

        public void PolyObjMove(string name, double x, double y, double angle)
        {
            int objIndex = zedGraphControl.GraphPane.GraphObjList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.GraphObjList[objIndex];
                if (obj.GetType().Name == "PolyObject")
                {
                    ((PolyObject)obj).Points = new PointD[((PolyObject)obj).originalPointList.Count()];
                    for (int i = 0; i < ((PolyObject)obj).originalPointList.Count(); i++)
                    {
                        var pt = ((PolyObject)obj).originalPointList[i];
                        double X2 = pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle) + x;
                        double Y2 = pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle) + y;
                        ((PolyObject)obj).Points[i] = new PointD(X2, Y2);
                    }
                }
                zedGraphControl.Invalidate();
            }
        }

        ///<summary>
        ///Add a line to the navigator
        ///</summary>
        public void LineCreate(string name, IPointListEdit ptList, Color lineColor, bool showLine = true, bool IsY2Axis = true, int lineWidth = 1)
        {
            if (showLine)
            {
                var line = zedGraphControl.GraphPane.AddCurve(name, ptList, lineColor, SymbolType.None);
                line.Line.Width = lineWidth;
                line.Tag = name;
                line.IsY2Axis = IsY2Axis;
            }
            else
            {
                var line = zedGraphControl.GraphPane.AddCurve(name, ptList, lineColor, SymbolType.Default);
                line.Line.IsVisible = false;
                line.Tag = name;
                line.IsY2Axis = IsY2Axis;
            }
        }

        ///<summary>
        ///Add a line to the navigator
        ///</summary>
        public void LineCreate(string name, IPointListEdit ptList)
        {
            var line = zedGraphControl.GraphPane.AddCurve(name, ptList, Color.Black, SymbolType.None);
            line.Tag = name;
        }

        public void LineAddSingleData(string name, double x, double y)
        {
            if (zedGraphControl.GraphPane != null)
            {
                int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
                if (objIndex >= 0)
                {
                    var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                    if (obj.GetType().Name == "LineItem")
                    {
                        ((IPointListEdit) ((LineItem) obj).Points).Add(x, y);
                        zedGraphControl.GraphPane.AxisChange();
                        zedGraphControl.Invalidate();
                    }
                }
            }
        }

        public void LineUpdateData(string name, List<double> xList, List<double> yList)
        {
            int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                if (obj.GetType().Name == "LineItem")
                {
                    lock (xList)
                    {
                        lock (yList)
                        {
                            var list = (IPointListEdit)((LineItem)obj).Points;
                            list.Clear();
                            for (int i = 0; i < xList.Count(); i++)
                                list.Add(xList[i], yList[i]);
                        }
                    }

                    zedGraphControl.GraphPane.AxisChange();
                    zedGraphControl.Invalidate();
                }
            }
        }
        
        ///<summary>
        ///Add a line to the navigator
        ///</summary>
        public void ImageFromMatrixCreate(string name, int[,] values, int width, int height, int xPos, int yPos, ZOrder zorder)
        {            
            int[,] data = values;
            
            // Possible brushes (fill yourself)
            Brush[] brushes = new Brush[] {
                Brushes.Red,     // <- color for 0
                Brushes.Green,   // <- color for 1
                Brushes.Blue,    // <- color for 2
                Brushes.Yellow,  // <- ...
                Brushes.Cyan
            };

            // Let resulting bitmap be about 200x200
            int step_x = width / (data.GetUpperBound(0) - data.GetLowerBound(0) + 1);
            int step_y = height / (data.GetUpperBound(1) - data.GetLowerBound(1) + 1);

            Bitmap result = new Bitmap((data.GetUpperBound(0) - data.GetLowerBound(0) + 1) * step_x,
                (data.GetUpperBound(1) - data.GetLowerBound(1) + 1) * step_y);

            using (Graphics gc = Graphics.FromImage(result))
            {
                for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
                {
                    for (int j = data.GetLowerBound(1); j <= data.GetUpperBound(1); ++j)
                    {
                        int v = data[i, j];
                        gc.FillRectangle(brushes[v % brushes.Length], new Rectangle(j * step_x, i * step_y, step_x, step_y));
                    }
                }
            }

            var imgFromMatrix = new ImageFromMatrix(result, xPos, yPos, width, height)
            {
                width = width,
                height = height,
                xPos = xPos,
                yPos = yPos,
                Tag = name,
                ZOrder = zorder,
                IsVisible = true,
            };

            zedGraphControl.GraphPane.GraphObjList.Add(imgFromMatrix);
            zedGraphControl.Refresh();
        }

        public void ImageFromMatrixUpdateData(string name, int[,] values)
        {
            int objIndex = zedGraphControl.GraphPane.GraphObjList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.GraphObjList[objIndex];

                if (obj.GetType().Name == "ImageFromMatrix")
                {
                    var imgFromMatrix = (ImageFromMatrix)obj;

                    int xSize = values.GetUpperBound(0) - values.GetLowerBound(0) + 1;
                    int ySize = values.GetUpperBound(1) - values.GetLowerBound(1) + 1;

                    //var result = new DirectBitmap(xSize, ySize);

                    for (int i = 0; i < xSize; ++i)
                    {
                        for (int j = 0; j < ySize; ++j)
                        {
                            int alpha = 255;
                            int r = values[i, j];
                            int v = values[i, j];
                            int b = 0;
                            v = Math.Max(0, v);
                            v = Math.Min(255, v);

                            if (r > 0)
                            {
                                int colorValue = (alpha << 24) + (r << 16) + (v << 8) + (b);
                                //result.Bits[i + xSize * (ySize - j - 1)] = colorValue;
                            }
                            else
                            {
                                int colorValue = (alpha << 24) + (0xDD << 16) + (0xDD << 8) + (0xDD);
                                //result.Bits[i + xSize * (ySize - j - 1)] = colorValue;
                            }
                        }
                    }
                    //imgFromMatrix.Image = result.Bitmap;

                    zedGraphControl.GraphPane.AxisChange();
                    zedGraphControl.Invalidate();
                }
            }
        }

        public void ImageUpdateData(string name, Image image)
        {
            int objIndex = zedGraphControl.GraphPane.GraphObjList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.GraphObjList[objIndex];

                if (obj.GetType().Name == "ImageFromMatrix")
                {
                    var imgFromMatrix = (ImageFromMatrix)obj;
                                        
                    imgFromMatrix.Image = image;

                    zedGraphControl.GraphPane.AxisChange();
                    zedGraphControl.Invalidate();
                }
            }
        }

        ///<summary>
        ///Add a point list to the navigator
        ///</summary>
        public void PointListCreate(string name, IPointListEdit ptList, Color lineColor, int lineWidth = 1, bool LineIsVisible = false, int radius=1)
        {
            var pointList = zedGraphControl.GraphPane.AddCurve(name, ptList, lineColor, SymbolType.Circle);
            pointList.Line.Width = lineWidth;
            pointList.Line.IsVisible = LineIsVisible;
            pointList.Symbol.Size = radius;
            pointList.Tag = name;
        }

        ///<summary>
        ///Add a point list to the navigator
        ///</summary>
        public void PointListCreate(string name, IPointListEdit ptList)
        {
            PointListCreate(name, ptList, Color.Black);
        }

        ///<summary>
        ///Add a point to the point list
        ///</summary>
        public void PointListAddSingleData(string name, double x, double y)
        {
            int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                if (obj.GetType().Name == "LineItem")
                {
                    ((IPointListEdit)((LineItem)obj).Points).Add(x, y);
                    zedGraphControl.GraphPane.AxisChange();
                    zedGraphControl.Invalidate();
                }
            }
        }

        ///<summary>
        ///Add multiple points to the point list
        ///</summary>
        public void PointListAddMultipleData(string name, List<double> xList, List<double> yList)
        {
            if (zedGraphControl.GraphPane != null)
            {
                int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
                if (objIndex >= 0)
                {
                    var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                    if (obj.GetType().Name == "LineItem")
                    {
                        lock (xList)
                        {
                            lock (yList)
                            {
                                var list = (IPointListEdit)((LineItem)obj).Points;
                                lock (list)
                                {
                                    for (int i = 0; i < xList.Count(); i++)
                                        list.Add(xList[i], yList[i]);
                                }
                            }
                        }

                        zedGraphControl.GraphPane.AxisChange();
                        zedGraphControl.Invalidate();
                    }
                }
            }
        }


        ///<summary>
        ///Update the point list with a list of points
        ///</summary>
        public void PointListUpdateData(string name, List<double> xList, List<double> yList)
        {
            int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                if (obj.GetType().Name == "LineItem")
                {
                    lock (xList)
                    {
                        lock (yList)
                        {
                            var list = (IPointListEdit)((LineItem)obj).Points;
                            list.Clear();
                            for (int i = 0; i < xList.Count(); i++)
                                list.Add(xList[i], yList[i]);
                        }
                    }

                    zedGraphControl.GraphPane.AxisChange();
                    zedGraphControl.Invalidate();
                }
            }
        }

        ///<summary>
        ///Update the point list with a list of points
        ///</summary>
        public void PointListClear(string name)
        {
            int objIndex = zedGraphControl.GraphPane.CurveList.IndexOfTag(name);
            if (objIndex >= 0)
            {
                var obj = zedGraphControl.GraphPane.CurveList[objIndex];

                if (obj.GetType().Name == "LineItem")
                {
                    var list = (IPointListEdit)((LineItem)obj).Points;
                    list.Clear();
                }
            }
        }


        private void SetPointMode(SymbolType type)
        {
            foreach (var elt in zedGraphControl.GraphPane.CurveList)
            {
                LineItem lineItem = (LineItem)elt;
                lineItem.Line.Width = 5;
                lineItem.Line.IsVisible = false;
                lineItem.Symbol.Type = type;
                lineItem.Symbol.Size = 2;
                lineItem.Symbol.Fill.Type = FillType.Solid;
            }

            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetFixedYScale(double min, double max)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.YAxis.Scale.MaxAuto = false;
            myPane.YAxis.Scale.Max = max;
            myPane.YAxis.Scale.MinAuto = false;
            myPane.YAxis.Scale.Min = min;
            myPane.YAxis.Scale.MagAuto = false;
            myPane.YAxis.Scale.FormatAuto = false;
            myPane.YAxis.CrossAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetFixedY2Scale(double min, double max)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.Y2Axis.Scale.MaxAuto = false;
            myPane.Y2Axis.Scale.Max = max;
            myPane.Y2Axis.Scale.MinAuto = false;
            myPane.Y2Axis.Scale.Min = min;
            myPane.Y2Axis.Scale.MagAuto = false;
            myPane.Y2Axis.Scale.FormatAuto = false;
            myPane.Y2Axis.CrossAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetFixedYMin(double min)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.YAxis.Scale.MinAuto = false;
            myPane.YAxis.Scale.Min = min;
            myPane.YAxis.Scale.MagAuto = false;
            myPane.YAxis.Scale.FormatAuto = false;
            myPane.YAxis.CrossAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetFixedXScale(double min, double max)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.XAxis.Scale.MaxAuto = false;
            myPane.XAxis.Scale.Max = max;
            myPane.XAxis.Scale.MinAuto = false;
            myPane.XAxis.Scale.Min = min;
            myPane.XAxis.Scale.MagAuto = false;
            myPane.XAxis.Scale.FormatAuto = false;
            myPane.XAxis.CrossAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetAutoYScale()
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.YAxis.Scale.MaxAuto = true;
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MagAuto = true;
            myPane.YAxis.Scale.FormatAuto = true;
            myPane.YAxis.CrossAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        public void SetAutoXScale()
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.XAxis.Scale.MaxAuto = true;
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MagAuto = true;
            myPane.XAxis.Scale.FormatAuto = true;
            // Scale the axes
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        private void zedGraphControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DoubleClickEventArgs evtArgs = new DoubleClickEventArgs();

            zedGraphControl.GraphPane.ReverseTransform(e.Location, out double xPos, out double yPos);
            evtArgs.xPos = (int)xPos;
            evtArgs.yPos = (int)yPos;
            MouseDbleClick?.Invoke(sender, evtArgs);
        }
    }
        
    public  class PolyObject  : ZedGraph.PolyObj 
    {
        public PointD[] originalPointList;
    }
    
    public class ImageFromMatrix : ZedGraph.ImageObj
    {
        public double width { get; set; }
        public double height { get; set; }
        public double xPos { get; set; }
        public double yPos { get; set; }
        public int pixelSize { get; set; }

        public ImageFromMatrix(Image image, double left, double top, double width, double height) : base (image, left, top, width, height)
        {
        }
    }


    public class DoubleClickEventArgs : EventArgs
    {
        public int xPos { get; set; }
        public int yPos { get; set; }
    }    
}
