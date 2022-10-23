using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            if(socket!=null)
                socket.Send(Encoding.Unicode.GetBytes("b"));
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            if(socket!=null)
                socket.Send(Encoding.Unicode.GetBytes("s"));
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            if(socket!=null)
            socket.Send(Encoding.Unicode.GetBytes("n"));
        }

        private void Button_Clicked_3(object sender, EventArgs e)
        {
            if (State())
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(Textbox.Text), Convert.ToInt32(8080)));
                byte[] data = new byte[256];
                int bytes = socket.Receive(data);
                var temp = int.Parse(Encoding.Unicode.GetString(data, 0, bytes));
                Slider.Value = temp;
            }
            else
            {
                socket.Close();
            }
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (socket != null)
                socket.Send(Encoding.Unicode.GetBytes(Convert.ToInt32(Slider.Value).ToString()+'|'));
        }

        private bool State()
        {
            b_next.IsEnabled = b_previus.IsEnabled = b_start.IsEnabled = Slider.IsEnabled = !Slider.IsEnabled;
            b_connect.Text = !Slider.IsEnabled ? "Присоедениться" : "Отключиться";
            b_next.BackgroundColor = b_previus.BackgroundColor = b_start.BackgroundColor = b_connect.BackgroundColor = Color.FromHex("FF4987");
            return Slider.IsEnabled;
        }
    }
}