using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ComponentModel;

namespace IpScanner.Windows
{
    public partial class Lista_stacji
    {
        int iterator = 0;
        string[] dane;
        string baza_path;
        public List<ListaStacjiLista> items = new List<ListaStacjiLista>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;

        public Lista_stacji() => InitializeComponent();

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

        private void FillFilterComboBox()
        {
            FilterComboBox.Items.Add("Filtruj według:");
            FilterComboBox.Items.Add("Lp.");
            FilterComboBox.Items.Add("Nazwa stacji");
            FilterComboBox.Items.Add("Link do streama");
            FilterComboBox.Items.Add("Link do stacji");
            FilterComboBox.SelectedIndex = 0;
        }

        public void Set_iterator(int i, string path)
        {
            iterator = i;
            baza_path = path;
            FillFilterComboBox();
            Pokaz();
        }   

        private void FilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Net_radio_browser net_Radio_Browser = new Net_radio_browser();
            net_Radio_Browser.Set_Data("https://google.com", items[((dynamic)ListView.SelectedItems[0]).LP - 1].Radio , 0);
            net_Radio_Browser.Show();
        }
    }
}