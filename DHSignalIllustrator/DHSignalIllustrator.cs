using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Windows.Input;


namespace DHSignalIllustrator
{
    public partial class DHSignalIllustrator : Form
    {
        //*************configure****************

        //the number of lines in chart
        const int LINE_NUM = 8;

        const int DATA_NUM_PER_PACKAGE = 16;

        //"+1" for pressing state byte
        const int DATA_BYTES_PER_PACKAGE = DATA_NUM_PER_PACKAGE * 2 + 1;

        const int PORT_RECEIVED_BYTES_THRESHOLD = DATA_BYTES_PER_PACKAGE * 1;//Fetch 30 packages at one time.

        /*When fetching data from port, display them as one point(for each line). 
        The stride is the number of packages for one time fetching.*/
        const int DRAWING_STRIDE = PORT_RECEIVED_BYTES_THRESHOLD / DATA_BYTES_PER_PACKAGE;

        //the number of buffer rows 
        //'20' is adjustable.                       
        const int BUFFER_RECORD_NUM = 20 * DRAWING_STRIDE;

        //the display range of x axis 
        const int MAX_RANGE_X = 100;

        const int MARKER_SIZE = 10;
        const int LINE_BORDER_WIDTH = 2;

        Color[] colors =
        {
            Color.LightCoral,
            Color.Coral,
            Color.Gold,
            Color.YellowGreen,
            Color.RoyalBlue,
            Color.Turquoise,
            Color.SlateBlue,
            Color.Black,
        };

        //************************************** 
        SerialPort com;             //port

        byte[] readBuffer;

        bool closing;               //whether the port is being closed
        bool listening;             //whether the port is listening

        ChartDrawer drawer;
        DataBuffer dataBuffer;
        StateMachine sMachine;     

        public DHSignalIllustrator()
        {
            InitializeComponent();

            configureUI();

            readBuffer = new byte[4096];
            closing = false;
            listening = false;

            drawer = new ChartDrawer(DRAWING_STRIDE, LINE_NUM, signalChart,this);
            dataBuffer = new DataBuffer(BUFFER_RECORD_NUM, DATA_BYTES_PER_PACKAGE, "Cached_Data.txt", drawer, this);
            sMachine = new StateMachine(dataBuffer);

            //*********************3D Chart*******************************

            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.SetStyle(ControlStyles.UserPaint, true);//When this flag is set to true,
            //the control paints itself and is not painted by the system operation

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SetStyle(ControlStyles.DoubleBuffer, true);//when set Control.DoubleBuffered to true, 
            //it will set the ControlStyles.AllPaintingInWmPaint and ControlStyles.OptimizedDoubleBuffer to true      
                 
        }

        ~DHSignalIllustrator()
        {
            if (com.IsOpen)
            {
                stopCom(this, null);
            }

            sMachine.stop();
        }


        private void configureUI()
        {
            //******************Result Chart**********************
            //resultChart.Series.Clear();
            //resultChart.ChartAreas[0].AxisX.Minimum = 0;
            ////chart.ChartAreas[0].AxisX.Maximum = 100;
            //resultChart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            //resultChart.ChartAreas[0].AxisX.Interval = 20;

            //resultChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            //resultChart.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = double.NaN;
            //resultChart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 2;

            ////chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            ////chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;

            //resultChart.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            //resultChart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            //resultChart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            //resultChart.ChartAreas[0].AxisX.ScrollBar.Size = 8;

            //for (int i = 0; i < LINE_NUM; i++)
            //{
            //    Series line = new Series();
            //    line.MarkerStyle = MarkerStyle.Cross;
            //    line.MarkerSize = MARKER_SIZE;
            //    line.MarkerColor = Color.Transparent;
            //    line.BorderWidth = LINE_BORDER_WIDTH;
            //    line.ChartType = SeriesChartType.Line;
            //    line.Points.AddXY(0, 200);
            //    line.Color = colors[i];
            //    line.IsVisibleInLegend = false;
            //    resultChart.Series.Add(line);
            //}
            //******************Result Chart**********************

            //combo box
            string[] portsName = SerialPort.GetPortNames();
            foreach (string port in portsName)
            {
                portsList.Items.Add(port);
            }

            //Btn
            stopBtn.Enabled = false;
            stopBtn.BackColor = Color.DimGray;
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

                setBtnsListenState();
                prepareToDraw();
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

                setBtnsLoadingState();
                loadingPlotPanel();

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
            Button btn = (Button)sender;

            //CheckBox checkBox = (CheckBox)sender;
            int lineNum = Convert.ToInt32(btn.Tag.ToString()) - 1;
            if (btn.BackColor != Color.SlateGray)
            {
                signalChart.setAppearanceOfLine(lineNum, false);
                btn.BackColor = Color.SlateGray;
            }
            else
            {
                signalChart.setAppearanceOfLine(lineNum, true);
                btn.BackColor = colors[lineNum];
            }
            signalChart.Refresh();
        }

        public void refreshCountLabels(Int64 numCount, Int64 moveCount)
        {
            string numString, mCountString;
            if (numCount > 10000)
            {
                numCount /= 1000;
                numString = Convert.ToString(numCount) + "K";
            }
            else
            {
                numString = Convert.ToString(numCount);
            }
            if (moveCount > 10000)
            {
                moveCount /= 1000;
                mCountString = Convert.ToString(moveCount) + "K";
            }
            else
            {
                mCountString = Convert.ToString(moveCount);
            }
            totalPLabel.Text = numString;
            moveCLabel.Text = mCountString;
        }

        public void setBtnsStandByState()
        {
            enableAllTextinput(true);
            portsList.Enabled = true;
            startBtn.Enabled = true;
            stopBtn.Enabled = false;
            refreshBtn.Enabled = true;
            startBtn.BackColor = Color.DarkGray;
            stopBtn.BackColor = Color.DimGray;
        }

        public void setBtnsListenState()
        {
            enableAllTextinput(false);
            portsList.Enabled = false;
            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            refreshBtn.Enabled = false;
            startBtn.BackColor = Color.DimGray;
            stopBtn.BackColor = Color.DarkGray;
        }

        public void setBtnsLoadingState()
        {
            enableAllTextinput(false);
            portsList.Enabled = false;
            startBtn.Enabled = false;
            stopBtn.Enabled = false;
            refreshBtn.Enabled = false;
            startBtn.BackColor = Color.DimGray;
            stopBtn.BackColor = Color.DimGray;
        }

        public void enableAllTextinput(bool isEnable)
        {
            winWidth.Enabled = isEnable;
            filterMin.Enabled = isEnable;
            filterMax.Enabled = isEnable;
        }

        public void showLoadingLabel(bool isShow)
        {
            loadingLabel.Visible = isShow;
        }

        private void loadingPlotPanel()
        {
            loadingLabel.Text = "Loading...";
        }

        public void prepareToDraw()
        {
            Graphics g = PlotPanel.CreateGraphics();
            g.Clear(Color.White);
            loadingLabel.Text = "No Data";
            loadingLabel.BackColor = Color.White;
            showLoadingLabel(true);
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            int[] parameters = new int[3];
            try
            {           
                parameters[0] = Convert.ToInt32(winWidth.Text);
                parameters[1] = Convert.ToInt32(filterMin.Text);
                parameters[2] = Convert.ToInt32(filterMax.Text);
                
                if(parameters[0] <= 0 || parameters[1] < 0 || parameters[2] < 0)
                {
                    throw new Exception();
                }

                prepareToDraw();
                loadingPlotPanel();
                setBtnsLoadingState();
                sMachine.setWinWidthAndFilterRange(parameters);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Wrong format. Please input again!");
            }

            //sMachine.setWinWidthAndFilterRange();
        }
    }

    
}
