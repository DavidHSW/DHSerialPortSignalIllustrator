using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace DHSignalIllustrator
{
    public class DataBuffer
    {
        byte[,] buffer;

        int[] resultBuffer;
        int bufferColumnIndex;
        int bufferRowIndex;
        int bufferColumnSize;
        int bufferRowSize;
        int dataNumPerPackage;
        //bool isSameRegionPeak;

        Object[] objects;
        Form mainForm;
        ChartDrawer drawer;
        DataPersistanceHelper persistanceHelper;
        public int filterMin { get; set; }
        public int filterMax { get; set; }

        public DataBuffer(int bufferRowSize, int bufferColumnSize, string cacheName, ChartDrawer drawer, DHSignalIllustrator mainForm)
        {
            buffer = new byte[bufferRowSize, bufferColumnSize];
            bufferColumnIndex = 0;
            bufferRowIndex = 0;
            dataNumPerPackage = (bufferColumnSize - 1) / 2;
            this.bufferColumnSize = bufferColumnSize;
            this.bufferRowSize = bufferRowSize;

            this.drawer = drawer;
            this.mainForm = mainForm;
            persistanceHelper = new DataPersistanceHelper(cacheName);
            objects = new Object[3];
            filterMin = 3;
            filterMax = 10;
            resultBuffer = new int[dataNumPerPackage + 1];
            //isSameRegionPeak = false;
        }

        public bool add(byte data)
        {
            buffer[bufferRowIndex, bufferColumnIndex++] = data;
            if (bufferColumnIndex >= bufferColumnSize)
            {
                resetColumnIndex();
                bufferRowIndex++;

                if (bufferRowIndex % drawer.drawingStride == 0)//Display points
                {
                    //processResultBuffer();
                    objects[0] = bufferRowIndex;
                    objects[1] = bufferColumnSize - 1;
                    objects[2] = buffer;
                    mainForm.Invoke(drawer.drawPointsDg, objects);
                }

                if (bufferRowIndex == bufferRowSize)//Refresh buffer
                {
                    persistanceHelper.saveBuffer(bufferRowSize, dataNumPerPackage, buffer);
                    bufferRowIndex = 0;
                }

                return true;//Change state to "wait for header one"
            }
            return false;
        }

        public void openFile()
        {
            persistanceHelper.openFile();
        }

        public void closeFile()
        {
            persistanceHelper.closeFile();
            //drawer.clearResultChart();

            startProcess(false);
        }

        public void saveBuffer()
        {
            //Discard the package at bufferRowIndex when the buffer is not filled up but saveBuffer() is called (like clicking stop button).
            persistanceHelper.saveBuffer(bufferRowIndex, dataNumPerPackage, buffer);
            resetBufferIndex();
        }

        public void resetRowIndex()
        {
            bufferRowIndex = 0;
        }

        public void resetColumnIndex()
        {
            bufferColumnIndex = 0;
        }

        public void resetBufferIndex()
        {
            resetColumnIndex();
            resetRowIndex();
        }

        public void setWinWidthAndFilterRange(int[] parameter)
        {
            persistanceHelper.winWidth = parameter[0];
            filterMin = parameter[1];
            filterMax = parameter[2];
            startProcess(true);
        }

        private void startProcess(bool isRefresh)
        {
            drawer.moveCount = 0;
            ThreadStart drawThreadStarter = delegate
            {
                persistanceHelper.processCachedData(dataNumPerPackage, drawer.setNumCount, processSignalBuffer, drawer.updateResultChart, isRefresh);
            };
            new Thread(drawThreadStarter).Start();
        }

        private void processSignalBuffer(int[,] signalBuffer, int row, int col)
        {
            int total, temp;
            int moveValidation = 0;
            int validatioeRecord = 0;

            for (int j = 0; j < col; j++)
            {
                total = 0;
                for (int i = 0; i < row; i++)
                {
                    total += signalBuffer[i,j];//Sum up.
                    if(i == 2)
                    {
                        resultBuffer[j] = signalBuffer[i, j];//Get initial value. The 3rd value of all 5 values.
                    }
                }

                //Calculate background value.
                temp = resultBuffer[j] - (total / row);

                //Filter noise.
                temp = (temp > filterMin && temp < filterMax) ? 1 : 0;
                //temp = (temp < 1000) ? 0 : temp;
                resultBuffer[j] = resultBuffer[j] * temp;

                //Validation check.
                if (temp != 0)
                {
                    moveValidation++;
                    if (moveValidation > validatioeRecord)
                    {
                        validatioeRecord = moveValidation;
                    }
                }
                else
                {
                    moveValidation = 0;
                }
            }

            //Clear unwanted points.
            if (validatioeRecord > 4)
            {
                for (int j = 0; j < col + 1; j++)
                {
                    resultBuffer[j] = 0;
                }
                //isSameRegionPeak = false;
            } else if (validatioeRecord > 0) // && !isSameRegionPeak)
            {
                //isSameRegionPeak = true;
                drawer.moveCount++;
            }

            //Draw points
            //drawer.drawResultPoints(resultBuffer);

            //Cache points for 3D chart.
            drawer.cache3DData(resultBuffer);
        }
    }
}
