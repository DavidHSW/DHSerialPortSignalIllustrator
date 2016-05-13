using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Runtime.Serialization;

namespace DHSignalIllustrator
{
    /// <summary>
    /// A class representing all the characteristics of the Line
    /// segments that make up a curve on the graph.
    /// </summary>
    /// 
    [Serializable]
    public class LineStyle : ICloneable, ISerializable
    {
        #region Variables

        private DashStyle _mlinePattern = DashStyle.Solid;
        private Color _mlineColor = Color.Black;
        private float _mlineThickness = 1.0f;
        private PlotLinesMethodEnum _mpltLineMethod = PlotLinesMethodEnum.Lines;
        private bool _misVisible = true;
        #endregion

        #region Constructors

        public LineStyle()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// 
        public LineStyle(LineStyle rhs)
        {
            _mlinePattern = rhs._mlinePattern;
            _mlineColor = rhs._mlineColor;
            _mlineThickness = rhs._mlineThickness;
            _mpltLineMethod = rhs._mpltLineMethod;
            _misVisible = rhs._misVisible;
        }
        /// <summary>
        /// Implement the <see cref="ICloneable" /> interface in a typesafe manner by just
        /// calling the typed version of <see cref="Clone" />
        /// </summary>
        /// <returns>A deep copy of this object</returns>
        object ICloneable.Clone()
        {
            return new LineStyle(this);
        }
        #endregion

        #region Properties

        public bool IsVisible
        {
            get { return _misVisible; }
            set { _misVisible = value; }
        }

        public PlotLinesMethodEnum PlotMethod
        {
            get { return _mpltLineMethod; }
            set { _mpltLineMethod = value; }
        }

        virtual public DashStyle Pattern
        {
            get { return _mlinePattern; }
            set { _mlinePattern = value; }
        }

        public float Thickness
        {
            get { return _mlineThickness; }
            set { _mlineThickness = value; }
        }

        virtual public Color LineColor
        {
            get { return _mlineColor; }
            set { _mlineColor = value; }
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
        protected LineStyle(SerializationInfo info, StreamingContext context)
        {
            _mlinePattern = (DashStyle)info.GetValue("linePattern", typeof(DashStyle));
            _mlineColor = (Color)info.GetValue("lineColor", typeof(Color));
            _mlineThickness = info.GetSingle("LineThickness");
            _misVisible = info.GetBoolean("isVisible");
        }
        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> instance with the data needed to serialize the target object
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data</param>
        /// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("linePattern", _mlinePattern);
            info.AddValue("lineColor", _mlineColor);
            info.AddValue("LineThickness", _mlineThickness);
            info.AddValue("isVisible", _misVisible);
        }
        #endregion


        #region Enum

        public enum PlotLinesMethodEnum
        {
            Lines = 0,
            Splines = 1
        }
        #endregion
    }
}


