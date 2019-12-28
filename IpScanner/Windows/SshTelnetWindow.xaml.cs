using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IpScanner.Windows
{
    public partial class SshTelnetWindow
    {
        private SshClient ssh = null;
        private ShellStream shellStream = null;
        private string notepad, header, port;
        private bool connected = false;

        private Thread thread = null;

        internal static SshTelnetWindow SshTelnet;

        public SshTelnetWindow()
        {
            InitializeComponent();

            HostIpTextBox.Focus();

            DisconnectButton.IsEnabled = false;

            StatusLabel.Content = "Status: Rozłączono";

            SshTelnet = this;

            thread = new Thread(new ThreadStart(ReceiveSSHData)) { IsBackground = true };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (!connected)
                    {
                        switch (MessageBox.Show(messageBoxText: "Naciśnięto klawisz enter\nCzy chcesz uruchomić połączenie?",
                            caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                ConnectButton_Click(sender: sender, e: e);
                                break;
                            case MessageBoxResult.No:
                                break;
                            case MessageBoxResult.Cancel:
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetText(string HEADER, string PORT, string NOTEPAD, string ip)
        {
            header = HEADER;
            port = PORT;
            notepad = NOTEPAD;

            SSHInfoHeader.Header = header;
            PortNumberComboBox.Text = port;
            HostIpTextBox.Text = ip;
        }

        private string GetPortNumber(string data)
        {
            data = data.Substring(data.IndexOf(": ") + 2);
            return data.Substring(0, data.IndexOf(" "));
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            SSHConsoleTextBox.Clear();

            StatusLabel.Content = "Status: Łączenie";

            if (string.IsNullOrEmpty(HostIpTextBox.Text) && PortNumberComboBox.SelectedIndex != 0
                && string.IsNullOrEmpty(UserNameTextBox.Text) && string.IsNullOrEmpty(PasswordTextBox.Password))
            {
                MessageBox.Show(messageBoxText: "Nie podano żadnych danych\nPodaj adres, numer portu, " +
                    "nazwę użytkownika oraz hasło i spróbuj ponownie", caption: "Informacja",
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else if (string.IsNullOrEmpty(PasswordTextBox.Password))
            {
                PasswordTextBox.Focus();
                MessageBox.Show(messageBoxText: "Nie podano hasła\nPodaj hasło i spróbuj ponownie",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else if (string.IsNullOrEmpty(UserNameTextBox.Text))
            {
                UserNameTextBox.Focus();
                MessageBox.Show(messageBoxText: "Nie podano nazwy użytkownika\nPodaj nazwę użytkownika i spróbuj ponownie",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else if (PortNumberComboBox.SelectedIndex == 0)
            {
                PortNumberComboBox.Focus();
                switch (MessageBox.Show(messageBoxText: "Nie podano portu\nPortem SSH jest 22\nZgadzasz się?",
                    caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        PortNumberComboBox.SelectedIndex = 1;
                        ConnectButton_Click(sender: sender, e: e);
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Podaj własny port i spróbuj ponownie", caption: "Informacja",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }

            else if (string.IsNullOrEmpty(HostIpTextBox.Text))
            {
                HostIpTextBox.Focus();
                MessageBox.Show(messageBoxText: "Nie podano adresu hosta\nPodaj adres i spróbuj ponownie",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }

            else
            {
                bool result = false;

                try
                {
                    LoadLabel.Visibility = Visibility.Visible;
                    LoadProgressBar.Visibility = Visibility.Visible;
                    SSHConsoleTextBox.Visibility = Visibility.Hidden;

                    result = await AsyncConnectTask(HostIpTextBox.Text);

                    if (TracertPingClass.CheckData(GetPortNumber(PortNumberComboBox.SelectedItem.ToString())) && result)
                    {
                        LoadLabel.Content = "Sprawdzanie poprawnośći adesu ip: Zakończono";
                        LoadProgressBar.Visibility = Visibility.Hidden;
                        await Task.Delay(1500);
                        LoadProgressBar.Visibility = Visibility.Visible;
                        LoadLabel.Content = "Status: Łączenie";

                        var success = await ConnectTask(HostIpTextBox.Text, Convert.ToInt32(GetPortNumber(PortNumberComboBox.SelectedItem.ToString())),
                              UserNameTextBox.Text, PasswordTextBox.Password);

                        if (success.Item1 == "success")
                        {
                            LoadLabel.Visibility = Visibility.Hidden;
                            LoadProgressBar.Visibility = Visibility.Hidden;
                            SSHConsoleTextBox.Visibility = Visibility.Visible;
                            ConnectButton.IsEnabled = false;
                            DisconnectButton.IsEnabled = true;
                            StatusLabel.Content = ("Status: Połączono");
                            connected = true;
                            CommandTextBox.Focus();
                            thread.Start();
                        }

                        else
                        {
                            LoadLabel.Visibility = Visibility.Hidden;
                            LoadProgressBar.Visibility = Visibility.Hidden;
                            LoadLabel.Content = ("Sprawdzanie poprawności adresu ip...");
                            connected = false;
                            DisconnectButton.IsEnabled = false;
                            ConnectButton.IsEnabled = true;
                            StatusLabel.Content = ("Status: Rozłączono");
                            thread.Abort();
                            MessageBox.Show($"Wystąpił błąd\nTreść błędu: {success.Item1}\nRodzaj błędu: {success.Item2}",
                                caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                        }
                    }

                    else
                    {
                        if (!result)
                            throw new FormatException($"{HostIpTextBox.Text} jest nieprawidłowym adresem ip\n" +
                                $"Podaj poprawny i spróbuj ponownie");
                        else if (!TracertPingClass.CheckData(PortNumberComboBox.SelectedItem.ToString()))
                            throw new ArgumentException($"{PortNumberComboBox.SelectedItem.ToString()} jest nieprawidłowym numerem portu\n" +
                                $"Podaj poprawny i spróbuj ponownie");
                    }

                }
                catch (Exception ex)
                {
                    LoadLabel.Visibility = Visibility.Hidden;
                    LoadProgressBar.Visibility = Visibility.Hidden;
                    connected = false;
                    DisconnectButton.IsEnabled = false;
                    ConnectButton.IsEnabled = true;
                    StatusLabel.Content = ("Status: Rozłączono");
                    LoadLabel.Content = ("Sprawdzanie poprawności adresu ip...");
                    MessageBox.Show($"Wystąpił błąd\nTreść błędu: {ex.Message}\nRodzaj błędu: {ex.GetType()}",
                        caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                    thread.Abort();
                    ClearButton_Click(sender: sender, e: e);
                }
            }
        }

        private Task<bool> AsyncConnectTask(string ip) => Task.Run(() => TracertPingClass.CheckIp(ip));

        private Task<Tuple<string, string>> ConnectTask(string host, int port, string username,
            string password) => Task.Run(() => Connect(host, port, username, password));

        private Tuple <string, string> Connect (string host, int port, string username, string password)
        {
            try
            {
                ssh = new SshClient(host, port, username, password);
                ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(120);
                ssh.Connect();
                shellStream = ssh.CreateShellStream("vt100", 80, 60, 800, 600, 65536);
                return Tuple.Create("success", "successs");
            }
            catch (Exception ex) { return Tuple.Create(ex.Message, ex.GetType().ToString()); }
        }

        private void ReceiveSSHData()
        {
            while (true)
            {
                try
                {
                    if (shellStream != null && shellStream.DataAvailable)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            SSHConsoleTextBox.AppendText(shellStream.Read());
                            SSHConsoleTextBox.ScrollToEnd();
                        }));
                    }
                }
                catch (Exception) { }
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ssh.Disconnect();
            StatusLabel.Content = ("Status: Rozłączono");
            connected = false;
            DisconnectButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;
            thread.Abort();

            switch (MessageBox.Show(messageBoxText: $"\nCzy chcesz zapisać historię? ", caption: "SSH log",
                        button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Information))
            {
                case MessageBoxResult.Yes:
                    switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                                caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"/{notepad}.txt");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                                caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                            {
                                FileName = notepad,
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
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(SSHConsoleTextBox.Text);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PortNumberComboBox.SelectedIndex = 0;
            PasswordTextBox.Clear();
            HostIpTextBox.Clear();
            UserNameTextBox.Clear();
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void CommandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (CommandTextBox.Text == "clear") SSHConsoleTextBox.Clear();
                    shellStream.Write(CommandTextBox.Text + "\n");
                    shellStream.Flush();
                    CommandTextBox.Clear();                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd\nTreść błędu: {ex.Message}\nRodzaj błędu: {ex.GetType()}",
                    caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
        }
    }
}