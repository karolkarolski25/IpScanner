using System.Windows;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System;
using System.Windows.Input;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace IpScanner.Windows
{
    public partial class Host_data
    {
        private string publicIp, isp, coord, local, hostname, localization;
        private readonly WebClient webClient = new WebClient();

        public Host_data()
        {
            InitializeComponent();

            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Web_DownloadStringCompleted);
            webClient.DownloadStringAsync(new Uri("https://www.whatismyip.net/"));
        }

        private static string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        private void Web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string data = Encode(e.Result.ToString());

                ProgressLabel.Visibility = Visibility.Hidden;
                DownloadStringProgressBar.Visibility = Visibility.Hidden;
                PublicDataTextBox.Visibility = Visibility.Visible;

                publicIp = GetPublicDataClass.GetIp(data);
                isp = GetPublicDataClass.GetIsp(data);
                local = GetPublicDataClass.GetLocal(data);
                localization = GetPublicDataClass.GetLocation(data);
                hostname = GetPublicDataClass.GetHostname(data);
                coord = GetPublicDataClass.GetCoords(data);

                PublicDataTextBox.AppendText($"\nAdres IP: {publicIp}\n\n");
                PublicDataTextBox.AppendText($"Nazwa hosta: {hostname}\n\n");
                PublicDataTextBox.AppendText($"ISP: {isp}\n\n");
                PublicDataTextBox.AppendText($"Lokalizacja: {localization}\n\n");
                PublicDataTextBox.AppendText($"Koordynaty: {coord}\n\n");
                PublicDataTextBox.AppendText($"Dane lokalne: {local}\n\n");

                MapButton.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                DownloadStringProgressBar.Visibility = Visibility.Hidden;
                ProgressLabel.Visibility = Visibility.Hidden;
                PublicDataTextBox.Visibility = Visibility.Visible;
                PublicDataTextBox.Text = "\n\n\n\n\nBrak połączenia z internetem\nSprawdź połaczenie i spróbuj ponownie";
            }
        }

        private void ShowNetworkInterfaces(bool ShowMac)
        {
            Fill();
            textbox.AppendText(textData: $"Otzrymane dane: {IPGlobalProperties.GetIPGlobalProperties().HostName}" +
                $"{IPGlobalProperties.GetIPGlobalProperties().DomainName} " +
                $"{IPGlobalProperties.GetIPGlobalProperties().DhcpScopeName}\n\n");
            if (NetworkInterface.GetAllNetworkInterfaces() == null || NetworkInterface.GetAllNetworkInterfaces().Length < 1)
            {
                MessageBox.Show(messageBoxText: "Brak interfejsów sieciowych",
                    caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                return;
            }

            textbox.AppendText(textData: $" Ilość interfejsów sieciowych: {NetworkInterface.GetAllNetworkInterfaces().Length}\n");
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                textbox.AppendText("\n");
                textbox.AppendText(textData: $"{adapter.Description}\n");
                textbox.AppendText(textData: string.Empty.PadLeft(totalWidth: adapter.Description.Length - 5, paddingChar: '=') + '\n');
                textbox.AppendText(textData: $"Typ interfejsu: {adapter.NetworkInterfaceType}\n");
                textbox.AppendText(textData: "Adres fizyczny: ");
                for (int i = 0; i < adapter.GetPhysicalAddress().GetAddressBytes().Length; i++)
                {
                    if (ShowMac)
                        textbox.AppendText(textData: $"{adapter.GetPhysicalAddress().GetAddressBytes()[i].ToString(format: "X2")}");
                    else
                        textbox.AppendText(textData: "xx");
                    if (i != adapter.GetPhysicalAddress().GetAddressBytes().Length - 1)
                        textbox.AppendText(textData: "-");
                }
                textbox.AppendText(textData: "\n");
            }
        }

        private void Fill()
        {
            textbox.AppendText(textData: $"Twoja nazwa: {Dns.GetHostName()} \n\n");
            textbox.AppendText(textData: "Twoje ip: \n\n");
            foreach (IPAddress ip in Dns.GetHostAddresses(hostNameOrAddress: Dns.GetHostName())) textbox.AppendText
                    (textData: $"{ip.ToString()}\n");
        }   

        private void SaveToNotepadPrivateData()
        {
            switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ", 
                caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Prywatne dane.txt", true);
                    break;
                case MessageBoxResult.No:
                    MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "Prywatne dane",
                        DefaultExt = ".text",
                        Filter = "Text documents (.txt)|*.txt"
                    };
                    if (Dialog(dlg) == true) Save(dlg.FileName, true);
                    break;
                case MessageBoxResult.Cancel:
                default:
                    break;
            }     
        }

        private void SaveToNotepadPublicData()
        {
            switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                    caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Publiczne dane.txt", false);
                    break;
                case MessageBoxResult.No:
                    MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                        caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "Publiczne dane",
                        DefaultExt = ".text",
                        Filter = "Text documents (.txt)|*.txt"
                    };
                    if (Dialog(dlg) == true) Save(dlg.FileName, false);
                    break;
                case MessageBoxResult.Cancel:
                default:
                    break;
            }
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();

        private void Save(string path, bool privateData)
        {
            if (privateData)
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"\nTwoja nazwa: {Dns.GetHostName()} \n\n");
                    writer.WriteLine("");
                    writer.WriteLine("\nTwoje ip: \n\n");
                    writer.WriteLine("");
                    foreach (IPAddress ip in Dns.GetHostAddresses(hostNameOrAddress: Dns.GetHostName()))
                        writer.WriteLine($"{ip.ToString()}\n");
                    writer.WriteLine("");
                    writer.WriteLine($"\n\nOtzrymane dane: {IPGlobalProperties.GetIPGlobalProperties().HostName}" +
                       $"{IPGlobalProperties.GetIPGlobalProperties().DomainName} " +
                       $"{IPGlobalProperties.GetIPGlobalProperties().DhcpScopeName}\n\n");
                    if (NetworkInterface.GetAllNetworkInterfaces() == null || NetworkInterface.GetAllNetworkInterfaces().Length < 1)
                    {
                        MessageBox.Show(messageBoxText: "Brak interfejsów sieciowych",
                            caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                        return;
                    }

                    writer.WriteLine($"Ilość interfejsów sieciowych .................... : " +
                        $"{NetworkInterface.GetAllNetworkInterfaces().Length}\n");
                    foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        IPInterfaceProperties properties = adapter.GetIPProperties();
                        writer.WriteLine("\n");
                        writer.WriteLine($"{adapter.Description}\n");
                        writer.WriteLine(string.Empty.PadLeft(totalWidth: adapter.Description.Length, paddingChar: '=') + '\n');
                        writer.WriteLine($"Typ interfejsu .......................... : {adapter.NetworkInterfaceType}\n");
                        writer.Write("Adres fizyczny .......................... : ");
                        for (int i = 0; i < adapter.GetPhysicalAddress().GetAddressBytes().Length; i++)
                        {
                            writer.Write($"{adapter.GetPhysicalAddress().GetAddressBytes()[i].ToString(format: "X2")}");
                            if (i != adapter.GetPhysicalAddress().GetAddressBytes().Length - 1)
                                writer.Write("-");
                        }
                        writer.WriteLine("\n");
                    }
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"\nAdres IP: {publicIp}");
                    writer.WriteLine("");
                    writer.WriteLine($"ISP: {isp}");
                    writer.WriteLine("");
                    writer.WriteLine($"Nazwa hosta: {hostname}");
                    writer.WriteLine("");
                    writer.WriteLine($"Lokalizacja: {localization}");
                    writer.WriteLine("");
                    writer.WriteLine($"Koordynaty: {coord}");
                    writer.WriteLine("");
                    writer.WriteLine($"Dane lokalne: {local}");
                }
            }
        }


        private void ShowMessageBoxAndCopy(string toCopy, string toShow)
        {
            Clipboard.SetText($"{toCopy}");
            MessageBox.Show(messageBoxText: toShow, caption: "Informacja", 
                button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
        }

        private void MapButton_MouseEnter(object sender, MouseEventArgs e)
        {
            MapButton.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
        }

        private void MapButton_MouseLeave(object sender, MouseEventArgs e)
        {
            MapButton.Effect = null;
        }

        private void MapButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MapButton.Effect = new DropShadowEffect { Color = new Color { R = 0, G = 191, B = 255 } };
        }

        private void MapButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MapButton.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
            MapWindow mapWindow = new MapWindow();
            mapWindow.SetCoord(coord);
            mapWindow.Show();
        }

        private void CopyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (CopyComboBox.SelectedIndex)
            {
                case 1:
                    ShowMessageBoxAndCopy(GetIp.Subnet(), $"Pomyślnie skopiowano prywatny adres IP {GetIp.Subnet()}");
                    break;
                case 2:
                    ShowMessageBoxAndCopy(GetMac.HostMac(), $"Pomyślnie skopiowano adres MAC {GetMac.HostMac()}");
                    break;
                case 3:
                    ShowMessageBoxAndCopy($"{GetIp.Subnet()} {GetMac.HostMac()}", 
                        $"Pomyślnie skopiowano dane: {GetIp.Subnet()} {GetMac.HostMac()}");
                    break;
                case 4:
                    ShowMessageBoxAndCopy($"{publicIp}", $"Pomyślnie skopiowano publiczny adres IP {publicIp}");
                    break;
                case 5:
                    ShowMessageBoxAndCopy($"{isp}", $"Pomyślnie skopiowano ISP: {isp}");
                    break;
                case 6:
                    ShowMessageBoxAndCopy($"{hostname}", $"Pomyślnie skopiowano nazwę hosta: {publicIp}");
                    break;
                case 7:
                    ShowMessageBoxAndCopy(localization, $"Pomyślnie skopiowano lokalizaję: {localization}");
                    break;
                case 8:
                    ShowMessageBoxAndCopy(coord, $"Pomyślnie skopiowano dane lokalizayjne: {coord}");
                    break;
                default:
                    break;
            }
        }

        private void HideMacAdressCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            textbox.Clear();
            ShowNetworkInterfaces(ShowMac: false);
        }

        private void HideMacAdressCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            textbox.Clear();
            ShowNetworkInterfaces(ShowMac: true);
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch(SaveDataToNotepadComboBox.SelectedIndex)
            {
                case 1:
                    SaveToNotepadPrivateData();
                    break;
                case 2:
                    SaveToNotepadPublicData();
                    break;
                default:
                    break;
            }
        }
    }
}