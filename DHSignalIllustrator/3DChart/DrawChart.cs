using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace DHSignalIllustrator
{
    /// <summary>
    /// A class handler all methods for drawing cuver on the chart area
    /// </summary>
    /// 
    [Serializable]
    public class DrawChart : ISerializable
    {
        #region Variables

        private DHSignalIllustrator form1;
        private ChartTypeEnum _mchartType;
        private int[,] cmap;
        private bool _misColorMap = true;
        private bool _misHiddenLine = false;
        private bool _misInterp = false;
        private int _mnumberInterp = 2;
        private int _mnumberContours = 10;
        #endregion 

        #region Enum

        public enum ChartTypeEnum
        {
            Line,
            Mesh,
            MeshZ,
            Waterfall,
            Surface,
        }
        #endregion

        #region Constructors

        public DrawChart(DHSignalIllustrator fm1)
        {
            form1 = fm1;
        }
        #endregion

        #region Properties

        public int NumberContours
        {
            get { return _mnumberContours; }
            set { _mnumberContours = value; }
        }

        public int NumberInterp
        {
            get { return _mnumberInterp; }
            set { _mnumberInterp = value; }
        }

        public bool IsInterp
        {
            get { return _misInterp; }
            set { _misInterp = value; }
        }

        public bool IsColorMap
        {
            get { return _misColorMap; }
            set { _misColorMap = value; }
        }

        public bool IsHiddenLine
        {
            get { return _misHiddenLine; }
            set { _misHiddenLine = value; }
        }

        public int[,] CMap
        {
            get { return cmap; }
            set { cmap = value; }
        }

        public ChartTypeEnum ChartType
        {
            get { return _mchartType; }
            set { _mchartType = value; }
        }
        #endregion

        #region Serialization

        /// <summary>
        /// Constructor for deserializing objects
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data
        /// </param>
        /// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data
        /// </param>
        /// 
        protected DrawChart(SerializationInfo info, StreamingContext context)
        {
            _mchartType = (ChartTypeEnum)info.GetValue("chartType", typeof(ChartTypeEnum));
            _misColorMap = info.GetBoolean("isColorMap");
            _misHiddenLine = info.GetBoolean("isHiddenLine");
            _misInterp = info.GetBoolean("isInterp");
            _mnumberInterp = info.GetInt32("numberInterp");
            _mnumberContours = info.GetInt32("numberContours");
        }
         /// <summary>
        /// Populates a <see cref="SerializationInfo"/> instance with the data needed to serialize the target object
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data</param>
        /// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("chartType", _mchartType);
            info.AddValue("isColorMap", _misColorMap);
            info.AddValue("isHiddenLine", _misHiddenLine);
            info.AddValue("isInterp", _misInterp);
            info.AddValue("numberInterp", _mnumberInterp);
            info.AddValue("numberContours", _mnumberContours);

        }
        #endregion

        #region Methods

        public void AddChart(Graphics g, DataSeries ds, ChartStyle cs, ChartStyle2D cs2d)
        {
            switch (ChartType)
            {
                case ChartTypeEnum.Line:
                    AddLine(g, ds, cs);
                    break;
                case ChartTypeEnum.Mesh:
                    AddMesh(g, ds, cs);
                    AddColorBar(g, ds, cs, cs2d);
                    break;
                case ChartTypeEnum.MeshZ:
                    AddMeshZ(g, ds, cs);
                    AddColorBar(g, ds, cs, cs2d);
                    break;
                case ChartTypeEnum.Waterfall:
                    AddWaterfall(g, ds, cs);
                    AddColorBar(g, ds, cs, cs2d);
                    break;
                case ChartTypeEnum.Surface:
                    AddSurface(g, ds, cs, cs2d);
                    AddColorBar(g, ds, cs, cs2d);
                    break;
           }
        }

        private void AddLine(Graphics g, DataSeries ds, ChartStyle cs)
        {
            Pen aPen = new Pen(ds.LineStyle.LineColor, ds.LineStyle.Thickness);
            aPen.DashStyle = ds.LineStyle.Pattern;
            Matrix3 m = Matrix3.AzimuthElevation(cs.Elevation, cs.Azimuth);
            Point3[] pts = new Point3[ds.PointList.Count];

            // Find zmin and zmax values, then perform transformation on points:
            float zmin = 0;
            float zmax = 0;
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = (Point3)ds.PointList[i];
                zmin = Math.Min(zmin, pts[i].Z);
                zmax = Math.Max(zmax, pts[i].Z);
                pts[i].Transform(m, form1, cs);
            }

            // Draw line:
            if (ds.LineStyle.IsVisible == true)
            {
                for (int i = 1; i < pts.Length; i++)
                {
                    Color color = AddColor(cs, pts[i], zmin, zmax);
                    if (IsColorMap)
                    {
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawLine(aPen, pts[i - 1].X, pts[i - 1].Y, pts[i].X, pts[i].Y);
                }
            }
            aPen.Dispose();
        }

        private void AddMesh(Graphics g, DataSeries ds, ChartStyle cs)
        {
            Pen aPen = new Pen(ds.LineStyle.LineColor, ds.LineStyle.Thickness);
            aPen.DashStyle = ds.LineStyle.Pattern;
            SolidBrush aBrush = new SolidBrush(Color.White);
            Matrix3 m = Matrix3.AzimuthElevation(cs.Elevation, cs.Azimuth);
            PointF[] pta = new PointF[4];
            Point3[,] pts = ds.PointArray;

            // Find the minumum and maximum z values:
            float zmin = ds.ZDataMin();
            float zmax = ds.ZDataMax();
            
            // Perform transformations on points:
            for (int i = 0; i < pts.GetLength(0); i++)
            {
                for (int j = 0; j < pts.GetLength(1); j++)
                {
                    pts[i, j].Transform(m, form1, cs);
                }
            }

            // Draw color mesh:
            for (int i = 0; i < pts.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < pts.GetLength(1) - 1; j++)
                {
                    int ii = i;
                    if (cs.Azimuth >= -180 && cs.Azimuth < 0)
                    {
                        ii = pts.GetLength(0) - 2 - i;
                    }
                    pta[0] = new PointF(pts[ii, j].X, pts[ii, j].Y);
                    pta[1] = new PointF(pts[ii, j + 1].X, pts[ii, j + 1].Y);
                    pta[2] = new PointF(pts[ii + 1, j + 1].X, pts[ii + 1, j + 1].Y);
                    pta[3] = new PointF(pts[ii + 1, j].X, pts[ii + 1, j].Y);
                    if (!IsHiddenLine)
                    {
                        g.FillPolygon(aBrush, pta);
                    }
                    if (IsColorMap)
                    {
                        Color color = AddColor(cs, pts[ii, j], zmin, zmax);
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawPolygon(aPen, pta);
                }
            }
            aPen.Dispose();
            aBrush.Dispose();
        }

        private void AddMeshZ(Graphics g, DataSeries ds, ChartStyle cs)
        {
            Pen aPen = new Pen(ds.LineStyle.LineColor, ds.LineStyle.Thickness);
            aPen.DashStyle = ds.LineStyle.Pattern;
            SolidBrush aBrush = new SolidBrush(Color.White);
            Matrix3 m = Matrix3.AzimuthElevation(cs.Elevation, cs.Azimuth);
            PointF[] pta = new PointF[4];
            Point3[,] pts = ds.PointArray;
            Point3[,] pts1 = new Point3[pts.GetLength(0), pts.GetLength(1)];
            Color color;

            // Find the minumum and maximum z values:
            float zmin = ds.ZDataMin();
            float zmax = ds.ZDataMax();

            for (int i = 0; i < pts.GetLength(0); i++)
            {
                for (int j = 0; j < pts.GetLength(1); j++)
                {
                    // Make a deep copy the points array:
                    pts1[i, j] = new Point3(pts[i, j].X, pts[i, j].Y, pts[i, j].Z, 1);
                    // Perform transformations on points:
                    pts[i, j].Transform(m, form1, cs);
                }
            }
            //Draw mesh using Z-order method:
            for (int i = 0; i < pts.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < pts.GetLength(1) - 1; j++)
                {
                    int ii = i;
                    if (cs.Azimuth >= -180 && cs.Azimuth < 0)
                    {
                        ii = pts.GetLength(0) - 2 - i;
                    }
                    pta[0] = new PointF(pts[ii, j].X, pts[ii, j].Y);
                    pta[1] = new PointF(pts[ii, j + 1].X, pts[ii, j + 1].Y);
                    pta[2] = new PointF(pts[ii + 1, j + 1].X, pts[ii + 1, j + 1].Y);
                    pta[3] = new PointF(pts[ii + 1, j].X, pts[ii + 1, j].Y);
                    g.FillPolygon(aBrush, pta);
                    if (IsColorMap)
                    {
                        color = AddColor(cs, pts[ii, j], zmin, zmax);
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawPolygon(aPen, pta);
                }
            }
            //Draw cyrtain lines
            Point3[] pt3 = new Point3[4];
            for (int i = 0; i < pts1.GetLength(0); i++)
            {
                int jj = pts1.GetLength(0) - 1;
                if (cs.Elevation >= 0)
                {
                    if (cs.Azimuth >= -90 && cs.Azimuth <= 90)
                        jj = 0;
                }
                else if (cs.Elevation < 0)
                {
                    jj = 0;
                    if (cs.Azimuth >= -90 && cs.Azimuth <= 90)
                        jj = pts1.GetLength(0) - 1;
                }

                if (i < pts1.GetLength(0) - 1)
                {
                    pt3[0] = new Point3(pts1[i, jj].X, pts1[i, jj].Y, pts1[i, jj].Z, 1);
                    pt3[1] = new Point3(pts1[i + 1, jj].X, pts1[i + 1, jj].Y, pts1[i + 1, jj].Z, 1);
                    pt3[2] = new Point3(pts1[i + 1, jj].X, pts1[i + 1, jj].Y, cs.ZMin, 1);
                    pt3[3] = new Point3(pts1[i, jj].X, pts1[i, jj].Y, cs.ZMin, 1);
                    for (int k = 0; k < 4; k++)
                        pt3[k].Transform(m, form1, cs);
                    pta[0] = new PointF(pt3[0].X, pt3[0].Y);
                    pta[1] = new PointF(pt3[1].X, pt3[1].Y);
                    pta[2] = new PointF(pt3[2].X, pt3[2].Y);
                    pta[3] = new PointF(pt3[3].X, pt3[3].Y);
                    g.FillPolygon(aBrush, pta);
                    if (IsColorMap)
                    {
                        color = AddColor(cs, pt3[0], zmin, zmax);
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawPolygon(aPen, pta);
                }
            }
            for (int j = 0; j < pts1.GetLength(1); j++)
            {
                int ii = 0;
                if (cs.Elevation >= 0)
                {
                    if (cs.Azimuth >= 0 && cs.Azimuth <= 180)
                        ii = pts1.GetLength(1) - 1;
                }
                else if (cs.Elevation < 0)
                {
                    if (cs.Azimuth >= -180 && cs.Azimuth <= 0)
                        ii = pts1.GetLength(1) - 1;
                }
                if (j < pts1.GetLength(1) - 1)
                {
                    pt3[0] = new Point3(pts1[ii, j].X, pts1[ii, j].Y, pts1[ii, j].Z, 1);
                    pt3[1] = new Point3(pts1[ii, j + 1].X, pts1[ii, j + 1].Y, pts1[ii, j + 1].Z, 1);
                    pt3[2] = new Point3(pts1[ii, j + 1].X, pts1[ii, j + 1].Y, cs.ZMin, 1);
                    pt3[3] = new Point3(pts1[ii, j].X, pts1[ii, j].Y, cs.ZMin, 1);
                    for (int k = 0; k < 4; k++)
                        pt3[k].Transform(m, form1, cs);
                    pta[0] = new PointF(pt3[0].X, pt3[0].Y);
                    pta[1] = new PointF(pt3[1].X, pt3[1].Y);
                    pta[2] = new PointF(pt3[2].X, pt3[2].Y);
                    pta[3] = new PointF(pt3[3].X, pt3[3].Y);
                    g.FillPolygon(aBrush, pta);
                    if (IsColorMap)
                    {
                        color = AddColor(cs, pt3[0], zmin, zmax);
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawPolygon(aPen, pta);
                }
            }
            aPen.Dispose();
            aBrush.Dispose();
        }

        private void AddWaterfall(Graphics g, DataSeries ds, ChartStyle cs)
        {
            Pen aPen = new Pen(ds.LineStyle.LineColor, ds.LineStyle.Thickness);
            aPen.DashStyle = ds.LineStyle.Pattern;
            SolidBrush aBrush = new SolidBrush(Color.White);
            Matrix3 m = Matrix3.AzimuthElevation(cs.Elevation, cs.Azimuth);
            Point3[,] pts = ds.PointArray;
            Point3[] pt3 = new Point3[pts.GetLength(0) + 2];
            PointF[] pta = new PointF[pts.GetLength(0) + 2];
            Color color;

            // Find the minumum and maximum z values:
            float zmin = ds.ZDataMin();
            float zmax = ds.ZDataMax();

            for (int j = 0; j < pts.GetLength(1); j++)
            {
                int jj = j;
                if (cs.Elevation >= 0)
                {
                    if (cs.Azimuth >= -90 && cs.Azimuth < 90)
                    {
                        jj = pts.GetLength(1) - 1 - j;
                    }
                }
                else if (cs.Elevation < 0)
                {
                    jj = pts.GetLength(1) - 1 - j;
                    if (cs.Azimuth >= -90 && cs.Azimuth < 90)
                    {
                        jj = j;
                    }
                }
                for (int i = 0; i < pts.GetLength(0); i++)
                {
                    pt3[i + 1] = pts[i, jj];
                    if (i == 0)
                    {
                        pt3[0] = new Point3(pt3[i + 1].X, pt3[i + 1].Y, cs.ZMin, 1);
                    }
                    if (i == pts.GetLength(0) - 1)
                    {
                        pt3[pts.GetLength(0) + 1] = new Point3(pt3[i + 1].X,
                            pt3[i + 1].Y, cs.ZMin, 1);
                    }
                }

                for (int i = 0; i < pt3.Length; i++)
                {
                    pt3[i].Transform(m, form1, cs);
                    pta[i] = new PointF(pt3[i].X, pt3[i].Y);
                }
                g.FillPolygon(aBrush, pta);

                for (int i = 1; i < pt3.Length; i++)
                {
                    if (IsColorMap)
                    {
                        color = AddColor(cs, pt3[i], zmin, zmax);
                        aPen = new Pen(color, ds.LineStyle.Thickness);
                        aPen.DashStyle = ds.LineStyle.Pattern;
                    }
                    g.DrawLine(aPen, pta[i - 1], pta[i]);
                }
            } 
            aPen.Dispose();
            aBrush.Dispose();
        }

        private void AddSurface(Graphics g, DataSeries ds, ChartStyle cs, ChartStyle2D cs2d)
        {
            Pen aPen = new Pen(ds.LineStyle.LineColor, ds.LineStyle.Thickness);
            aPen.DashStyle = ds.LineStyle.Pattern;
            SolidBrush aBrush = new SolidBrush(Color.White);
            Matrix3 m = Matrix3.AzimuthElevation(cs.Elevation, cs.Azimuth);
            PointF[] pta = new PointF[4];
            Point3[,] pts = ds.PointArray;
            Point3[,] pts1 = new Point3[pts.GetLength(0), pts.GetLength(1)];

            // Find the minumum and maximum z values:
            float zmin = ds.ZDataMin();
            float zmax = ds.ZDataMax();

            // Perform transformation on points:
            for (int i = 0; i < pts.GetLength(0); i++)
            {
                for (int j = 0; j < pts.GetLength(1); j++)
                {
                    // Make a deep copy the points array:
                    pts1[i, j] = new Point3(pts[i, j].X, pts[i, j].Y, pts[i, j].Z, 1);
                    // Perform transformation on points:
                    pts[i, j].Transform(m, form1, cs);
                }
            }

            // Draw surface:
            if (!IsInterp)
            {
                for (int i = 0; i < pts.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < pts.GetLength(1) - 1; j++)
                    {
                        int ii = i;
                        if (cs.Azimuth >= -180 && cs.Azimuth < 0)
                        {
                            ii = pts.GetLength(0) - 2 - i;
                        }
                        pta[0] = new PointF(pts[ii, j].X, pts[ii, j].Y);
                        pta[1] = new PointF(pts[ii, j + 1].X, pts[ii, j + 1].Y);
                        pta[2] = new PointF(pts[ii + 1, j + 1].X, pts[ii + 1, j + 1].Y);
                        pta[3] = new PointF(pts[ii + 1, j].X, pts[ii + 1, j].Y);
                        Color color = AddColor(cs, pts[ii, j], zmin, zmax);
                        aBrush = new SolidBrush(color);
                        g.FillPolygon(aBrush, pta);
                        if (ds.LineStyle.IsVisible)
                        {
                            g.DrawPolygon(aPen, pta);
                        }
                    }
                }
            }

            // Draw refined surface:
            else if (IsInterp)
            {
               for (int i = 0; i < pts.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < pts.GetLength(1) - 1; j++)
                    {
                        int ii = i;
                        if (cs.Azimuth >= -180 && cs.Azimuth < 0)
                         {
                             ii = pts.GetLength(0) - 2 - i;
                         }
                        Point3[] points = new Point3[4];

                        points[0] = pts1[ii, j];
                        points[1] = pts1[ii, j + 1];
                        points[2] = pts1[ii + 1, j + 1];
                        points[3] = pts1[ii + 1, j];

                        Interp(g, cs, cs2d, m, points, zmin, zmax, 1);

                        pta[0] = new PointF(pts[ii, j].X, pts[ii, j].Y);
                        pta[1] = new PointF(pts[ii, j + 1].X, pts[ii, j + 1].Y);
                        pta[2] = new PointF(pts[ii + 1, j + 1].X, pts[ii + 1, j + 1].Y);
                        pta[3] = new PointF(pts[ii + 1, j].X, pts[ii + 1, j].Y);

                        if (ds.LineStyle.IsVisible)
                        {
                            g.DrawPolygon(aPen, pta);
                        }
                    }
                }
            }
        }



        public void AddColorBar(Graphics g, DataSeries ds, ChartStyle cs, ChartStyle2D cs2d)
        {
            if (cs.IsColorBar && IsColorMap)
            {
                Pen aPen = new Pen(Color.Black, 1);
                SolidBrush aBrush = new SolidBrush(cs.TickColor);
                StringFormat sFormat = new StringFormat();
                sFormat.Alignment = StringAlignment.Near;
                SizeF size = g.MeasureString("A", cs.TickFont);

                int x, y, width, height;
                Point3[] pts = new Point3[64];
                PointF[] pta = new PointF[4];
                float zmin, zmax;
                zmin = ds.ZDataMin();
                zmax = ds.ZDataMax();
                float dz = (zmax - zmin) / 63;
                x = 5 * form1.PlotPanel.Width / 6;
                y = form1.PlotPanel.Height / 10;
                width = form1.PlotPanel.Width / 25;
                height = 8 * form1.PlotPanel.Height / 10;

                // Add color bar:
                for (int i = 0; i < 64; i++)
                {
                    pts[i] = new Point3(x, y, zmin + i * dz, 1);
                }
                for (int i = 0; i < 63; i++)
                {
                    Color color = AddColor(cs, pts[i], zmin, zmax);
                    aBrush = new SolidBrush(color);
                    float y1 = y + height - (pts[i].Z - zmin) * height / (zmax - zmin);
                    float y2 = y + height - (pts[i + 1].Z - zmin) * height / (zmax - zmin);
                    pta[0] = new PointF(x, y2);
                    pta[1] = new PointF(x + width, y2);
                    pta[2] = new PointF(x + width, y1);
                    pta[3] = new PointF(x, y1);
                    g.FillPolygon(aBrush, pta);
                }
                g.DrawRectangle(aPen, x, y, width, height);

                // Add ticks and labels to the color bar:
                float ticklength = 0.1f * width;
                for (float z = zmin; z <= zmax; z = z + (zmax - zmin) / 6)
                {
                    float yy = y + height - (z - zmin) * height / (zmax - zmin);
                    g.DrawLine(aPen, x, yy, x + ticklength, yy);
                    g.DrawLine(aPen, x + width, yy, x + width - ticklength, yy);
                    g.DrawString((Math.Round(z, 2)).ToString(), cs.TickFont, Brushes.Black,
                        new PointF(x + width + 5, yy - size.Height / 2), sFormat);
                }
            }
        }

        private Color AddColor(ChartStyle cs, Point3 pt,
            float zmin, float zmax)
        {
            int colorLength = CMap.GetLength(0);
            int cindex = (int)Math.Round((colorLength * (pt.Z - zmin) +
                        (zmax - pt.Z)) / (zmax - zmin));
            if (cindex < 1)
                cindex = 1;
            if (cindex > colorLength)
                cindex = colorLength;
            Color color = Color.FromArgb(CMap[cindex - 1, 0],
                CMap[cindex - 1, 1], CMap[cindex - 1, 2],
                CMap[cindex - 1, 3]);
            return color;
        }


        private void Interp(Graphics g, ChartStyle cs, ChartStyle2D cs2d, Matrix3 m,
             Point3[] pta, float zmin, float zmax, int flag)
        {
            SolidBrush aBrush = new SolidBrush(Color.Black);
            PointF[] points = new PointF[4];
            int npoints = NumberInterp;
            Point3[,] pts = new Point3[npoints + 1, npoints + 1];
            Point3[,] pts1 = new Point3[npoints + 1, npoints + 1];
            float x0 = pta[0].X;
            float y0 = pta[0].Y;
            float x1 = pta[2].X;
            float y1 = pta[2].Y;
            float dx = (x1 - x0) / npoints;
            float dy = (y1 - y0) / npoints;
            float C00 = pta[0].Z;
            float C10 = pta[3].Z;
            float C11 = pta[2].Z;
            float C01 = pta[1].Z;
            float x, y, C;
            Color color;

            if(flag == 1) // For Surface chart
            {
                for (int i = 0; i <= npoints; i++)
                {
                    x = x0 + i * dx;
                    for (int j = 0; j <= npoints; j++)
                    {
                        y = y0 + j * dy;
                        C = (y1 - y) * ((x1 - x) * C00 +
                            (x - x0) * C10) / (x1 - x0) / (y1 - y0) +
                            (y - y0) * ((x1 - x) * C01 +
                            (x - x0) * C11) / (x1 - x0) / (y1 - y0);
                        pts[i, j] = new Point3(x, y, C, 1);
                        pts[i, j].Transform(m, form1, cs);
                    }
                }

                for (int i = 0; i < npoints; i++)
                {
                    for (int j = 0; j < npoints; j++)
                    {

                        color = AddColor(cs, pts[i, j], zmin, zmax);
                        aBrush = new SolidBrush(color);
                        points[0] = new PointF(pts[i, j].X, pts[i, j].Y);
                        points[1] = new PointF(pts[i + 1, j].X, pts[i + 1, j].Y);
                        points[2] = new PointF(pts[i + 1, j + 1].X, pts[i + 1, j + 1].Y);
                        points[3] = new PointF(pts[i, j + 1].X, pts[i, j + 1].Y);
                        g.FillPolygon(aBrush, points);
                        aBrush.Dispose();
                    }
                }
            }
            else if (flag == 2) // For XYColor chart
            {
                for (int i = 0; i <= npoints; i++)
                {
                    x = x0 + i * dx;
                    for (int j = 0; j <= npoints; j++)
                    {
                        y = y0 + j * dy;
                        C = (y1 - y) * ((x1 - x) * C00 +
                            (x - x0) * C10) / (x1 - x0) / (y1 - y0) +
                            (y - y0) * ((x1 - x) * C01 +
                            (x - x0) * C11) / (x1 - x0) / (y1 - y0);
                        pts[i, j] = new Point3(x, y, C, 1);
                    }
                }

                for (int i = 0; i < npoints; i++)
                {
                    for (int j = 0; j < npoints; j++)
                    {

                        color = AddColor(cs, pts[i, j], zmin, zmax);
                        aBrush = new SolidBrush(color);
                        points[0] = cs2d.Point2D(new PointF(pts[i, j].X, pts[i, j].Y), cs);
                        points[1] = cs2d.Point2D(new PointF(pts[i + 1, j].X, pts[i + 1, j].Y), cs);
                        points[2] = cs2d.Point2D(new PointF(pts[i + 1, j + 1].X, pts[i + 1, j + 1].Y), cs);
                        points[3] = cs2d.Point2D(new PointF(pts[i, j + 1].X, pts[i, j + 1].Y), cs);
                        g.FillPolygon(aBrush, points);
                        aBrush.Dispose();
                    }
                }
            }
            else if(flag == 3)  // For XYColor3D chart
            {
                for (int i = 0; i <= npoints; i++)
                {
                    x = x0 + i * dx;
                    for (int j = 0; j <= npoints; j++)
                    {
                        y = y0 + j * dy;
                        C = (y1 - y) * ((x1 - x) * C00 +
                            (x - x0) * C10) / (x1 - x0) / (y1 - y0) +
                            (y - y0) * ((x1 - x) * C01 +
                            (x - x0) * C11) / (x1 - x0) / (y1 - y0);
                        pts1[i, j] = new Point3(x, y, C, 1);
                        pts[i, j] = new Point3(x, y, cs.ZMin, 1);
                        pts[i, j].Transform(m, form1, cs);
                    }
                }

                for (int i = 0; i < npoints; i++)
                {
                    for (int j = 0; j < npoints; j++)
                    {

                        color = AddColor(cs, pts1[i, j], zmin, zmax);
                        aBrush = new SolidBrush(color);
                        points[0] = new PointF(pts[i, j].X, pts[i, j].Y);
                        points[1] = new PointF(pts[i + 1, j].X, pts[i + 1, j].Y);
                        points[2] = new PointF(pts[i + 1, j + 1].X, pts[i + 1, j + 1].Y);
                        points[3] = new PointF(pts[i, j + 1].X, pts[i, j + 1].Y);
                        g.FillPolygon(aBrush, points);
                        aBrush.Dispose();
                    }
                }
            }
        }
        #endregion
    }
}
