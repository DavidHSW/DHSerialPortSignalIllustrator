using System;

namespace DHSignalIllustrator
{
    /// <summary>
    /// A class representing a data point be indentify by x, y and z value
    /// </summary>
    public class Point4
    {
        #region Variables

        public Point3 _mpoint3 = new Point3();
        public float _mV = 0;
        #endregion

        #region COnstructors

        public Point4()
        {
        }

        public Point4(Point3 pt3, float v)
        {
            _mpoint3 = pt3;
            _mV = v;
        }
        #endregion
    }
}
