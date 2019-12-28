using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace IpScanner.Windows
{
    public partial class MoBoInfo
    {
        public MoBoInfo()
        {
            InitializeComponent();
            FillMoBo();
        }

        private Task FillMoBo() => Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (ManagementObject queryObj in new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_BaseBoard").Get())
                {
                    MoBoInfoTextBox.AppendText("\n-----------------------------------\n");
                    MoBoInfoTextBox.AppendText("Win32_BaseBoard instance\n");
                    MoBoInfoTextBox.AppendText("-----------------------------------\n\n\n");
                    MoBoInfoTextBox.AppendText($"Caption: {queryObj["Caption"]}\n\n");

                    if (queryObj["ConfigOptions"] == null)
                        MoBoInfoTextBox.AppendText($"ConfigOptions: {queryObj["ConfigOptions"]}\n\n");
                    else
                    {
                        string[] arrConfigOptions = (string[])queryObj["ConfigOptions"];
                        foreach (string arrValue in arrConfigOptions)
                        {
                            MoBoInfoTextBox.AppendText($"ConfigOptions: {arrValue}\n\n");
                        }
                    }

                    MoBoInfoTextBox.AppendText($"CreationClassName: {queryObj["CreationClassName"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Depth: {queryObj["Depth"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Description: {queryObj["Description"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Height: {queryObj["Height"]}\n\n");
                    MoBoInfoTextBox.AppendText($"HostingBoard: {queryObj["HostingBoard"]}\n\n");
                    MoBoInfoTextBox.AppendText($"HotSwappable: {queryObj["HotSwappable"]}\n\n");
                    MoBoInfoTextBox.AppendText($"InstallDate: {queryObj["InstallDate"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Manufacturer: {queryObj["Manufacturer"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Model: {queryObj["Model"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Name: {queryObj["Name"]}\n\n");
                    MoBoInfoTextBox.AppendText($"OtherIdentifyingInfo: {queryObj["OtherIdentifyingInfo"]}\n\n");
                    MoBoInfoTextBox.AppendText($"PartNumber: {queryObj["PartNumber"]}\n\n");
                    MoBoInfoTextBox.AppendText($"PoweredOn: {queryObj["PoweredOn"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Product: {queryObj["Product"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Removable: {queryObj["Removable"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Replaceable: {queryObj["Replaceable"]}\n\n");
                    MoBoInfoTextBox.AppendText($"RequirementsDescription: {queryObj["RequirementsDescription"]}\n\n");
                    MoBoInfoTextBox.AppendText($"RequiresDaughterBoard: {queryObj["RequiresDaughterBoard"]}\n\n");
                    MoBoInfoTextBox.AppendText($"SerialNumber: {queryObj["SerialNumber"]}\n\n");
                    MoBoInfoTextBox.AppendText($"SKU: {queryObj["SKU"]}\n\n");
                    MoBoInfoTextBox.AppendText($"SlotLayout: {queryObj["SlotLayout"]}\n\n");
                    MoBoInfoTextBox.AppendText($"SpecialRequirements: {queryObj["SpecialRequirements"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Status: {queryObj["Status"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Tag: {queryObj["Tag"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Version: {queryObj["Version"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Weight: {queryObj["Weight"]}\n\n");
                    MoBoInfoTextBox.AppendText($"Width: {queryObj["Width"]}\n");
                }
            });         
        });
    }
}
