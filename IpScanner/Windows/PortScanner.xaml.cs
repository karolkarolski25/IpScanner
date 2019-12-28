using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace IpScanner.Windows
{
    public partial class PortScanner
    {
        List<PortClass> portClassList = new List<PortClass>();

        int startPort, endPort, openedPorts = 0, closedPorts = 0, index = 0;
        static int waitingForResponses;
        static int maxQueriesAtOneTime = 100;
        bool check = false;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        CollectionView view;

        public PortScanner() => InitializeComponent();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    StartButton_Click(sender: sender, e: e);
                    break;
                default:
                    break;
            }
        }   

        public void SetIp(string ip) => IpTextBox.Text = ip;      

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!check)
            {
                try
                {
                    if (CheckPortRangeClass.Result(RangePortTextBox.Text, IpTextBox.Text))
                    {
                        check = true;
                        StartButton.Content = ("Stop");

                        ProgessBar.Visibility = Visibility.Visible;
                        StateLabel.Content = ("Stan: Monitorowanie");

                        openedPorts = 0;
                        closedPorts = 0;

                        IPAddress.TryParse(IpTextBox.Text, out IPAddress IpAddress);

                        startPort = Convert.ToInt32(RangePortTextBox.Text.Substring(0, RangePortTextBox.Text.IndexOf('-')));
                        endPort = Convert.ToInt32(RangePortTextBox.Text.Substring(RangePortTextBox.Text.IndexOf('-') + 1));

                        ThreadPool.QueueUserWorkItem(StartScan, IpAddress);

                        StateLabel.Content = ("Stan: Monitorowanie");
                    }
                    else throw new Exception("Podany zakres portów jest nieprawidłowy\nPodaj poprawny i spróbuj ponownie");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: $"Wystąpił błąd podczas próby skanowania otwartych portów\n" +
                        $"Treść błędu: {ex.Message}", caption: "Informacja", button: MessageBoxButton.OK, 
                        icon: MessageBoxImage.Information);
                }
            }
            else
            {
                StartButton.Content = "Start";
                check = false;
            }
        }

        private void StartScan(object o)
        {
            IPAddress ipAddress = o as IPAddress;

            Parallel.For(startPort, endPort + 1, (indexer, state) =>
            {
                if (check)
                {
                    Application.Current.Dispatcher.Invoke(callback: new Action(() => 
                    { ScanPortLabel.Content = ($"Skanowanie portu: {indexer}"); }));

                    while (waitingForResponses >= maxQueriesAtOneTime)
                        Thread.Sleep(0);

                    try
                    {
                        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        s.BeginConnect(new IPEndPoint(ipAddress, indexer), EndConnect, s);
                        Interlocked.Increment(ref waitingForResponses);
                    }
                    catch (Exception) { }
                }
                else
                {
                    state.Stop();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        StateLabel.Content = ("Stan: Zatrzymano");
                        ProgessBar.Visibility = Visibility.Hidden;
                        portClassList.Clear();
                        index = 0;
                    }));
                    return;
                }
            });
        }

        private void EndConnect(IAsyncResult ar)
        {
            try
            {
                DecrementResponses();

                Socket s = ar.AsyncState as Socket;

                s.EndConnect(ar);

                if (s.Connected)
                {
                    int openPort = Convert.ToInt32(s.RemoteEndPoint.ToString().Split(':')[1]);

                    index++;

                    portClassList.Add(item: new PortClass()
                    {
                        Lp = index,
                        Numer = openPort.ToString(),
                        Nazwa = "OK",
                        Status = "Otwaty"
                    });

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ScanPortResultListView.Items.Refresh();
                        ScanPortResultListView.ItemsSource = portClassList;
                        ScanPortResultListView.SelectedItem = ScanPortResultListView.Items.Count;
                        ScanPortResultListView.ScrollIntoView(ScanPortResultListView.SelectedItem);
                        view = (CollectionView)CollectionViewSource.GetDefaultView(ScanPortResultListView.ItemsSource);

                        ConnectedPortLabel.Content = ($"Połączono TCP na porcie: {openPort}");
                    }));

                    openedPorts++;

                    s.Disconnect(true);
                }
            }
            catch (Exception) { closedPorts++; }

            Application.Current.Dispatcher.Invoke(callback: new Action(() =>
            {
                OpenedPortLabel.Content = ($"Otwartych portów: {openedPorts}");
                ClosedPortLabel.Content = ($"Zamkniętych portów: {closedPorts}");
            }));
        }

        private void IncrementResponses()
        {
            Interlocked.Increment(ref waitingForResponses);
            PrintWaitingForResponses();
        }

        private void RangePortTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => RangePortTextBox.Clear();

        private void RangePortTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (RangePortTextBox.Text.Length == 0) RangePortTextBox.Text = ("np. 15-5900 (0 - 65535)");
        }

        private void IpTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => IpTextBox.Clear();

        private void IpTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (IpTextBox.Text.Length == 0) IpTextBox.Text = ("np. 192.168.1.1");
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ScanPortResultListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ScanPortResultListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void PortFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (portClassList.Count > 0)
            {
                CollectionViewSource.GetDefaultView(ScanPortResultListView.ItemsSource).Refresh();
                view.Filter = UserFilter;
            }
        }

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(PortFilterTextBox.Text)) return true;
            else
            {
                switch (FilterComboBox.SelectedIndex)
                {
                    case 0:
                        return true;
                    case 1:
                        return ((item as PortClass).Lp.ToString().IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 2:
                        return ((item as PortClass).Numer.IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 3:
                        return ((item as PortClass).Nazwa.IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 4:
                        return ((item as PortClass).Status.IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    default:
                        return true;
                }
            }
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
                WaitForResponseLabel.Content = ($"Czekam na odpowiedz od {waitingForResponses} portów");
                
                if(waitingForResponses == 0)
                {
                    StateLabel.Content = "Stan: Zakończono";
                    check = false;
                    ProgessBar.Visibility = Visibility.Hidden;
                }
            }));
        }
    }
}