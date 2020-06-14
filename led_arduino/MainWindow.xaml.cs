using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace led_arduino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPort sp = new SerialPort();
        float temp;
        float humidity;
        float light;

        float temp_thres;
        float humidity_thres;
        float light_thres;

        ChartValues<ObservableValue> TempValues = new ChartValues<ObservableValue> {};
        ChartValues<ObservableValue> HumiValues = new ChartValues<ObservableValue> {};
        ChartValues<ObservableValue> LightValues = new ChartValues<ObservableValue> {};

        public MainWindow()
        {
            InitializeComponent();

            /// Timer document: https://www.wpf-tutorial.com/misc/dispatchertimer/
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            ///

            

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Humidity",
                    LabelPoint = point => point.X + "%",
                    Values = HumiValues
                },
                new LineSeries
                {
                    Title = "Temperature",
                    LabelPoint = point => point.X + "°C",
                    Values = TempValues,
                },
                new LineSeries
                {
                    Title = "Light",
                    LabelPoint = point => point.X + "%",
                    Values = LightValues,
                }
            };
            ///

            Labels = new[] { "" };
            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        void timer_Tick(object sender, EventArgs e)
        {
            currentTime.Content = DateTime.Now.ToString("h:mm:ss tt");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedcomboitem = sender as ComboBox;
            string name = selectedcomboitem.SelectedItem as string;
        }

        private void Conect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string portName = COM.SelectedItem as string;
                sp.PortName = portName;
                sp.BaudRate = 9600;
                sp.DataReceived += new SerialDataReceivedEventHandler(DataReceiveHandler);
                sp.Open();
                status.Text = "Connected";
            }
            catch (Exception)
            {
                MessageBox.Show("Pls give a valid port number or check your connection!");
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sp.Close();
                status.Text = "Disconnected";
            }
            catch (Exception)
            {
                MessageBox.Show("Can not disconnect!");
            }
        }

        private void ON_Click(object sender, RoutedEventArgs e)
        {
            light_thres = float.Parse(nguong_anh_sang.Text);
            humidity_thres = float.Parse(nguong_do_am.Text);
            temp_thres = float.Parse(nguong_nhiet_do.Text);
            string str = "s " + temp_thres + " " + humidity_thres + " " + light_thres + "\n";
            Console.Write(str);
            sp.Write(str);
        }

        private void DataReceiveHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string indata = sp.ReadLine();
            string[] s = indata.Split(' ');
            if(s[0].Trim() == "0")
            {
                temp = float.Parse(s[1]);
                humidity = float.Parse(s[2]);
                light = float.Parse(s[3]);
                this.Dispatcher.Invoke(() =>
                {
                    TempValues.Add(new ObservableValue(temp));
                    HumiValues.Add(new ObservableValue(humidity));
                    LightValues.Add(new ObservableValue(light));
                    Labels.Append(DateTime.Now.ToString());
                    nhiet_do.Text = temp.ToString();
                    do_am.Text = humidity.ToString();
                    anh_sang.Text = light.ToString();
                });
            }
            Console.Write("Data:");
            Console.WriteLine(indata);
        }
    }

}
