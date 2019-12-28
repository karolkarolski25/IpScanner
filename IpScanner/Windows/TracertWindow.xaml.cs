using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IpScanner.Windows
{
    public partial class TracertWindow
    {
        internal static TracertWindow tracertWindow;

        public TracertWindow()
        {
            InitializeComponent();
            TracertHostTextBox.Focus();
            tracertWindow = this;
        }

        private Task FillTask(string ipAddress, int maxHops, int timeout) => Task.Run(() => Fill(ipAddress, maxHops, timeout));

        private Task <bool> CheckIpTask(string ip) => Task.Run(() => TracertPingClass.CheckIp(ip));

        private Task <string> DoGetHostEntryTask(string ip) => Task.Run(() => TracertPingClass.DoGetHostEntry(ip));

        public async void AutoTracert(string ip)
        {
            await Task.Delay(1750);
            TracertHostTextBox.Text = ip;
            HowMuchTextBox.Text = "30";
            TimeoutTextBox.Text = "1000";
            TracertButton_Click(sender: new object(), e: new RoutedEventArgs());
        }

        private async void TracertButton_Click(object sender, RoutedEventArgs e)
        {
            TracertResultTextBox.Clear();
            if (string.IsNullOrEmpty(HowMuchTextBox.Text) && string.IsNullOrEmpty(TimeoutTextBox.Text)
                && string.IsNullOrEmpty(TracertHostTextBox.Text))
            {
                MessageBox.Show(messageBoxText: "Nie podano żadnych danych\nPodaj adres, długość opóźnienia oraz ilość bram " +
                    "i spróbuj ponownie", caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else if (string.IsNullOrEmpty(HowMuchTextBox.Text))
            {
                HowMuchTextBox.Focus();
                switch (MessageBox.Show("Nie podano maksymalnej ilości bram\nDomyślna wartość wynosi 30\nZgadzasz się?",
                    caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        HowMuchTextBox.Text = "30";
                        TracertButton_Click(sender: sender, e: e);
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Podaj nową ilość i spróbuj ponownie",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }

            else if (string.IsNullOrEmpty(TracertHostTextBox.Text))
            {
                TracertHostTextBox.Focus();
                MessageBox.Show(messageBoxText: "Nie podano adresu hosta\nPodaj adres i spróbuj ponownie",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else if (string.IsNullOrEmpty(TimeoutTextBox.Text))
            {
                TimeoutTextBox.Focus();
                switch (MessageBox.Show("Nie podano opóźnienia\nDomyślnie ilość wynosi 10000 ms\nZgadzasz się?",
                    caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        TimeoutTextBox.Text = "10000";
                        TracertButton_Click(sender: sender, e: e);
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Podaj nową ilość i spróbuj ponownie",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }

            else
            {
                try
                {
                    if (await CheckIpTask(TracertHostTextBox.Text) && TracertPingClass.CheckData(HowMuchTextBox.Text)
                        && TracertPingClass.CheckData(TimeoutTextBox.Text))
                    {
                        TracertProgressBar.Visibility = Visibility.Visible;
                        TracertButton.Visibility = Visibility.Hidden;
                        ProcessLabel.Content = ("Proces w trakcie");
                        await FillTask(await DoGetHostEntryTask(TracertHostTextBox.Text), Convert.ToInt32(HowMuchTextBox.Text),
                            Convert.ToInt32(TimeoutTextBox.Text));
                        TracertResultTextBox.AppendText("\n\n ----------------------------------KONIEC----------------------------------");
                        TracertProgressBar.Visibility = Visibility.Hidden;
                        TracertButton.Visibility = Visibility.Visible;
                        ProcessLabel.Content = ("Oczekiwanie . . .");
                        EndFunction();
                    }
                    else
                    {
                        if (!await CheckIpTask(TracertHostTextBox.Text))
                            throw new FormatException($"{TracertHostTextBox.Text} jest nieprawidłowym adresem ip");
                        else if (!TracertPingClass.CheckData(HowMuchTextBox.Text))
                            throw new FormatException($"{HowMuchTextBox.Text} jest nieprawidłową ilością bram");
                        else if (!TracertPingClass.CheckData(TimeoutTextBox.Text))
                            throw new FormatException($"{TimeoutTextBox.Text} jest nieprawidłową długością opóźnienia");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: $"Wystąpił błąd podczas próby trasowania\n" +
                        $"Któraś z podanych wartości jest nieprawidłowa\nTreść błędu: {ex.Message}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                }
            }
        }

        private void EndFunction()
        {
            switch (MessageBox.Show(messageBoxText: $"\nCzy chcesz zapisać historię? ", caption: "Ping log",
                       button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
            {
                case MessageBoxResult.Yes:
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                                caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"/Tracert_log.txt");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                            {
                                FileName = "Tracert_log",
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

        private void Save(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            using (StreamWriter writer = new StreamWriter(path)) writer.Write(TracertResultTextBox.Text);
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void Fill(string ipAddress, int maxHops, int timeout)
        {
            Parallel.ForEach(TracertPingClass.Tracert(ipAddress, maxHops, timeout), (data, state) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TracertResultTextBox.AppendText($"{data}\n\n");
                    TracertResultTextBox.ScrollToEnd();
                }));
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    TracertButton_Click(sender: sender, e: e);
                    break;
                default:
                    break;
            }
        }
    }
}