using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Net.NetworkInformation;
using System.Net;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Data;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

namespace IpScanner.Windows
{
    public partial class IpScannerWindow 
    {
        List<User> items = new List<User>();

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;
        private string subnet;
        private int aktywnych = 0, nieznanych = 0, nieaktywnych = 0, iterator = 0, rangeIterator;
        private bool check = false;
        private static int waitingForResponses;
        private static int maxQueriesAtOneTime = 50;
        private DispatcherTimer RefreshTimer = new DispatcherTimer();

        public IpScannerWindow()
        {
            InitializeComponent();

            RefreshTimer.Tick += new EventHandler(RefreshTimerTick);
            RefreshTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            ipList.Items.Refresh();
            ipList.ItemsSource = items;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(paramName: nameof(sender));

            else if (e == null)
                throw new ArgumentNullException(paramName: nameof(e));

            switch (MessageBox.Show(messageBoxText: "Czy na pewno?", caption: "Exit",
                button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    e.Cancel = false;
                    break;
                case MessageBoxResult.No:
                    e.Cancel = true;
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!check)
            {
                if (IpTextBox.Text.Length < 1)
                {
                    MessageBox.Show(messageBoxText: "Aby rozpocząć skanowanie, musisz podać jakieś dane",
                        caption: "Error", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz rozpocząć skanowanie na podstawie własnego ip?",
                        caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            YourMaskSearch_Click(e: e, sender: sender);
                            PingButton_Click(sender: sender, e: e);
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Wprowadź własne ip i rozpocznij skanowanie",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            break;
                    }
                }

                else if (IpTextBox.Text != GetIp.Result())
                {
                    MessageBox.Show(messageBoxText: "Twoje ip różni się od podanego", caption: "Informacja",
                        button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz rozpocząć skanowanie na podstawie własnego ip?",
                        caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            YourMaskSearch_Click(e: e, sender: sender);
                            PingButton_Click(sender: sender, e: e);
                            break;
                        case MessageBoxResult.No:
                            PingButton_Click(sender: sender, e: e);
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    check = true;
                    pingButton.Content = "Stop";
                    StartThreadAsync();
                }
            }
            else ChangeStartButton(true);      
        }

        private void ChangeStartButton(bool czyPokazKoniec)
        {
            pingButton.Content = "Start";
            check = false;
            if (czyPokazKoniec) StopPing_Click(sender: new object(), e: new RoutedEventArgs());
        }

        private void StartThreadAsync()
        {
            try
            {
                var rangeTuple = SetEndSearch();
                if (rangeTuple.Item3)
                {
                    if (rangeTuple.Item2 >= 0)
                    {
                        Stan_label.Content = ("Stan: Szukam");
                        pingStatus.Minimum = rangeTuple.Item1;
                        rangeIterator = rangeTuple.Item1;
                        pingStatus.Maximum = rangeTuple.Item2;
                        pingStatus.Value = rangeTuple.Item1;
                        subnet = IpTextBox.Text;
                        if (items.Count > 0)
                        {
                            items.Clear();
                            ipList.Items.Refresh();
                            ipList.ItemsSource = items;
                        }

                        RefreshTimer.Start();

                        Parallel.For(rangeTuple.Item1, rangeTuple.Item2 + 1, async (indexer, state) =>
                        {
                            if (check)
                            {
                                await IncrementTask();
                                Task task = GetNameAsync(rangeTuple.Item2, indexer, subnet);
                                Interlocked.Increment(ref waitingForResponses);
                            }
                            else
                            {
                                state.Stop();
                                return;
                            }
                        });
                    }
                    else throw new ArgumentException();
                }
                else ChangeStartButton(false);
            }
            catch (Exception ex)
            {
                ChangeStartButton(false);
                MessageBox.Show(messageBoxText: $"Podano błędną wartość adresu IP lub zakresu przeszukiwania\n" +
                    $"Podaj poprawne i spróbuj ponownie\nTreść błędu: {ex.Message}", caption: "ERROR",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
        }        

        private Task IncrementTask() => Task.Run(() =>
        {
            while (waitingForResponses >= maxQueriesAtOneTime)
                Thread.Sleep(0);
        });

        private Task GetNameAsync(int koniec, int numer, string subnet) => Task.Run(() =>
        {
            if (numer <= koniec)
            {
                string subnetn = "." + numer.ToString(), nazwa = "";
                try { nazwa = Dns.GetHostEntry(address: IPAddress.Parse(ipString: subnet + subnetn)).HostName; }
                catch (Exception) { nazwa = "Nieznany"; }
                Function(numer, nazwa);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ChangeStartButton(false);
                    Stan_label.Foreground = Brushes.Green;
                    Stan_label.Content = ("Zakończono");
                    switch (MessageBox.Show(messageBoxText: $"Znalezionych urządzeń: {items.Count}\n" +
                        $"Aktywnych: {aktywnych}\nNieaktywnych: {nieaktywnych}\nNieznanych: {nieznanych} " +
                        $"\nCzy chcesz zapisać tabelę urządzeń? ", caption: "Staystyka",
                        button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
                    {
                        case MessageBoxResult.Yes:
                            switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                                        caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                            {
                                case MessageBoxResult.Yes:
                                    Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + 
                                        "/Tabela znalezionych urządzeń w sieci.txt");
                                    break;
                                case MessageBoxResult.No:
                                    MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                                    SaveFileDialog dlg = new SaveFileDialog
                                    {
                                        FileName = "Tabela znalezionych urządzeń w sieci",
                                        DefaultExt = ".text",
                                        Filter = "Text documents (.txt)|*.txt"
                                    };
                                    if (Dialog(dlg) == true) Save(dlg.FileName);
                                    break;
                            }
                            break;
                        case MessageBoxResult.No:
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            break;
                    }
                }));
            }
        });

        private Task RefreshTask => Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ipList.Items.Refresh();
                    ipList.ItemsSource = items;
                }));
            }
        });

        private void IncrementResponses()
        {
            Interlocked.Increment(ref waitingForResponses);
            PrintWaitingForResponses();
        }

        private void DecrementResponses()
        {
            Interlocked.Decrement(ref waitingForResponses);
            PrintWaitingForResponses();
        }

        private void PrintWaitingForResponses()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                WaitingForResponseLabel.Content = ($"Aktywnych zapytań: {waitingForResponses}");

                if (waitingForResponses == 0)
                {
                    pingButton.Content = "Start";
                    check = false;
                    StopPing_Click(sender: new object(), e: new RoutedEventArgs());
                }
            }));
        }

        private Tuple<int, int, bool> SetEndSearch()
        {
            if (RangeTextBox.Text == "Podaj zakres (np. 10-254)")
            {
                switch (MessageBox.Show(messageBoxText: "Nie podano końca oraz początku zakresu\n" +
                    "Domyślny koniec wynosi 0-254\nCzy zgadzasz się?",
                    caption: "Informacja", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
                {
                    case MessageBoxResult.Yes:
                        RangeTextBox.Text = ("0-254");
                        return Tuple.Create(0, 254, true);
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Podaj nowy koniec zakresu i zatwierdź ponownie",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        return Tuple.Create(-1, -1, false);
                    case MessageBoxResult.Cancel:
                        MessageBox.Show(messageBoxText: "Szukanie nie zostało rozpoczęte",
                             caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        return Tuple.Create(-1, -1, false);
                }
            }

            else
            {
                try
                {                    
                    if(CheckPortRangeClass.ResultIpScanner(RangeTextBox.Text, IpTextBox.Text + "1")) //+1 zeby ip wygladalo 192.168.1.1
                    {
                        return Tuple.Create(Convert.ToInt32(RangeTextBox.Text.Substring(0, RangeTextBox.Text.IndexOf('-'))),
                            Convert.ToInt32(RangeTextBox.Text.Substring(RangeTextBox.Text.IndexOf('-') + 1)), true);                     
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: $"Podano błędną wartość zakresu\nPodaj inną i spróbuj ponownie\n" +
                        $"Treść błędu: {ex.Message}",caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                }
            }
            return Tuple.Create(-1, -1, false);
        }

        private void Function(int i, string nazwa)
        {
            string subnetn = $".{i.ToString()}";

            Ping ping = new Ping();
            PingReply pingReply = null;
            try
            {
                pingReply = ping.Send(subnet + subnetn, timeout: 100);

                Application.Current.Dispatcher.Invoke(callback: new Action(() =>
                {
                    Stan_label.Content = ($"Postęp: {subnet}{subnetn}");

                    DecrementResponses();

                    if (pingReply.Status == IPStatus.Success)
                    {
                        try
                        {
                            aktywnych++;
                            iterator++;
                            rangeIterator++;
                            items.Add(item: new User()
                            {
                                Lp = i,
                                Ip = subnet + subnetn,
                                Nazwa = nazwa,
                                Mac = GetMac.Result(ipAddress: subnet + subnetn),
                                Stan = "Włączony"
                            });
                            ipList.SelectedItem = ipList.Items.Count;
                            ipList.ScrollIntoView(item: ipList.SelectedItem);
                        }
                        catch (Exception)
                        {
                            nieznanych++;
                            iterator++;
                            rangeIterator++;
                            items.Add(item: new User()
                            {
                                Lp = i,
                                Ip = subnet + subnetn,
                                Nazwa = "Nieznany",
                                Mac = GetMac.Result(ipAddress: subnet + subnetn),
                                Stan = "Włączony"
                            });
                            ipList.SelectedItem = ipList.Items.Count;
                            ipList.ScrollIntoView(item: ipList.SelectedItem);
                        }
                    }

                    else
                    {
                        nieaktywnych++;
                        iterator++;
                        rangeIterator++;
                        if ((bool)showUnknown_checkbox.IsChecked)
                        {
                            items.Add(item: new User()
                            {
                                Lp = i,
                                Ip = subnet + subnetn,
                                Nazwa = "Nieznany",
                                Mac = "Nieznany",
                                Stan = "Wyłączony"
                            });
                            ipList.SelectedItem = ipList.Items.Count;
                            ipList.ScrollIntoView(item: ipList.SelectedItem);
                        }
                    }
                    ipList.SelectedItem = ipList.Items.Count;
                    ipList.ScrollIntoView(item: ipList.SelectedItem);
                    pingStatus.Value = rangeIterator;

                    ICollectionView view1 = CollectionViewSource.GetDefaultView(ipList.ItemsSource);
                    view1.Refresh();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR {i} {nazwa} {subnet + subnetn}");
                MessageBox.Show(messageBoxText: $"Wystąpił błąd\n{ex.Message}", caption: "ERROR",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                MessageBox.Show(messageBoxText: "Szukanie zostało zatrzymane\nSpróbuj ponownie", caption: "Informacja",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                Application.Current.Dispatcher.Invoke(callback: new Action(() => { ChangeStartButton(false); }));
            }       
        }

        private void Save(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"Lp.         Adres ip:            Nazwa:          " +
                    "            Adres MAC:                 Stan:");
                writer.WriteLine(string.Empty.PadLeft(96, paddingChar: '='));
                for (int i = 0; i < items.Count; i++)
                {
                    writer.WriteLine($"{items[i].Lp}          {items[i].Ip}          {items[i].Nazwa}          " +
                        $"          {items[i].Mac}          {items[i].Stan}");
                    writer.WriteLine(string.Empty.PadLeft(96, paddingChar: '='));
                }
            }
        }

        private static bool? Dialog(SaveFileDialog dlg) => dlg.ShowDialog();

        private void StopPing_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            else if (e == null)
                throw new ArgumentNullException(nameof(e));

            check = false;

            switch (MessageBox.Show(messageBoxText: $"Znalezionych urządzeń: {items.Count}\n" +
                   $"Aktywnych: {aktywnych}\nNieaktywnych: {nieaktywnych}\nNieznanych: {nieznanych} " +
                   $"\nCzy chcesz zapisać tabelę urządzeń? ", caption: "Staystyka",
                   button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
            {
                case MessageBoxResult.Yes:
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                             caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + 
                                "/Tabela znalezionych urządzeń w sieci.txt");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                            {
                                FileName = "Tabela znalezionych urządzeń w sieci",
                                DefaultExt = ".text",
                                Filter = "Text documents (.txt)|*.txt"
                            };
                            if (Dialog(dlg) == true) Save(dlg.FileName);
                            break;
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    break;
                default:
                    break;
            }
        }

        private void YourMaskSearch_Click(object sender, RoutedEventArgs e)
        {
            IpTextBox.Text = GetIp.Result();
            MessageBox.Show(messageBoxText: $"Znaleziono: {GetIp.Result()}", caption: "Informacja", 
                button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            else if (e == null)
                throw new ArgumentNullException(nameof(e));

            switch (e.Key)
            {
                case Key.Enter:
                    PingButton_Click(sender: sender, e: e);
                    break;
                case Key.F3:
                    ipList.Focus();
                    break;
                case Key.F4:
                    ipList.Focus();
                    break;
                case Key.F2:
                    RangeTextBox.Focus();
                    break;
                case Key.F1:
                    IpTextBox.Focus();
                    break;
                case Key.LeftCtrl:
                    switch (MessageBox.Show(messageBoxText: "Naciśnięto lewy ctrl\n\nCzy chcesz rozpocząć wyszukiwanie na " +
                        "podstawie swojego ip?",
                       caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            YourMaskSearch_Click(sender: sender, e: e);
                            break;
                        case MessageBoxResult.No:
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void RangeTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => RangeTextBox.Clear();

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(ipListFilterTextBox.Text)) return true;
            else
            {
                switch(FilterComboBox.SelectedIndex)
                {
                    case 0:
                        return ((item as User).Lp.ToString().IndexOf(ipListFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 1:
                        return ((item as User).Ip.IndexOf(ipListFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 2:
                        return ((item as User).Nazwa.IndexOf(ipListFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 3:
                        return ((item as User).Mac.IndexOf(ipListFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 4:
                        return ((item as User).Stan.IndexOf(ipListFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    default:
                        return true;
                }            
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch(Convert.ToByte(((MenuItem)sender).Tag))
            {
                case 1:
                    SshTelnetWindow.SshTelnet.SetText("SSH info", "22", "SSH_log", items[0].Ip);
                    IpScannerMainWindow.ipScannerMainWindow.TabControlIndex = 5;
                    break;

                case 2:
                    SshTelnetWindow.SshTelnet.SetText("Telnet info", "23", "Telnet_log", items[0].Ip);
                    IpScannerMainWindow.ipScannerMainWindow.TabControlIndex = 5;
                    break;

                case 3:
                    PingWindow.pingWindow.AutoPing(items[0].Ip, true);
                    IpScannerMainWindow.ipScannerMainWindow.TabControlIndex = 3;
                    break;

                case 4:
                    TracertWindow.tracertWindow.AutoTracert(ip: items[0].Ip);
                    IpScannerMainWindow.ipScannerMainWindow.TabControlIndex = 4;
                    break;

                case 5: 
                    HTTP hTTP = new HTTP();
                    hTTP.SetLink(link: items[0].Ip);
                    hTTP.Show();
                    break;

                case 6:
                    Process.Start("explorer.exe", @"ftp://" + items[0].Ip);
                    break;

                case 7:
                    Clipboard.SetText(text: items[0].Ip);
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano adres IP: {items[0].Ip}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;

                case 8:
                    Clipboard.SetText(text: items[0].Mac);
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano adres MAC: {items[0].Mac}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;

                case 9:
                    Clipboard.SetText(text: items[0].Nazwa);
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano nazwę: {items[0].Nazwa}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;

                default:
                    break;
            }
        }

        private void IpListFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ipList.Items.Count > 0)
            {
                CollectionViewSource.GetDefaultView(ipList.ItemsSource).Refresh();
                view.Filter = UserFilter;
            }
        }

        private void RangeTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            else if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (RangeTextBox.Text.Length == 0) RangeTextBox.Text = "Podaj zakres (np. 10-254)";

            else if (RangeTextBox.Text == "321edgeedge123")
            {
                switch (MessageBox.Show(messageBoxText: "Czy chcesz pokazać wszystkie okna?",
                    caption: "Informacja", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }
        }

        private void SortClick_Click(object sender, RoutedEventArgs e)
        {         
            ipList.SelectedItem = null;
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ipList.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ipList.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }
}