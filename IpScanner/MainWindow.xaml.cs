using Imieniny;
using IpScanner.Class;
using IpScanner.Windows;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;

namespace IpScanner sdf
{
    public partial class MainWindow : Window
    {
        static readonly WebClient wb = new WebClient();
        static readonly FlowDocument flowDocument = new FlowDocument();

        private bool showMoreData = false;
        private List<WeatherData> weatherData = new List<WeatherData>();

        private string coord = null;

        public string time;

        internal static MainWindow mainWindow;

        public MainWindow()
        {
            InitializeComponent();

            mainWindow = this;

            DateLabel.Content = DateTime.Today.ToLongDateString().ToUpper();

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                time = DateTime.Now.ToString("dd / MM / yyyy  HH : mm : ss");
                TimeLabel.Content = DateTime.Now.ToString("HH : mm : ss");
            }, Dispatcher);

            try { Show(Encode(wb.DownloadString(new Uri("https://imienniczek.pl/dzis")))); }
            catch (Exception)
            {
                AppendParagraph(string.Empty, '\n' + new string(' ', 40) + "Brak połączenia internetowego");
                MoreAndLessInformationsButton.IsEnabled = false;
            }
            GetForecast();
        }

        #region Imieniny

        private void SetLinksAndDates(string data)
        {
            string[] links = new string[2], dates = new string[2];

            links[0] = data.Substring(data.IndexOf("slink") + 14);
            links[1] = links[0].Substring(links[0].IndexOf("slink") + 13);

            dates[0] = links[0].Substring(links[0].IndexOf("; ") + 2);
            dates[1] = links[1].Substring(links[1].IndexOf(">") + 1);

            links[0] = links[0].Substring(0, links[0].IndexOf("\""));
            links[1] = links[1].Substring(0, links[1].IndexOf("\""));

            dates[0] = dates[0].Substring(0, dates[0].IndexOf('<'));
            dates[1] = dates[1].Substring(0, dates[1].IndexOf(" &"));

            links[0] = "https://imienniczek.pl/" + links[0];
            links[1] = "https://imienniczek.pl/" + links[1];

            DayBeforeHyperLinkText.Text = dates[0];
            DayAfterHyperLinkText.Text = dates[1];

            DayBefore.NavigateUri = new Uri(links[0]);
            DayAfter.NavigateUri = new Uri(links[1]);
            Today.NavigateUri = new Uri("https://imienniczek.pl/dzis");
        }

        private void AppendParagraph(string data, string header)
        {
            try
            {
                Run run = new Run(header) { FontSize = 20 };
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(run);
                paragraph.Inlines.Add(data);
                flowDocument.Blocks.Add(paragraph);
                BasicInfoTextBox.Document = flowDocument;
            }
            catch (Exception) { }
        }

        private string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        private void Show(string data)
        {
            string toShowData = null, header = null;

            SetLinksAndDates(data);

            //-------------------------------------------------

            ProcessNameDayDataClass.NamesInfo(data, ref header, ref toShowData);
            AppendParagraph(toShowData, header);

            //-------------------------------------------------

            ProcessNameDayDataClass.DayInfo(data, ref toShowData);
            AppendParagraph(toShowData, string.Empty);

            //-------------------------------------------------

            ProcessNameDayDataClass.DayInfo2(data, ref toShowData);
            AppendParagraph(toShowData, string.Empty);

            //-------------------------------------------------------------

            ProcessNameDayDataClass.SaintsInfo(data, ref header, ref toShowData);
            AppendParagraph(toShowData, header);

            //--------------------------------------------------------------------

            ProcessNameDayDataClass.PolandEventsInfo(data, ref header, ref toShowData);
            AppendParagraph(toShowData, header);

            //--------------------------------------------------------------------

            ProcessNameDayDataClass.WorldEventsInfo(data, ref header, ref toShowData);
            AppendParagraph(toShowData, header);

            //---------------------------------------------------------------------

            ProcessNameDayDataClass.PersonBornInfo(data, ref header, ref toShowData);
            AppendParagraph(toShowData, header);
        }

        private void DayHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            BasicInfoTextBox.Document.Blocks.Clear();
            Show(Encode(wb.DownloadString(e.Uri)));
            BasicInfoTextBox.ScrollToHome();
        }

        private void MoreAndLessInformationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!showMoreData)
            {
                showMoreData = true;
                MoreAndLessInformationsButton.Content = "Pokaż mniej";
                MoreAndLessInformationsButton.Margin = new Thickness(27, 418, 0, 0);
                DayBeforeTemp.Margin = new Thickness(217, 0, 0, 9);
                DayAfterTemp.Margin = new Thickness(0, 0, 217, 9);
                TodayTemp.Margin = new Thickness(330, 0, 330, 9);
                BasicInfoTextBox.Height = 240;
                WeatherListView.Visibility = Visibility.Hidden;
                WeatherProgressBar.Visibility = Visibility.Hidden;
                DownloadWeatherLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                showMoreData = false;
                MoreAndLessInformationsButton.Content = "Pokaż więcej";
                MoreAndLessInformationsButton.Margin = new Thickness(27, 262, 0, 0);
                DayBeforeTemp.Margin = new Thickness(217, 0, 0, 166);
                DayAfterTemp.Margin = new Thickness(0, 0, 217, 166);
                TodayTemp.Margin = new Thickness(330, 0, 330, 166);
                BasicInfoTextBox.Height = 82;
                BasicInfoTextBox.ScrollToHome();
                if (!showMoreData) WeatherListView.Visibility = Visibility.Visible;
            }
        }

        #endregion Imieniny

        #region Pogoda

        private void GetForecast()
        {
            wb.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Web_DownloadStringCompleted);
            wb.DownloadStringAsync(new Uri("https://www.whatismyip.net/"));
        }

        private async void Web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                coord = GetPublicDataClass.GetCoords(e.Result.ToString())
                    .Replace("Lat ", string.Empty).Replace(" , Lon", string.Empty);

                var coords = WeatherClass.GetCoords(coord);

                string url = WeatherClass.CurrentUrl.Replace("@LOC@", "lat=" + coords.Item1 + "&lon=" + coords.Item2);
                url = url.Replace("@QUERY@=", string.Empty);

                await DisplayForecast(WeatherClass.Encode(wb.DownloadString(url)));

                WeatherProgressBar.Visibility = Visibility.Hidden;
                DownloadWeatherLabel.Visibility = Visibility.Hidden;
            }
            catch (Exception) { WeatherListView.Visibility = Visibility.Hidden; }
        }

        private void ResizeColumns()
        {
            if (WeatherListView.View is GridView gv)
            {
                foreach (var c in gv.Columns)
                {
                    if (double.IsNaN(c.Width)) c.Width = c.ActualWidth;
                    c.Width = double.NaN;
                }
            }
        }

        private Task DisplayForecast(string xml) => Task.Run(() =>
        {
            XmlDocument xml_doc = new XmlDocument();
            xml_doc.LoadXml(xml);

            weatherData.Clear();
            char degrees = (char)176;

            string temp, pres, img, direction, name, speed, humidity, location;
            temp = pres = img = direction = name = speed = humidity = location = null;

            foreach (XmlNode time_node in xml_doc.SelectNodes("//current"))
            {
                // Get city name
                XmlNode city_node = time_node.SelectSingleNode("city");
                location = city_node.Attributes["name"].Value;

                // Get the temperature.
                XmlNode temp_node = time_node.SelectSingleNode("temperature");
                temp = temp_node.Attributes["value"].Value;

                //Get the Pressure
                XmlNode pres_node = time_node.SelectSingleNode("pressure");
                pres = pres_node.Attributes["value"].Value;

                //Get the humidity
                XmlNode humNode = time_node.SelectSingleNode("humidity");
                humidity = humNode.Attributes["value"].Value;

                //Get the image name
                XmlNode imgNode = time_node.SelectSingleNode("weather");
                img = imgNode.Attributes["icon"].Value;
            }

            foreach (XmlNode wind_node in xml_doc.SelectNodes("//wind"))
            {
                //Get speed and wind name
                XmlNode speed_node = wind_node.SelectSingleNode("speed");
                speed = speed_node.Attributes["value"].Value;
                name = speed_node.Attributes["name"].Value;

                //Get wind direction
                XmlNode direction_node = wind_node.SelectSingleNode("direction");
                direction = direction_node.Attributes["name"].Value;
            }

            weatherData.Add(new WeatherData()
            {
                Location = location,
                Temperature = Convert.ToString(temp) + degrees + " C",
                Pressure = pres + " hPa",
                WindSpeed = Convert.ToString(speed) + " km/h",
                WindName = WeatherClass.Translate(name),
                WindDirection = WeatherClass.Translate(direction),
                Humidity = humidity + " %",
                ImageSource = "/Assets/Icons/WeatherIcons/" + img + ".ico"
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                ResizeColumns();

                WeatherListView.ItemsSource = weatherData;
                WeatherListView.Items.Refresh();

                WeatherListView.Visibility = Visibility.Visible;
            });
        });
    
        private void MoreWeatherDataButton_Click(object sender, RoutedEventArgs e)
        {
            WeatherWindow weatherWindow = new WeatherWindow();
            weatherWindow.SetLocation(coord, false);
            weatherWindow.Show();
        }

        #endregion Pogoda

        private void ShowIpScannerWindowButton_Click(object sender, RoutedEventArgs e) => new IpScannerMainWindow().Show();

        private void ShowRadioWindowButton_Click(object sender, RoutedEventArgs e) => new RadioWindow().Show();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    Close();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch(MessageBox.Show(messageBoxText: "Na pewno chcesz zakończyć pracę z programem", 
                caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
            {
                
                case MessageBoxResult.Yes:
                    Environment.Exit(0);
                    break;
                case MessageBoxResult.Cancel:
                case MessageBoxResult.No:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            DropShadowEffect dropShadowEffect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    ExitButton.Effect = dropShadowEffect;
                    break;
                case 2:
                    MinimizeButton.Effect = dropShadowEffect;
                    break;
                case 3:
                    ShowIpScannerWindowButton.Effect = dropShadowEffect;
                    break;
                case 4:
                    ShowRadioWindowButton.Effect = dropShadowEffect;
                    break;
                case 5:
                    ComputerUsageButton.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            switch(Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    ExitButton.Effect = null;
                    break;
                case 2:
                    MinimizeButton.Effect = null;
                    break;
                case 3:
                    ShowIpScannerWindowButton.Effect = null;
                    break;
                case 4:
                    ShowRadioWindowButton.Effect = null;
                    break;
                case 5:
                    ComputerUsageButton.Effect = null;
                    break;
                default:
                    break;
            }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DropShadowEffect dropShadowEffect = new DropShadowEffect { Color = new Color { R = 0, G = 191, B = 255 } };
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    ExitButton.Effect = dropShadowEffect;
                    break;
                case 2:
                    MinimizeButton.Effect = dropShadowEffect;
                    break;
                case 3:
                    ShowIpScannerWindowButton.Effect = dropShadowEffect;
                    break;
                case 4:
                    ShowRadioWindowButton.Effect = dropShadowEffect;
                    break;
                case 5:
                    ComputerUsageButton.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }

        public void ExitButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();

        private void MinimizeButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;

        private void ShowIpScannerWindowButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => new IpScannerMainWindow().Show();

        private void ShowRadioWindowButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => new RadioWindow().Show();

        private void ComputerUsageButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => new ComputerInfoWindow().Show();
    }

    class Events
    {
        public string Year { get; set; }
        public string EventName { get; set; }
    }

    class PersonBorn
    {
        public string Year { get; set; }
        public string Name { get; set; }
    }

    class WeatherData
    {
        public string Location { get; set; }
        public string Temperature { get; set; }
        public string Pressure { get; set; }
        public string WindSpeed { get; set; }
        public string WindName { get; set; }
        public string WindDirection { get; set; }
        public string Humidity { get; set; }
        public string ImageSource { get; set; }
    }
}
