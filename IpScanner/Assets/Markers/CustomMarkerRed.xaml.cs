using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GMap.NET.WindowsPresentation;
using IpScanner;

namespace WpfApp.Markers
{
    public partial class CustomMarkerRed
    {
        Popup Popup;
        Label Label;
        GMapMarker Marker;
        MapWindow mapWindow;

        public CustomMarkerRed(MapWindow window, GMapMarker marker, string title)
        {
            InitializeComponent();

            mapWindow = window;
            Marker = marker;

            Popup = new Popup();
            Label = new Label();

            MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);

            var converter = new BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#505461");

            Popup.Placement = PlacementMode.Mouse;
            {
                Label.Background = brush;
                Label.Foreground = Brushes.White;
                Label.BorderBrush = Brushes.WhiteSmoke;
                Label.BorderThickness = new Thickness(1);
                Label.Padding = new Thickness(5);
                Label.FontSize = 16;
                Label.Content = title;
            }
            Popup.Child = Label;
        }

        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Popup.IsOpen = true;
        }
    }
}