using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly SemaphoreSlim signal = new SemaphoreSlim(0, int.MaxValue);
        public Form1()
        {
            InitializeComponent();
            this.Text = "Key Sender";
            Process[] processCollection = Process.GetProcesses();
            List<string> MyProcessList = new List<string>();
            foreach (Process p in processCollection)
            {
                MyProcessList.Add(p.ProcessName);

            }
            MyProcessList.Sort();
            listBox1.DataSource = MyProcessList;
            //Try to find RDP by default and highlight it
            int msrdc = listBox1.FindString("msrdc");
            if (msrdc != -1)
            {
                listBox1.SetSelected(msrdc, true);
            }
            comboBox1.Items.Add(0);
            comboBox1.Items.Add(1);
            comboBox1.Items.Add(3);
            comboBox1.Items.Add(5);
            comboBox1.Items.Add(10);
            comboBox1.SelectedIndex = 3; //default to 5 min
        }


        private CancellationTokenSource _canceller;

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        private async void button1_Click(object sender, EventArgs e)
        {

            button1.Enabled = false;
            button2.Enabled = true;
            string curItem = listBox1.SelectedItem.ToString();
            int currentMyComboBoxIndex = (int)comboBox1.SelectedItem;
            int time = ConvertMin(currentMyComboBoxIndex);
            _canceller = new CancellationTokenSource();
            await Task.Run(() =>
            {
                do
                {

                    Process p = Process.GetProcessesByName(curItem).FirstOrDefault();
                    if (p != null)
                    {
                        IntPtr current = GetForegroundWindow();
                        IntPtr h = p.MainWindowHandle;
                        SetForegroundWindow(h);
                        //SendKeys.SendWait(RandomString(1));
                        SendKeys.SendWait("+");
                        SetForegroundWindow(current);
                        //500000 ~ 9minutes
                        //240000 ~ 4 minutes
                        Thread.Sleep(time);
                    }
                    if (_canceller.Token.IsCancellationRequested)
                        break;

                } while (true);
            });

            _canceller.Dispose();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            _canceller.Cancel();
            button2.Enabled = false;
            button1.Enabled = true;
        }

        private int ConvertMin (int num)
        {
            if (num.Equals(1))
            {
                return 60000;
            }
            if (num.Equals(3))
            {
                return 180000;
            }
            if (num.Equals(5))
            {
                return 300000;
            }
            if (num.Equals(10))
            {
                return 590000;
            }
            // defualt to 3 sec if combobox is set to 0
            return 3000;
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
