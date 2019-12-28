using System;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Effects;
using System.Text;
using System.Windows.Controls;

namespace IpScanner.Windows
{
    public partial class Radio
    {
        //czas
        string baza_path = @"Baza1.txt", historia_path = @"Baza2.txt", pom_path = @"Pom.txt";
        int licznik = 0, licznik1 = 1, licznik3 = 1, licznik4 = 0, czas_minutnik;
        bool czy_licz = false, czy_pauza = false, czy_rand = false, czy_mute = false, czy_znaleziono = false,
            czy_minutnik_liczy = false;
        string stacja, link, tytul;
        string[] dane;
        double aktualny_vol;

        internal static Radio radio;

        DispatcherTimer czas = new DispatcherTimer();
        DispatcherTimer _timer;
        DispatcherTimer download = new DispatcherTimer();
        TimeSpan _time;
        Stopwatch stoper = new Stopwatch();
        Random rand = new Random();
        WebClient web = new WebClient();

        string currentTime = string.Empty;

        public Radio()
        {
            InitializeComponent();

            DispatcherTimer AudioTimer = new DispatcherTimer();
            AudioTimer.Tick += AudioTimer_Tick;
            AudioTimer.Interval = TimeSpan.FromMilliseconds(10);
            AudioTimer.Start(); 

            DispatcherTimer download = new DispatcherTimer(new TimeSpan(0, 0, 10), DispatcherPriority.Normal, delegate
            {
                try
                {
                    web.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Web_DownloadStringCompleted);                   
                    web.DownloadStringAsync(new Uri(link));
                }
                catch (Exception) { Source(); }
            }, Dispatcher);

            czas.Tick += new EventHandler(Czas_tick);
            czas.Interval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 1);

            AudioDevicesComboBox.SelectedIndex = 0;

            Odczyt();
            Czas();
            Ustaw_czas();

            FillAudio();

            Stop_minutnik_button.IsEnabled = false;

            Stacja_textbox.Text = ("Stacja: \n\n\n Tytuł: ");

            radio = this;
        }

        private void AudioTimer_Tick(object sender, EventArgs e)
        {
            if (AudioDevicesComboBox.SelectedItem != null)
            {
                var selectedDevice = (NAudio.CoreAudioApi.MMDevice)AudioDevicesComboBox.SelectedItem;
                LeftAudioProgressBar.Value = ((int)Math.Round(selectedDevice.AudioMeterInformation.PeakValues[0] * 100, 2));
                MainAudioProgressBar.Value = ((int)Math.Round(selectedDevice.AudioMeterInformation.MasterPeakValue * 100, 2));
                RightAudioProgressBar.Value = ((int)Math.Round(selectedDevice.AudioMeterInformation.PeakValues[1] * 100, 2));
            }
        }

        private async void FillAudio()
        {
            await Task.Delay(1);
            Zapisane_utwory.zapisane_Utwory.Set_path(historia_path);
            Nowa_stacja.nowa_Stacja.Set_path(baza_path);
            Nowa_stacja.nowa_Stacja.Set_iterator(licznik, baza_path);
            Co_jest_grane.co_Jest_Grane.Set_path(baza_path);

            var urzadzenia = new NAudio.CoreAudioApi.MMDeviceEnumerator().EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.All,
                NAudio.CoreAudioApi.DeviceState.Active);
            foreach (var i in urzadzenia) AudioDevicesComboBox.Items.Add(i);
        }

        private void Czas()
        {
            for (int i = 0; i <= 23; i++) Hours_combobox.Items.Add(i);
            for (int i = 0; i <= 59; i++)
            {
                Minutes_combobox.Items.Add(i);
                Seconds_combobox.Items.Add(i);
            }
        }

        private int Los(int poczatek) => rand.Next(poczatek, licznik);

        private string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        private void Web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            tytul = WebUtility.HtmlDecode(Encode(e.Result));

            tytul = tytul.Substring(tytul.IndexOf("1k9L1") + 7);
            tytul = tytul.Substring(0, tytul.IndexOf("</p>"));

            Stacja_textbox.Text = ("Stacja: " + dane[0] + "\n\n\n Tytuł: " + tytul);
        }

        private void Czas_tick(object sender, EventArgs e)
        {
            if (stoper.IsRunning)
            {
                TimeSpan ts = stoper.Elapsed;
                currentTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
                Time_label.Content = (currentTime);
            }
        }

        private void Zapisz(string gdzie)
        {
            using (Stream sw = File.Create(gdzie)) { }
        }

        private MessageBoxResult Dialog(string string1, string string2, MessageBoxButton button) =>
            MessageBox.Show(string1, string2, button, MessageBoxImage.Warning);

        private void Wypelnij()
        {
            using (StreamReader sw = File.OpenText(baza_path))
            {
                while ((dane = sw.ReadLine()?.Split('|')) != null)
                {
                    Nazwa_combobox.Items.Add(dane[0]);
                    licznik++;
                }
            }
        }

        private void Pisz()
        {
            string[] dane = new string[9];
            dane[0] = "Radio RMF FM|https://tunein.com/radio/Radio-RMF-FM-960-s1217/" +
                "|" + "http://31.192.216.6/rmf_fm";
            dane[1] = "Radio ZET|https://tunein.com/radio/Radio-ZET-1075-s15990/" +
                "|" + "https://zt.cdn.eurozet.pl/zet-tun.mp3";
            dane[2] = "Radio RMF MAXXX|https://tunein.com/radio/Radio-RMF-MAXXX-967-s48192/" +
                "|" + "http://31.192.216.7/rmf_maxxx";
            dane[3] = "Radio Eska Poznan|https://tunein.com/radio/Radio-ESKA-POZNAN-930-s16210/" +
                "|" + "http://pldm.ml/radio.php?id=-1&url=http://www.eskago.pl/radio/eska-poznan";
            dane[4] = "BBC Radio 1|https://tunein.com/radio/BBC-Radio-1-988-s24939/" + "|" +
                "http://bbcmedia.ic.llnwd.net/stream/bbcmedia_radio1_mf_q?s=1557919204&e=1557933604&h=7cb33962db481c7bf07eb3199c74e584";
            dane[5] = "hr1|https://tunein.com/radio/hr-1-990-s7866/" + "|" + "http://bbcmedia.ic.llnwd.net/stream/bbcmedia_radio1_mf_q?s=1557919944&e=1557934344&h=5c2351f7b5bc99516bd7b6d4defcd8e7";
            dane[6] = "1.FM - Alternative Rock X Hits Radio|https://tunein.com/radio/1FM---Alternative-Rock-X-Hits-Radio-s47734/" +
                "|" + "http://strm112.1.fm/x_mobile_mp3";
            dane[7] = "RTL Deutschlands Hit-Radio|https://tunein.com/radio/RTL-Deutschlands-Hit-Radio-s144167/" +
                "|" + "http://rtldtl.hoerradar.de/rtldtl-national-mp3-128?sABC=5pr58q9n%230%2397r44n24osr67050s8s403p4n0n9rqqr%23GharVA&amsparams=playerid:TuneIN;skey:1558547866";
            dane[8] = "Radio Zlote Przeboje|https://tunein.com/radiozlote/" +
                "|" + "http://poznan7.radio.pionier.net.pl:8000/tuba9-1.mp3";

            using (StreamWriter sw = File.AppendText(baza_path)) { for (int i = 0; i < dane.Length; i++) sw.WriteLine(dane[i]); }
        }

        private void Odczyt()
        {
            try
            {
                using (StreamReader sw = File.OpenText(pom_path))
                {
                    dane = sw.ReadLine()?.Split('|');
                    baza_path = dane[0];
                    historia_path = dane[1];
                }
                Wypelnij();
            }
            catch (Exception)
            {
                MessageBox.Show(messageBoxText: "Baza danych została stworzona na nowo\nPowód: Brak bazy", caption: "ERROR",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                if (!(File.Exists(pom_path))) Zapisz(pom_path);

                Zapisz(baza_path);
                Zapisz(historia_path);
                File.WriteAllText(pom_path, (baza_path + "|" + historia_path));

                switch (Dialog("Czy chcesz pobrać przykładową bazę danych?",
                    "Opcja opcjonalna", MessageBoxButton.YesNo))
                {
                    case MessageBoxResult.Yes:
                        Pisz();
                        Wypelnij();
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Aby odtwarzać radio internetowe, dodaj nowe stacje",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        break;
                }
            }
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void Source()
        {
            using (StreamReader sw = File.OpenText(baza_path))
            {
                while ((dane = sw.ReadLine()?.Split('|')) != null)
                {
                    if (stacja == dane[0]) czy_znaleziono = true;

                    else licznik3++;

                    if (czy_znaleziono)
                    {
                        if (licznik1 == licznik3)
                        {
                            Main_player.Source = new Uri(dane[2], UriKind.RelativeOrAbsolute);
                            link = dane[1];
                            break;
                        }
                        else licznik1++;
                    }
                }
            }
        }

        private async void Nazwa_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(300);
            Play_button_Click(sender: sender, e: e);
        }

        public void Play_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Nazwa_combobox.Items.Count < 1) throw new ArgumentNullException("Lista stacji jest pusta");

                Stacja_textbox.Clear();
                State_player_label.Content = ("Aktualny stan: Buforowanie");

                if (czy_licz && !czy_pauza)
                {
                    stoper.Reset();
                    Time_label.Content = ("00:00:00");
                }

                czas.Start();
                czy_licz = true;

                if (!czy_pauza)
                {
                    if (!(Nazwa_combobox.SelectedItem == null)) stacja = Nazwa_combobox.SelectedItem.ToString();

                    if (Nazwa_combobox.SelectedItem == null && !czy_rand)
                    {
                        Nazwa_combobox.SelectedIndex = 0;
                        stacja = Nazwa_combobox.SelectedItem.ToString();
                    }

                    if (Nazwa_combobox.SelectedItem == null && czy_rand)
                    {
                        Nazwa_combobox.SelectedIndex = Los(0);
                        stacja = Nazwa_combobox.SelectedItem.ToString();
                    }
                    Source();
                }

                czy_pauza = false;

                Main_player.Play();
                licznik1 = 1;
                licznik3 = 1;
                download.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(messageBoxText: $"Wystąpił błąd\nTreść błędu: {ex.Message}", 
                    caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
        }

        private void Stop_player()
        {
            download.Stop();
            licznik1 = 1;
            licznik3 = 1;
            Now_playing_label.Content = ("Zatrzymano");
            Main_player.Stop();
            if (stoper.IsRunning)
            {
                stoper.Stop();
                Time_label.Content = ("00:00:00");
            }
            Stacja_textbox.Text = ("Stacja: ");
            State_player_label.Content = ("Aktualny stan: STOP");
        }

        private void Stop_button_Click(object sender, RoutedEventArgs e) => Stop_player();

        private void Volume_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Vol_label.Content = ("Głośność: " + Math.Round(Volume_slider.Value, 0) + " %");
            Main_player.Volume = Volume_slider.Value / 100;
            aktualny_vol = Volume_slider.Value / 100;
            if (czy_mute)
            {
                czy_mute = false;
                Mute_button.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this),
                    "/Asstes/Icons/RadioIcons/SpeakerIcon.ico")));
            }
        }

        private void Stacja_w_prawo()
        {
            if (!czy_rand)
            {
                Nazwa_combobox.SelectedIndex = licznik4;
                licznik4++;
                if (licznik4 > licznik - 1) licznik4 = 0;
            }
            else
            {
                licznik4++;
                if (licznik4 >= licznik - 1) licznik4 = 0;
                Nazwa_combobox.SelectedIndex = rand.Next(licznik4, licznik - 1);
            }
        }

        private void Stacja_w_lewo()
        {
            if (!czy_rand)
            {
                Nazwa_combobox.SelectedIndex = licznik4 - 1;
                licznik4--;
                if (licznik4 < 0) licznik4 = licznik - 1;
            }

            else
            {
                licznik4--;
                if (licznik4 <= 0) licznik4 = licznik;
                Nazwa_combobox.SelectedIndex = Los(licznik4);
            }
        }

        private void Prawo_button_Click(object sender, RoutedEventArgs e) => Stacja_w_prawo();

        private void Lewo_button_Click(object sender, RoutedEventArgs e) => Stacja_w_lewo();

        private Task EndTimeTask() => Task.Run(async () =>
        {
            await Task.Delay(100);
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(10);
                Application.Current.Dispatcher.Invoke(new Action(() => { Czas_slider.Value = i; }));
            }
        });

        private async void Co_robic()
        {
            await EndTimeTask();
            Stop_minutnik_button.IsEnabled = false;
            Start_minutnik_button.IsEnabled = true;
            Ustaw_czas();
            switch (Decision_combobox.SelectedIndex)
            {
                case 0:
                    MainWindow.mainWindow.ExitButton_PreviewMouseLeftButtonUp(sender: new object(), 
                        e: new MouseButtonEventArgs(mouse: Mouse.PrimaryDevice, 0, MouseButton.Left));
                    break;
                case 1:
                    Environment.Exit(0);
                    break;
                case 2:
                    Mute_button_Click(sender: new object(), e: new RoutedEventArgs());
                    break;
                case 3:
                    await MuteTask(true, 0);
                    Stop_player();
                    break;
                case 4:
                    await MuteTask(true, 0);
                    Pause_player();
                    break;
                case 5:
                    Stacja_w_prawo();
                    break;
                case 6:
                    Stacja_w_lewo();
                    break;
                case 7:
                    await MuteTask(true, 0);
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case 8:
                    break;
                default:
                    break;
            }
        }

        private void Main_player_MediaOpened(object sender, RoutedEventArgs e)
        {
            State_player_label.Content = ("Aktualny stan: Play");
            stoper.Start();
        }

        private void Ile_czasu()
        {
            czas_minutnik = Convert.ToInt32(Hours_combobox.SelectedItem) * 3600 +
                Convert.ToInt32(Minutes_combobox.SelectedItem) * 60 + Convert.ToInt32(Seconds_combobox.SelectedItem);
            Czas_slider.Maximum = czas_minutnik;

            _time = TimeSpan.FromSeconds(czas_minutnik);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                Minutnik_label.Content = ("Pozostały czas: " + _time.ToString("c"));

                if (czas_minutnik == 10)
                {
                    MessageBox.Show(messageBoxText: "Po naciśnięciu przycisku OK zostanie 10 sekund do końca odliczania",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                }

                if (_time == TimeSpan.Zero)
                {
                    _timer.Stop();
                    czy_minutnik_liczy = false;
                    Czas_slider.Minimum = 0;
                    Czas_slider.Maximum = 100;
                    Czas_slider.TickFrequency = 10;
                    Co_robic();
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
                Czas_slider.Value = czas_minutnik;
                czas_minutnik--;
            }, Application.Current.Dispatcher);
        }

        private void Allstationsplaying_button_Click(object sender, RoutedEventArgs e)
        {
            Co_jest_grane.co_Jest_Grane.DownloadButton_Click(sender: sender, e: e);
            RadioWindow.radioWindow.TabControlIndex = 3;
        }

        private void Yt_button_Click(object sender, RoutedEventArgs e)
        {
            Net_radio_browser browser_window = new Net_radio_browser();
            browser_window.Set_Data("https://google.com", tytul, 0);
            browser_window.Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    Pause_button_Click(sender: sender, e: e);
                    break;
                case Key.Enter:
                    Play_button_Click(sender: sender, e: e);
                    break;
                default:
                    break;
            }
        }

        private void Start_minutnik_button_Click(object sender, RoutedEventArgs e)
        {
            if (Decision_combobox.SelectedItem != null)
            {
                var when = TimeClass.Result(Convert.ToInt32(Hours_combobox.SelectedItem), 
                    Convert.ToInt32(Minutes_combobox.SelectedItem), Convert.ToInt32(Seconds_combobox.SelectedItem));
                var currentTime = TimeClass.CurrentTimeResult(TimeClass.CurrentTime().Item1, 
                    TimeClass.CurrentTime().Item2, TimeClass.CurrentTime().Item3);

                switch (MessageBox.Show(messageBoxText: $"Jest godzina {currentTime.Item1} : {currentTime.Item2} :" +
                    $" {currentTime.Item3}\nCzas upłynie o godzinie {when.Item1} : {when.Item2} : {when.Item3}\nCzy zgadzasz się?",
                      caption: "Czas", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        int czas = Convert.ToInt32(Hours_combobox.SelectedItem) * 3600 +
                            Convert.ToInt32(Minutes_combobox.SelectedItem) * 60 + Convert.ToInt32(Seconds_combobox.SelectedItem);
                        if (!czy_minutnik_liczy) Ile_czasu();
                        _timer.Start();
                        Start_minutnik_button.IsEnabled = false;
                        Stop_minutnik_button.IsEnabled = true;
                        try { Czas_slider.TickFrequency = (czas / (czas / (czas / 10))); }
                        catch (DivideByZeroException) { Czas_slider.TickFrequency = czas; }
                        Czas_slider.Maximum = czas;
                        Czas_slider.Value = czas;
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Podaj nowy czas i spróbuj ponownie", caption: "Informacja",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }

            else
            {
                MessageBox.Show(messageBoxText: "Wybierz czynność", caption: "Informacja", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }
        }

        private void Ustaw_czas()
        {
            Hours_combobox.SelectedIndex = 0;
            Minutes_combobox.SelectedIndex = 0;
            Seconds_combobox.SelectedIndex = 0;
        }

        private void Stop_minutnik_button_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            Stop_minutnik_button.IsEnabled = false;
            Start_minutnik_button.IsEnabled = true;
            czy_minutnik_liczy = true;
        }

        private void Refresh_button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            licznik = 0;
            Nazwa_combobox.Items.Clear();
            Wypelnij();
            Refresh_button.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
        }

        private void Reset_minutnik_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Start_minutnik_button.IsEnabled = true;
                Stop_minutnik_button.IsEnabled = false;
                czy_minutnik_liczy = false;
                _timer.Stop();
                Ustaw_czas();
                Czas_slider.Maximum = 100;
                Czas_slider.Value = 100;
                Minutnik_label.Content = ("Pozostały czas: 00:00:00");
            }
            catch (Exception) { }
        }

        private void Balance_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = Balance_slider.Value / 100;
            Main_player.Balance = (value);
            Balance_label.Content = ("Balans: " + Math.Round(value, 2));
        }

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            try { using (StreamWriter sw = File.AppendText(historia_path))
                    sw.WriteLine(dane[0] + '|' + tytul + '|' + MainWindow.mainWindow.time); }
            catch (Exception) { }
        }

        private void Browser_button_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(link); }
            catch (Exception) { }
        }

        private Task MuteTask(bool mute, int n) => Task.Run(async () =>
        {
            if (!mute)
            {
                for (int i = 0; i < n; i++)
                {
                    await Task.Delay(3);
                    Application.Current.Dispatcher.Invoke(new Action(() => { Main_player.Volume = (double)i / 100; }));
                }
            }

            else
            {
                for (int i = n; i >= 0; i--)
                {
                    await Task.Delay(3);
                    Application.Current.Dispatcher.Invoke(new Action(() => { Main_player.Volume = (double)i / 100; }));
                }
            }
        });      

        private async void Mute_button_Click(object sender, RoutedEventArgs e)
        {
            if (!czy_mute)
            {
                czy_mute = true;
                Mute_button.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this),
                    "/Assets/Icons/RadioIcons/MuteIcon.ico")));
                await MuteTask(true, Convert.ToInt32(Volume_slider.Value));
            }

            else
            {
                czy_mute = false;
                Mute_button.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this),
                    "/Assets/Icons/RadioIcons/SpeakerIcon.ico")));
                await MuteTask(false, Convert.ToInt32(Volume_slider.Value));
            }

        }

        private void Rand_button_Click(object sender, RoutedEventArgs e)
        {
            if (!czy_rand)
            {
                DropShadowEffect dropShadowEffect = new DropShadowEffect { Color = new Color { R = 118, G = 169, B = 252 } };                
                czy_rand = true;
                Rand_button.Effect = dropShadowEffect;
            }

            else
            {
                czy_rand = false;
                Rand_button.Effect = null;
            }
        }

        private void Pause_player()
        {
            download.Stop();
            czy_pauza = true;
            Main_player.Pause();
            stoper.Stop();
            State_player_label.Content = ("Aktualny stan: PAUZA");
        }

        private void Pause_button_Click(object sender, RoutedEventArgs e) => Pause_player();

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    Play_button.Effect = null;
                    break;
                case 2:
                    Stop_button.Effect = null;
                    break;
                case 3:
                    Prawo_button.Effect = null;
                    break;
                case 4:
                    Pause_button.Effect = null;
                    break;
                case 5:
                    Lewo_button.Effect = null;
                    break;
                case 6:
                    if (!czy_rand) Rand_button.Effect = null;
                    else Rand_button.Effect = new DropShadowEffect { Color = new Color { R = 118, G = 169, B = 252 } };
                    break;
                case 7:
                    Mute_button.Effect = null;
                    break;
                case 8:
                    Refresh_button.Effect = null;
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
                    Play_button.Effect = dropShadowEffect;
                    break;
                case 2:
                    Stop_button.Effect = dropShadowEffect;
                    break;
                case 3:
                    Prawo_button.Effect = dropShadowEffect;
                    break;
                case 4:
                    Pause_button.Effect = dropShadowEffect;
                    break;
                case 5:
                    Lewo_button.Effect = dropShadowEffect;
                    break;
                case 6:
                    Rand_button.Effect = dropShadowEffect;
                    break;
                case 7:
                    Mute_button.Effect = dropShadowEffect;
                    break;
                case 8:
                    Refresh_button.Effect = dropShadowEffect;
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
                    Play_button.Effect = dropShadowEffect;
                    break;
                case 2:
                    Stop_button.Effect = dropShadowEffect;
                    break;
                case 3:
                    Prawo_button.Effect = dropShadowEffect;
                    break;
                case 4:
                    Pause_button.Effect = dropShadowEffect;
                    break;
                case 5:
                    Lewo_button.Effect = dropShadowEffect;
                    break;
                case 6:
                    Rand_button.Effect = dropShadowEffect;
                    break;
                case 7:
                    Mute_button.Effect = dropShadowEffect;
                    break;
                case 8:
                    Refresh_button.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }
    }
}