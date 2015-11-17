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
    public partial class Form1 : Form
    {
        //*************configure****************

        //the number of lines in chart
        const int LINE_NUM = 8;

        const int DATA_NUM_PER_PACKAGE = 16;
        const int PORT_RECEIVED_BYTES_THRESHOLD = 1024;

        //"+1" for pressing state byte
        const int DATA_BYTES_PER_PACKAGE = DATA_NUM_PER_PACKAGE * 2 + 1;

        //When retrieve data from port, display these data as one point(for rugular displaying). 
        //The stride is the number of point in one time retrieving.
        const int DRAWING_STRIDE =  PORT_RECEIVED_BYTES_THRESHOLD / DATA_BYTES_PER_PACKAGE;

        //the number of buffer rows                        
        const int BUFFER_RECORD_NUM = 20 * DRAWING_STRIDE;

        //the display range of x axis 
        const int MAX_RANGE_X = 100;

        const int MARKER_SIZE = 10;
        const int LINE_BORDER_WIDTH = 2;

        //************************************** 
        SerialPort com;             //port

        byte[] readBuffer;

        bool closing;               //whether the port is being closing
        bool listening;             //whether the port is listening

        ChartDrawer drawer;
        DataBuffer dataBuffer;
        StateMachine sMachine;

        public Form1()
        {
            InitializeComponent();

            configureUI();

            readBuffer = new byte[4096];
            closing = false;
            listening = false;

            drawer = new ChartDrawer(DRAWING_STRIDE, LINE_NUM, signalChart, MAX_RANGE_X);
            dataBuffer = new DataBuffer(BUFFER_RECORD_NUM, DATA_BYTES_PER_PACKAGE, "Cached_Data.txt", drawer, this);
            sMachine = new StateMachine(dataBuffer);
        }

        ~Form1()
        {
            if (com.IsOpen)
            {
                stopCom(this, null);
            }

            sMachine.stop();
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
                com.ReceivedBytesThreshold = PORT_RECEIVED_BYTES_THRESHOLD;
            }

            try
            {
                closing = false;
                com.Open();
                com.DiscardInBuffer();
                startBtn.Enabled = false;
                portsList.Enabled = false;
                stopBtn.Enabled = true;

                sMachine.start();
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
                while (listening)
                {
                    Application.DoEvents();
                }
                com.Close();

                startBtn.Enabled = true;
                portsList.Enabled = true;
                stopBtn.Enabled = false;

                sMachine.stop();
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
                sMachine.parseData(readBuffer[i]);
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

    interface State
    {
        void receivedHeaderOne();
        void receivedHeaderTwo();
        void receivedData(bool doRefresh);
    }

    class WaitingForHeaderOne : State
    {
        StateMachine machine;
        DataBuffer buffer;

        public WaitingForHeaderOne(){}
        public WaitingForHeaderOne(StateMachine machine, DataBuffer buffer)
        {
            this.machine = machine;
            this.buffer = buffer;
        }
        public void receivedHeaderOne()
        {
            machine.setCurrentState(machine.getWaitingForHeaderTwoState());
            buffer.resetColumnIndex();
        }
        public void receivedHeaderTwo() { }
        public void receivedData(bool doRefresh) { }
    }

    class WaitingForHeaderTwo : State
    {
        StateMachine machine;

        public WaitingForHeaderTwo(){}
        public WaitingForHeaderTwo(StateMachine machine) {this.machine = machine; }

        public void receivedHeaderOne() { }
        public void receivedHeaderTwo()
        {
            machine.setCurrentState(machine.getWaitingForDataState());
        }
        public void receivedData(bool doRefresh) { }

    }

    class WaitingForData : State
    {
        StateMachine machine;

        public WaitingForData(){}
        public WaitingForData(StateMachine machine) {this.machine = machine;}

        public void receivedHeaderOne()
        {
            machine.setCurrentState(machine.getWaitingForHeaderTwoState());
        }

        public void receivedHeaderTwo() { }

        public void receivedData(bool doRefresh)
        {
            if(doRefresh)
            {
                machine.setCurrentState(machine.getWaitingForHeaderOneState());
            }
        }
    }

    class StateMachine
    {
        State comState;  //current (customized) state of the port
        WaitingForHeaderOne wFHOne;
        WaitingForHeaderTwo wFHTwo;
        WaitingForData wFData;

        DataBuffer buffer;

        public StateMachine(DataBuffer recorder)
        {
            wFHOne = new WaitingForHeaderOne(this, recorder);
            wFHTwo = new WaitingForHeaderTwo(this);
            wFData = new WaitingForData(this);
            comState = wFHOne;

            this.buffer = recorder;

        }

        public State getWaitingForHeaderOneState()
        {
            return wFHOne;
        }

        public State getWaitingForHeaderTwoState()
        {
            return wFHTwo;
        }

        public State getWaitingForDataState()
        {
            return wFData;
        }

        public void setCurrentState(State s)
        {
            comState = s;
        }

        public void parseData(byte dataByte)
        {
            if (dataByte == 0x7E)
            {
                comState.receivedHeaderOne();
            }
            else if (dataByte == 0x45)
            {
                comState.receivedHeaderTwo();
            }
            else
            {
                comState.receivedData(buffer.add(dataByte));
            }
        }

        public void saveBuffer()
        {
            buffer.saveBuffer();
            buffer.resetBufferIndex();
            setCurrentState(wFHOne);
        }

        public void start()
        {
            buffer.openFile();
        }

        public void stop()
        {
            saveBuffer();
            buffer.closeFile();
        }
    }

    public class DataBuffer
    {
        byte[,] buffer;
        int bufferColumnIndex;
        int bufferRowIndex;

        Object[] objects;
        int bufferColumnSize;
        int bufferRowSize;
        int dataNumCountPerPackage;
        Form mainForm;
        ChartDrawer drawer;
        DataPersistanceHelper persistanceHelper;

        public DataBuffer(int bufferRowSize, int bufferColumnSize, string cacheName, ChartDrawer drawer, Form mainForm)
        {
            buffer = new byte[bufferRowSize, bufferColumnSize];
            bufferColumnIndex = 0;
            bufferRowIndex = 0;

            this.bufferColumnSize = bufferColumnSize;
            this.bufferRowSize = bufferRowSize;
            this.dataNumCountPerPackage = (bufferColumnSize - 1) / 2;
            this.drawer = drawer;
            this.mainForm = mainForm;
            this.persistanceHelper = new DataPersistanceHelper(cacheName);
            this.objects = new Object[3];
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
                    objects[0] = bufferRowIndex;
                    objects[1] = bufferColumnSize - 1;
                    objects[2] = buffer;
                    mainForm.Invoke(drawer.drawPointsDg, objects);
                }

                if (bufferRowIndex == bufferRowSize)//Refresh buffer
                {
                    persistanceHelper.saveBuffer(bufferRowSize, (bufferColumnSize - 1) / 2, buffer);
                    bufferRowIndex = 0;
                }

                return true;//time to change state to "wait for header one"
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
        }

        public void saveBuffer()
        {
            persistanceHelper.saveBuffer(bufferRowSize, dataNumCountPerPackage, buffer);
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
    }

    public class DataPersistanceHelper
    {
        FileStream file;
        StreamWriter sw;
        string fileName;
        public DataPersistanceHelper(string fileName)//name: "Cached_Data.txt"
        {
            this.fileName = fileName;
        }

        public void openFile()
        {
            file = new FileStream(fileName, FileMode.Append);//, FileAccess.Write, FileShare.Write, 4096, true);
            sw = new StreamWriter(file);
        }

        public void closeFile()
        {
            sw.Close();
            file.Close();
        }

        public void saveBuffer(int bufferRowSize, int dataNumPerPackage, byte[,] buffer)
        {

            for (int i = 0; i < bufferRowSize; i++)
            {
                string temp = "";
                for (int j = 0; j < dataNumPerPackage; j++)
                {
                    temp += (Convert.ToString(buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1]) + " ");
                }
                temp += (Convert.ToString(buffer[i, dataNumPerPackage * 2]) + "\r\n");
                sw.Write(temp);
            }

        }
    }

    public class ChartDrawer
    {
        static int maxX = 0; //the x value of the most recent added points
        
        Chart signalChart;
        int pointsNumDisplayedInOnePackage;
        int maxXRange;

        public delegate void drawPointsDelegate(int bufferRowIndex, int pressStateIndex, byte[,] buffer);
        public drawPointsDelegate drawPointsDg;
        public int drawingStride { get; set; }

        public ChartDrawer(int drawingStride, int pointsNumDisplayedInOnePackage, Chart signalChart, int maxXRange)
        {
            this.drawingStride = drawingStride;
            this.pointsNumDisplayedInOnePackage = pointsNumDisplayedInOnePackage;
            this.signalChart = signalChart;
            this.drawPointsDg = new drawPointsDelegate(drawPoints);
            this.maxXRange = maxXRange;
        }

        public void drawPoints(int bufferRowIndex, int pressStateIndex, byte[,] buffer)
        {
            maxX++;

            //Caculate average value
            for (int j = 0; j < pointsNumDisplayedInOnePackage; j++)
            {
                int temp = 0;

                for (int i = bufferRowIndex - drawingStride; i < bufferRowIndex; i++)
                {
                    temp += buffer[i, j * 2] * 16 * 16 + buffer[i, j * 2 + 1];
                }             
                signalChart.Series[j].Points.AddXY(maxX, temp / drawingStride);
            }

            //Detect press state
            for (int i = bufferRowIndex - drawingStride; i < bufferRowIndex; i++)
            {
                if (buffer[i, pressStateIndex] == 0x01)
                {
                    foreach (Series ser in signalChart.Series)
                    {
                        ser.Points.Last().MarkerColor = Color.Red;
                    }
                    break;
                }
            }

            //Shift chart
            if (maxX >= maxXRange)
            {
                signalChart.ChartAreas[0].AxisX.Minimum++;
                signalChart.ChartAreas[0].AxisX.Maximum++;

                //Remove the points that can't be displayed
                //for (int i = 0; i < pointsNumDisplayedInOnePackage; i++) signalChart.Series[i].Points.RemoveAt(0);
                foreach (Series ser in signalChart.Series)
                {
                    ser.Points.RemoveAt(0);
                }
            }
        }
    }

    
}
