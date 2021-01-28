using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;



namespace CheckOnPC
{
    public partial class Form1 : Form
    {
        private static String pc_name;
        private static bool check; //вкл/выкл
        public Form1()
        {
            const string Path = @"D:Proga.txt";
            InitializeComponent();
            timer1.Interval=Int32.Parse(this.numericUpDown1.Value.ToString())*1000;
            if (File.Exists(Path))
            {
                var result = MessageBox.Show("Найдены сохранения, восстановить?", "Recovery", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    recovery();
                }
            }
        }
        public bool Thread_StateCheck(string pc_name1)
        {            
            pc_name = pc_name1;             
            threadStart();
            return check;            
        }
        public void threadStart()
        {
        //пинг компов  
            try
            {
                PingReply pir;
                Ping pi = new Ping();
                pir = pi.Send(pc_name);
                if (pir.Status != IPStatus.Success)
                {
                    check = false;
                }
                else
                {
                    check = true;
                }
            }
            catch (Exception)
            {
                check = false;
            }
        }
        //включение таймера
        private void button3_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {                
                this.button3.Text = "Включить таймер";
                this.timer1.Enabled = false;                
            }
            else
            {
                this.button3.Text = "Выключить таймер";
                this.timer1.Enabled = true;
                this.timer1.Start();                
            } 
        }
        //Таймер событие 
        private void timer1_Tick(object sender, EventArgs e)
        {
            Thread tr = new Thread(new ThreadStart(timer_custom));
            tr.SetApartmentState(ApartmentState.MTA);
            tr.Start();
        }
        //Timer в поток
        public void timer_custom()
        {
            bool off = false;
            string CompOff = "";
            for (int i = 0; i <= this.dataGridView1.Rows.Count - 2; i++)
            {
                //if (StateCheck(dataGridView1.Rows[i].Cells[0].Value.ToString())) //--распоточил (см check)
                if (Thread_StateCheck(dataGridView1.Rows[i].Cells[0].Value.ToString()))
                {
                    dataGridView1.Rows[i].Cells[1].Value = true;
                }
                else
                {
                    dataGridView1.Rows[i].Cells[1].Value = false;
                }

                if (dataGridView1.Rows[i].Cells[1].Value.Equals(false) && this.WindowState == FormWindowState.Minimized) //если выключен
                {
                    off = true;

                    if (CompOff == "")
                    {
                        CompOff = this.dataGridView1.Rows[i].Cells[0].Value.ToString() + " Выключен";
                    }
                    else
                    {
                        CompOff = CompOff + "\n" + this.dataGridView1.Rows[i].Cells[0].Value.ToString() + " Выключен";
                    }

                    switch (i)
                    {
                        case 0:
                            notifyIcon1.Icon = Properties.Resources.FirstOff;
                            break;
                        case 1:
                            notifyIcon1.Icon = Properties.Resources.SecondOff;
                            break;
                        case 2:
                            notifyIcon1.Icon = Properties.Resources.ThirdOff;
                            break;
                        case 3:
                            notifyIcon1.Icon = Properties.Resources.FourthOff;
                            break;
                    }
                }
                if (off == false)
                {
                    if (i == this.dataGridView1.Rows.Count - 2)
                    {
                        notifyIcon1.Icon = Properties.Resources.AllOn;
                        CompOff = "Все компьютеры включены";
                    }
                }
            }
            if (CompOff != "Все компьютеры включены")
            {
                notifyIcon1.Text = CompOff;
                notifyIcon1.ShowBalloonTip(timer1.Interval, "Информация",
                    CompOff, ToolTipIcon.Info);
            }
            else
            {
                notifyIcon1.Text = CompOff;
            }
        }
        //добавление в грид
        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Add(this.textBox2.Text,
                           Thread_StateCheck(this.textBox2.Text));
        }        
        //изменение интервала таймера     
        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            timer1.Interval = Int32.Parse(numericUpDown1.Value.ToString())*1000;
        }
        //передача в файл
        private void button2_Click(object sender, EventArgs e)
        {
            const string Path = @"D:Proga.txt";
            using (StreamWriter sw = File.CreateText(Path))
            {
                try
                {
                    for (int i = 0; i <= this.dataGridView1.Rows.Count-2; i++)
                    {
                        sw.WriteLine(this.dataGridView1.Rows[i].Cells[0].Value.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                sw.Close();
            }     
        } 
        //восстановление
        public void recovery()
        {
            {
                const string Path = @"D:Proga.txt";
                string[] col_str = new string[InFile(Path)];
                using (StreamReader file = File.OpenText(Path))
                {
                    int i = 0;
                    while (!file.EndOfStream)
                    {
                        col_str.SetValue(file.ReadLine(), i);
                        i++; //количество записей  
                    }

                    for (int j = 0; j <= i - 1; j++)
                    {
                        try
                        {
                            this.dataGridView1.Rows.Add(col_str[j].ToString(),
                                Thread_StateCheck(col_str[j].ToString()));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    file.Close();
                }
            }        
        }                
        //Количество строк в файле
        public int InFile(string FilePath)
        {
            int number = 0;
            using (StreamReader file1 = File.OpenText(FilePath))
            {                 
                while (!file1.EndOfStream)
                {
                    number++;
                    file1.ReadLine();
                }
                file1.Close();                
            }
            return number;                        
        }      
        //В статус бар
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;                
            }
        }       
        //двойное нажатие на иконку в трее
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }              
    }
}