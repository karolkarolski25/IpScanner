using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IpScanner.Windows
{
    public partial class PingWindow
    {
        internal static PingWindow pingWindow;

        bool connected = false;

        public PingWindow()
        {
            InitializeComponent();
            PingHostTextBox.Focus();
            pingWindow = this;
        }

        public async void AutoPing(string ip, bool auto)
        {
            if (auto)
            {
                await Task.Delay(1500);
                HowMuchTextBox.Text = "10";
                PingHostTextBox.Text = ip;
                PingButton_Click(sender: new object(), e: new RoutedEventArgs());
            }
        }

        private Task<Tuple<string, double>> PingTask(string ip) => Task.Run(() => TracertPingClass.Ping(ip));

        private Task<bool> CheckIpTask(string ip) => Task.Run(() => TracertPingClass.CheckIp(ip));

        private async void Fill(ulong ile, string ip)
        {
            double average = 0;
            int maks = 0, mini = 1000000000;
            ulong i = 1;
            Console.WriteLine(connected);
            while (connected)
            {
                var data = await PingTask(ip);
                if (Convert.ToInt32(data.Item2) > maks) maks = Convert.ToInt32(data.Item2);
                if (Convert.ToInt32(data.Item2) < mini) mini = Convert.ToInt32(data.Item2);
                average += data.Item2;
                PingResultTextBox.AppendText($"{i}. {data.Item1}\n\n");
                PingResultTextBox.ScrollToEnd();

                if (ile != 0)
                    if (i >= ile) break;
                i++;
            }
           
            PingResultTextBox.AppendText($"\nŚrednie opóźnienie: {Math.Round(average / i, 2)} ms");
            PingResultTextBox.AppendText($"\nMinimalne opóźnienie: {mini} ms");
            PingResultTextBox.AppendText($"\nMaksymalne opóźnienie: {maks} ms");
            PingResultTextBox.AppendText("\n\n\n-------------------------KONIEC-------------------------");

            connected = false;

            PingButton.Content = "Pinguj";
            LoadLabel.Content = ("Sprawdzanie poprawnośći adresu ip...");

            switch (MessageBox.Show(messageBoxText: $"\nCzy chcesz zapisać historię? ", caption: "Ping log",
                       button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
            {
                case MessageBoxResult.Yes:
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                                caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"/Ping_log.txt");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                            {
                                FileName = "Ping_log",
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
            using (StreamWriter writer = new StreamWriter(path)) { writer.Write(PingResultTextBox.Text); }
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                PingResultTextBox.Clear();
                try
                {
                    if (string.IsNullOrEmpty(HowMuchTextBox.Text) && string.IsNullOrEmpty(PingHostTextBox.Text))
                    {
                        MessageBox.Show(messageBoxText: "Nie podano żadnych danych\nPodaj adres oraz ilość i spróbuj ponownie",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    }

                    else if (string.IsNullOrEmpty(HowMuchTextBox.Text))
                    {
                        HowMuchTextBox.Focus();
                        switch (MessageBox.Show("Nie podano ilości pingowania lub jest nieprawidłowa\n" +
                            "Czy chcesz pingować w nieskończoność?", caption: "Zapytanie", 
                            button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                HowMuchTextBox.Text = "inf";
                                PingTest();
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

                    else if (string.IsNullOrEmpty(PingHostTextBox.Text))
                    {
                        PingHostTextBox.Focus();
                        MessageBox.Show(messageBoxText: "Nie podano adresu hosta\nPodaj adres i spróbuj ponownie",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    }

                    else PingTest();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: "Podano błędą wartości ilości\nPodaj poprawną i spróbuj ponownie",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                PingButton.Content = "Pinguj";
                LoadLabel.Content = ("Sprawdzanie poprawnośći adresu ip...");
                connected = false;
            }
        }

        private async void PingTest()
        {
            PingResultTextBox.Visibility = Visibility.Hidden;
            LoadLabel.Visibility = Visibility.Visible;
            LoadProgressBar.Visibility = Visibility.Visible;
            if (await CheckIpTask(PingHostTextBox.Text))
            {
                LoadLabel.Content = ("Zakończono sprawdzanie poprawności");
                await Task.Delay(1750);
                LoadLabel.Content = ("Uruchamianie procesu");
                await Task.Delay(1000);
                LoadLabel.Visibility = Visibility.Hidden;
                LoadProgressBar.Visibility = Visibility.Hidden;
                PingResultTextBox.Visibility = Visibility.Visible;

                connected = true;
                PingButton.Content = "STOP";
                PingResultTextBox.Clear();
                if (HowMuchTextBox.Text == "inf")
                    Fill(0, PingHostTextBox.Text);
                else
                    try { Fill(Convert.ToUInt64(HowMuchTextBox.Text), PingHostTextBox.Text); }
                    catch (Exception ex) { MessageBox.Show(messageBoxText: $"Podano błędną ilość testów\n" +
                        $"Podaj nową wartość i spróbuj ponownie\nTreść błędu: {ex.Message}", 
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information); }
            }
            else
            {
                LoadLabel.Visibility = Visibility.Hidden;
                LoadProgressBar.Visibility = Visibility.Hidden;
                PingResultTextBox.Clear();
                MessageBox.Show(messageBoxText: "Podano błędny adres hosta lub host nie odpowiada\n" +
                    "Podaj poprawny i spróbuj ponownie", caption: "Informacja", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    PingButton_Click(sender, e);
                    break;
                default:
                    break;
            }
        }
    }
}