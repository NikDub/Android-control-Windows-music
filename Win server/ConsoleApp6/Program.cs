using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using AudioSwitcher.AudioApi.CoreAudio;

namespace ConsoleApp6
{
    internal class Program
    {
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 2;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        static IntPtr handle = GetConsoleWindow();

        static void Main()
        {
            // Hide
            ShowWindow(handle, SW_HIDE);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            socket.Listen(10);

            while (true)
            {
                Socket handler = socket.Accept();
                Task.Factory.StartNew(() => Run(handler));
            }
        }
        static void Run(Socket handler)
        {
            byte[] data = new byte[256];
            Console.WriteLine("Connect " + handler.RemoteEndPoint);
            handler.Send(Encoding.Unicode.GetBytes(defaultPlaybackDevice.Volume.ToString()));
            try
            {
                while (true)
                {
                    int bytes = handler.Receive(data);
                    var temp = Encoding.Unicode.GetString(data, 0, bytes);

                    if (temp == "b")
                    {
                        keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                    }
                    else if (temp == "s")
                    {
                        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                    }
                    else if (temp == "n")
                    {
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                    }
                    else if (int.TryParse(temp.Split('|').First(), out int volumeLVL))
                    {
                        defaultPlaybackDevice.Volume = volumeLVL;
                    }
                    else
                        throw new Exception();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnect " + handler.RemoteEndPoint);
                handler.Close();
            }
        }
    }
}
