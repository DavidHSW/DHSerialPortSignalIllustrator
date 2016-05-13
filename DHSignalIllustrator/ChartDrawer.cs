using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace DHSignalIllustrator
{
    public class ChartDrawer
    {
        //static int maxX = 0; //the x value of the most recently added points

        DHLineChart.DHLineChart signalChart;
        //Chart resultChart;
        DHSignalIllustrator mainForm;
        int[] yValuesBuffer;
        int maxLinesOnChart;
        Int64 numCount;
        public Int64 moveCount { get; set; }
        int index_y = 0;

        public delegate void draw3DChartDlg();
        public delegate void drawPointsDelegate(int bufferRowIndex, int pressStateIndex, byte[,] buffer);
        public drawPointsDelegate drawPointsDg;
        public int drawingStride { get; set; }
       
        //******************3D Chart*************************
        //Define a graphic variable to graphic
        private Graphics gB;
        private Bitmap bitmap, bm;
        //Define instances of class
        private ChartStyle cs;
        private ChartStyle2D cs2d;
        private DataSeries ds;
        private DrawChart dc;
        private ChartFunctions cf;
        private ColorMap cm;

        private int[,] colortype;
        private float azimuth = -37;
        private float elevation = 30;
        private int Th = 45;
        private decimal cmin = 0, cmax = 100;
        Point3[,] pts;
        //************************************

        //public ChartDrawer(int drawingStride, int pointsNumDisplayedInOnePackage, DHLineChart.DHLineChart signalChart, Chart resultChart,DHSignalIllustrator mainForm)
        public ChartDrawer(int drawingStride, int pointsNumDisplayedInOnePackage, DHLineChart.DHLineChart signalChart, DHSignalIllustrator mainForm)
        {
            this.numCount = 0;
            this.moveCount = 0;
            this.drawingStride = drawingStride;
            this.maxLinesOnChart = pointsNumDisplayedInOnePackage;
            this.signalChart = signalChart;
            //this.resultChart = resultChart;
            this.mainForm = mainForm;
            this.drawPointsDg = new drawPointsDelegate(drawPoints);
            this.yValuesBuffer = new int[pointsNumDisplayedInOnePackage];
            signalChart.setNumOfLines(pointsNumDisplayedInOnePackage);

            //***********3D Chart******************
            cs = new ChartStyle(mainForm);
            cs2d = new ChartStyle2D(mainForm);
            ds = new DataSeries();
            dc = new DrawChart(mainForm);
            cf = new ChartFunctions();
            cm = new ColorMap();

            cs.GridStyle.LineColor = Color.LightGray;
            cs.GridStyle.Pattern = DashStyle.Dash;
            cs.Title = "No Title";
            cs.XLabel = "Line";
            cs.YLabel = "Point";
            cs.ZLabel = "Value";

            cs.IsColorBar = true;

            cs.Elevation = elevation;
            cs.Azimuth = azimuth;

            cs2d.ChartBackColor = Color.White;
            cs2d.ChartBorderColor = Color.Black;


            ds.LineStyle.IsVisible = false;
            //ds.LineStyle.IsVisible = true;

            //dc.ChartType = DrawChart.ChartTypeEnum.Mesh;
            //dc.ChartType = DrawChart.ChartTypeEnum.MeshZ;
            //dc.ChartType = DrawChart.ChartTypeEnum.Waterfall;
            dc.ChartType = DrawChart.ChartTypeEnum.Surface;


            //dc.IsColorMap = false;
            dc.IsColorMap = true;
            dc.IsHiddenLine = true;
            //dc.IsHiddenLine = false;

            dc.IsInterp = true;
            dc.NumberInterp = 6;
            dc.CMap = cm.Rainbow(Th);

            //************************************
        }

        public void setNumCount(Int64 numCount)
        {
            this.numCount = numCount;
            cs.XMin = 0;
            cs.XMax = 8;
            cs.YMin = 0;
            cs.YMax = numCount;
            cs.ZMin = 0;
            cs.ZMax = 1;
            cs.XTick = 1;
            cs.YTick = numCount / 5;
            cs.ZTick = 1;
            pts = new Point3[8, numCount];
            index_y = 0;

        }

        public void drawPoints(int bufferRowIndex, int pressStateIndex, byte[,] buffer)
        {
            //Caculate average value
            bool flag = false;

            for (int j = 0; j < maxLinesOnChart; j++)
            {
                yValuesBuffer[j] = 0;
            }

            for (int i = bufferRowIndex - drawingStride; i < bufferRowIndex; i++)
            {
                for (int j = 0; j < maxLinesOnChart; j++)
                {
                    yValuesBuffer[j] += ((buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1]) / drawingStride);
                }
                //Detect press state
                flag = (buffer[i, pressStateIndex] == 0x01) ? true : false;
            }
            signalChart.addYValues(yValuesBuffer, flag);
        }

        //******************Result Chart**********************
        //public void drawResultPoints(int[] resultPoints)
        //{
        //    for (int i = 0; i < 8; i++)
        //    {
        //        resultChart.Series[i].Points.AddY(resultPoints[i]);
        //    }

        //}

        //public void refreshChartScale()
        //{
        //    resultChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(1);
        //    resultChart.ChartAreas[0].AxisX.ScaleView.Zoom((numCount - 100) < 0 ? 0 : numCount - 100, numCount);
        //}

        //public void clearResultChart()
        //{
        //    for(int i = 0; i < resultChart.Series.Count(); i++)
        //    {
        //        resultChart.Series[i].Points.Clear();
        //    }
        //}
        //******************Result Chart**********************

        //*************3D Chart*******************
        private void DrawPlot(int[,] colortype)
        {
            gB.SmoothingMode = SmoothingMode.AntiAlias;
            dc.CMap = colortype;

            //cf.Peak3D(ds, cs);
            //cf.ReadDataFromFile(ds, cs);
            loadData();
            cs.AddChartStyle(gB);
            dc.AddChart(gB, ds, cs, cs2d);
        }
        private void InitialBitmap()
        {
            //Define bitmap
            Size size = new Size(mainForm.PlotPanel.Width, mainForm.PlotPanel.Height);
            bitmap = new Bitmap(size.Width, size.Height);
            gB = Graphics.FromImage(bitmap);
        }
        private void RefreshBackground()
        {
            gB.Clear(mainForm.PlotPanel.BackColor);
            DrawPlot(colortype);

            Size sz = mainForm.PlotPanel.Size;
            Rectangle rt = new Rectangle(0, 0, sz.Width, sz.Height);
            bm = bitmap.Clone(rt, bitmap.PixelFormat);

            //Draw chart on main thread.
            if (mainForm.PlotPanel.InvokeRequired)
            {
                draw3DChartDlg dlg = delegate ()
                {
                    mainForm.PlotPanel.BackgroundImage = bm;
                    updateUI(numCount, moveCount, false);
                    //mainForm.showLoadingLabel(false);
                    //mainForm.refreshCountLabels(numCount, moveCount);
                    //mainForm.setBtnsStandByState();
                };
                mainForm.PlotPanel.Invoke(dlg);
            }
            else
            {
                mainForm.PlotPanel.BackgroundImage = bm;
                updateUI(numCount,moveCount,false);
            }

        }

        private void updateUI(Int64 numCount, Int64 moveCount, bool isShowLoadingLabel)
        {
            mainForm.showLoadingLabel(isShowLoadingLabel);
            mainForm.refreshCountLabels(numCount, moveCount);
            mainForm.setBtnsStandByState();
        }

        public void draw3DChart()
        {
            InitialBitmap();
            Color bckColor = mainForm.PlotPanel.BackColor;
            gB.Clear(bckColor);
            colortype = cm.FallGrYl();
            RefreshBackground();
        }

        public void updateResultChart(bool isDataLoaded)
        {
            if (isDataLoaded)
            {
                draw3DChart();
            }
            else if (mainForm.PlotPanel.InvokeRequired)
            {
                draw3DChartDlg dlg = delegate ()
                {
                    mainForm.prepareToDraw();
                    mainForm.setBtnsStandByState();
                };
                mainForm.PlotPanel.Invoke(dlg);
            }
            else
            {
                mainForm.prepareToDraw();
                mainForm.setBtnsStandByState();
            }
        }

        public void cache3DData(int[] resultPoints)
        {
            for (int x = 0; x < 8; x++)
            {
                int t = resultPoints[x];
                if (t > cs.ZMax)
                {
                    cs.ZMax = t;
                    cs.ZTick = t / 5;
                }
                pts[x, index_y] = new Point3(x, index_y, t, 1);
            }
            index_y++;
        }

        private void loadData()
        {
            ds.PointArray = pts;

            ds.XDataMin = pts[0, 0].X;
            ds.YDataMin = pts[0, 0].Y;
            ds.XSpacing = pts[1, 0].X - pts[0, 0].X;
            ds.YSpacing = pts[0, 1].Y - pts[0, 0].Y;
            ds.XNumber = pts.GetLength(0);
            ds.YNumber = pts.GetLength(1);
        }
        //********************************

    }
}
