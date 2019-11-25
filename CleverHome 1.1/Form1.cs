using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


/// <summary>
/// ////////////рабочая версия
/// </summary>

namespace CleverHome_1._1
{
    public partial class Form1 : Form
    {
        // string curTimeLong = DateTime.Now.ToLongTimeString();
        bool isConnected = false;
        bool isInRange;          //управление отоплением
        SerialPort serialport;
        public Form1()
        {
            InitializeComponent();
            // Открываем порт, и задаем скорость в 9600 бод
            //serialPort.PortName = "COM12";
            //serialPort.BaudRate = 9600;
            //serialPort.DtrEnable = true;
            //serialPort.Open();
            //serialPort.DataReceived += serialPort_DataReceived;

            timer1.Enabled = true;  //таймер времени        
            timer1.Interval = 1000;

            timer2.Enabled = true;  //таймер на отопление
            timer2.Interval = 1000;


            DateTime data = DateTime.Now;
        }


        private void serialport_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string DataString = serialport.ReadLine();

            if (DataString.Contains(":"))
            {
                string[] word = DataString.Split(':');
                string temper = word[0];
                string hidim = word[1];

                this.BeginInvoke(new LineReceivedEvent(LineReceived), temper, hidim);
            }
        }
        private delegate void LineReceivedEvent(string temper, string hidim);
        private void LineReceived(string temper, string hidim)
        {
            textBox1.Text = temper;
            textBox2.Text = hidim;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (isConnected == false)
            {
                return;
            }
            else
            {
                if (button1.Text.Equals("Включить"))
                {
                    button1.Text = "Выключить";
                    serialport.Write("1");
                }
                else
                {
                    button1.Text = "Включить";
                    serialport.Write("0");
                }
            }
            // serialPort.Write("1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            // Получаем список COM портов доступных в системе
            string[] portnames = SerialPort.GetPortNames();
            // Проверяем есть ли доступные
            if (portnames.Length == 0)
            {
                MessageBox.Show("COM порты не найдены");
            }
            foreach (string s in portnames)
            {
                //добавляем доступные COM порты в список              
                comboBox1.Items.Add(s);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                connectToArduino();
            }
            else
            {
                disconnectFromArduino();
            }

        }


        private void connectToArduino()
        {
            try
            {
                isConnected = true;
                string selectedPort = comboBox1.GetItemText(comboBox1.SelectedItem);
                serialport = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
                //  serialport.Open();

                //     serialPort.PortName = "COM12";
                //   serialPort.BaudRate = 9600;
                serialport.DtrEnable = true;
                serialport.Open();
                serialport.DataReceived += serialport_DataReceived;
                button3.Text = "Disconnect";
            }
            catch (Exception e)
            {
                MessageBox.Show("не удалось открыть порт");
                isConnected = false;
                connectToArduino();

            };
        }

        private void disconnectFromArduino()
        {
            isConnected = false;
            serialport.Close();
            button3.Text = "Connect";
        }

        /*
        public void DrawStringPointF()
        {
            // Create string to draw.
            String drawString = DateTime.Now.ToString();

            // Create font and brush.
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Create point for upper-left corner of drawing.
            PointF drawPoint = new PointF(150.0F, 150.0F);

            // Draw string to screen.
            Graphics.FromHwnd(this.Handle).DrawString(drawString, drawFont, drawBrush, drawPoint);
        }
        */

        private Boolean ProverkaVremeniOtoplenie() {

            var start = TimeSpan.Parse("00:47:00.0000000");
            var end = TimeSpan.Parse("00:47:30.0000000");
            var now = DateTime.Now.TimeOfDay;

            isInRange = start <= now && now <= end;
            return isInRange;
        }

        private void StartOtoplenie(bool isInRange) {

            if (isInRange == true)
            {
                label5.Text = "Отопление включено";
            }
            else
            {
                label5.Text = "Отопление OFF";
            }
        }
        
    


        private void timer1_Tick(object sender, EventArgs e)
        {
            //label3.Text = DateTime.Now.ToLongTimeString();
            label4.Text = DateTime.Now.ToString("F");          
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            ProverkaVremeniOtoplenie();
            StartOtoplenie(isInRange);
        }
    }
    

}
