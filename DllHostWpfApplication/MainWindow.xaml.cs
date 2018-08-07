using System;
using System.Windows;
using System.Windows.Controls;

namespace DllHostWpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        private void cwX_button_Click(object sender, RoutedEventArgs e)
        {
            Slider_ahh1.Value += 1;
            Slider_ahh2.Value += 1;
            Slider_ahh3.Value += 1;
            Slider_ahh4.Value += 1;
            string senderId;
            if (sender is Button)
            {
                senderId = (sender as Button).Name;
                senderId = senderId.Replace("Button_ahh", "");
            }
            else
            {
                senderId = "<unknown>";
            }
            TextBox_ahh1.Text = "textcw1_" + senderId;
            TextBox_ahh2.Text = "textcw2_" + senderId;
            TextBox_ahh3.Text = "textcw3_" + senderId;
            TextBox_ahh4.Text = "textcw4_" + senderId;
        }

        private void Button_ResetSliders_Click(object sender, RoutedEventArgs e)
        {
            Slider_ahh1.Value = 8;
            Slider_ahh2.Value = 8;
            Slider_ahh3.Value = 8;
            Slider_ahh4.Value = 8;
        }

        private void Button_ahhX_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            { 
                System.Console.WriteLine((sender as Button).Name + " clicked");
            }
        }
    }
}
