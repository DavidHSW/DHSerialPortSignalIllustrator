using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DHLineChart
{
    public partial class DHLineChart : UserControl
    {
        const int NUM_GRID_LINES = 10;
        //const int NUM_LINES_DEFAULT = 8;
        const int NUM_POINTS_DISPLAY = 200;
        const int LABEL_SPACING_X = 40;
        const int LABEL_SPACING_Y = 30;
        const double CHART_TOP_INSET_RATIO = 1.1;

        int numOfLines;

        //Point to the tail of circular queue.
        int queueTailIndex;

        double maxYAxis;//Must not be zero!
        int maxYIndex;

        //The x axis position of a point in chart. 
        int[] position_x;

        bool[] lineDisplayOption;
        bool[] lineShowMarkerOption;

        //The actual y values of points. Used for calculate y axis position every time when redraw.
        //NOTICE: Use actual value but position like position_x because it will 
        //cause little difference when calculate the round value every time, which makes the position 
        //incorrect.Especially when zooming the window. (Reason: y value is a part of calculation but x is not).
        int[,] values_y;

        Pen[] pens =
            {
                new Pen(Color.LightCoral,2),
                new Pen(Color.Coral,2),
                new Pen(Color.Gold,2),
                new Pen(Color.YellowGreen,2),
                new Pen(Color.RoyalBlue,2),
                new Pen(Color.Turquoise,2),
                new Pen(Color.SlateBlue,2),
                new Pen(Color.Black,2),
            };
        Pen markerPen = new Pen(Color.Red, 3);

        public delegate void drawLineDlg();

        public DHLineChart()
        {
            InitializeComponent();
            maxYAxis = 1.0;

            position_x = new int[NUM_POINTS_DISPLAY];
            lineShowMarkerOption = new bool[NUM_POINTS_DISPLAY];

            //queueTailIndex must be less(<) than NUM_POINTS_DISPLAY
            queueTailIndex = 0;
        }

        private void chartArea_Paint(object sender, PaintEventArgs e)
        {
            drawGrid(e.Graphics);
        }

        private void lineArea_Paint(object sender, PaintEventArgs e)
        {
            updateLine(e.Graphics);
        }

        private void drawGrid(Graphics graphics)
        {
            //for (int i = 0; i < NUM_GRID_LINES; i++)
            //{

            //    //Draw x grid lines.
            //    graphics.DrawLine(Pens.Black, LABEL_SPACING_Y, i * interval_x, this.Size.Width, i * interval_x);

            //    //Draw y grid lines.
            //    graphics.DrawLine(Pens.Black, i * interval_y + LABEL_SPACING_Y, 0, i * interval_y + LABEL_SPACING_Y, this.Size.Height - LABEL_SPACING_X);
            //}
            graphics.DrawLine(Pens.Black, LABEL_SPACING_Y, this.Size.Height - LABEL_SPACING_X, this.Size.Width, this.Size.Height - LABEL_SPACING_X);

            graphics.DrawLine(Pens.Black, LABEL_SPACING_Y, 0, LABEL_SPACING_Y, this.Size.Height - LABEL_SPACING_X);

            //}

            for (int i = 0, y = 0; i < NUM_GRID_LINES; i++)
            {
                y = (int)((this.Size.Height - LABEL_SPACING_X) * (1 - i / (double)NUM_GRID_LINES));
                graphics.DrawLine(Pens.Black, LABEL_SPACING_Y - 5, y, LABEL_SPACING_Y, y);
                graphics.DrawLine(Pens.LightGray, LABEL_SPACING_Y, y, this.Width, y);
            }

            graphics.TranslateTransform(LABEL_SPACING_Y / 2, (this.Height - LABEL_SPACING_X) / 2);
            graphics.RotateTransform(-90);
            graphics.DrawString("Signal", this.Font, Brushes.Black, 25, -20);//LABEL_SPACING_Y / 2 - 10, (this.Height - LABEL_SPACING_X - 90) / 2, new StringFormat(StringFormatFlags.DirectionVertical));
            graphics.ResetTransform();
            graphics.DrawString("Point", this.Font, Brushes.Black, this.Width - LABEL_SPACING_Y - 30, this.Height - LABEL_SPACING_X + 10);

        }

        private void updateLine(Graphics graphics)
        {
            //Used for index y position.
            int startIndex = queueTailIndex;
            int endIndex = (queueTailIndex + 1) % NUM_POINTS_DISPLAY;

            //Used for index x position.
            int xIndex = 0;

            while (endIndex != queueTailIndex)
            {
                //Draw lines;
                for (int i = 0, x1, y1, x2, y2; i < numOfLines; i++)
                {
                    if (!lineDisplayOption[i]) continue;

                    x1 = calculateXPosition(xIndex);
                    y1 = calculateYPosition(values_y[i, startIndex]);
                    x2 = calculateXPosition(xIndex + 1);
                    y2 = calculateYPosition(values_y[i, endIndex]);
                    graphics.DrawLine(pens[i], x1, y1, x2, y2);

                    if (lineShowMarkerOption[endIndex]) graphics.DrawEllipse(markerPen, x1 - 5, y1 - 5, 10, 10);
                }
                startIndex = endIndex;
                endIndex = ++endIndex % NUM_POINTS_DISPLAY;
                xIndex++;
            }

        }

        private int calculateXPosition(int xIndex)
        {
            return xIndex * (this.Size.Width - LABEL_SPACING_Y) / NUM_POINTS_DISPLAY + LABEL_SPACING_Y;
        }

        private int calculateYPosition(int yValue)
        {
            return Convert.ToInt32((1 - yValue / (maxYAxis * CHART_TOP_INSET_RATIO)) * (this.Size.Height - LABEL_SPACING_X));
        }

        public void setNumOfLines(int num)
        {
            numOfLines = num;
            values_y = new int[num, NUM_POINTS_DISPLAY];
            lineDisplayOption = new bool[num];
            for (int i = 0; i < num; i++)
            {
                lineDisplayOption[i] = true;
                lineShowMarkerOption[i] = false;
            }
        }

        public void setAppearanceOfLine(int index, bool hide)
        {
            lineDisplayOption[index] = hide;
        }

        public void addYValues(int[] yValues, bool showMarker)
        {
            for (int i = 0; i < numOfLines; i++)
            {
                values_y[i, queueTailIndex] = yValues[i];

                if (yValues[i] >= maxYAxis)
                {
                    maxYAxis = yValues[i];
                }
                else if (maxYIndex == queueTailIndex)
                {
                    findAndSetYAxisMax();
                }
            }

            lineShowMarkerOption[queueTailIndex] = showMarker;

            queueTailIndex = ++queueTailIndex % NUM_POINTS_DISPLAY;

            if (lineArea.InvokeRequired)
            {
                drawLineDlg dlg = delegate ()
                {
                    lineArea.Invalidate();
                };
                lineArea.Invoke(dlg);
            }
            else
            {
                lineArea.Invalidate();
            }

        }


        private void findAndSetYAxisMax()
        {
            double s = 0;
            for (int i = 0; i < numOfLines; i++)
            {
                for (int j = 0; j < NUM_POINTS_DISPLAY; j++)
                {
                    if (values_y[i, j] > s)
                    {
                        s = values_y[i, j];
                        maxYIndex = j;
                    }
                }
            }

            maxYAxis = (s != 0) ? (s) : 1.0;
        }
    }
}
