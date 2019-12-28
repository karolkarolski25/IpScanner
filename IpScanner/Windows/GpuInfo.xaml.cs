using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace IpScanner.Windows
{
    public partial class GpuInfo
    {
        List<string> list = new List<string>();

        public GpuInfo()
        {
            InitializeComponent();
            FillGraphic();
        }

        private List<string> Graphic(string hwclass, string syntax)
        {
            try
            {
                list.Clear();
                foreach (ManagementObject queryObj in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM "
                    + hwclass).Get()) list.Add(queryObj[syntax].ToString());
                return list;
            }
            catch (Exception)
            {
                list.Add(string.Empty);
                return list;
            }
        }

        private Task FillGraphic() => Task.Run(() =>
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GraphicTextBox.AppendText("\nKarta graficzna\n\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Model: \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "Name")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Pamieć VRAM (AdapterRAM): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "AdapterRAM")) GraphicTextBox.AppendText($"" +
                        $"{Math.Round(Convert.ToDouble(s) / 1024 / 1024, 2)} MB\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Nazwa procesora graficznego (VideoProcessor): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "VideoProcessor")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Wersja sterownika (DriverVersion): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "DriverVersion")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Zainstalowane sterowniki (InstalledDisplayDrivers): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "InstalledDisplayDrivers")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Status: \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "Status")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Nazwa tworzenia systemu: \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "SystemCreationClassName")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Nazwa systemu (SystemName): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "SystemName")) GraphicTextBox.AppendText($"{s}\n");
                    GraphicTextBox.AppendText("-------------------------------------------------\n");
                    GraphicTextBox.AppendText("Opis trybu (VideoModeDescription): \n\n");
                    foreach (string s in Graphic("Win32_VideoController", "VideoModeDescription")) GraphicTextBox.AppendText($"{s}\n");
                });         
            }
            catch (Exception) { }
        });
    }
}
