using AudioSwitcher.AudioApi.CoreAudio;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace qrcode
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        public MainWindow()
        {
            Hide();
            InitializeComponent();
            string str = "";
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();

                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            str += addr.Address.ToString() + "|";
                    }
                }
            }
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(200);
            image.Source = Convert(qrCodeImage);

            Task.Factory.StartNew(() => MainServer());
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        void MainServer()
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
        void Run(Socket handler)
        {
            byte[] data = new byte[256];
            //MessageBox.Show("Connect " + handler.RemoteEndPoint);
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
                //MessageBox.Show("Disconnect " + handler.RemoteEndPoint);
                handler.Close();
            }
        }

        public void ButtonClose(object sender, RoutedEventArgs args)
        {
            Show();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
