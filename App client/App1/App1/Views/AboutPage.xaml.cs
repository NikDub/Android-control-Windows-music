using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App1.Views
{
    public partial class AboutPage : ContentPage
    {
        Socket socket = null;
        public AboutPage()
        {
            InitializeComponent();
            State();
            b_next.BackgroundColor = b_previus.BackgroundColor = b_start.BackgroundColor = b_connect.BackgroundColor = Color.FromHex("FF4987"); //hot fix
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (socket != null)
                socket.Send(Encoding.Unicode.GetBytes("b"));
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            if (socket != null)
                socket.Send(Encoding.Unicode.GetBytes("s"));
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            if (socket != null)
                socket.Send(Encoding.Unicode.GetBytes("n"));
        }

        private async void Button_Clicked_3(object sender, EventArgs e)
        {
            if (State())
            {
                var scan = new MainPage();
                scan.Disappearing += (sender2, e2) =>
                {
                    var str = (sender2 as MainPage).str;
                    var ipArray = str.Split(new char[] { '|' }).Where(r=>r != "");

                    var allowIpAddress = new List<string>();
                    foreach (string ipAddress in ipArray)
                    {
                        if (IsIpAddressReachable(ipAddress))
                        {
                            allowIpAddress.Add(ipAddress);
                        }
                    }

                    Task.WaitAny(allowIpAddress.Select(item => Task.Factory.StartNew(() =>
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        socket.Connect(new IPEndPoint(IPAddress.Parse(item), Convert.ToInt32(8080)));
                        byte[] data = new byte[256];
                        int bytes = socket.Receive(data);
                        var temp = int.Parse(Encoding.Unicode.GetString(data, 0, bytes));
                        Slider.Value = temp;
                    })).ToArray(), 1000);
                };
                await Navigation.PushModalAsync(scan);
            }
            else
            {
                socket.Close();
            }
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (socket != null)
                socket.Send(Encoding.Unicode.GetBytes(Convert.ToInt32(Slider.Value).ToString() + '|'));
        }

        private bool State()
        {
            b_next.IsEnabled = b_previus.IsEnabled = b_start.IsEnabled = Slider.IsEnabled = !Slider.IsEnabled;
            b_connect.Text = !Slider.IsEnabled ? "Присоедениться" : "Отключиться";
            b_next.BackgroundColor = b_previus.BackgroundColor = b_start.BackgroundColor = b_connect.BackgroundColor = Color.FromHex("FF4987");
            return Slider.IsEnabled;
        }

        static bool IsIpAddressReachable(string ipAddress)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(IPAddress.Parse(ipAddress), timeout: 1000);
                return (reply.Status == IPStatus.Success);
            }
            catch (PingException)
            {
                return false;
            }
        }
    }
}