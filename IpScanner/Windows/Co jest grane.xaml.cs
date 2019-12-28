using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Threading.Tasks;

namespace IpScanner.Windows
{
    public partial class Co_jest_grane
    {
        private int licznik = 0;
        private string baza_path;
        private bool abort = false, abortTitle = false;
        public List<CoJestGraneLista> items = new List<CoJestGraneLista>();
        private readonly WebClient web = new WebClient();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;

        internal static Co_jest_grane co_Jest_Grane;

        public Co_jest_grane()
        {
            InitializeComponent();
            co_Jest_Grane = this;
        }

        public void Set_path(string path) => baza_path = path;

        public async void Wypelnij()
        {
            string[] dane;
            using (StreamReader sw = File.OpenText(baza_path))
            {
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadLabel.Content = "Poberanie listy w toku";

                await Task.Delay(1000);

                while ((dane = sw.ReadLine()?.Split('|')) != null)
                {
                    items.Add(new CoJestGraneLista()
                    {
                        LP = (licznik++) + 1,
                        Stacja = dane[0],
                        Tytul = await Download(dane[1])
                    });
                    DownloadLabel.Content = $"Poberanie stacji nr. {licznik + 1}";
                    ListView.ItemsSource = items;
                    ListView.Items.Refresh();
                    if (abort) break;

                    ICollectionView view1 = CollectionViewSource.GetDefaultView(ListView.ItemsSource);
                    view1.Refresh();
                }

                DownloadProgressBar.Visibility = Visibility.Hidden;
                DownloadLabel.Content = "Zakończono pobieranie";

                await Task.Delay(1500);

                DownloadLabel.Content = "Oczekiwanie . . .";
                abort = true;
                DownloadButton.Content = "Pobierz listę";
                abortTitle = false;
            }
        }

        private string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        private Task<string> Download(string link) => Task.Run(() =>
        {
            string html = WebUtility.HtmlDecode(Encode(web.DownloadString(link)));
            html = html.Substring(html.IndexOf("1k9L1") + 7);
            html = html.Substring(0, html.IndexOf("</p>"));
            return html;
        });


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
                        return ((item as CoJestGraneLista).LP.ToString().IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 2:
                        return ((item as CoJestGraneLista).Stacja.IndexOf(FilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 3:
                        return ((item as CoJestGraneLista).Tytul.IndexOf(FilterTextBox.Text,
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
                    Net_radio_browser net_Radio_Browser = new Net_radio_browser();
                    net_Radio_Browser.Set_Data("https://google.com", items[((dynamic)ListView.SelectedItems[0]).LP - 1].Tytul, 0);
                    net_Radio_Browser.Show();
                    break;
                case 2:
                    Radio.radio.Nazwa_combobox.Text = items[((dynamic)ListView.SelectedItems[0]).LP - 1].Stacja;
                    RadioWindow.radioWindow.TabControlIndex = 0;
                    break;
                default:
                    break;
            }
        }

        public void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!abortTitle)
            {
                items.Clear();
                licznik = 0;
                Wypelnij();
                DownloadButton.Content = "Przewij proces";
                abortTitle = true;
                abort = false;
            }
            else
            {
                abort = true;
                DownloadButton.Content = "Pobierz listę";
                abortTitle = false;
            }
        }
    }
}