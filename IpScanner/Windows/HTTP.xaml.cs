using System.Windows.Input;

namespace IpScanner
{
    public partial class HTTP : System.Windows.Window
    {
        public HTTP() => InitializeComponent();
        private void Window_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
        public void SetLink(string link) => NetBrowser.Navigate(source: new System.Uri(uriString: "http://" + link));
    }
}