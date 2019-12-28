using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace IpScanner.Windows
{
    public partial class Nowa_stacja
    {
        internal static Nowa_stacja nowa_Stacja;

        private int ilosc = 0;
        private string baza_path;
        private WebClient web = new WebClient();

        //Lista stacji
        private int iterator = 0;
        private string[] dane;
        private string baza_path_list_station;
        public List<ListaStacjiLista> items = new List<ListaStacjiLista>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;

        public Nowa_stacja()
        {
            InitializeComponent();
            nowa_Stacja = this;
        }

        #region NowaStacja

        void Bar()
        {
            for (int i = 0; i <= 100; i++)
            {
                Postep_progressBar.Dispatcher.Invoke(() => Postep_progressBar.Value = i, DispatcherPriority.Background);
                Thread.Sleep(10);
                Progress_label.Content = ("Postęp: " + i + " %");
            }
        }

        public void Set_path(string path) => baza_path = path;

        private bool Czy_jest(string html)
        {
            string[] dane;
            using (StreamReader sw = File.OpenText(baza_path))
            {
                while ((dane = sw.ReadLine()?.Split('|')) != null)
                {
                    if (dane[0] == html) return true;
                }
            }
            return false;
        }

        private string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        private void Web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string html = Encode(e.Result);

            html = html.Substring(html.IndexOf(" title=") + 8);
            html = html.Substring(0, html.IndexOf("\""));

            if (!Czy_jest(html))
            {
                using (StreamWriter sw = File.AppendText(baza_path))
                {
                    sw.WriteLine(html + '|' + Linkstacji_textbox.Text + '|' + Linkstream_textbox.Text);

                    ilosc++;
                    iterator++;

                    if (ilosc >= 1 || ilosc < 5)
                    {
                        MessageBox.Show(messageBoxText: $"Dodano: {ilosc} stację\nNazwa stacji: {html}", caption: "Informacja",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        Dodanych_label.Content = $"Dodano: {ilosc} stację";
                    }
                    if (ilosc >= 5)
                    {
                        MessageBox.Show(messageBoxText: $"Dodano: {ilosc} stacji\nNazwa stacji: {html}", caption: "Informacja",
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        Dodanych_label.Content = $"Dodano: {ilosc} stacji";
                    }
                }
                Clear();
            }
            else
            {
                MessageBox.Show(messageBoxText: "Podana stacja jest już zapisana\nPodaj inną",
                    caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                Clear();
            }
        }

        private void Clear()
        {
            Linkstacji_textbox.Clear();
            Linkstream_textbox.Clear();
            Postep_progressBar.Value = 0;
        }

        private void Dodaj_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Linkstacji_textbox.Text) && !string.IsNullOrWhiteSpace(Linkstream_textbox.Text))
                {
                    Bar();
                    web.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Web_DownloadStringCompleted);
                    web.DownloadStringAsync(new Uri(Linkstacji_textbox.Text));
                }
                else Dodanych_label.Content = ("Pola nie mogą być puste");
            }
            catch (Exception)
            {
                MessageBox.Show(messageBoxText: "Wystąpił błąd",
                    caption: "EROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
        }

        private void Web_button_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start("https://tunein.com/"); }
            catch (Exception) { }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DropShadowEffect dropShadowEffect = new DropShadowEffect { Color = new Color { R = 0, G = 191, B = 255 } };
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    Help_button.Effect = dropShadowEffect;
                    break;
                case 2:
                    RefreshButton.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    Help_button.Effect = null;
                    break;
                case 2:
                    RefreshButton.Effect = null;
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
                    Help_button.Effect = dropShadowEffect;
                    break;
                case 2:
                    RefreshButton.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    try
                    {
                        Net_radio_browser browser_window = new Net_radio_browser();
                        browser_window.Set_Data("https://netradiohelper.azurewebsites.net/", null, 1);
                        browser_window.Show();
                    }
                    catch (Exception) { }
                    break;
                case 2:
                    RefreshButton.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
                    ListView.ItemsSource = null;
                    ListView.Items.Refresh();
                    items.Clear();
                    Pokaz();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region ListaStacji

        public void Set_iterator(int i, string path)
        {
            iterator = i;
            baza_path_list_station = path;
            Pokaz();
        }

        private void Pokaz()
        {
            using (StreamReader sw = File.OpenText(baza_path))
            {
                for (int i = 0; i < iterator; i++)
                {
                    dane = sw.ReadLine()?.Split('|');
                    items.Add(item: new ListaStacjiLista()
                    {
                        LP = i + 1,
                        Radio = dane[0],
                        Stream = dane[1],
                        Link = dane[2]
                    });
                }
                ListView.Items.Refresh();
                ListView.ItemsSource = items;
                view = (CollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (items.Count > 0)
            {
                CollectionViewSource.GetDefaultView(ListView.ItemsSource).Refresh();
                view.Filter = UserFilter;
            }
        }

        private void SortClick_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(FilterTextBox.Text)) return true;
            else
            {
                switch (FilterComboBox.SelectedIndex)
                {
                    case 0:
                        return true;
                    case 1:
                        return ((item as ListaStacjiLista).LP.ToString().IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 2:
                        return ((item as ListaStacjiLista).Radio.IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 3:
                        return ((item as ListaStacjiLista).Stream.IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 4:
                        return ((item as ListaStacjiLista).Link.IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    default:
                        return true;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Convert.ToByte(((MenuItem)sender).Tag))
                {
                    case 1:
                        Net_radio_browser net_Radio_Browser = new Net_radio_browser();
                        net_Radio_Browser.Set_Data("https://google.com", items[((dynamic)ListView.SelectedItems[0]).LP - 1].Radio, 0);
                        net_Radio_Browser.Show();
                        break;
                    case 2:
                        Radio.radio.Nazwa_combobox.Text = items[((dynamic)ListView.SelectedItems[0]).LP - 1].Radio;
                        RadioWindow.radioWindow.TabControlIndex = 0;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception) { }
        }

        #endregion   

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Dodaj_button_Click(sender: sender, e: e);
                    break;
                default:
                    break;
            }
        }
    }
}