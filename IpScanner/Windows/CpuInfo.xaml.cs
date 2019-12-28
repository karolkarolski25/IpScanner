using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace IpScanner.Windows
{
    public partial class CpuInfo
    {
        public CpuInfo()
        {
            InitializeComponent();
            FillCpu();
        }

        private Task FillCpu() => Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (ManagementObject queryObj in new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_Processor").Get())
                {
                    CpuInfoTextBox.AppendText("-----------------------------------\n");
                    CpuInfoTextBox.AppendText("Win32_Processor instance\n");
                    CpuInfoTextBox.AppendText("-----------------------------------\n\n\n");
                    CpuInfoTextBox.AppendText($"Caption: {queryObj["Caption"]}\n\n");
                    CpuInfoTextBox.AppendText($"AddressWidth: {queryObj["AddressWidth"]}\n\n");
                    CpuInfoTextBox.AppendText($"Architecture: {queryObj["Architecture"]}\n\n");
                    CpuInfoTextBox.AppendText($"AssetTag: {queryObj["AssetTag"]}\n\n");
                    CpuInfoTextBox.AppendText($"Availability: {queryObj["Availability"]}\n\n");
                    CpuInfoTextBox.AppendText($"Characteristics: {queryObj["Characteristics"]}\n\n");
                    CpuInfoTextBox.AppendText($"ConfigManagerErrorCode: {queryObj["ConfigManagerErrorCode"]}\n\n");
                    CpuInfoTextBox.AppendText($"ConfigManagerUserConfig: {queryObj["ConfigManagerUserConfig"]}\n\n");
                    CpuInfoTextBox.AppendText($"CpuStatus: {queryObj["CpuStatus"]}\n\n");
                    CpuInfoTextBox.AppendText($"CreationClassName: {queryObj["CreationClassName"]}\n\n");
                    CpuInfoTextBox.AppendText($"CurrentClockSpeed: {queryObj["CurrentClockSpeed"]}\n\n");
                    CpuInfoTextBox.AppendText($"CurrentVoltage: {queryObj["CurrentVoltage"]}\n\n");
                    CpuInfoTextBox.AppendText($"DataWidth: {queryObj["DataWidth"]}\n\n");
                    CpuInfoTextBox.AppendText($"Description: {queryObj["Description"]}\n\n");
                    CpuInfoTextBox.AppendText($"DeviceID: {queryObj["DeviceID"]}\n\n");
                    CpuInfoTextBox.AppendText($"ErrorCleared: {queryObj["ErrorCleared"]}\n\n");
                    CpuInfoTextBox.AppendText($"ErrorDescription: {queryObj["ErrorDescription"]}\n\n");
                    CpuInfoTextBox.AppendText($"ExtClock: {queryObj["ExtClock"]}\n\n");
                    CpuInfoTextBox.AppendText($"Family: {queryObj["Family"]}\n\n");
                    CpuInfoTextBox.AppendText($"InstallDate: {queryObj["InstallDate"]}\n\n");
                    CpuInfoTextBox.AppendText($"L2CacheSize: {queryObj["L2CacheSize"]}\n\n");
                    CpuInfoTextBox.AppendText($"L2CacheSpeed: {queryObj["L2CacheSpeed"]}\n\n");
                    CpuInfoTextBox.AppendText($"L3CacheSize: {queryObj["L3CacheSize"]}\n\n");
                    CpuInfoTextBox.AppendText($"L3CacheSpeed: {queryObj["L3CacheSpeed"]}\n\n");
                    CpuInfoTextBox.AppendText($"LastErrorCode: {queryObj["LastErrorCode"]}\n\n");
                    CpuInfoTextBox.AppendText($"Level: {queryObj["Level"]}\n\n");
                    CpuInfoTextBox.AppendText($"LoadPercentage: {queryObj["LoadPercentage"]}\n\n");
                    CpuInfoTextBox.AppendText($"Manufacturer: {queryObj["Manufacturer"]}\n\n");
                    CpuInfoTextBox.AppendText($"MaxClockSpeed: {queryObj["MaxClockSpeed"]}\n\n");
                    CpuInfoTextBox.AppendText($"Name: {queryObj["Name"]}\n\n");
                    CpuInfoTextBox.AppendText($"NumberOfCores: {queryObj["NumberOfCores"]}\n\n");
                    CpuInfoTextBox.AppendText($"NumberOfEnabledCore: {queryObj["NumberOfEnabledCore"]}\n\n");
                    CpuInfoTextBox.AppendText($"NumberOfLogicalProcessors: {queryObj["NumberOfLogicalProcessors"]}\n\n");
                    CpuInfoTextBox.AppendText($"OtherFamilyDescription: {queryObj["OtherFamilyDescription"]}\n\n");
                    CpuInfoTextBox.AppendText($"PartNumber: {queryObj["PartNumber"]}\n\n");
                    CpuInfoTextBox.AppendText($"PNPDeviceID: {queryObj["PNPDeviceID"]}\n\n");
                    CpuInfoTextBox.AppendText($"PowerManagementSupported: {queryObj["PowerManagementSupported"]}\n\n");
                    CpuInfoTextBox.AppendText($"ProcessorId: {queryObj["ProcessorId"]}\n\n");
                    CpuInfoTextBox.AppendText($"ProcessorType: {queryObj["ProcessorType"]}\n\n");
                    CpuInfoTextBox.AppendText($"Revision: {queryObj["Revision"]}\n\n");
                    CpuInfoTextBox.AppendText($"Role: {queryObj["Role"]}\n\n");
                    CpuInfoTextBox.AppendText($"SecondLevelAddressTranslationExtensions: {queryObj["SecondLevelAddressTranslationExtensions"]}\n\n");
                    CpuInfoTextBox.AppendText($"SerialNumber: {queryObj["SerialNumber"]}\n\n");
                    CpuInfoTextBox.AppendText($"SocketDesignation: {queryObj["SocketDesignation"]}\n\n");
                    CpuInfoTextBox.AppendText($"Status: {queryObj["Status"]}\n\n");
                    CpuInfoTextBox.AppendText($"StatusInfo: {queryObj["StatusInfo"]}\n\n");
                    CpuInfoTextBox.AppendText($"Stepping: {queryObj["Stepping"]}\n\n");
                    CpuInfoTextBox.AppendText($"SystemCreationClassName: {queryObj["SystemCreationClassName"]}\n\n");
                    CpuInfoTextBox.AppendText($"SystemName: {queryObj["SystemName"]}\n\n");
                    CpuInfoTextBox.AppendText($"ThreadCount: {queryObj["ThreadCount"]}\n\n");
                    CpuInfoTextBox.AppendText($"UniqueId: {queryObj["UniqueId"]}\n\n");
                    CpuInfoTextBox.AppendText($"UpgradeMethod: {queryObj["UpgradeMethod"]}\n\n");
                    CpuInfoTextBox.AppendText($"Version: {queryObj["Version"]}\n\n");
                    CpuInfoTextBox.AppendText($"VirtualizationFirmwareEnabled: {queryObj["VirtualizationFirmwareEnabled"]}\n\n");
                    CpuInfoTextBox.AppendText($"VMMonitorModeExtensions: {queryObj["VMMonitorModeExtensions"]}\n\n");
                    CpuInfoTextBox.AppendText($"VoltageCaps: {queryObj["VoltageCaps"]}\n\n");
                }
            });         
        });
    }
}
