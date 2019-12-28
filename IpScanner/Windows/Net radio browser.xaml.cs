using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using System;
using mshtml;

namespace IpScanner
{
    public partial class Net_radio_browser : Window
    {
        int Tries, mode;
        string tytul;
        public Net_radio_browser()
        {
            InitializeComponent();
            wb1.LoadCompleted += new LoadCompletedEventHandler(Bws_LoadCompleted);
            wb1.Navigated += new NavigatedEventHandler(Bws_Navigated);    
        }

        void Bws_Navigated(object sender, NavigationEventArgs e)
        {
            HideScriptErrors(wb1, true);
        }

        void Bws_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (mode == 0)
            {
                switch (e.Uri.AbsolutePath)
                {
                    case "/":
                        DoExample1();
                        break;
                }
            }
        }

        public void Set_Data(string link, string zmienna, int tryb)
        {
            tytul = zmienna;
            mode = tryb;
            wb1.Navigate(link);
        }

        private void DoExample1() => TryFillInBing(tytul);
        
        void TryFillInBing(string name)
        {
            Thread.Sleep(500);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (!UpdateTextInput(0, "q", name) && Tries < 6)
                {
                    Tries++;
                    TryFillInBing(name);
                }
                else
                {
                    IHTMLFormElement form = GetForm(0);
                    form.submit();
                }
            }));
        }

        private bool UpdateTextInput(int formId, string name, string text)
        {
            bool successful = false;
            IHTMLFormElement form = GetForm(formId);
            if (form != null)
            {
                var element = form.item(name: name);
                if (element != null)
                {
                    var textinput = element as HTMLInputElement;
                    textinput.value = text;
                    successful = true;
                }
            }

            return successful;
        }

        private IHTMLFormElement GetForm(int formNo)
        {
            IHTMLDocument2 doc = (IHTMLDocument2)wb1.Document;
            IHTMLElementCollection forms = doc.forms;
            var ix = 0;
            foreach (IHTMLFormElement f in forms)
                if (ix++ == formNo)
                    return f;

            return null;
        }

        void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance 
                | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, 
                new object[] { Hide });
        }
    }
}
