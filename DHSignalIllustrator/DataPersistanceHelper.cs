using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DHSignalIllustrator
{
    public class DataPersistanceHelper
    {
        FileStream file;
        FileStream historyFile;
        StreamWriter sw;
        string historyFileName = "History_Data.txt";
        string fileName;

        public int winWidth { get; set; }

        public delegate void processDataWhenStoreDlg(int[,] data,int row,int col);
        public delegate void completionHandlerDlg(bool isDataLoaded);
        public delegate void setNumCountDlg(Int64 numCount);

        public DataPersistanceHelper(string fileName)//name: "Cached_Data.txt"
        {
            this.fileName = fileName;
            this.winWidth = 5;
        }

        public void openFile()
        {
            file = new FileStream(fileName, FileMode.Create);//, FileAccess.Write, FileShare.Write, 4096, true);
            sw = new StreamWriter(file);
        }

        public void closeFile()
        {
            sw.Close();
            file.Close();
        }

        public void saveBuffer(int bufferRowSize, int dataNumPerPackage, byte[,] buffer)
        {
            string temp;
            for (int i = 0; i < bufferRowSize; i++)
            {
                temp = "";
                for (int j = 0; j < dataNumPerPackage; j++)
                {
                    temp += (Convert.ToString(buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1]) + " ");
                }
                temp += (Convert.ToString(buffer[i, dataNumPerPackage * 2]) + "\r\n");
                sw.Write(temp);
            }

        }

        public void processCachedData(int dataNumPerPackage, setNumCountDlg setNumCount, processDataWhenStoreDlg process, completionHandlerDlg doneProcessing, bool isRefresh)
        {
            StreamWriter historySW = new StreamWriter(historyFileName,true);
            StreamReader sr = new StreamReader(fileName);

            int[,] tempBuffer = new int[winWidth, dataNumPerPackage];
            int queueTailIndex = 0;
            Int64 numCount = 0;
            string line;
            string[] nums;
            char[] spToken = new char[1] { ' ' };

            //Calcualte line number;
            while(sr.ReadLine() != null)
            {
                numCount++;
            }

            if (numCount == 0)
            {
                historySW.Close();
                sr.Close();
                doneProcessing(false);
                return;
            }

            setNumCount(numCount);
            sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            sr.BaseStream.Position = 0;
            //numCount = 0;

            while ((line = sr.ReadLine()) != null)
            {
                if (!isRefresh)
                {
                    historySW.WriteLine(line);
                }

                nums = line.Split(spToken);
                for (int j = 0; j < nums.Length - 1; j++)
                {
                    tempBuffer[queueTailIndex, j] = Convert.ToInt32(nums[j]);
                }

                process(tempBuffer, winWidth, dataNumPerPackage);
                queueTailIndex = (queueTailIndex + 1) % winWidth;
            }

            //refreshChartScale();
            doneProcessing(true);
            historySW.Close();
            sr.Close();

        }
    }
}
