﻿using System;
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
        //*************config****************

        const int LINE_NUM = 2;                        //the number of line in chart
        const int MAX_RANGE_X = 10;                    //the display range of x axis 
        const int DATA_BIT_NUM = 2 * LINE_NUM + 1;     //the number of bits in one package
        const int BUFFER_RECORD_NUM = 3;               //the number of buffer rows 
        const int MARKER_SIZE = 10;
        const int LINE_BORDER_WIDTH = 2; 

        //***********************************

        int maxX = 0;             //the x value of the most recent added points
        SerialPort com;           //port
        processStatus status;       //current (customized) status of the port

        byte[,] buffer;           //col:DATA_BIT_NUM  row: BUFFER_RECORD_NUM
        int bufferColumnIndex;
        int bufferRowIndex;

        public Form1()
        {
            InitializeComponent();

            configureUI();

            resetStatus();

            buffer = new byte[BUFFER_RECORD_NUM,DATA_BIT_NUM];

        }

        ~Form1()
        {
            if (com.IsOpen)
            {
                com.Close();
            }
        }

        private void configureUI()
        {
            //chart
            signalChart.Series.Clear();
            signalChart.ChartAreas[0].AxisX.Minimum = 0;
            signalChart.ChartAreas[0].AxisX.Maximum = MAX_RANGE_X - 1;
            signalChart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            signalChart.ChartAreas[0].AxisX.Title = "Unit 1";
            signalChart.ChartAreas[0].AxisY.Title = "Unit 2";
            signalChart.ChartAreas[0].AxisX.Interval = 0.5;
            //signalChart.ChartAreas[0].AxisX.LabelStyle.Format = "F2";


            for (int i = 0; i < LINE_NUM; i++)
            {
                Series line = new Series("Chanel " + (i + 1).ToString());
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

        private void addPoints(object sender, EventArgs e)
        {
            maxX++;

            //Display the pressing status (0x01:pressed 0x00:not pressed)
            Color color;
            if (buffer[bufferRowIndex - 1, DATA_BIT_NUM - 1] == 0x01)
            {
                color = Color.Red;
            }
            else
            {
                color = Color.Transparent;
            }

            //Draw points on chart
            DataPoint point;
            for (int i = 0; i < LINE_NUM; i++)
            {
                point = new DataPoint(maxX, buffer[bufferRowIndex - 1, i * 2] * 16 * 16 + buffer[bufferRowIndex - 1, i * 2 + 1]);
                point.MarkerColor = color;
                signalChart.Series[i].Points.Add(point);
            }

            //Shift chart
            if (maxX >= MAX_RANGE_X)
            {
                signalChart.ChartAreas[0].AxisX.Minimum++;
                signalChart.ChartAreas[0].AxisX.Maximum++;

                //Remove the points that can't be displayed
                for (int j = 0; j < LINE_NUM; j++) signalChart.Series[j].Points.RemoveAt(0);
            }
             
        }

        public void startToCom(object sender, EventArgs e)
        {
            if (portsList.SelectedItem == null)
            {
                MessageBox.Show("Please choose a port first!");
                return;
            }

            if (com == null)
            {
                com = new SerialPort(portsList.SelectedItem.ToString());
                com.DataReceived += new SerialDataReceivedEventHandler(onDataReceived);
                com.BaudRate = 115200;
                com.DataBits = 8;
            }

           //if (!com.IsOpen)
           // {
            try
            {
                com.Open();
                startBtn.Enabled = false;
                portsList.Enabled = false;
                stopBtn.Enabled = true;
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show("The current port is occupied by other program! Please try another one!");
            }
            //}

        }

        public void stopCom(object sender, EventArgs e)
        {
            if (com != null && com.IsOpen) 
            {
                com.Close();
                startBtn.Enabled = true;
                portsList.Enabled = true;
                stopBtn.Enabled = false;

                saveBuffer();
                resetStatus();

            }
        }

        private void onDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buf = new byte[com.BytesToRead];
                com.Read(buf, 0, com.BytesToRead);
                //string str = buf[0].ToString();//Convert.ToString(buf[0],2);//System.Text.Encoding.ASCII.GetString(buf);
                //int i = BitConverter.ToInt16(buf, 0);
 
                if (buf[0] == 0x7E) 
                {
                    status = processStatus.WaitingForHeaderTwo;
                    bufferColumnIndex = 0;
                }

                //header: Ox7E = 126, Ox45 = 69
                switch (status)
                {
                    case processStatus.WaitingForHeaderOne:
                        if (buf[0] == 0x7E) status = processStatus.WaitingForHeaderTwo;
                        break;
                    case processStatus.WaitingForHeaderTwo:
                        if (buf[0] == 0x45) status = processStatus.WaitingForData;
                        break;
                    case processStatus.WaitingForData:
                        buffer[bufferRowIndex,bufferColumnIndex++] = buf[0];
                        if (bufferColumnIndex >= DATA_BIT_NUM)
                        {
                            bufferColumnIndex = 0;
                            bufferRowIndex++;

                            this.Invoke(new EventHandler(addPoints));
                            status = processStatus.WaitingForHeaderOne;

                            if (bufferRowIndex == BUFFER_RECORD_NUM)
                            {
                                saveBuffer();
                                bufferRowIndex = 0;
                            }
                        }
                        break;
                    default:
                        break;
                }

            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }

        private void saveBuffer()
        {
            FileStream file = new FileStream("Cached_Data.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(file);

            for (int i = 0; i < bufferRowIndex; i++)
            {
                sw.Write("\r\n");
                for (int j = 0; j < DATA_BIT_NUM; j++)
                {
                    sw.Write(buffer[i,j]);
                    sw.Write(' ');
                }
            }
            sw.Close();
            file.Close();
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
