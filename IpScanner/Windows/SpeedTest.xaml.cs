using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace IpScanner.Windows
{
    public partial class SpeedTest
    {
        string czas, link, rodzaj, sciezka;
        double rozmiar, predkosc;

        WebClient webClient;
        Stopwatch sw = new Stopwatch();

        public SpeedTest()
        {
            InitializeComponent();
            CopyComboBox.IsEnabled = false;
            CopyButton.IsEnabled = false;
            SizeSlider.Value = 1;
        }

        private void Fill()
        {
            string[] s = { "Czas", "Rozmiar pliku", "Prędkość łącza", "Wszystkie dane do notatnika" };

            for (int i = 0; i < s.Length; i++) CopyComboBox.Items.Add(s[i]);
        }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(start: ThreadFunction);

            t.Start();

            if (t.IsAlive)
            {
                Label.Visibility = Visibility.Visible;
                ProgressBar.Visibility = Visibility.Visible;
                ResultTextBox.Visibility = Visibility.Hidden;
            }
        }

        private void ThreadFunction()
        {    
            try
            {
                var watch = new Stopwatch();

                byte[] data;
                using (var client = new System.Net.WebClient())
                {
                    watch.Start();
                    data = client.DownloadData(address: $"http://dl.google.com/googletalk/googletalk-setup.exe?t={DateTime.Now.Ticks}");
                    watch.Stop();
                }

                Application.Current.Dispatcher.Invoke(callback: new Action(() =>
                {
                    rozmiar = data.Length;
                    for (int i = 0; i < 2; i++) rozmiar /= 1024;

                    czas = $"{watch.Elapsed.Hours:00} : { watch.Elapsed.Minutes:00} : {watch.Elapsed.Seconds:00} . " +
                    $"{ watch.Elapsed.Milliseconds / 10:00}";

                    ResultTextBox.AppendText(textData: $"\n\nCzas: {czas}\n\n");
                    ResultTextBox.AppendText(textData: $"Rozmiar pobranego pliku: {Math.Round(rozmiar, 2)} MB\n\n");

                    predkosc = data.LongLength / watch.Elapsed.TotalSeconds;
                    predkosc /= 100000;

                    ResultTextBox.AppendText(textData: $"Prędkość pobierania: {Math.Round(predkosc *= 1.25, 3)} Mb/s \n\n");

                    Label.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ResultTextBox.Visibility = Visibility.Visible;
                    CopyComboBox.IsEnabled = true;
                    CopyButton.IsEnabled = true;
                    Fill();
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(callback: new Action(() =>
               {
                   Label.Visibility = Visibility.Hidden;
                   ProgressBar.Visibility = Visibility.Hidden;
                   ResultTextBox.Visibility = Visibility.Visible;
                   CopyComboBox.IsEnabled = false;
                   StartTestButton.IsEnabled = false;
                   ResultTextBox.Text = "\n\n\nBrak połączenia z internetem\nSprawdź połaczenie i spróbuj ponownie";
                   MessageBox.Show(messageBoxText: "Aby móc zobaczyć publiczne dane wymagane jest połączenie z internetem\n" +
                       "\nSprawdź połączenie i spróbuj ponownie", caption: "Error", button: MessageBoxButton.OK, 
                       icon: MessageBoxImage.Error);
               }));
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    switch (MessageBox.Show(messageBoxText: "Naciśnięto lewy ctrl\n\nCzy chcesz rozpocząć testowanie prędkości łącza?",
                        caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            StartTestButton_Click(sender: sender, e: e);
                            break;
                        case MessageBoxResult.No:
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                        default:
                            break;
                    }
                    break;
                case Key.Enter:
                    CopyButton_Click(sender: sender, e: e);
                    break;
                case Key.Up:
                    CopyComboBox.Focus();
                    break;
                case Key.Down:
                    CopyComboBox.Focus();
                    break;
                default:
                    break;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CopyComboBox.SelectedIndex)
            {
                case 0:
                    Clipboard.SetText(text: czas);
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano czas testowania {czas}",
                         caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;
                case 1:
                    Clipboard.SetText(text: rozmiar.ToString());
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano rozmiar pobranego pliku {rozmiar}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;
                case 2:
                    Clipboard.SetText(text: predkosc.ToString());
                    MessageBox.Show(messageBoxText: $"Pomyślnie skopiowano prędkość łącza {predkosc}",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    break;
                case 3:
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                          caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Prędkość łącza.txt");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                            {
                                FileName = "Prędkość łącza",
                                DefaultExt = ".text",
                                Filter = "Text documents (.txt)|*.txt"
                            };
                            if (Dialog(dlg) == true) Save(dlg.FileName);
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

        private void Save(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"Czas: {czas}");
                writer.WriteLine("");
                writer.WriteLine($"Rozmiar pobranego pliku: {rozmiar} MB");
                writer.WriteLine("");
                writer.WriteLine($"Prędkość pobierania: {predkosc} Mb/s");
            }
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                switch (SizeSlider.Value)
                {
                    case 1:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 5MB";
                        link = "http://noc.pirx.pl/5mb.bin";
                        rodzaj = "bin";
                        break;
                    case 2:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 10MB";
                        link = "http://noc.gts.pl/10mb.gts";
                        rodzaj = "gts";
                        break;
                    case 3:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 50MB";
                        link = "http://noc.pirx.pl/50mb.bin";
                        rodzaj = "bin";
                        break;
                    case 4:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 100MB";
                        link = "http://noc.pirx.pl/100mb.bin";
                        rodzaj = "bin";
                        break;
                    case 5:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 250MB";
                        link = "http://noc.pirx.pl/250mb.bin";
                        rodzaj = "bin";
                        break;
                    case 6:
                        ChoosenOneLabel.Content = "Wybrany rozmiar: 500MB";
                        link = "http://noc.pirx.pl/500mb.bin";
                        rodzaj = "bin";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                SizeSlider.Value = 3;
            }
        }

        private void StartDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            StateLabel.Visibility = Visibility.Visible;
            StateLabel.Content = ("Stan: Pobieranie");
            sciezka = $"{Environment.GetFolderPath(folder: Environment.SpecialFolder.Desktop)}/test.{rodzaj}";
            DownloadFile(urlAddress: link, location: sciezka);
            StartDownloadButton.IsEnabled = false;
            StopDownloadButton.IsEnabled = true;
        }

        private void StopDownloadButton_Click(object sender, RoutedEventArgs e) => webClient.CancelAsync();

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void UploadFile(string urlAdress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.UploadFileCompleted += new UploadFileCompletedEventHandler(CompletedUpload);
                webClient.UploadProgressChanged += new UploadProgressChangedEventHandler(UploadProgressChanged);

                sw.Start();

                try
                {
                    webClient.Credentials = CredentialCache.DefaultCredentials;
                    //webClient.UploadFileAsync(;
                }
                catch (Exception)
                {

                }
            }
        }

        private void DownloadFile(string urlAddress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedDownload);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);

                sw.Start();

                try
                {
                    webClient.DownloadFileAsync(urlAddress.StartsWith("http://",
                        comparisonType: StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) :
                        new Uri("http://" + urlAddress), location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: ex.Message);
                }
            }
        }

        private void CompletedUpload(object sender, AsyncCompletedEventArgs e)
        {

        }

        private void UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {

        }
          
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double speed = e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds;
            labelSpeed.Content = $"Prędkość: {speed.ToString("0.00")} kb/s ({Math.Round(speed / 125, 2)} Mbps)";

            progressBar.Value = e.ProgressPercentage;

            labelPerc.Content = $"Procent: {e.ProgressPercentage.ToString()} %";

            labelDownloaded.Content = $" Pobrano: {(e.BytesReceived / 1024d / 1024d).ToString("0.00")} MB" +
                $" / {(e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00")} MB";

            TimeLabel.Content = $"Czas: {sw.Elapsed.Hours:00} : { sw.Elapsed.Minutes:00} :" +
                $" {sw.Elapsed.Seconds:00} . { sw.Elapsed.Milliseconds / 10:00}";
        }

        private void CompletedDownload(object sender, AsyncCompletedEventArgs e)
        {
            sw.Reset();

            StopDownloadButton.IsEnabled = false;
            StartDownloadButton.IsEnabled = true;

            StateLabel.Content = ("Stan: Zakończono");

            if (e.Cancelled == true)
            {
                MessageBox.Show(messageBoxText: "Testowanie zostało przerwane", caption: "Informacja",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else
            {
                MessageBox.Show(messageBoxText: "Testowanie zakónczończone powodzeniem", caption: "Inforamcja", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                switch (MessageBox.Show(messageBoxText: $"Czy chcesz usunąć pobrany plik,\nktóry znajduje się w " +
                   $"{Environment.GetFolderPath(folder: Environment.SpecialFolder.Desktop)}\\test.{rodzaj}",
                   caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        if (File.Exists($"{Environment.GetFolderPath(folder: Environment.SpecialFolder.Desktop)}/test.{rodzaj}"))
                        {
                            File.Delete($"{Environment.GetFolderPath(folder: Environment.SpecialFolder.Desktop)}/test.{rodzaj}");
                            MessageBox.Show(messageBoxText: "Pomyślnie usunięto pobrany plik", caption: "Informacja",
                                button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(messageBoxText: "Plik nie istnieje", caption: "Informacja",
                                 button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
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
        }
    }
}