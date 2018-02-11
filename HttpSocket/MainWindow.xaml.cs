using System;
using System.Windows;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HttpSocket.Models;
using System.IO;
using System.Threading;

namespace HttpSocket
{
    public partial class MainWindow : Window
    {
        protected int portNumber = 0;
        protected Thread server;

        public MainWindow()
        {
            InitializeComponent();

            // for debugging
            if (!AttachConsole(-1))
                AllocConsole();
    
            rootDitrectory.TextWrapping = TextWrapping.NoWrap;
        }

        protected void Button_Click(object sender, EventArgs e)
        {
           
            if (!CheckPortNumber())
                return;

            if (rootDitrectory.Text != "")
            {
                try
                {
                    Directory.SetCurrentDirectory(rootDitrectory.Text);
                }
                catch (DirectoryNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
         
            server = new Thread(() => AsynchronousSocketListener.StartListening(portNumber)); 
            server.Start();

            button.IsEnabled = false;
            
        }

        // After disposing socket something try reference it   
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Models.AsynchronousSocketListener.Manager = false;
            Models.AsynchronousSocketListener.allDone.Set();
            button.IsEnabled = true;
        }

        protected bool CheckPortNumber()
        {
            int number;
            bool isValid = int.TryParse(port.Text, out number);
            if (isValid && 1024 < number && number < 11000)
            {
                portNumber = number;
                return true;
            }
            else
            {
                MessageBox.Show("Iveskite svieka skaiciu nuo 1024 iki 65535");
                return false;
            }
        }

        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
    }
}
