﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace CleverHome_1._1
{
    public partial class Form1 : Form
    {
 
        bool isConnected = false;
        bool isInRange;          //управление отоплением
        bool provopendoors = false;

        SerialPort serialport;
        String temper, hidim, dostup;

        int a;  //хранит значение техтбокс3
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
            timer2.Interval = 2000;   //таймер отопления
            timer3.Interval = 3000;   //таймер открытия двери

            DateTime data = DateTime.Now;
        }

        private void serialport_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                string DataString = serialport.ReadLine();

                if (DataString.Contains(":"))
                {
                    string[] word = DataString.Split(':');
                    temper = word[0];
                    hidim = word[1];
                    dostup = word[2];
                    this.BeginInvoke(new LineReceivedEvent(LineReceived), temper, hidim, dostup);
                }
            }
            catch (Exception exException) { };
        }

        private delegate void LineReceivedEvent(string temper, string hidim,string dostup);

        private void LineReceived(string temper, string hidim,string dostup)
        {
            textBox1.Text = temper;
            textBox2.Text = hidim;
            textBox3.Text = dostup;
        }

        private void LineReceived(string dostup)
        {
            textBox3.Text = dostup;
        }
        
        private void button1_Click(object sender, EventArgs e)      //включчение (лед13)
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
        }

        private void button2_Click(object sender, EventArgs e)      //обновление портов
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

        private void button3_Click(object sender, EventArgs e)   //cоединиться
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
                serialport.DtrEnable = true;
                serialport.Open();
                serialport.DataReceived += serialport_DataReceived;
                button3.Text = "Отсоединиться";
                timer2.Enabled = true;  //таймер на отопление
                timer3.Enabled = true;   //таймер открытия двери
            }
            catch (Exception e)
            {
                MessageBox.Show("не удалось открыть порт");
                isConnected = false;
               // connectToArduino();

            };
        }


        private void disconnectFromArduino()
        {
            isConnected = false;
            serialport.Close();
            button3.Text = "Соединиться";
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
            var start = TimeSpan.Parse("16:30:00.0000000");   //в этом промежутке отопление включено c 16-00 до 7-30
            var end = TimeSpan.Parse("23:59:59.000000");     //

            var start2 = TimeSpan.Parse("00:00:00.0000000");
            var end2 = TimeSpan.Parse("07:29:00.0000000");

            var now = DateTime.Now.TimeOfDay;
            bool isInRange1 = start <= now && now <= end;
            bool isInRange2 = start2 <= now && now <= end2;
            if (isInRange1 || isInRange2)
            {
                // isInRange = !isInRange;
                isInRange = true;
                return isInRange;
            }
            else
            {
                isInRange = false;
                return isInRange;
            }
        }


        private void StartOtoplenieNight(bool isInRange) {
            try
            {
                if (isConnected == true && temper.Length > 0)
                {
                    String tempe = temper.Replace("\r", "");
                    String temp = tempe.Replace(".", ",");
                    //    CultureInfo temp_culture = Thread.CurrentThread.CurrentCulture;
                    //    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                    double temperatura = double.Parse(temp);

                    //   Thread.CurrentThread.CurrentCulture = temp_culture;
                    if (isInRange == true && (temperatura < 27.10))       // включение отопления в вечернее время
                    {
                        label5.Text = " включено";
                    }
                    else
                    {
                        label5.Text = " выключено";
                    }

                    if (isInRange == false && (temperatura < 27.50))    //включение отопления в дневное время
                    {
                        label7.Text = " включено";
                    }
                    else
                    {
                        label7.Text = " выключено";
                    }
                }
            }
            catch (Exception e) { };
        }
 

        private void timer1_Tick(object sender, EventArgs e)        //таймер на время, отобразить время
        {
            label4.Text = DateTime.Now.ToString("F");

        }

        private void timer2_Tick(object sender, EventArgs e)        //таймер на отопление
        {
            ProverkaVremeniOtoplenie();
            StartOtoplenieNight(isInRange);
        }


        private void timer3_Tick_1(object sender, EventArgs e)
        {
            proverkaopendoors();
            opendoors(provopendoors);
        }


        private void button4_Click(object sender, EventArgs e)      //закрыть дверь
        {
            if (provopendoors == false)
            {
                label8.Text = "Дверь открыта";
            }
            else
            {
                a = Int32.Parse(textBox3.Text);
                if (a == 872227033)
                    label8.Text = "Дверь закрыта";
                opendoors( false);              
            }
        }


        void opendoors(bool provopendoors)
        { if (provopendoors == false)
            {
                label8.Text = "Дверь закрыта";
                provopendoors = false;
            }
            else
            {
                label8.Text = "Дверь открыта";
            }
        }


        private Boolean proverkaopendoors()
        {
            a = Int32.Parse(textBox3.Text);
            if (a==872227033)
            {             
                provopendoors = true;
                return provopendoors;     
            }
            else
            { 
                provopendoors = false;
                return provopendoors;
            } 
        }
    }
}
