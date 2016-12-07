using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
//using System.Diagnostics;

namespace xyz_grapher
{
    public partial class Form1 : Form
    {
        private string port = string.Empty;
        private Int32 baud = 0;
        private Thread readThread;
        private string dataType = "Decimal";

        private Int16[] x_array_int = new Int16[500];
        private Int16[] y_array_int = new Int16[500];
        private Int16[] z_array_int = new Int16[500];

        private double[] x_array_double = new double[500];
        private double[] y_array_double = new double[500];
        private double[] z_array_double = new double[500];

        private readonly object m_Locker = new object();
        private bool signalThread;

        public Form1()
        {
            InitializeComponent();
            populateBaudRates();

            // Populate Dropdown with valid datatypes and select Doubles as default type
            comboBox3.Items.Add("Decimal");
            comboBox3.Items.Add("Integer");
            comboBox3.SelectedItem = dataType;

            modifyCharts();
        }

        private void modifyCharts()
        {
            // To change the color for the line on the charts
            chart1.Series[0].Color = Color.Crimson;
            chart2.Series[0].Color = Color.DodgerBlue;
            chart3.Series[0].Color = Color.Green;

            // disable vertical grid lines and turn off labels
            // There are all sorts of options for the charts below that are commented out

            //chart1.ChartAreas[0].AxisY.IsMarginVisible = false;
            //chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            //chart1.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
            //chart1.ChartAreas[0].AxisX.MinorTickMark.Enabled = false;
            //chart1.ChartAreas[0].AxisX.Interval = 0;
            //chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            //chart1.ChartAreas[0].AxisX.LineWidth = 0;
            chart1.ChartAreas[0].AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            //chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            //chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            //chart1.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;
            //chart1.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;
            //chart1.ChartAreas[0].AxisY.LineWidth = 0;

            //chart2.ChartAreas[0].AxisY.IsMarginVisible = false;
            //chart2.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            //chart2.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
            //chart2.ChartAreas[0].AxisX.MinorTickMark.Enabled = false;
            //chart2.ChartAreas[0].AxisX.Interval = 0;
            //chart2.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            //chart2.ChartAreas[0].AxisX.LineWidth = 0;
            chart2.ChartAreas[0].AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            //chart2.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            //chart2.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            //chart2.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;
            //chart2.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;
            //chart2.ChartAreas[0].AxisY.LineWidth = 0;

            //chart3.ChartAreas[0].AxisY.IsMarginVisible = false;
            //chart3.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            //chart3.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
            //chart3.ChartAreas[0].AxisX.MinorTickMark.Enabled = false;
            //chart3.ChartAreas[0].AxisX.Interval = 0;
            //chart3.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            //chart3.ChartAreas[0].AxisX.LineWidth = 0;
            chart3.ChartAreas[0].AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            //chart3.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            //chart3.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            //chart3.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;
            //chart3.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;
            //chart3.ChartAreas[0].AxisY.LineWidth = 0;

        }

        private void populateBaudRates()
        {
            //Put a list of baud rates into the combo box
            //comboBox2.Items.Add(110);
            //comboBox2.Items.Add(300);
            //comboBox2.Items.Add(600);
            //comboBox2.Items.Add(1200);
            //comboBox2.Items.Add(2400);
            //comboBox2.Items.Add(4800);
            comboBox2.Items.Add(9600);
            comboBox2.Items.Add(14400);
            comboBox2.Items.Add(19200);
            comboBox2.Items.Add(28800);
            comboBox2.Items.Add(38400);
            comboBox2.Items.Add(56000);
            comboBox2.Items.Add(57600);
            comboBox2.Items.Add(115200);
        }

        private void scanForCommPorts()
        {
            //show list of valid com ports
            // Get a list of serial port names.
            List<string> ports = new List<string>();

            ports = SerialPort.GetPortNames().ToList();

            if (ports.Count() == 0)
            {
                //add an entry to make sure that we don't have an empty list for testing.
                ports.Add("[No Comm Port]");
            }

            comboBox1.Items.Clear();

            // Display each port name to the console.
            foreach (string portname in ports)
            {
                comboBox1.Items.Add(portname);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                if (port.ToString() != string.Empty && baud != 0)
                {
                    // can't open dummy port
                    if (port.ToString() != "[No Comm Port]")
                    {
                        open_serial();
                        lock (m_Locker)
                        {
                            signalThread = true;
                        }

                        readThread = new Thread(new ThreadStart(readSerial));
                        readThread.Start();

                        comboBox1.Enabled = false;
                        comboBox2.Enabled = false;
                        comboBox3.Enabled = false;
                        button1.Text = "Stop";
                    }

                    //for testing chart formatting
                    //else
                    //{
                    //    UpdateCharts();
                    //}
                }
            }
            else
            {
                //end thread
                lock (m_Locker)
                {
                    signalThread = false;
                }

                if (!readThread.Join(1000))
                {
                    try
                    {
                        readThread.Abort();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                //close serial port
                close_serial();

                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                button1.Text = "Start";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            port = comboBox1.SelectedItem.ToString();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            baud = Convert.ToInt32(comboBox2.SelectedItem);
        }

        private void open_serial()
        {
            // opens serial port to starts data capture
            serialPort1.PortName = port;
            serialPort1.BaudRate = baud;
            try
            {
                serialPort1.Open();
            }
            catch
            {
                serialPort1.Close();
            }

        }

        private void close_serial()
        {
            // closes serial port and ends data capture
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
        }

        private void readSerial()
        {
            string input;
            bool run = true;

            //throw away the first few readings.
            //for (int i=0; i < 10; i++)
            //{
            //    input = serialPort1.ReadLine();
            //}

            while (run)
            {
                input = serialPort1.ReadLine();
                input = input.Replace("\n", String.Empty);
                input = input.Replace("\r", String.Empty);

                //split string on commas
                string[] input_array = input.Split(',');
                int array_length = input_array.Length;

                if (array_length > 0)
                {
                    if (dataType == "Decimal")
                    {
                        try
                        {
                            x_array_double[x_array_double.Length - 1] = Convert.ToDouble(input_array[0]);
                            Array.Copy(x_array_double, 1, x_array_double, 0, x_array_double.Length - 1);
                        }
                        catch
                        {
                            //if the cast fails do nothing
                            Array.Copy(x_array_double, 1, x_array_double, 0, x_array_double.Length - 1);
                        }
                    }
                    else
                    {
                        try
                        {
                            x_array_int[x_array_int.Length - 1] = Convert.ToInt16(input_array[0]);
                            Array.Copy(x_array_int, 1, x_array_int, 0, x_array_int.Length - 1);
                        }
                        catch
                        {
                            Array.Copy(x_array_int, 1, x_array_int, 0, x_array_int.Length - 1);
                        }
                    }
                }

                if (array_length > 1)
                {
                    if (dataType == "Decimal")
                    {
                        try
                        {
                            y_array_double[y_array_double.Length - 1] = Convert.ToDouble(input_array[1]);
                            Array.Copy(y_array_double, 1, y_array_double, 0, y_array_double.Length - 1);
                        }
                        catch
                        {
                            Array.Copy(y_array_double, 1, y_array_double, 0, y_array_double.Length - 1);
                        }
                    }
                    else
                    {
                        try
                        {
                            y_array_int[y_array_int.Length - 1] = Convert.ToInt16(input_array[1]);
                            Array.Copy(y_array_int, 1, y_array_int, 0, y_array_int.Length - 1);
                        }
                        catch
                        {
                            Array.Copy(y_array_int, 1, y_array_int, 0, y_array_int.Length - 1);
                        }
                    }
                }

                if (array_length > 2)
                {
                    if (dataType == "Decimal")
                    {
                        try
                        {
                            z_array_double[z_array_double.Length - 1] = Convert.ToDouble(input_array[2]);
                            Array.Copy(z_array_double, 1, z_array_double, 0, z_array_double.Length - 1);
                        }
                        catch
                        {
                            Array.Copy(z_array_double, 1, z_array_double, 0, z_array_double.Length - 1);
                        }
                    }
                    else
                    {
                        try
                        {
                            z_array_int[z_array_int.Length - 1] = Convert.ToInt16(input_array[1]);
                            Array.Copy(z_array_int, 1, z_array_int, 0, z_array_int.Length - 1);
                        }
                        catch
                        {
                            Array.Copy(z_array_int, 1, z_array_int, 0, z_array_int.Length - 1);
                        }
                    }
                }

                if (chart1.IsHandleCreated && chart2.IsHandleCreated && chart3.IsHandleCreated)
                {
                    Invoke((MethodInvoker)delegate { UpdateCharts(); });
                }

                lock (m_Locker)
                {
                    run = signalThread;
                }

            }
        }

        private void UpdateCharts()
        {
            chart1.Series["X Axis"].Points.Clear();
            chart2.Series["Y Axis"].Points.Clear();
            chart3.Series["Z Axis"].Points.Clear();

            if (dataType == "Decimal")
            {
                for (int i = 0; i < x_array_double.Length; i++)
                {
                    chart1.Series["X Axis"].Points.AddY(x_array_double[i]);
                }

                for (int i = 0; i < y_array_double.Length; i++)
                {
                    chart2.Series["Y Axis"].Points.AddY(y_array_double[i]);
                }

                for (int i = 0; i < z_array_double.Length; i++)
                {
                    chart3.Series["Z Axis"].Points.AddY(z_array_double[i]);
                }
            }
            else
            {
                for (int i = 0; i < x_array_int.Length; i++)
                {
                    chart1.Series["X Axis"].Points.AddY(x_array_int[i]);
                }

                for (int i = 0; i < y_array_int.Length; i++)
                {
                    chart2.Series["Y Axis"].Points.AddY(y_array_int[i]);
                }

                for (int i = 0; i < z_array_int.Length; i++)
                {
                    chart3.Series["Z Axis"].Points.AddY(z_array_int[i]);
                }
            }

        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            // rescan for comm ports
            scanForCommPorts();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
                    dataType = comboBox3.SelectedItem.ToString();
        }

    }
}
