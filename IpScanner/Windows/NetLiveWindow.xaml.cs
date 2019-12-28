using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace IpScanner.Windows
{
    public partial class NetLiveWindow
    {
        private const string CategoryName = "Network Interface";
        bool CzyTest = false, internetConnection = true;
        double uploadedSum = 0, downloadedSum = 0;
        private PerformanceCounter performanceCounterSent = null;
        private PerformanceCounter performanceCounterReceived = null;
        private PerformanceCounterCategory performanceCounterCategory = null;
        private List<string> NicList = new List<string>();
        private List<double> checkNullDownloadList = new List<double>(), checkNullUploadList = new List<double>();
        private DispatcherTimer timer;

        public NetLiveWindow()
        {
            InitializeComponent();

            CheckNet();

            DispatcherTimer netTimer = new DispatcherTimer(new TimeSpan(0, 0, 10), DispatcherPriority.Normal, 
                delegate { CheckNet(); }, Dispatcher);

            DownloadValues = new ChartValues<ObservableValue> { };
            UploadValues = new ChartValues<ObservableValue> { };

            SeriesCollection = new SeriesCollection
            {
                new LineSeries { Values = DownloadValues, },
                new LineSeries { Values = UploadValues, }
            };

            Fill();

            DataContext = this;
        }

        private async void CheckNet()
        {
            if (await NetLiveCheckClass.CheckInternetConnection())
            {
                internetConnection = true;
                StateLabel.Content = "Stan: Monitorowanie w trakcie";
            }
            else
            {
                internetConnection = false;
                StateLabel.Content = "Stan: Oczekwianie na połączenie";
            }
        }

        private async void Timer_TickAsync(object sender, EventArgs e)
        {
            if (internetConnection)
            {
                if (CzyTest)
                {
                    double download = Math.Round((performanceCounterReceived.NextValue() / 1024), 2);
                    double upload = Math.Round((performanceCounterSent.NextValue() / 1024), 2);

                    uploadedSum += upload;
                    downloadedSum += download;

                    DownloadLabel.Content = ($"Pobieranie: {NetLiveCheckClass.CheckDownload(download).Item1} " +
                        $"{NetLiveCheckClass.CheckDownload(download).Item2}");
                    UploadLabel.Content = ($"Wysyłanie: {NetLiveCheckClass.CheckUpload(upload).Item1} " +
                        $"{NetLiveCheckClass.CheckUpload(upload).Item2}");

                    DownloadedLabel.Content = ($"Pobrano: {NetLiveCheckClass.CheckDownloaded(downloadedSum).Item1} " +
                        $"{NetLiveCheckClass.CheckDownloaded(downloadedSum).Item2}");
                    UploadedLabel.Content = ($"Wysłano: {NetLiveCheckClass.CheckUploaded(uploadedSum).Item1} " +
                        $"{NetLiveCheckClass.CheckUploaded(uploadedSum).Item2}");

                    double downloadChartValue = Math.Round(download / 125, 2);
                    double uploadChartValue = Math.Round(upload / 125, 2);

                    DownloadValues.Add(new ObservableValue(downloadChartValue));
                    UploadValues.Add(new ObservableValue(uploadChartValue));

                    if (!(bool)IgnoreWarningCheckBox.IsChecked)
                    {
                        checkNullDownloadList.Add(downloadChartValue);
                        checkNullUploadList.Add(uploadChartValue);

                        if (checkNullDownloadList.Count == 10 && checkNullUploadList.Count == 10
                            && await CheckValues(checkNullDownloadList, checkNullUploadList))
                        {
                            switch (MessageBox.Show(messageBoxText: "Wybrany interfejs sieciowy nie wykazuje ruchu\n" +
                                "Czy chcesz wybrać inny?", caption: "Pytanie", button: MessageBoxButton.YesNoCancel, 
                                icon: MessageBoxImage.Question))
                            {
                                case MessageBoxResult.Yes:
                                    Random random = new Random();
                                    int s;
                                    do { s = random.Next(0, NICComboBox.Items.Count); } while (s == NICComboBox.SelectedIndex);
                                    NICComboBox.SelectedIndex = s;
                                    MessageBox.Show(messageBoxText: $"Wybrany interfejs sieciowy: " +
                                        $"{NICComboBox.SelectedItem.ToString()}", caption: "Informacja", 
                                        button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                                    break;
                                case MessageBoxResult.No:
                                case MessageBoxResult.Cancel:
                                    IgnoreWarningCheckBox.IsChecked = true;
                                    break;
                                default:
                                    break;
                            }
                            checkNullDownloadList.Clear();
                            checkNullUploadList.Clear();
                        }
                    }

                    if (DownloadValues.Count > 21 && UploadValues.Count > 21)
                    {
                        DownloadValues.RemoveAt(0);
                        UploadValues.RemoveAt(0);
                    }
                }
            }
        }

        private Task<bool> CheckValues(List<double> downloadList, List<double> uploadList) => Task.Run(() =>
        {
            for (int i = 0; i < downloadList.Count; i++)
            {
                if (downloadList[i] != 0 && uploadList[i] != 0) return false;
            }
            return true;
        });

        private async void Fill()
        {
            try
            {
                await InitTask();
                NICComboBox.Items.Clear();
                foreach (string s in NicList) NICComboBox.Items.Add(s);

                performanceCounterSent = new PerformanceCounter(categoryName: CategoryName, counterName: "Bytes Sent/sec", 
                    instanceName: NICComboBox.Items[0].ToString());

                performanceCounterReceived = new PerformanceCounter(categoryName: CategoryName, counterName: "Bytes Received/sec", 
                    instanceName: NICComboBox.Items[0].ToString());

                NICComboBox.SelectedIndex = 0;

                CzyTest = true;
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
                timer.Tick += Timer_TickAsync;
                timer.IsEnabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(messageBoxText: $"Wystąpił błąd\nTreść błędu: {ex.Message}", 
                    caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
        }

        private Task InitTask() => Task.Run(() =>
        {
            performanceCounterCategory = new PerformanceCounterCategory(CategoryName);
            foreach (string s in performanceCounterCategory.GetInstanceNames()) NicList.Add(s);
        });

        private void NICComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string instance = NICComboBox.SelectedItem.ToString();
            performanceCounterSent.InstanceName = instance;
            performanceCounterReceived.InstanceName = instance;
            uploadedSum = 0;
            downloadedSum = 0;

            if (checkNullDownloadList.Count > 0 && checkNullUploadList.Count > 0)
            {
                checkNullDownloadList.Clear();
                checkNullUploadList.Clear();
            }

            if (DownloadValues.Count > 0) DownloadValues.Clear();
            if (UploadValues.Count > 0) UploadValues.Clear();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            switch (e.Key)
            {
                case Key.Enter:
                    if (CzyTest) StopGraphCheckBox.IsChecked = false;
                    else StopGraphCheckBox.IsChecked = true;
                    break;
                default:
                    break;
            }
        }
        
        public ChartValues<ObservableValue> DownloadValues { get; set; }

        public ChartValues<ObservableValue> UploadValues { get; set; }

        private void StopGraphCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CzyTest = false;
            timer.Stop();
            StateLabel.Content = "Stan: Monitorowanie zatrzymane";
        }

        private void StopGraphCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CzyTest = true;
            timer.Start();
            StateLabel.Content = "Stan: Monitorowanie w trakcie";
        }

        public SeriesCollection SeriesCollection { get; set; }
    }
}