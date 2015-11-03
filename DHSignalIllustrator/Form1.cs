using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;
using System.IO;


namespace DHSignalIllustrator
{
    public enum processStatus
    {
        WaitingForHeaderOne,
        WaitingForHeaderTwo,
        WaitingForData
    }

    public partial class Form1 : Form
    {
        //*************configure****************

        const int LINE_NUM = 8;                              //the number of line in chart
        const int BUFFER_RECORD_NUM = 500;                   //the number of buffer rows 
        const int DATA_NUM_PER_PACKAGE = 16;
        const int portReceivedBytesThreshold = 1024;

        const int DISPLAYED_DATA_BIT_NUM = 2 * LINE_NUM;     //the number of bits in one package to be displayed
        const int DATA_BYTES_PER_PACKAGE = DATA_NUM_PER_PACKAGE * 2 + 1;    //"+1" for pressing status byte
        const int drawingStride = portReceivedBytesThreshold / DATA_BYTES_PER_PACKAGE;  //When retrieve data from port, display these data as one point(for rugular displaying). The stride is the number of point in one time retrieving.

        const int MAX_RANGE_X = 100;                         //the display range of x axis 
        const int MARKER_SIZE = 10;
        const int LINE_BORDER_WIDTH = 2;

        //***********************************

        int maxX = 0;               //the x value of the most recent added points
        SerialPort com;             //port
        processStatus status;       //current (customized) status of the port

        byte[] readBuffer;
        byte[,] buffer;             //col:DATA_BYTES_PER_PACKAGE  row: BUFFER_RECORD_NUM
        int bufferColumnIndex; 
        int bufferRowIndex;
        bool closing;               //whether the port is being closing
        bool listening;             //whether the port is listening

        FileStream file;
        StreamWriter sw;

        public Form1()
        {
            InitializeComponent();

            configureUI();

            resetStatus();

            readBuffer = new byte[4096];
            buffer = new byte[BUFFER_RECORD_NUM,DATA_BYTES_PER_PACKAGE];
            closing = false;
            listening = false;

            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.MaximizeBox = false;
        }

        ~Form1()
        {
            if (com.IsOpen)
            {
                stopCom(this, null);
            }
            sw.Close();
            file.Close();
        }

        private void configureUI()
        {
            //chart
            signalChart.Series.Clear();
            signalChart.ChartAreas[0].AxisX.Minimum = 0;
            signalChart.ChartAreas[0].AxisX.Maximum = MAX_RANGE_X - 1;
            signalChart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            signalChart.ChartAreas[0].AxisX.Interval = 20;


            for (int i = 0; i < LINE_NUM; i++)
            {
                Series line = new Series("Channel " + (i + 1).ToString());
                line.MarkerStyle = MarkerStyle.Cross;
                line.MarkerSize = MARKER_SIZE;
                line.MarkerColor = Color.Transparent;
                line.BorderWidth = LINE_BORDER_WIDTH;
                line.ChartType = SeriesChartType.Line;
                line.Points.AddXY(0, 0);
                signalChart.Series.Add(line);
            }

            //combo box
            string[] portsName = SerialPort.GetPortNames();
            foreach (string port in portsName)
            {
                portsList.Items.Add(port);
            }

            //check box
            foreach (Control control in displayGroup.Controls)
            {
                if (control is CheckBox)
                {
                    (control as CheckBox).Checked = true;
                    (control as CheckBox).Enabled = Convert.ToInt32((control as CheckBox).Tag) <= LINE_NUM ? true : false;
                    (control as CheckBox).Visible = Convert.ToInt32((control as CheckBox).Tag) <= LINE_NUM ? true : false;
                }
            }

            //Btn
            stopBtn.Enabled = false;
        }

        private void resetStatus()
        {
            bufferColumnIndex = 0;
            bufferRowIndex = 0;
            status = processStatus.WaitingForHeaderOne;
        }

        public void startToCom(object sender, EventArgs e)
        {
            if (portsList.SelectedItem == null)
            {
                MessageBox.Show("Please choose a port first!");
                return;
            }

            if (com == null || com.PortName != portsList.SelectedItem.ToString())
            {
                com = new SerialPort(portsList.SelectedItem.ToString());
                com.DataReceived += new SerialDataReceivedEventHandler(onDataReceived);
                com.BaudRate = 115200;
                com.DataBits = 8;
                com.ReceivedBytesThreshold = portReceivedBytesThreshold;
            }

            try
            {
                closing = false;
                com.Open();
                com.DiscardInBuffer();
                startBtn.Enabled = false;
                portsList.Enabled = false;
                stopBtn.Enabled = true;

                file = new FileStream("Cached_Data.txt", FileMode.Append);//, FileAccess.Write, FileShare.Write, 4096, true);
                sw = new StreamWriter(file);
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show("The selected port is occupied by other program! Please try another one!");
                com = null;
            }
        }

        public void stopCom(object sender, EventArgs e)
        {
            if (com != null && com.IsOpen) 
            {
                closing = true;
                while(listening)
                {
                    Application.DoEvents();
                }
                com.Close();

                startBtn.Enabled = true;
                portsList.Enabled = true;
                stopBtn.Enabled = false;

                saveBuffer();
                resetStatus();

                sw.Close();
                file.Close();

            }
        }

        private void onDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (closing) return;
            try
            {
                listening = true;
                int readBytes = com.BytesToRead;
                com.Read(readBuffer, 0, readBytes);
                processData(readBuffer, readBytes);

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            finally
            {
                listening = false;
            }
        }

        private void processData(byte[] readBuffer, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (readBuffer[i] == 0x7E)
                {
                    status = processStatus.WaitingForHeaderTwo;
                    bufferColumnIndex = 0;
                }

                //header: Ox7E = 126, Ox45 = 69
                switch (status)
                {
                    case processStatus.WaitingForHeaderOne:
                        if (readBuffer[i] == 0x7E) status = processStatus.WaitingForHeaderTwo;
                        break;
                    case processStatus.WaitingForHeaderTwo:
                        if (readBuffer[i] == 0x45) status = processStatus.WaitingForData;
                        break;
                    case processStatus.WaitingForData:
                        {
                            buffer[bufferRowIndex, bufferColumnIndex++] = readBuffer[i];
                            if (bufferColumnIndex >= DATA_BYTES_PER_PACKAGE)
                            {
                                bufferColumnIndex = 0;
                                bufferRowIndex++;

                                if (bufferRowIndex % drawingStride == 0)
                                {
                                    this.Invoke(new EventHandler(drawPoints));
                                }

                                status = processStatus.WaitingForHeaderOne;

                                if (bufferRowIndex == BUFFER_RECORD_NUM)
                                {
                                    saveBuffer();
                                    bufferRowIndex = 0;
                                }
                            }
                            break;
                        }
                }
            }
        }

        private void drawPoints(object sender, EventArgs e)
        {
            maxX++;
            for (int j = 0; j < LINE_NUM; j++)
            {
                int temp = 0;

                for (int i = bufferRowIndex - drawingStride; i < bufferRowIndex; i++)
                {
                    temp += buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1];
                }

                signalChart.Series[j].Points.AddXY(maxX, temp / drawingStride);

            }

            for (int i = bufferRowIndex - drawingStride; i < bufferRowIndex; i++)
            {
                if (buffer[i, DATA_BYTES_PER_PACKAGE - 1] == 0x01)
                {
                    int count = signalChart.Series[0].Points.Count();
                    for (int j = 0; j < LINE_NUM; j++)
                    {
                        signalChart.Series[j].Points[count - 1].MarkerColor = Color.Red;
                    }
                    break;
                }
            }

            //Shift chart
            if (maxX >= MAX_RANGE_X)
            {
                signalChart.ChartAreas[0].AxisX.Minimum++;
                signalChart.ChartAreas[0].AxisX.Maximum++;

                //Remove the points that can't be displayed
                for(int i = 0; i < LINE_NUM; i++) signalChart.Series[i].Points.RemoveAt(0);
            }

            //    for (int index = bufferRowIndex - drawingStride; index < bufferRowIndex; index++)
            //    {
            //        maxX++;

            //        Display the pressing status (0x01:pressed 0x00:not pressed)
            //        Color color;
            //    if (buffer[index, DATA_BYTES_PER_PACKAGE - 1] == 0x01)
            //    {
            //        color = Color.Red;
            //    }
            //    else
            //    {
            //        color = Color.Transparent;
            //    }

            //    DataPoint point;
            //    if (maxX >= MAX_RANGE_X)
            //    {
            //        for (int i = 0; i < LINE_NUM; i++)
            //        {
            //            point = signalChart.Series[i].Points[0];
            //            point.MarkerColor = color;
            //            point.SetValueXY(maxX, buffer[index, i * 2] * 16 * 16 + buffer[index, i * 2 + 1]);
            //            signalChart.Series[i].Points.Add(point);

            //            Remove the points that can't be displayed
            //                signalChart.Series[i].Points.RemoveAt(0);
            //        }

            //        Shift chart
            //            signalChart.ChartAreas[0].AxisX.Minimum++;
            //        signalChart.ChartAreas[0].AxisX.Maximum++;
            //    }
            //    else
            //    {
            //        for (int i = 0; i < LINE_NUM; i++)
            //        {
            //            point = new DataPoint(maxX, buffer[index, i * 2] * 16 * 16 + buffer[index, i * 2 + 1]);
            //            point.MarkerColor = color;
            //            signalChart.Series[i].Points.Add(point);

            //        }
            //    }

            //    signalChart.ChartAreas[0].RecalculateAxesScale();

            //}
        }

        private void saveBuffer()
        {

            for (int i = 0; i < bufferRowIndex; i++)
            {
                sw.Write("\r\n");
                for (int j = 0; j < DATA_NUM_PER_PACKAGE; j++)
                {
                    sw.Write(buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1]);
                    sw.Write(' ');
                }
                sw.Write(buffer[i, DATA_BYTES_PER_PACKAGE - 1]);
            }

        }

        public void displayLine(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            int lineNum = Convert.ToInt32(checkBox.Tag.ToString()) - 1;
            if (checkBox.Checked)
            {
                signalChart.Series[lineNum].BorderWidth = LINE_BORDER_WIDTH;
                signalChart.Series[lineNum].MarkerSize = MARKER_SIZE;
            }
            else
            {
                signalChart.Series[lineNum].BorderWidth = 0;
                signalChart.Series[lineNum].MarkerSize = 0;
            }
        }
    }
}
