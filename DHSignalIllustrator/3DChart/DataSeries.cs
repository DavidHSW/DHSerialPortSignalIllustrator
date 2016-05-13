using System;
using System.Collections;
using System.Drawing;
using System.Runtime.Serialization;

namespace DHSignalIllustrator
{
    /// <summary>
    /// A class provide Arraylist to handler the data pint,
    /// and calculate the max and min value of data points
    /// </summary>
    /// 
    [Serializable]
    public class DataSeries : ISerializable
    {
        #region Variables

        private ArrayList _mpointList;
        private LineStyle _mlineStyle;
        private float _mxdataMin = -5;
        private float _mydataMin = -5;
        private float _mzzdataMin = -5;
        private float _mxSpacing = 1;
        private float _mySpacing = 1;
        private float _mzSpacing = 1;
        private int _mxNumber = 10;
        private int _myNumber = 10;
        private int _mzNumber = 10;
        private Point3[,] _mpointArray;
        private Point4[, ,] _mpoint4Array;
        #endregion

        #region Constructors

        public DataSeries()
        {
            _mlineStyle = new LineStyle();
            _mpointList = new ArrayList();
        }
        #endregion

        #region Properties

        public Point4[, ,] Point4Array
        {
            get { return _mpoint4Array; }
            set { _mpoint4Array = value; }
        }

        public Point3[,] PointArray
        {
            get { return _mpointArray; }
            set { _mpointArray = value; }
        }

        public int XNumber
        {
            get { return _mxNumber; }
            set { _mxNumber = value; }
        }

        public int YNumber
        {
            get { return _myNumber; }
            set { _myNumber = value; }
        }

        public int ZNumber
        {
            get { return _mzNumber; }
            set { _mzNumber = value; }
        }

        public float XSpacing
        {
            get { return _mxSpacing; }
            set { _mxSpacing = value; }
        }

        public float YSpacing
        {
            get { return _mySpacing; }
            set { _mySpacing = value; }
        }

        public float ZSpacing
        {
            get { return _mzSpacing; }
            set { _mzSpacing = value; }
        }

        public float XDataMin
        {
            get { return _mxdataMin; }
            set { _mxdataMin = value; }
        }

        public float YDataMin
        {
            get { return _mydataMin; }
            set { _mydataMin = value; }
        }

        public float ZZDataMin
        {
            get { return _mzzdataMin; }
            set { _mzzdataMin = value; }
        }

        public LineStyle LineStyle
        {
            get { return _mlineStyle; }
            set { _mlineStyle = value; }
        }

        public ArrayList PointList
        {
            get { return _mpointList; }
            set { _mpointList = value; }
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
        protected DataSeries(SerializationInfo info, StreamingContext context)
        {
            _mpointList = (ArrayList)info.GetValue("pointList", typeof(ArrayList));
            _mlineStyle = (LineStyle)info.GetValue("lineStyle", typeof(LineStyle));
            _mxdataMin = info.GetSingle("xdataMin");
            _mydataMin = info.GetSingle("ydataMin");
            _mzzdataMin = info.GetSingle("zzdataMin");
            _mxSpacing = info.GetSingle("xSpacing");
            _mySpacing = info.GetSingle("ySpacing");
            _mzSpacing = info.GetSingle("zSpacing");
            _mxNumber = info.GetInt32("xNumber");
            _myNumber = info.GetInt32("yNumber");
            _mzNumber = info.GetInt32("zNumber");
        }
        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> instance with the data needed to serialize the target object
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data</param>
        /// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("pointList", _mpointList);
            info.AddValue("lineStyle", _mlineStyle);
            info.AddValue("xdataMin", _mxdataMin);
            info.AddValue("ydataMin", _mydataMin);
            info.AddValue("zzdataMin", _mzzdataMin);
            info.AddValue("xSpacing", _mxSpacing);
            info.AddValue("ySpacing", _mySpacing);
            info.AddValue("zSpacing", _mzSpacing);
            info.AddValue("xNumber", _mxNumber);
            info.AddValue("yNumber", _myNumber);
            info.AddValue("zNumber", _mzNumber);
        }
        #endregion

        #region Methods

        public void AddPoint(Point3 pt)
        {
            PointList.Add(pt);
        }

        public float ZDataMin()
        {
            float zmin = 0;
            for (int i = 0; i < PointArray.GetLength(0); i++)
            {
                for (int j = 0; j < PointArray.GetLength(1); j++)
                {
                    zmin = Math.Min(zmin, PointArray[i, j].Z);
                }
            }
            return zmin;
        }

        public float ZDataMax()
        {
            float zmax = 0;
            for (int i = 0; i < PointArray.GetLength(0); i++)
            {
                for (int j = 0; j < PointArray.GetLength(1); j++)
                {
                    zmax = Math.Max(zmax, PointArray[i, j].Z);
                }
            }
            return (zmax == 0) ? 1f : zmax;
        }

        public float VDataMin()
        {
            float vmin = 0;
            for (int i = 0; i < Point4Array.GetLength(0); i++)
            {
                for (int j = 0; j < Point4Array.GetLength(1); j++)
                {
                    for (int k = 0; k < Point4Array.GetLength(2); k++)
                    {
                        vmin = Math.Min(vmin, Point4Array[i, j, k]._mV);
                    }
                }
            }
            return vmin;
        }

        public float VDataMax()
        {
            float vmax = 0;
            for (int i = 0; i < Point4Array.GetLength(0); i++)
            {
                for (int j = 0; j < Point4Array.GetLength(1); j++)
                {
                    for (int k = 0; k < Point4Array.GetLength(2); k++)
                    {
                        vmax = Math.Max(vmax, Point4Array[i, j, k]._mV);
                    }
                }
            }
            return vmax;
        }
        #endregion
    }
}

