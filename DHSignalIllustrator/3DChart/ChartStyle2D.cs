using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Runtime.Serialization;

namespace DHSignalIllustrator
{
    /// <summary>
    /// Class that handles the properties of the charting area for 2D(where the curves are
    /// actually drawn).
    /// </summary>
    /// 
   [Serializable]
    public class ChartStyle2D : ICloneable, ISerializable
    {
        #region Variables

        private DHSignalIllustrator form1;
        private Rectangle _mchartArea;
        private Color _mchartBackColor;
        private Color _mchartBorderColor;
        private Color _mplotBackColor = Color.White;
        private Color _mplotBorderColor = Color.Black;
        #endregion

        #region Constructors

        public ChartStyle2D(DHSignalIllustrator fm1)
        {
            form1 = fm1;
            Rectangle rect = form1.ClientRectangle;
            _mchartArea = new Rectangle(rect.X, rect.Y, rect.Width, 3 * rect.Height / 5);
            _mchartBackColor = fm1.BackColor;
            _mchartBorderColor = fm1.BackColor;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// 
        public ChartStyle2D(ChartStyle2D rhs)
        {
            _mchartArea = rhs._mchartArea;
            _mchartBackColor = rhs._mchartBackColor;
            _mchartBorderColor = rhs._mchartBorderColor;
            _mplotBackColor = rhs._mchartBorderColor;
            _mplotBorderColor = rhs._mchartBorderColor;
        }
          /// <summary>
        /// Implement the <see cref="ICloneable" /> interface in a typesafe manner by just
        /// calling the typed version of <see cref="Clone" />
        /// </summary>
        /// <returns>A deep copy of this object</returns>
        object ICloneable.Clone()
        {
            return new ChartStyle2D(this);
        }
        #endregion

        #region Properties

        public Color ChartBackColor
        {
            get { return _mchartBackColor; }
            set { _mchartBackColor = value; }
        }

        public Color ChartBorderColor
        {
            get { return _mchartBorderColor; }
            set { _mchartBorderColor = value; }
        }

        public Rectangle ChartArea
        {
            get { return _mchartArea; }
            set { _mchartArea = value; }
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
        protected ChartStyle2D(SerializationInfo info, StreamingContext context)
        {
            _mchartArea = (Rectangle)info.GetValue("chartArea", typeof(Rectangle));
            _mchartBackColor = (Color)info.GetValue("chartBackColor", typeof(Color));
            _mchartBorderColor = (Color)info.GetValue("chartBorderColor", typeof(Color));
            _mplotBackColor = (Color)info.GetValue("chartBorderColor", typeof(Color));
            _mplotBorderColor = (Color)info.GetValue("chartBorderColor", typeof(Color));
        }
        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> instance with the data needed to serialize the target object
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data</param>
        /// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("chartArea", _mchartArea);
            info.AddValue("chartBackColor", _mchartBackColor);
            info.AddValue("chartBorderColor", _mchartBorderColor);
            info.AddValue("plotBackColor", _mchartBorderColor);
            info.AddValue("plotBorderColor", _mplotBorderColor);
        }
        #endregion

        #region Methods

        public void AddChartStyle2D(Graphics g, ChartStyle cs3d)
        {
            Pen aPen = new Pen(Color.Black, 1f);

            SizeF tickFontSize = g.MeasureString("A", cs3d.TickFont);
            // Create vertical gridlines:
            float fX, fY;
            if (cs3d.IsYGrid == true)
            {
                aPen = new Pen(cs3d.GridStyle.LineColor, 1f);
                aPen.DashStyle = cs3d.GridStyle.Pattern;
                for (fX = cs3d.XMin + cs3d.XTick; fX < cs3d.XMax; fX += cs3d.XTick)
                {
                    g.DrawLine(aPen, Point2D(new PointF(fX, cs3d.YMin), cs3d),
                        Point2D(new PointF(fX, cs3d.YMax), cs3d));
                }
            }

            // Create horizontal gridlines:
            if (cs3d.IsXGrid == true)
            {
                aPen = new Pen(cs3d.GridStyle.LineColor, 1f);
                aPen.DashStyle = cs3d.GridStyle.Pattern;
                for (fY = cs3d.YMin + cs3d.YTick; fY < cs3d.YMax; fY += cs3d.YTick)
                {
                    g.DrawLine(aPen, Point2D(new PointF(cs3d.XMin, fY),cs3d),
                        Point2D(new PointF(cs3d.XMax, fY),cs3d));
                }
            }

            // Create the x-axis tick marks:
            for (fX = cs3d.XMin; fX <= cs3d.XMax; fX += cs3d.XTick)
            {
                PointF yAxisPoint = Point2D(new PointF(fX, cs3d.YMin), cs3d);
                g.DrawLine(Pens.Black, yAxisPoint, new PointF(yAxisPoint.X,
                                   yAxisPoint.Y - 5f));
            }

            // Create the y-axis tick marks:
            for (fY = cs3d.YMin; fY <= cs3d.YMax; fY += cs3d.YTick)
            {
                PointF xAxisPoint = Point2D(new PointF(cs3d.XMin, fY), cs3d);
                g.DrawLine(Pens.Black, xAxisPoint,
                    new PointF(xAxisPoint.X + 5f, xAxisPoint.Y));
            }

            aPen.Dispose();
        }

        private void AddLabels(Graphics g, ChartStyle cs3d)
        {
            float xOffset = ChartArea.Width / 30.0f;
            float yOffset = ChartArea.Height / 30.0f;
            SizeF labelFontSize = g.MeasureString("A", cs3d.LabelFont);
            SizeF titleFontSize = g.MeasureString("A", cs3d.TitleFont);
            SizeF tickFontSize = g.MeasureString("A", cs3d.TickFont);

            SolidBrush aBrush = new SolidBrush(cs3d.TickColor);
            StringFormat sFormat = new StringFormat();

            // Create the x-axis tick marks:
            aBrush = new SolidBrush(cs3d.TickColor);
            for (float fX = cs3d.XMin; fX <= cs3d.XMax; fX += cs3d.XTick)
            {
                PointF yAxisPoint = Point2D(new PointF(fX, cs3d.YMin), cs3d);
                sFormat.Alignment = StringAlignment.Far;
                SizeF sizeXTick = g.MeasureString(fX.ToString(), cs3d.TickFont);
                g.DrawString(fX.ToString(), cs3d.TickFont, aBrush,
                    new PointF(yAxisPoint.X + sizeXTick.Width / 2 + form1.PlotPanel.Left,
                    yAxisPoint.Y + 4f + form1.PlotPanel.Top), sFormat);
            }

            // Create the y-axis tick marks:
            for (float fY = cs3d.YMin; fY <= cs3d.YMax; fY += cs3d.YTick)
            {
                PointF xAxisPoint = Point2D(new PointF(cs3d.XMin, fY), cs3d);
                sFormat.Alignment = StringAlignment.Far;
                g.DrawString(fY.ToString(), cs3d.TickFont, aBrush,
                    new PointF(xAxisPoint.X - 3f + form1.PlotPanel.Left,
                    xAxisPoint.Y - tickFontSize.Height / 2 + form1.PlotPanel.Top), sFormat);
            }

            // Add horizontal axis label:
            aBrush = new SolidBrush(cs3d.LabelColor);
            SizeF stringSize = g.MeasureString(cs3d.XLabel, cs3d.LabelFont);
            g.DrawString(cs3d.XLabel, cs3d.LabelFont, aBrush,
                new Point(form1.PlotPanel.Left + form1.PlotPanel.Width / 2 -
                (int)stringSize.Width / 2, ChartArea.Bottom -
                (int)yOffset - (int)labelFontSize.Height));

            // Add y-axis label:
            sFormat.Alignment = StringAlignment.Center;
            stringSize = g.MeasureString(cs3d.YLabel, cs3d.LabelFont);
            // Save the state of the current Graphics object
            GraphicsState gState = g.Save();
            g.TranslateTransform(xOffset, yOffset + titleFontSize.Height
                + yOffset / 3 + form1.PlotPanel.Height / 2);
            g.RotateTransform(-90);
            g.DrawString(cs3d.YLabel, cs3d.LabelFont, aBrush, 0, 0, sFormat);
            // Restore it:
            g.Restore(gState);

            // Add title:
            aBrush = new SolidBrush(cs3d.TitleColor);
            stringSize = g.MeasureString(cs3d.Title, cs3d.TitleFont);
            if (cs3d.Title.ToUpper() != "NO TITLE")
            {
                g.DrawString(cs3d.Title, cs3d.TitleFont, aBrush,
                    new Point(form1.PlotPanel.Left + form1.PlotPanel.Width / 2 -
                    (int)stringSize.Width / 2, ChartArea.Top + (int)yOffset));
            }
            aBrush.Dispose();
        }

        public void SetPlotArea(Graphics g, ChartStyle cs3d)
        {
            // Draw chart area:
            SolidBrush aBrush = new SolidBrush(ChartBackColor);
            Pen aPen = new Pen(ChartBorderColor, 2);
            g.FillRectangle(aBrush, ChartArea);
            g.DrawRectangle(aPen, ChartArea);

            // Set PlotArea:
            float xOffset = ChartArea.Width / 30.0f;
            float yOffset = ChartArea.Height / 30.0f;
            SizeF labelFontSize = g.MeasureString("A", cs3d.LabelFont);
            SizeF titleFontSize = g.MeasureString("A", cs3d.TitleFont);
            if (cs3d.Title.ToUpper() == "NO TITLE")
            {
                titleFontSize.Width = 8f;
                titleFontSize.Height = 8f;
            }
            float xSpacing = xOffset / 3.0f;
            float ySpacing = yOffset / 3.0f;
            SizeF tickFontSize = g.MeasureString("A", cs3d.TickFont);
            float tickSpacing = 2f;
            SizeF yTickSize = g.MeasureString(cs3d.YMin.ToString(), cs3d.TickFont);
            for (float yTick = cs3d.YMin; yTick <= cs3d.YMax; yTick += cs3d.YTick)
            {
                SizeF tempSize = g.MeasureString(yTick.ToString(), cs3d.TickFont);
                if (yTickSize.Width < tempSize.Width)
                {
                    yTickSize = tempSize;
                }
            }
            float leftMargin = xOffset + labelFontSize.Width +
                        xSpacing + yTickSize.Width + tickSpacing;
            float rightMargin = 2 * xOffset;
            float topMargin = yOffset + titleFontSize.Height + ySpacing;
            float bottomMargin = yOffset + labelFontSize.Height +
                        ySpacing + tickSpacing + tickFontSize.Height;

            // Define the plot area:
            int plotX = ChartArea.X + (int)leftMargin;
            int plotY = ChartArea.Y + (int)topMargin;
            int plotWidth = ChartArea.Width - (int)leftMargin - (int)rightMargin;
            int plotHeight = ChartArea.Height - (int)topMargin - (int)bottomMargin;
            form1.PlotPanel.Left = plotX;
            form1.PlotPanel.Top = plotY;
            if (cs3d.IsColorBar)
                form1.PlotPanel.Width = 25 * plotWidth / 30;
            else
                form1.PlotPanel.Width = plotWidth;
            form1.PlotPanel.Height = plotHeight;
            AddLabels(g, cs3d);
        }

        public PointF Point2D(PointF pt, ChartStyle cs3d)
        {
            PointF aPoint = new PointF();
            if (pt.X < cs3d.XMin || pt.X > cs3d.XMax ||
                pt.Y < cs3d.YMin || pt.Y > cs3d.YMax)
            {
                pt.X = Single.NaN;
                pt.Y = Single.NaN;
            }
            aPoint.X = (pt.X - cs3d.XMin) *
                form1.PlotPanel.Width / (cs3d.XMax - cs3d.XMin);
            aPoint.Y = form1.PlotPanel.Height - (pt.Y - cs3d.YMin) *
                form1.PlotPanel.Height / (cs3d.YMax - cs3d.YMin);
            return aPoint;
        }
        #endregion
    }
}
