using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using WpfApp.Markers;

namespace IpScanner
{
    public partial class MapWindow : Window
    {
        GMapMarker currentMarker;
        ToolTip MapToolTip = new ToolTip();
        bool maximized = false;
        int currentZoom = 0;

        public MapWindow() => InitializeComponent();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                default:
                    break;
            }
        }

        public void SetCoord(string coords)
        {
            MainMap.MapProvider = GMapProviders.GoogleMap;

            MainMap.Position = new PointLatLng(lat: Convert.ToDouble
                ((coords.Substring(4, coords.IndexOf(',') - 5)).Replace('.', ',')),
                lng: Convert.ToDouble(coords.Substring(coords.IndexOf("Lon") + 4).Replace('.', ',')));

            MainMap.ShowCenter = false;

            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "Twoja pozycja lub pozycja jednego z serwerów");
                currentMarker.Offset = new Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }

            ZoomTask();
        }

        private Task ZoomTask() => Task.Run(async () =>
        {
            for (int i = 0; i <= 20; i++)
            {
                await Task.Delay(150);
                Application.Current.Dispatcher.Invoke(new Action(() => { ZoomSlider.Value = i; }));
            }

            await Task.Delay(1000);

            for (int i = 20; i >= 10; i--)
            {
                await Task.Delay(150);
                Application.Current.Dispatcher.Invoke(new Action(() => { ZoomSlider.Value = i; }));
            }
        });

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!maximized)
            {
                maximized = true;
                WindowState = WindowState.Maximized;
                ZoomSlider.TickFrequency = 0.5;
            }

            else
            {
                maximized = false;
                WindowState = WindowState.Normal;
                ZoomSlider.TickFrequency = 2;
            }
            ZoomSlider.Width = kk.ActualWidth - 313;
        }

        private void MagnifierButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 10;
            MainMap.ZoomAndCenterMarkers(null);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch (Exception) { }
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e) => ZoomSlider.Value++;

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => ZoomSlider.Value--;

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentZoom = (int)ZoomSlider.Value;
            MainMap.Zoom = currentZoom;
        }

        private void MainMap_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    ZoomSlider.Value++;
                    break;
                case MouseButton.Right:
                    ZoomSlider.Value--;
                    break;
                default:
                    break;
            }
        }

        private void MainMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ZoomSlider.Value++;
            else ZoomSlider.Value--;
        }

        private Task HideToolTipTask() => Task.Run(async () =>
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { ToolTipLabel.Visibility = Visibility.Visible; }));
            await Task.Delay(1760);
            Application.Current.Dispatcher.Invoke(new Action(() => { ToolTipLabel.Visibility = Visibility.Hidden; }));
        });

        private void MainMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle) MagnifierButton_Click(sender: sender, e: e);
            if (e.ChangedButton == MouseButton.Left) HideToolTipTask();
        }
    }
}
