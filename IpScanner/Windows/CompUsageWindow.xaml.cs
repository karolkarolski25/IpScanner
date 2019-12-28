using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System;
using System.Windows.Threading;
using System.Management;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace IpScanner.Windows
{
    public partial class CompUsageWindow
    {
        private bool CzyTest = false;
        private long ramSum = 0;
        private string whichDisk = null;
        private int cores = 0;

        private StringBuilder stringBuilder = new StringBuilder();

        public ChartValues<ObservableValue> CpuValues { get; set; }
        public ChartValues<ObservableValue> RamValues { get; set; }
        public ChartValues<ObservableValue> AvailableRamValues { get; set; }
        public ChartValues<ObservableValue> BusyRamValues { get; set; }
        public ChartValues<ObservableValue> DiskValues { get; set; }
        public ChartValues<ObservableValue> ReadDiskValues { get; set; }
        public ChartValues<ObservableValue> WriteDiskValues { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public SeriesCollection CpuSeriesCollection { get; set; }

        PerformanceCounter pc = new PerformanceCounter("Processor Information", "% Processor Time");
        PerformanceCounterCategory cat = new PerformanceCounterCategory("Processor Information");
        string[] instances;
        Dictionary<string, CounterSample> cs = new Dictionary<string, CounterSample>();

        PerformanceCounter CpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter RamUasge = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        PerformanceCounter AvailableRam = new PerformanceCounter("Memory", "Available MBytes");
        PerformanceCounter DiskPercentageUsage = null;
        PerformanceCounter DiskReadUsage = null;
        PerformanceCounter DiskWriteUsage = null;

        private DispatcherTimer timer = null;

        public CompUsageWindow()
        {
            string data = null;
            InitializeComponent();

            CpuValues = new ChartValues<ObservableValue> { };
            RamValues = new ChartValues<ObservableValue> { };
            AvailableRamValues = new ChartValues<ObservableValue> { };
            BusyRamValues = new ChartValues<ObservableValue> { };
            DiskValues = new ChartValues<ObservableValue> { };
            ReadDiskValues = new ChartValues<ObservableValue> { };
            WriteDiskValues = new ChartValues<ObservableValue> { };

            SeriesCollection = new SeriesCollection
            {
                new LineSeries { Values = CpuValues },
                new LineSeries { Values = RamValues },
                new LineSeries { Values = AvailableRamValues },
                new LineSeries { Values = BusyRamValues },
                new LineSeries { Values = DiskValues },
                new LineSeries { Values = ReadDiskValues },
                new LineSeries { Values = WriteDiskValues }
            };

            instances = cat.GetInstanceNames();

            cores = instances.Length;

            CpuSeriesCollection = new SeriesCollection { };

            for (int i = 0; i < cores; i++)
            {
                if (i == cores - 1) continue;
                else
                {
                    string s = instances[i];
                    pc.InstanceName = s;
                    cs.Add(s, pc.NextSample());

                    if (i == cores - 2) data = "Łącznie: (%)";
                    else data = $"Rdzeń: {(i + 1).ToString()} (%)";

                    CpuSeriesCollection.Add(new LineSeries
                    {
                        Title = data,
                        Values = new ChartValues<ObservableValue> { },
                        Fill = Brushes.Transparent
                    });
                }
            }

            MainVoidTask();

            DataContext = this;
        }

        private Task<List<string>> MainTask() => Task.Run(() =>
        {
            List<string> list = new List<string>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementObject mo in mos.Get()) ramSum += Convert.ToInt64(mo["Capacity"]);
            ramSum = (ramSum / 1024) / 1024;

            mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in mos.Get()) list.Add(mo["Name"].ToString());
            return list;
        });

        private async Task FillDiskTask() => await Task.Run(() =>
        {
            Parallel.ForEach(new PerformanceCounterCategory("PhysicalDisk").GetInstanceNames(), data =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (data == "_Total") DiskComboBox.Items.Add("Razem");
                    else DiskComboBox.Items.Add(data);
                }));
            });
        });

        private Task MainVoidTask() => Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                CzyTest = true;

                Parallel.ForEach(await MainTask(), data => { Application.Current.Dispatcher.Invoke(new Action(() => 
                { CpuName.Content = ($"Prcesor: {data}"); })); });

                await FillDiskTask();

                DiskComboBox.SelectedIndex = 0;

                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };

                timer.Tick += Timer_Tick;

                timer.IsEnabled = true;
            }));
        });

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (CzyTest)
            {
                long availableRam = Convert.ToInt64(AvailableRam.NextValue());
                double Cpu = Math.Round(CpuUsage.NextValue(), 2);
                double Ram = Math.Round(RamUasge.NextValue(), 2);
                double Disk = Math.Round(DiskPercentageUsage.NextValue(), 2);
                double DiskRead = Math.Round((DiskReadUsage.NextValue() / 1024) / 1024, 2);
                double DiskWrite = Math.Round((DiskWriteUsage.NextValue() / 1024) / 1024, 2);
                double BusyRam = ramSum - availableRam;

                CpuUsageLabel.Content = ($"Użycie CPU: {Math.Round(Cpu, 2)} %");
                RamUsageLabel.Content = ($"Użycie RAM: {BusyRam} MB / {ramSum} MB ({Math.Round(Ram, 2)} %) " +
                    $"Dostępne: {availableRam} MB");
                HardDiskLabel.Content = ($"Użycie Dysku twardego: {Disk} %");
                ReadDiskLabel.Content = ($"Odczyt: {DiskRead} MB/s");
                WriteDiskLabel.Content = ($"Zapis: {DiskWrite} MB/s");

                if (!(bool)CPUCheckBox.IsChecked)
                {
                    for (int i = 0; i < cores - 1; i++)
                    {
                        string s = instances[i];
                        pc.InstanceName = s;
                        CpuSeriesCollection[i].Values.Add(new ObservableValue(Math.Round(Calculate(cs[s], pc.NextSample()), 2)));
                        cs[s] = pc.NextSample();
                    }

                    CpuValues.Add(new ObservableValue(Cpu));

                    if (CpuValues.Count > 9)
                    {
                        for (int i = 0; i < cores; i++)
                        {
                            if (i == cores - 1) continue;
                            else CpuSeriesCollection[i].Values.RemoveAt(0);
                        }
                    }
                }

                if (!(bool)RamCheckBox.IsChecked)
                {
                    RamValues.Add(new ObservableValue(Ram));
                    AvailableRamValues.Add(new ObservableValue(availableRam));
                    BusyRamValues.Add(new ObservableValue(BusyRam));

                    if (RamValues.Count > 9)
                    {
                        RamValues.RemoveAt(0);
                        AvailableRamValues.RemoveAt(0);
                        BusyRamValues.RemoveAt(0);
                    }
                }

                if (!(bool)DiskUsageCheckBox.IsChecked)
                {
                    DiskValues.Add(new ObservableValue(Disk));

                    if (DiskValues.Count > 9)
                    {
                        DiskValues.RemoveAt(0);
                    }
                }

                if (!(bool)WriteAndReadCheckBox.IsChecked)
                {
                    ReadDiskValues.Add(new ObservableValue(DiskRead));
                    WriteDiskValues.Add(new ObservableValue(DiskWrite));

                    if (ReadDiskValues.Count > 9)
                    {
                        ReadDiskValues.RemoveAt(0);
                        WriteDiskValues.RemoveAt(0);
                    }
                }
            }
        }

        private static double Calculate(CounterSample oldSample, CounterSample newSample)
        {
            double timeInterval = newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec;
            if (timeInterval != 0) return 100 * (1 - ((newSample.RawValue - oldSample.RawValue) / timeInterval));
            return 0;
        }

        private void DiskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {    
            whichDisk = DiskComboBox.SelectedItem.ToString();
            if (whichDisk == "Razem") whichDisk = "_Total";

            DiskPercentageUsage = new PerformanceCounter("PhysicalDisk", "% Disk Read Time", whichDisk);
            DiskReadUsage = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", whichDisk);
            DiskWriteUsage = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", whichDisk);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (CzyTest)
                    {
                        StopCheckBox.IsChecked = true;
                        CzyTest = false;
                        StateLabel.Content = "Stan: Monitorowanie zatrzymane";
                    }
                    else
                    {
                        StopCheckBox.IsChecked = false;
                        CzyTest = true;
                        StateLabel.Content = "Stan: Monitorowanie w trakcie";
                    }
                    break;
                default:
                    break;
            }
        }

        private void StopAllChartsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CPUCheckBox.IsChecked = true;
            DiskUsageCheckBox.IsChecked = true;
            RamCheckBox.IsChecked = true;
            WriteAndReadCheckBox.IsChecked = true;
        }

        private void StopAllChartsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CPUCheckBox.IsChecked = false;
            DiskUsageCheckBox.IsChecked = false;
            RamCheckBox.IsChecked = false;
            WriteAndReadCheckBox.IsChecked = false;
        }

        private void StopCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CzyTest = false;
            StateLabel.Content = "Stan: Monitorowanie zatrzymane";
        }

        private void StopCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CzyTest = true;
            StateLabel.Content = "Stan: Monitorowanie w trakcie";
        }
    }
}