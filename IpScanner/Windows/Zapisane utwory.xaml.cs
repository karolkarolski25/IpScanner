using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace IpScanner.Windows
{
    public partial class Zapisane_utwory 
    {
        public int licznik = 1;
        string historia_path;
        public List<ZapisaneUtworyLista> items = new List<ZapisaneUtworyLista>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;

        internal static Zapisane_utwory zapisane_Utwory;

        public Zapisane_utwory()
        {
            InitializeComponent();
            zapisane_Utwory = this;
        }

        public void Set_path(string path)
        {
            historia_path = path;
            Pokaz();        
        }

        private void Pokaz()
        {
            try
            {
                items.Clear();
                BazaListView.ItemsSource = null;
                licznik = 1;

                string[] dane;
                using (StreamReader sw = File.OpenText(historia_path))
                {
                    while ((dane = sw.ReadLine()?.Split('|')) != null)
                        items.Add(new ZapisaneUtworyLista() { LP = (licznik++), Radio = dane[0], Title = dane[1], Time = dane[2] });
                }
                BazaListView.Items.Refresh();
                BazaListView.ItemsSource = items;
                view = (CollectionView)CollectionViewSource.GetDefaultView(BazaListView.ItemsSource);
            }
            catch (Exception) { }
        }

        private void PortFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (items.Count > 0)
            {
                CollectionViewSource.GetDefaultView(BazaListView.ItemsSource).Refresh();
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
                        return ((item as ZapisaneUtworyLista).LP.ToString().IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 2:
                        return ((item as ZapisaneUtworyLista).Radio.IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 3:
                        return ((item as ZapisaneUtworyLista).Title.IndexOf(PortFilterTextBox.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    case 4:
                        return ((item as ZapisaneUtworyLista).Time.IndexOf(PortFilterTextBox.Text, 
                            StringComparison.OrdinalIgnoreCase) >= 0);
                    default:
                        return true;
                }
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                BazaListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            BazaListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void BazaListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BazaListView.SelectedItem != null)
            {
                Net_radio_browser net_Radio_Browser = new Net_radio_browser();
                net_Radio_Browser.Set_Data("https://google.com", items[((dynamic)BazaListView.SelectedItems[0]).LP - 1].Title, 0);
                net_Radio_Browser.Show();
            }
        }

        private void RefreshButton_MouseEnter(object sender, MouseEventArgs e)
        {
            RefreshButton.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
        }

        private void RefreshButton_MouseLeave(object sender, MouseEventArgs e) => RefreshButton.Effect = null;

        private void RefreshButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RefreshButton.Effect = new DropShadowEffect { Color = new Color { R = 0, G = 191, B = 255 } };
        }

        private void RefreshButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RefreshButton.Effect = new DropShadowEffect { Color = new Color { R = 255, G = 165, B = 0 } };
            BazaListView.ItemsSource = null;
            BazaListView.Items.Refresh();
            items.Clear();
            Pokaz();
        }
    }
}