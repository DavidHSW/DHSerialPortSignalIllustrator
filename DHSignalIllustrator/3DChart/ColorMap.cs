using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace DHSignalIllustrator
{
    /// <summary>
    /// A class representing all of color type for mapping.
    /// </summary>
    public class ColorMap
    {
        #region Variables

        //private int colormapLength = 100;
        //private int alphaValue = 255;

        private int FieldmapLength = 100;
        private int Alpha = 255;
        #endregion

        #region Constructors

        public ColorMap()
        {
        }

        public ColorMap(int colorLength)
        {
            //colormapLength = colorLength;
            FieldmapLength = colorLength;
        }

        public ColorMap(int colorLength, int alpha)
        {
            //colormapLength = colorLength;
            //alphaValue = alpha;
            FieldmapLength = colorLength;
            Alpha = alpha;
        }
        #endregion

        #region Methods
        public int[,] Hot()
        {
            int[,] cmap = new int[FieldmapLength, 4];
            int n = 3 * FieldmapLength / 8;
            float[] red = new float[FieldmapLength];
            float[] green = new float[FieldmapLength];
            float[] blue = new float[FieldmapLength];
            for (int i = 0; i < FieldmapLength; i++)
            {
                if (i < n)
                    red[i] = 1.0f*(i+1) / n;
                else
                    red[i] = 1.0f;
                if (i < n)
                    green[i] = 0f;
                else if (i >= n && i < 2 * n)
                    green[i] = 1.0f * (i+1 - n) / n;
                else
                    green[i] = 1f;
                if (i < 2 * n)
                    blue[i] = 0f;
                else
                    blue[i] = 1.0f * (i + 1 - 2 * n) / (FieldmapLength - 2 * n);
                cmap[i, 0] = Alpha;
                cmap[i, 1] = (int)(255 * red[i]);
                cmap[i, 2] = (int)(255 * green[i]);
                cmap[i, 3] = (int)(255 * blue[i]);
            }
            return cmap;
        }

        //public int[,] Cool()
        //{
        //    int[,] cmap = new int[colormapLength, 4];
        //    float[] cool = new float[colormapLength];
        //    for (int i = 0; i < colormapLength; i++)
        //    {
        //        cool[i] = 1.0f * i / (colormapLength - 1);
        //        cmap[i, 0] = alphaValue;
        //        cmap[i, 1] = (int)(255 * cool[i]);
        //        cmap[i, 2] = (int)(255 * (1 - cool[i]));
        //        cmap[i, 3] = 255;
        //    }
        //    return cmap;
        //}
        public int[,] DeltaGrRd(decimal Max, decimal Min)
        {
            int[,] Fmap = new int[FieldmapLength, 4];

            decimal frac = FieldmapLength / (Max - Min);
            int Maxlen = (int)(Max * frac);
            //int Minlen = 0;
            //if (Min < 0)
            //    Minlen = (int)(-Min * frac);
            int Minlen = FieldmapLength - Maxlen;
            decimal temp = 1.0M / (Maxlen - 1);
            int n2 = Maxlen / 2;
            int con = 100;
            int con1 = 255 - con;
            for (int i = 0; i < Maxlen; i++)
            {
                Fmap[i, 0] = Alpha;
                if (i < n2)
                {

                    Fmap[FieldmapLength - 1 - i, 2] = con + (int)(con1 * i * 2 * temp);
                    Fmap[FieldmapLength - 1 - i, 1] = Fmap[FieldmapLength - 1 - i, 3] = 0;
                }
                else
                {
                    Fmap[FieldmapLength - 1 - i, 2] = 255;
                    Fmap[FieldmapLength - 1 - i, 1] = (int)(255 * (i - n2) * 2 * temp);
                    Fmap[FieldmapLength - 1 - i, 3] = Fmap[FieldmapLength - 1 - i, 1];
                }
            }

            temp = 1.0M / (Minlen - 1);
            n2 = Minlen / 2;
            con1 = 255 - con;
            int con2 = 0;
            for (int i = Maxlen; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                if (i >= (Maxlen + n2))
                {
                    Fmap[i - Maxlen, 1] = 255;
                    con2 = (int)(255 * (i - Maxlen - n2) * 2 * temp);
                    if (con2 > 255)
                        con2 = 255;
                    Fmap[i - Maxlen, 2] = con2;
                    Fmap[i - Maxlen, 3] = Fmap[i - Maxlen, 2];
                }
                else
                {

                    Fmap[i - Maxlen, 1] = con + (int)(con1 * (i - Maxlen) * 2 * temp);
                    Fmap[i - Maxlen, 3] = Fmap[i - Maxlen, 2] = 0;
                }
            }

            return Fmap;
        }

        public int[,] DeltaGrBl(decimal Max, decimal Min)
        {
            int[,] Fmap = new int[FieldmapLength, 4];

            decimal frac = FieldmapLength / (Max - Min);
            int Maxlen = (int)(Max * frac);
            //int Minlen = 0;
            //if (Min < 0)
            //    Minlen = (int)(-Min * frac);
            int Minlen = FieldmapLength - Maxlen;
            decimal temp = 1.0M / (Maxlen - 1);
            int n2 = Maxlen / 2;
            int Col1 = 100;
            int Col1c = 255 - Col1;
            int tmp = 0;
            for (int i = 0; i < Maxlen; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = Col1 + (int)(Col1c * i * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[FieldmapLength - 1 - i, 2] = tmp;
                tmp = (int)(255 * (i - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[FieldmapLength - 1 - i, 1] = tmp;
                Fmap[FieldmapLength - 1 - i, 3] = Fmap[FieldmapLength - 1 - i, 1];
            }
            int Col2 = 100;
            int Col2c = 255 - Col2;
            temp = 1.0M / (Minlen - 1);
            n2 = Minlen / 2;
            for (int i = Maxlen; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = Col2 + (int)(Col2c * (i - Maxlen) * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[i - Maxlen, 3] = tmp;
                tmp = (int)(255 * (i - Maxlen - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[i - Maxlen, 2] = tmp;
                Fmap[i - Maxlen, 1] = Fmap[i - Maxlen, 2];
            }

            return Fmap;
        }

        public int[,] DeltaRdBl(decimal Max, decimal Min)
        {
            int[,] Fmap = new int[FieldmapLength, 4];

            decimal frac = FieldmapLength / (Max - Min);
            int Maxlen = (int)(Max * frac);
            //int Minlen = 0;
            //if (Min < 0)
            //    Minlen = (int)(-Min * frac);
            int Minlen = FieldmapLength - Maxlen;
            decimal temp = 1.0M / (Maxlen - 1);
            int n2 = Maxlen / 2;
            int con = 150;
            int con1 = 255 - con;
            int tmp = 0;
            for (int i = 0; i < Maxlen; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = con + (int)(con1 * i * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[FieldmapLength - 1 - i, 1] = tmp;
                tmp = (int)(255 * (i - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[FieldmapLength - 1 - i, 2] = tmp;
                Fmap[FieldmapLength - 1 - i, 3] = Fmap[FieldmapLength - 1 - i, 2];
            }

            temp = 1.0M / (Minlen - 1);
            n2 = Minlen / 2;
            for (int i = Maxlen; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = con + (int)(con1 * (i - Maxlen) * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[i - Maxlen, 3] = tmp;
                tmp = (int)(255 * (i - Maxlen - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[i - Maxlen, 2] = tmp;
                Fmap[i - Maxlen, 1] = Fmap[i - Maxlen, 2];
            }

            return Fmap;
        }

        public int[,] DeltaRdGr(decimal Max, decimal Min)
        {
            int[,] Fmap = new int[FieldmapLength, 4];

            decimal frac = FieldmapLength / (Max - Min);
            int Maxlen = (int)(Max * frac);
            //int Minlen = 0;
            //if (Min < 0)
            //    Minlen = (int)(-Min * frac);
            int Minlen = FieldmapLength - Maxlen;
            decimal temp = 1.0M / (Maxlen - 1);
            int n2 = Maxlen / 2;
            int con = 100;
            int con1 = 255 - con;
            int tmp = 0;
            for (int i = 0; i < Maxlen; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = con + (int)(con1 * i * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[FieldmapLength - 1 - i, 1] = tmp;
                tmp = (int)(255 * (i - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[FieldmapLength - 1 - i, 2] = tmp;
                Fmap[FieldmapLength - 1 - i, 3] = Fmap[FieldmapLength - 1 - i, 2];
            }

            temp = 1.0M / (Minlen - 1);
            n2 = Minlen / 2;
            for (int i = Maxlen; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                tmp = con + (int)(con1 * (i - Maxlen) * 2 * temp);
                if (tmp > 255)
                    tmp = 255;
                Fmap[i - Maxlen, 2] = tmp;
                tmp = (int)(255 * (i - Maxlen - n2) * 2 * temp);
                if (tmp < 0)
                    tmp = 0;
                Fmap[i - Maxlen, 1] = tmp;
                Fmap[i - Maxlen, 3] = Fmap[i - Maxlen, 1];
            }

            return Fmap;
        }
        public int[,] Spring()
        {
            //Fuchsia Red1   204,57,123     2
            //Palace Blue7   50,103,173     9
            //Lavender8      116,124,128    7
            //Slate Gray9    138,152,146    5
            //Dark Citron4   157,170,74     4
            //Vibrant Green5 85,170,86      8
            //Super Lemon6   232,193,63     3
            //Lucite Green3  125,207,182    6

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 7;
            double fac = 150;
            int n = (int)(FieldmapLength / (float)Ncol);
            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if (Math.Abs(diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }
            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            double fac1 = ((182 - 63) / (float)num[0]);
            double fac2 = (86 - 63) / (float)num[1];
            double fac3 = (86 - 74) / (float)num[2];
            double fac4 = (146 - 74) / (float)num[3];
            double fac5 = (146 - 128) / (float)num[4];
            double fac6 = (173 - 128) / (float)num[5];
            double fac7 = (173 - 123) / (float)num[6];

            Blue[0, 0] = 182; Blue[0, 1] = -1; Blue[0, 2] = fac1;
            Blue[1, 0] = 63; Blue[1, 1] = 1; Blue[1, 2] = fac2;
            Blue[2, 0] = 86; Blue[2, 1] = -1; Blue[2, 2] = fac3;
            Blue[3, 0] = 74; Blue[3, 1] = 1; Blue[3, 2] = fac4;
            Blue[4, 0] = 146; Blue[4, 1] = -1; Blue[4, 2] = fac5;
            Blue[5, 0] = 128; Blue[5, 1] = 1; Blue[5, 2] = fac6;
            Blue[6, 0] = 173; Blue[6, 1] = -1; Blue[6, 2] = fac7;

            fac1 = ((207 - 193) / (float)num[0]);
            fac2 = (193 - 170) / (float)num[1];
            fac3 = (170 - 152) / (float)num[2];
            fac4 = (152 - 124) / (float)num[3];
            fac5 = (124 - 103) / (float)num[4];
            fac6 = (103 - 57) / (float)num[5];
            fac7 = (138 - 116) / (float)num[6];

            Green[0, 0] = 207; Green[0, 1] = -1; Green[0, 2] = fac1;
            Green[1, 0] = 193; Green[1, 1] = -1; Green[1, 2] = fac2;
            Green[2, 0] = 170; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 170; Green[3, 1] = -1; Green[3, 2] = fac3;
            Green[4, 0] = 152; Green[4, 1] = -1; Green[4, 2] = fac4;
            Green[5, 0] = 124; Green[5, 1] = -1; Green[5, 2] = fac5;
            Green[6, 0] = 103; Green[6, 1] = -1; Green[6, 2] = fac6;

            fac3 = (232 - 125) / (float)num[0];
            fac4 = (232 - 85) / (float)num[1];
            fac5 = (157 - 85) / (float)num[2];
            fac6 = (157 - 138) / (float)num[3];
            fac7 = (138 - 116) / (float)num[4];
            fac1 = (116 - 50) / (float)num[5];
            fac2 = (204 - 50) / (float)num[6];
            Red[0, 0] = 125; Red[0, 1] = 1; Red[0, 2] = fac3;
            Red[1, 0] = 232; Red[1, 1] = -1; Red[1, 2] = fac4;
            Red[2, 0] = 85; Red[2, 1] = 1; Red[2, 2] = fac5;
            Red[3, 0] = 157; Red[3, 1] = -1; Red[3, 2] = fac6;
            Red[4, 0] = 138; Red[4, 1] = -1; Red[4, 2] = fac7;
            Red[5, 0] = 116; Red[5, 1] = -1; Red[5, 2] = fac1;
            Red[6, 0] = 50; Red[6, 1] = 1; Red[6, 2] = fac2;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;

        }

        public int[,] Jet()
        {
            //Dark Red  120,0,0
            //Red       255,0,0
            //Yellow    255,255,0
            //Teal      0,255,255
            //Blue      0,0,255
            //Dark Blue 0,0,120

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 5;
            double fac = 120;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac1 = ((255 - fac) / (float)num[2]);
            double fac3 = (255) / (float)num[4];

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 0; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 0; Red[1, 1] = 0; Red[1, 2] = 0;
            Red[2, 0] = 0; Red[2, 1] = 1; Red[2, 2] = fac3;
            Red[3, 0] = 255; Red[3, 1] = 0; Red[3, 2] = 0;
            Red[4, 0] = 255; Red[4, 1] = -1; Red[4, 2] = fac1;

            fac1 = ((255) / (float)num[1]);
            fac3 = (255) / (float)num[3];

            Green[0, 0] = 0; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 0; Green[1, 1] = 1; Green[1, 2] = fac1;
            Green[2, 0] = 255; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 255; Green[3, 1] = -1; Green[3, 2] = fac3;
            Green[4, 0] = 0; Green[4, 1] = 0; Green[4, 2] = 0;

            fac1 = ((255 - fac) / (float)num[0]);
            fac3 = (255) / (float)num[2];

            Blue[0, 0] = fac; Blue[0, 1] = 1; Blue[0, 2] = fac1;
            Blue[1, 0] = 255; Blue[1, 1] = 0; Blue[1, 2] = 0;
            Blue[2, 0] = 255; Blue[2, 1] = -1; Blue[2, 2] = fac3;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 0; Blue[4, 1] = 0; Blue[4, 2] = 0;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] Mix4()
        {
            //Dark Red 200,0,0
            //Red      255,0,0 
            //orange   255,128,0
            //yellow   255,255,0
            //Green    0,128,0

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 6;
            double fac3 = 200;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac2 = ((255) / (float)num[3]);
            double fac4 = ((255 - fac3) / (float)num[5]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 0; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 0; Red[1, 1] = 0; Red[1, 2] = 0;
            Red[2, 0] = 0; Red[2, 1] = 0; Red[2, 2] = 0;
            Red[3, 0] = 0; Red[3, 1] = 1; Red[3, 2] = fac2;
            Red[4, 0] = 255; Red[4, 1] = 0; Red[4, 2] = 0;
            Red[5, 0] = 255; Red[5, 1] = -1; Red[5, 2] = fac4;

            fac2 = ((255) / (float)num[1]);
            fac4 = ((255) / (float)num[4]);

            Green[0, 0] = 0; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 0; Green[1, 1] = 1; Green[1, 2] = fac2;
            Green[2, 0] = 255; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 255; Green[3, 1] = 0; Green[3, 2] = 0;
            Green[4, 0] = 255; Green[4, 1] = -1; Green[4, 2] = fac4;
            Green[5, 0] = 0; Green[5, 1] = 0; Green[5, 2] = 0;

            fac2 = ((255) / (float)num[2]);
            fac4 = ((255 - fac3) / (float)num[0]);

            Blue[0, 0] = fac3; Blue[0, 1] = 1; Blue[0, 2] = fac4;
            Blue[1, 0] = 255; Blue[1, 1] = 0; Blue[1, 2] = 0;
            Blue[2, 0] = 255; Blue[2, 1] = -1; Blue[2, 2] = fac2;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 0; Blue[4, 1] = 0; Blue[4, 2] = 0;
            Blue[5, 0] = 0; Blue[5, 1] = 0; Blue[5, 2] = 0;
            double fac = 0.0;
            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] Mix3()
        {
            //Dark Red 200,0,0
            //Red      255,0,0 
            //orange   255,128,0
            //yellow   255,255,0
            //Green    0,128,0

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 6;
            double fac3 = 200;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac2 = ((255) / (float)num[3]);
            double fac4 = ((255 - fac3) / (float)num[5]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 0; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 0; Red[1, 1] = 0; Red[1, 2] = 0;
            Red[2, 0] = 0; Red[2, 1] = 0; Red[2, 2] = 0;
            Red[3, 0] = 0; Red[3, 1] = 1; Red[3, 2] = fac2;
            Red[4, 0] = 255; Red[4, 1] = 0; Red[4, 2] = 0;
            Red[5, 0] = 255; Red[5, 1] = -1; Red[5, 2] = fac4;

            fac2 = ((255) / (float)num[1]);
            fac4 = ((255) / (float)num[4]);

            Green[0, 0] = 0; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 0; Green[1, 1] = 1; Green[1, 2] = fac2;
            Green[2, 0] = 255; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 255; Green[3, 1] = 0; Green[3, 2] = 0;
            Green[4, 0] = 255; Green[4, 1] = -1; Green[4, 2] = fac2;
            Green[5, 0] = 0; Green[5, 1] = 0; Green[5, 2] = 0;

            fac2 = ((255) / (float)num[2]);
            fac4 = ((255 - fac3) / (float)num[0]);

            Blue[0, 0] = fac3; Blue[0, 1] = 1; Blue[0, 2] = fac4;
            Blue[1, 0] = 255; Blue[1, 1] = 0; Blue[1, 2] = 0;
            Blue[2, 0] = 255; Blue[2, 1] = -1; Blue[2, 2] = fac2;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 0; Blue[4, 1] = 0; Blue[4, 2] = 0;
            Blue[5, 0] = 0; Blue[5, 1] = 0; Blue[5, 2] = 0;
            double fac = 0.0;
            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] FallGrYl()
        {
            //Dark Green 0,51,0
            //Green    153,204,0
            //Red      255,204,0 
            //Yello    255,255,0
            //White   255,255,255

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 4;
            double fac1 = 153;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac4 = ((255 - fac1) / (float)num[2]);
            double fac6 = ((fac1) / (float)num[3]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 255; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 255; Red[1, 1] = 0; Red[1, 2] = 0;
            Red[2, 0] = 255; Red[2, 1] = -1; Red[2, 2] = fac4;
            Red[3, 0] = fac1; Red[3, 1] = -1; Red[3, 2] = fac6;

            fac4 = ((255 - 204) / (float)num[1]);
            fac6 = ((fac1) / (float)num[3]);

            Green[0, 0] = 255; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 255; Green[1, 1] = -1; Green[1, 2] = fac4;
            Green[2, 0] = 204; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 204; Green[3, 1] = -1; Green[3, 2] = fac6;

            fac4 = ((255) / (float)num[0]);

            Blue[0, 0] = 255; Blue[0, 1] = -1; Blue[0, 2] = fac4;
            Blue[1, 0] = 0; Blue[1, 1] = 0; Blue[1, 2] = 0;
            Blue[2, 0] = 0; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;

            double fac;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] FallRdGr()
        {
            //Dark Red 200,0,0
            //Red      255,0,0 
            //orange   255,128,0
            //yellow   255,255,0
            //Green    0,128,0

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 5;
            double fac = 200;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }
            double fac2 = ((fac) / (float)num[0]);
            double fac3 = ((fac) / (float)num[1]);
            double fac4 = ((255 - fac) / (float)num[4]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 0; Red[0, 1] = 1; Red[0, 2] = fac2;
            Red[1, 0] = 128; Red[1, 1] = 1; Red[1, 2] = fac2;
            Red[2, 0] = 255; Red[2, 1] = 0; Red[2, 2] = 0;
            Red[3, 0] = 255; Red[3, 1] = 0; Red[3, 2] = 0;
            Red[4, 0] = 255; Red[4, 1] = -1; Red[4, 2] = fac4;

            fac2 = ((255 - 128) / (float)num[3]);
            fac3 = ((128) / (float)num[1]);
            fac4 = ((255 - 128) / (float)num[2]);

            Green[0, 0] = 128; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 128; Green[1, 1] = 1; Green[1, 2] = fac3;
            Green[2, 0] = 255; Green[2, 1] = -1; Green[2, 2] = fac4;
            Green[3, 0] = 128; Green[3, 1] = -1; Green[3, 2] = fac2;
            Green[4, 0] = 0; Green[4, 1] = 0; Green[4, 2] = 0;

            Blue[0, 0] = 0; Blue[0, 1] = 0; Blue[0, 2] = 0;
            Blue[1, 0] = 0; Blue[1, 1] = 0; Blue[1, 2] = 0;
            Blue[2, 0] = 0; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 0; Blue[4, 1] = 0; Blue[4, 2] = 0;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] Rainbow(int ind)
        {
            //Red       255,0,0
            //Orange    255,102,0 or 255,165,0
            //Yellow    255,255,0
            //Green     0,127,0
            //Blue      0,0,255
            //Indigo    111,0,255 75,0,130 or 51,51,153 or 
            //Violet    127,0,255 or 238,130,238 or 128,0,128 or 139,0,255 or 143,0,255

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 7;
            double fac = 150;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac2 = ((fac) / (float)num[2]);
            double fac6 = ((255 - 111) / (float)num[1]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 255; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 255; Red[1, 1] = -1; Red[1, 2] = fac6;
            Red[2, 0] = fac; Red[2, 1] = -1; Red[2, 2] = fac2;
            Red[3, 0] = 0; Red[3, 1] = 0; Red[3, 2] = 0;
            Red[4, 0] = 255; Red[4, 1] = 0; Red[4, 2] = 0;
            Red[5, 0] = 255; Red[5, 1] = 0; Red[5, 2] = 0;//Orange
            Red[6, 0] = 255; Red[6, 1] = 0; Red[6, 2] = 0;//Red

            fac2 = ((fac) / (float)num[2]);
            double fac3 = ((255 - 102) / (float)num[5]);
            double fac5 = ((255 - 127) / (float)num[3]);
            fac6 = ((fac) / (float)num[6]);

            Green[0, 0] = 0; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 0; Green[1, 1] = 0; Green[1, 2] = 0;
            Green[2, 0] = fac; Green[2, 1] = -1; Green[2, 2] = fac2;
            Green[3, 0] = 255; Green[3, 1] = -1; Green[3, 2] = fac5;
            Green[4, 0] = 255; Green[4, 1] = 0; Green[4, 2] = 0;
            Green[5, 0] = 255; Green[5, 1] = -1; Green[5, 2] = fac3;
            Green[6, 0] = fac; Green[6, 1] = -1; Green[6, 2] = fac6;

            fac2 = ((fac) / (float)num[6]);
            fac3 = ((255 - 102) / (float)num[5]);
            double fac4 = ((175) / (float)num[4]);
            fac5 = ((255 - 127) / (float)num[1]);
            fac6 = ((255 - 111) / (float)num[0]);

            Blue[0, 0] = 255; Blue[0, 1] = -1; Blue[0, 2] = fac5;
            Blue[1, 0] = 255; Blue[1, 1] = -1; Blue[1, 2] = fac6;
            Blue[2, 0] = 255; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 175; Blue[4, 1] = -1; Blue[4, 2] = fac4;
            Blue[5, 0] = 255 - 102; Blue[5, 1] = -1; Blue[5, 2] = fac3;
            Blue[6, 0] = fac; Blue[6, 1] = -1; Blue[6, 2] = fac2;


            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] RainbowOld(int ind)
        {
            //Red       255,0,0
            //Orange    255,102,0 or 255,165,0
            //Yellow    255,255,0
            //Green     0,127,0
            //Blue      0,0,255
            //Indigo    111,0,255 75,0,130 or 51,51,153 or 
            //Violet    127,0,255 or 238,130,238 or 128,0,128 or 139,0,255 or 143,0,255

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 7;
            double fac = 150;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac2 = ((fac) / (float)num[2]);
            double fac6 = ((255 - 111) / (float)num[1]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = 255; Red[0, 1] = 0; Red[0, 2] = 0;
            Red[1, 0] = 255; Red[1, 1] = -1; Red[1, 2] = fac6;
            Red[2, 0] = fac; Red[2, 1] = -1; Red[2, 2] = fac2;
            Red[3, 0] = 0; Red[3, 1] = 0; Red[3, 2] = 0;
            Red[4, 0] = 255; Red[4, 1] = 0; Red[4, 2] = 0;
            Red[5, 0] = 255; Red[5, 1] = 0; Red[5, 2] = 0;//Orange
            Red[6, 0] = 255; Red[6, 1] = 0; Red[6, 2] = 0;//Red

            fac2 = ((fac) / (float)num[2]);
            double fac3 = ((255 - 102) / (float)num[5]);
            double fac5 = ((255 - 127) / (float)num[3]);
            fac6 = ((fac) / (float)num[6]);

            Green[0, 0] = 0; Green[0, 1] = 0; Green[0, 2] = 0;
            Green[1, 0] = 0; Green[1, 1] = 0; Green[1, 2] = 0;
            Green[2, 0] = fac; Green[2, 1] = -1; Green[2, 2] = fac2;
            Green[3, 0] = 255; Green[3, 1] = -1; Green[3, 2] = fac5;
            Green[4, 0] = 255; Green[4, 1] = 0; Green[4, 2] = 0;
            Green[5, 0] = 255; Green[5, 1] = -1; Green[5, 2] = fac3;
            Green[6, 0] = fac; Green[6, 1] = -1; Green[6, 2] = fac6;

            fac2 = ((fac) / (float)num[6]);
            fac3 = ((255 - 102) / (float)num[5]);
            double fac4 = ((175) / (float)num[4]);
            fac5 = ((255 - 127) / (float)num[1]);
            fac6 = ((255 - 111) / (float)num[0]);

            Blue[0, 0] = 255; Blue[0, 1] = -1; Blue[0, 2] = fac5;
            Blue[1, 0] = 255; Blue[1, 1] = -1; Blue[1, 2] = fac6;
            Blue[2, 0] = 255; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 0; Blue[3, 1] = 0; Blue[3, 2] = 0;
            Blue[4, 0] = 175; Blue[4, 1] = -1; Blue[4, 2] = fac4;
            Blue[5, 0] = 255 - 102; Blue[5, 1] = -1; Blue[5, 2] = fac3;
            Blue[6, 0] = fac; Blue[6, 1] = -1; Blue[6, 2] = fac2;


            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }


        public int[,] Mix2()
        {
            //Red       255,0,0
            //Orange    255,165,0 or 255,102,0
            //Yellow    255,255,0
            //Green     0,128,0
            //Blue      0,0,255
            //Indigo    75,0,130 or 51,51,153 or 111,0,255
            //Violet    238,130,238 or 128,0,128 or 139,0,255 or 143,0,255 or 127,0,255

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 10;
            double fac = 150;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac1 = ((255 - 150) / (float)num[0]);
            double fac2 = ((255 - 150) / (float)num[2]);
            double fac3 = ((255 - 150) / (float)num[4]);
            double fac4 = ((255 - 150) / (float)num[9]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = fac; Red[0, 1] = -1; Red[0, 2] = fac1;
            Red[1, 0] = 0; Red[1, 1] = -1; Red[1, 2] = 0;
            Red[2, 0] = fac; Red[2, 1] = -1; Red[2, 2] = fac1;
            Red[3, 0] = 0; Red[3, 1] = -1; Red[3, 2] = 0;
            Red[4, 0] = fac; Red[4, 1] = -1; Red[4, 2] = fac1;
            Red[5, 0] = 0; Red[5, 1] = -1; Red[5, 2] = 0;
            Red[6, 0] = 255; Red[6, 1] = 0; Red[6, 2] = 0;
            Red[7, 0] = 255; Red[7, 1] = 0; Red[7, 2] = 0;
            Red[8, 0] = 255; Red[8, 1] = 0; Red[8, 2] = 0;
            Red[9, 0] = 255; Red[9, 1] = -1; Red[9, 2] = fac1;


            fac1 = ((255 - 150) / (float)num[5]);
            fac3 = ((255 - 150) / (float)num[7]);
            fac2 = (150) / (float)num[0];
            fac4 = (150) / (float)num[8];

            Green[0, 0] = fac; Green[0, 1] = -1; Green[0, 2] = fac2;
            Green[1, 0] = 0; Green[1, 1] = -1; Green[1, 2] = 0;
            Green[2, 0] = 255; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 255; Green[3, 1] = -1; Green[3, 2] = 0;
            Green[4, 0] = 255; Green[4, 1] = 0; Green[4, 2] = 0;
            Green[5, 0] = 255; Green[5, 1] = -1; Green[5, 2] = fac1;
            Green[6, 0] = 255; Green[6, 1] = 0; Green[6, 2] = 0;
            Green[7, 0] = 255; Green[7, 1] = -1; Green[7, 2] = fac3;
            Green[8, 0] = fac; Green[8, 1] = -1; Green[8, 2] = fac4;
            Green[9, 0] = 0; Green[9, 1] = 0; Green[9, 2] = 0;

            fac1 = ((255 - 150) / (float)num[1]);
            fac3 = ((255 - 150) / (float)num[3]);
            fac2 = (150) / (float)num[4];
            fac4 = (150) / (float)num[6];
            double fac5 = (150) / (float)num[8];

            Blue[0, 0] = 255; Blue[0, 1] = -1; Blue[0, 2] = 0;
            Blue[1, 0] = 255; Blue[1, 1] = -1; Blue[1, 2] = fac1;
            Blue[2, 0] = 255; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 255; Blue[3, 1] = -1; Blue[3, 2] = fac3;
            Blue[4, 0] = fac; Blue[4, 1] = -1; Blue[4, 2] = fac2;
            Blue[5, 0] = 0; Blue[5, 1] = -1; Blue[5, 2] = 0;
            Blue[6, 0] = fac; Blue[6, 1] = -1; Blue[6, 2] = fac4;
            Blue[7, 0] = 0; Blue[7, 1] = -1; Blue[7, 2] = 0;
            Blue[8, 0] = fac; Blue[8, 1] = -1; Blue[8, 2] = fac5;
            Blue[9, 0] = 0; Blue[9, 1] = 0; Blue[9, 2] = 0;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[FieldmapLength - 1 - i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] Mix1()
        {
            //Red       255,0,0
            //Orange    255,165,0 or 255,102,0
            //Yellow    255,255,0
            //Green     0,128,0
            //Blue      0,0,255
            //Indigo    75,0,130 or 51,51,153 or 111,0,255
            //Violet    238,130,238 or 128,0,128 or 139,0,255 or 143,0,255 or 127,0,255

            int[,] Fmap = new int[FieldmapLength, 4];
            int Ncol = 10;
            double fac = 150;
            int n = (int)(FieldmapLength / (float)Ncol);

            int[] num = new int[Ncol];
            int diff = FieldmapLength - n * Ncol;
            for (int i = 0; i < Ncol; i++)
            {
                num[i] = n;
                if ((diff) > Ncol - i - 1)
                    num[i] = n + 1;
            }

            double fac1 = ((255 - 150) / (float)num[0]);
            double fac2 = ((255 - 150) / (float)num[2]);
            double fac3 = ((255 - 150) / (float)num[4]);
            double fac4 = ((255 - 150) / (float)num[9]);

            double[,] Red = new double[Ncol, 3];
            double[,] Green = new double[Ncol, 3];
            double[,] Blue = new double[Ncol, 3];

            Red[0, 0] = fac; Red[0, 1] = -1; Red[0, 2] = fac1;
            Red[1, 0] = 0; Red[1, 1] = -1; Red[1, 2] = 0;
            Red[2, 0] = fac; Red[2, 1] = -1; Red[2, 2] = fac1;
            Red[3, 0] = 0; Red[3, 1] = -1; Red[3, 2] = 0;
            Red[4, 0] = fac; Red[4, 1] = -1; Red[4, 2] = fac1;
            Red[5, 0] = 0; Red[5, 1] = -1; Red[5, 2] = 0;
            Red[6, 0] = 255; Red[6, 1] = 0; Red[6, 2] = 0;
            Red[7, 0] = 255; Red[7, 1] = 0; Red[7, 2] = 0;
            Red[8, 0] = 255; Red[8, 1] = 0; Red[8, 2] = 0;
            Red[9, 0] = 255; Red[9, 1] = -1; Red[9, 2] = fac1;


            fac1 = ((255 - 150) / (float)num[5]);
            fac3 = ((255 - 150) / (float)num[7]);
            fac2 = (150) / (float)num[0];
            fac4 = (150) / (float)num[8];

            Green[0, 0] = fac; Green[0, 1] = -1; Green[0, 2] = fac2;
            Green[1, 0] = 0; Green[1, 1] = -1; Green[1, 2] = 0;
            Green[2, 0] = 255; Green[2, 1] = 0; Green[2, 2] = 0;
            Green[3, 0] = 255; Green[3, 1] = -1; Green[3, 2] = 0;
            Green[4, 0] = 255; Green[4, 1] = 0; Green[4, 2] = 0;
            Green[5, 0] = 255; Green[5, 1] = -1; Green[5, 2] = fac1;
            Green[6, 0] = 255; Green[6, 1] = 0; Green[6, 2] = 0;
            Green[7, 0] = 255; Green[7, 1] = -1; Green[7, 2] = fac3;
            Green[8, 0] = fac; Green[8, 1] = -1; Green[8, 2] = fac4;
            Green[9, 0] = 0; Green[9, 1] = 0; Green[9, 2] = 0;

            fac1 = ((255 - 150) / (float)num[1]);
            fac3 = ((255 - 150) / (float)num[3]);
            fac2 = (150) / (float)num[4];
            fac4 = (150) / (float)num[6];
            double fac5 = (150) / (float)num[8];

            Blue[0, 0] = 255; Blue[0, 1] = -1; Blue[0, 2] = 0;
            Blue[1, 0] = 255; Blue[1, 1] = -1; Blue[1, 2] = fac1;
            Blue[2, 0] = 255; Blue[2, 1] = 0; Blue[2, 2] = 0;
            Blue[3, 0] = 255; Blue[3, 1] = -1; Blue[3, 2] = fac3;
            Blue[4, 0] = fac; Blue[4, 1] = -1; Blue[4, 2] = fac2;
            Blue[5, 0] = 0; Blue[5, 1] = -1; Blue[5, 2] = 0;
            Blue[6, 0] = fac; Blue[6, 1] = -1; Blue[6, 2] = fac4;
            Blue[7, 0] = 0; Blue[7, 1] = -1; Blue[7, 2] = 0;
            Blue[8, 0] = fac; Blue[8, 1] = -1; Blue[8, 2] = fac5;
            Blue[9, 0] = 0; Blue[9, 1] = 0; Blue[9, 2] = 0;

            int st = 0;
            int stop = 0;
            for (int j = 0; j < Ncol; j++)
            {
                st = stop;
                stop = st + num[j];
                for (int i = st; i < stop; i++)
                {
                    Fmap[i, 0] = Alpha;
                    fac = Red[j, 0] + Red[j, 1] * Red[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 1] = (int)fac;
                    fac = Green[j, 0] + Green[j, 1] * Green[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 2] = (int)fac;
                    fac = Blue[j, 0] + Blue[j, 1] * Blue[j, 2] * (i - st);
                    if (fac > 255)
                        fac = 255;
                    if (fac < 0)
                        fac = 0;
                    Fmap[i, 3] = (int)fac;
                }
            }

            return Fmap;
        }

        public int[,] Hot2()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            int n = FieldmapLength / 3;
            int fac = 255 / n;

            for (int i = 0; i < FieldmapLength; i++)
            {
                if (i < n)
                    Fmap[FieldmapLength - 1 - i, 1] = (i + 1) * fac;
                else
                    Fmap[FieldmapLength - 1 - i, 1] = 255;
                if (i < n)
                    Fmap[FieldmapLength - 1 - i, 2] = 0;
                else if (i >= n && i < 2 * n)
                    Fmap[FieldmapLength - 1 - i, 2] = (i + 1 - n) * fac;
                else
                    Fmap[FieldmapLength - 1 - i, 2] = 255;
                if (i < 2 * n)
                    Fmap[FieldmapLength - 1 - i, 3] = 0;
                else
                {
                    int te = fac * (i + 1 - 2 * n);
                    if (te > 255)
                        te = 255;
                    Fmap[FieldmapLength - 1 - i, 3] = te;
                }
                Fmap[i, 0] = Alpha;
            }
            return Fmap;
        }
        public int[,] Hot1()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            int n = FieldmapLength / 3;
            int fac = 255 / n;

            for (int i = 0; i < FieldmapLength; i++)
            {
                if (i < n)
                    Fmap[i, 1] = (i + 1) * fac;
                else
                    Fmap[i, 1] = 255;
                if (i < n)
                    Fmap[i, 2] = 0;
                else if (i >= n && i < 2 * n)
                    Fmap[i, 2] = (i + 1 - n) * fac;
                else
                    Fmap[i, 2] = 255;
                if (i < 2 * n)
                    Fmap[i, 3] = 0;
                else
                {
                    int te = fac * (i + 1 - 2 * n);
                    if (te > 255)
                        te = 255;
                    Fmap[i, 3] = te;
                }
                Fmap[i, 0] = Alpha;
            }
            return Fmap;
        }

        public int[,] Cool2(int Red)
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float cool;
            for (int i = 0; i < FieldmapLength; i++)
            {
                cool = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = Red;
                Fmap[i, 2] = (int)(255 * cool);
                Fmap[i, 3] = (int)(255 * (1 - cool));
            }
            return Fmap;
        }
        public int[,] Cool22(int Red)
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float cool;

            for (int i = 0; i < FieldmapLength; i++)
            {
                cool = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = Red;
                Fmap[i, 2] = (int)(255 * (1 - cool));
                Fmap[i, 3] = (int)(255 * cool);
            }
            return Fmap;
        }
        public int[,] Cool1(int Red)
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float cool;
            for (int i = 0; i < FieldmapLength; i++)
            {
                cool = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = (int)(255 * cool);
                Fmap[i, 2] = Red;
                Fmap[i, 3] = (int)(255 * (1 - cool));
            }
            return Fmap;
        }
        public int[,] Cool11(int Red)
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float cool;
            for (int i = 0; i < FieldmapLength; i++)
            {
                cool = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = (int)(255 * (1 - cool));
                Fmap[i, 2] = Red;
                Fmap[i, 3] = (int)(255 * cool);
            }
            return Fmap;
        }
        public int[,] Cool()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float cool;
            for (int i = 0; i < FieldmapLength; i++)
            {
                cool = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = (int)(255 * cool);
                Fmap[i, 2] = (int)(255 * (1 - cool));
                Fmap[i, 3] = 255;
            }
            return Fmap;
        }

        public int[,] Summer()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float summer;
            for (int i = 0; i < FieldmapLength; i++)
            {
                summer = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = (int)(255 * summer);
                Fmap[i, 2] = (int)(255 * 0.5f * (1 + summer));
                Fmap[i, 3] = (int)(255 * 0.4f);
            }
            return Fmap;
        }

        public int[,] Autumn()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            for (int i = 0; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = 255;
                Fmap[i, 2] = (int)(255 * i * temp);
                Fmap[i, 3] = 0;
            }
            return Fmap;
        }

        public int[,] Winter()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            float winter;
            for (int i = 0; i < FieldmapLength; i++)
            {
                winter = temp * i;
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = 0;
                Fmap[i, 2] = (int)(255 * winter);
                Fmap[i, 3] = (int)(255 * (1.0f - 0.5f * winter));
            }
            return Fmap;
        }

        public int[,] Gray()
        {
            int[,] Fmap = new int[FieldmapLength, 4];
            float temp = 1.0f / (FieldmapLength - 1);
            for (int i = 0; i < FieldmapLength; i++)
            {
                Fmap[i, 0] = Alpha;
                Fmap[i, 1] = (int)(255 * i * temp);
                Fmap[i, 2] = Fmap[i, 3] = Fmap[i, 1];
            }
            return Fmap;
        }

        #endregion
    }
       
}


