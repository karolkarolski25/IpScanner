using IpScanner.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace IpScanner.Windows
{
    public partial class WeatherWindow : Window
    {
        private List<WeatherData> weatherData = new List<WeatherData>();
        private bool abort = false, msg = true, ended = false;

        public WeatherWindow() => InitializeComponent();

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Enter:
                    if (LocationTextBox.Text.Length > 0 && ended) ForecastButton_Click(sender, e);
                    else if (!ended) AbortButton_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch (Exception) { }
        }

        private async void ForecastButton_Click(object sender, RoutedEventArgs e)
        {
            string url = null;

            if (DataComboBox.SelectedIndex == 3)
            {
                var coords = WeatherClass.GetCoords(LocationTextBox.Text);

                url = WeatherClass.ForecastUrl.Replace("@LOC@", "lat=" + coords.Item1 + "&lon=" + coords.Item2);
                url = url.Replace("@QUERY@=", string.Empty);
            }
            
            else
            {
                url = WeatherClass.ForecastUrl.Replace("@LOC@", LocationTextBox.Text);
                url = url.Replace("@QUERY@", WeatherClass.QueryCodes[DataComboBox.SelectedIndex]);
            }

            using (WebClient client = new WebClient())
            {
                try
                {
                    GetWeatherInfoLabel.Visibility = Visibility.Visible;
                    GetWeatherProgressBar.Visibility = Visibility.Visible;
                    AbortButton.Visibility = Visibility.Visible;
                    ForecastButton.Visibility = Visibility.Hidden;
                    GetWeatherInfoLabel.Content = "Pobieranie informacji o pogodzie";
                    ended = false;
                    await DisplayForecast(WeatherClass.Encode(client.DownloadString(url)));
                }
                catch (Exception)
                {
                    GetWeatherInfoLabel.Visibility = Visibility.Hidden;
                    GetWeatherProgressBar.Visibility = Visibility.Hidden;
                    AbortButton.Visibility = Visibility.Hidden;
                    ForecastButton.Visibility = Visibility.Visible;
                    ended = true;
                }
            }
        }

        private Task DisplayForecast(string xml) => Task.Run(() =>
        {
            XmlDocument xml_doc = new XmlDocument();
            xml_doc.LoadXml(xml);

            Application.Current.Dispatcher.Invoke(() =>
            {
                XmlNode loc_node = xml_doc.SelectSingleNode("weatherdata/location");
                CityLabel.Content = "Miasto: " + loc_node.SelectSingleNode("name").InnerText;
                CountryLabel.Content = "Kraj: " + loc_node.SelectSingleNode("country").InnerText;
                XmlNode geo_node = loc_node.SelectSingleNode("location");
                LatLabel.Content = "Szerokość geo: " + geo_node.Attributes["latitude"].Value;
                LongLabel.Content = "Długość geo: " + geo_node.Attributes["longitude"].Value;
                IdLabel.Content = "ID: " + geo_node.Attributes["geobaseid"].Value;
            });

            weatherData.Clear();
            char degrees = (char)176;

            foreach (XmlNode time_node in xml_doc.SelectNodes("//time"))
            {
                // Get the time in UTC.
                DateTime time = DateTime.Parse(time_node.Attributes["from"].Value, null, DateTimeStyles.AssumeUniversal);

                // Get the temperature.
                XmlNode temp_node = time_node.SelectSingleNode("temperature");
                string temp = temp_node.Attributes["value"].Value;

                //Get the Pressure
                XmlNode pres_node = time_node.SelectSingleNode("pressure");
                string pres = pres_node.Attributes["value"].Value;

                //Get the wind direction
                XmlNode windNode = time_node.SelectSingleNode("windDirection");
                string direction = windNode.Attributes["name"].Value;

                //Get the wind speed and name
                XmlNode speedNode = time_node.SelectSingleNode("windSpeed");
                string name = speedNode.Attributes["name"].Value;
                double speed = Math.Round(Convert.ToDouble(speedNode.Attributes["mps"].Value.Replace('.', ',')), 2);

                //Get the humidity
                XmlNode humNode = time_node.SelectSingleNode("humidity");
                string humidity = humNode.Attributes["value"].Value;

                //Get the cluds
                XmlNode clNode = time_node.SelectSingleNode("clouds");
                string value = clNode.Attributes["value"].Value;
                string all = clNode.Attributes["all"].Value;

                //Get the rain
                XmlNode rainNode = time_node.SelectSingleNode("symbol");
                string rain = rainNode.Attributes["name"].Value;
                string img = rainNode.Attributes["var"].Value;

                weatherData.Add(new WeatherData()
                {
                    Day = DateTimeFormatInfo.CurrentInfo.GetDayName(time.DayOfWeek),
                    Time = time.ToShortTimeString(),
                    Temperature = Convert.ToString(temp) + degrees + " C",
                    Pressure = pres + " hPa",
                    WindSpeed = Convert.ToString(speed) + " km/h",
                    WindName = WeatherClass.Translate(name),
                    WindDirection = WeatherClass.Translate(direction),
                    Humidity = humidity + " %",
                    Clouds = value + ' ' + all + " %",
                    Rain = rain,
                    ImgSource = "/Assets/Icons/WeatherIcons/" + img + ".ico"
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    lvwForecast.ItemsSource = weatherData;
                    lvwForecast.Items.Refresh();
                });

                ICollectionView view = CollectionViewSource.GetDefaultView(lvwForecast.ItemsSource);
                view.Refresh();

                if (abort) break;
            }
            DownloadedData();
        });

        private void DownloadedData()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                GetWeatherProgressBar.Visibility = Visibility.Hidden;
                GetWeatherInfoLabel.Content = ("Pobieranie ukończone");
                await Task.Delay(2000);
                GetWeatherInfoLabel.Visibility = Visibility.Hidden;
                AbortButton.Visibility = Visibility.Hidden;
                ForecastButton.Visibility = Visibility.Visible;
                abort = false;
                ended = true;
            });
        }

        private void DisplayError(WebException exception)
        {
            try
            {
                StreamReader reader = new StreamReader(exception.Response.GetResponseStream());
                XmlDocument response_doc = new XmlDocument();
                response_doc.LoadXml(reader.ReadToEnd());
                XmlNode message_node = response_doc.SelectSingleNode("//message");
                MessageBox.Show(messageBoxText: message_node.InnerText, caption: "Information",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show("Unknown error\n" + ex.Message); }
        }

        private void DataComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataComboBox.SelectedIndex == 3)
            {
                if (msg)
                {
                    MessageBox.Show(messageBoxText: "Podaj koordynaty oddzielając je spacją np. 58.2002 65.9854",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                }
            }
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e) => abort = true;

        public void SetLocation(string data, bool _msg)
        {
            LocationTextBox.Text = data;
            msg = _msg;
            DataComboBox.SelectedIndex = 3;
            ForecastButton_Click(sender: new object(), e: new RoutedEventArgs());
        }
    }

    class WeatherData
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string Temperature { get; set; }
        public string Pressure { get; set; }
        public string WindSpeed { get; set; }
        public string WindName { get; set; }
        public string WindDirection { get; set; }
        public string Humidity { get; set; }
        public string Clouds { get; set; }
        public string Rain { get; set; }
        public string ImgSource { get; set; }
    }
}
