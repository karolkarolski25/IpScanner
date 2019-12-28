using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace IpScanner.Windows
{
    public partial class RadioWindow : Window
    {
        internal static RadioWindow radioWindow;

        internal int TabControlIndex
        {
            get { return Tabs.SelectedIndex; }
            set { Dispatcher.Invoke(new Action(() => { Tabs.SelectedIndex = value; })); }
        }

        public RadioWindow()
        {
            InitializeComponent();
            radioWindow = this;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch (Exception) { }
        }

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
       
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            switch (Convert.ToByte(((Button)sender).Tag))
            {
                case 1:
                    ExitButton.Effect = null;
                    break;
                case 2:
                    MinimizeButton.Effect = null;
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
                    ExitButton.Effect = dropShadowEffect;
                    break;
                case 2:
                    MinimizeButton.Effect = dropShadowEffect;
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
                    ExitButton.Effect = dropShadowEffect;
                    break;
                case 2:
                    MinimizeButton.Effect = dropShadowEffect;
                    break;
                default:
                    break;
            }
        }

        private void ExitButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Close();

        private void MinimizeButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => 
            WindowState = WindowState.Minimized;

    }
}
