using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Data;
using System.IO;

namespace IpScanner.Windows
{
    public partial class TcpUdpConnections
    {
        private const int AF_INET = 2;
        public static int index = 0;

        List<TcpProcessRecord> TcpActiveConnections = new List<TcpProcessRecord>();
        List<UdpProcessRecord> UdpActiveConnections = new List<UdpProcessRecord>();

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);


        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        DispatcherTimer tmrDataRefreshTimer = new DispatcherTimer();

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private CollectionView view;

        public TcpUdpConnections()
        {
            InitializeComponent();

            tscProtocolType.Items.Add(Protocol.TCP);
            tscProtocolType.Items.Add(Protocol.UDP);

            tmrDataRefreshTimer.Tick += TmrDataRefreshTimer_Tick;
            tmrDataRefreshTimer.Interval = new TimeSpan(0, 0, 1);

            TsbStopCapture_Click(sender: new object(), e: new RoutedEventArgs());
        }

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(FilterTextBox.Text)) return true;
            else
            {
                switch (tscProtocolType.SelectedIndex)
                {
                    case 0:
                        switch (FilterComboBox.SelectedIndex)
                        {
                            case 0:
                                return true;
                            case 1:
                                return ((item as TcpProcessRecord).LocalAddress.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 2:
                                return ((item as TcpProcessRecord).LocalPort.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 3:
                                return ((item as TcpProcessRecord).RemoteAddress.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 4:
                                return ((item as TcpProcessRecord).RemotePort.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 5:
                                return ((item as TcpProcessRecord).State.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 6:
                                return ((item as TcpProcessRecord).ProcessId.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 7:
                                return ((item as TcpProcessRecord).ProcessName.IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            default:
                                return true;
                        }
                    case 1:
                        switch (FilterComboBox.SelectedIndex)
                        {
                            case 0:
                                return true;
                            case 1:
                                return ((item as UdpProcessRecord).LocalAddress.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 2:
                                return ((item as UdpProcessRecord).LocalPort.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 3:
                                return ((item as UdpProcessRecord).RemoteAddress.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 4:
                                return ((item as UdpProcessRecord).RemotePort.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 5:
                                return ((item as UdpProcessRecord).State.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 6:
                                return ((item as UdpProcessRecord).ProcessId.ToString().IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            case 7:
                                return ((item as UdpProcessRecord).ProcessName.IndexOf(FilterTextBox.Text, 
                                    StringComparison.OrdinalIgnoreCase) >= 0);
                            default:
                                return true;
                        }
                    default:
                        return true;
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!tmrDataRefreshTimer.IsEnabled)
            {
                if (gdvSocketConnections.Items.Count > 0)
                {
                    CollectionViewSource.GetDefaultView(gdvSocketConnections.ItemsSource).Refresh();
                    view.Filter = UserFilter;
                }
            }

            else
            {           
                switch (MessageBox.Show(messageBoxText: "Filtrowanie dostępne tylko po zatrzymaniu\nCzy chcesz zatrzymać?", 
                    caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        break;
                    case MessageBoxResult.Yes:
                        TsbStopCapture_Click(sender: sender, e: e);
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        break;
                }
            }
        }

        private void SortClick_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                gdvSocketConnections.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            gdvSocketConnections.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private static Task<List<TcpProcessRecord>> GetAllTcpConnectionsTask() => Task.Run(() => GetAllTcpConnections());

        private static List<TcpProcessRecord> GetAllTcpConnections()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {            
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                if (result != 0)
                    return new List<TcpProcessRecord>();

                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                          new IPAddress(tcpRow.localAddr),
                                          new IPAddress(tcpRow.remoteAddr),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.localPort[1],
                                              tcpRow.localPort[0] }, 0),
                                          BitConverter.ToUInt16(new byte[2] {
                                              tcpRow.remotePort[1],
                                              tcpRow.remotePort[0] }, 0),
                                          tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd\nTreść błędu: {ex.Message}", caption: "ERROR", 
                    button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList() : new List<TcpProcessRecord>();
        }

        private static Task<List<UdpProcessRecord>> GetAllUdpConnectionsTask() => Task.Run(() => GetAllUdpConnections());

        private static List<UdpProcessRecord> GetAllUdpConnections()
        {
            int bufferSize = 0;
            List<UdpProcessRecord> udpTableRecords = new List<UdpProcessRecord>();

            uint result = GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true,
                AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

            IntPtr udpTableRecordPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                result = GetExtendedUdpTable(udpTableRecordPtr, ref bufferSize, true,
                    AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID);

                if (result != 0)
                    return new List<UdpProcessRecord>();

                MIB_UDPTABLE_OWNER_PID udpRecordsTable = (MIB_UDPTABLE_OWNER_PID)
                    Marshal.PtrToStructure(udpTableRecordPtr, typeof(MIB_UDPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)udpTableRecordPtr +
                    Marshal.SizeOf(udpRecordsTable.dwNumEntries));

                for (int i = 0; i < udpRecordsTable.dwNumEntries; i++)
                {
                    MIB_UDPROW_OWNER_PID udpRow = (MIB_UDPROW_OWNER_PID)
                        Marshal.PtrToStructure(tableRowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    udpTableRecords.Add(new UdpProcessRecord(new IPAddress(udpRow.localAddr),
                        BitConverter.ToUInt16(new byte[2] { udpRow.localPort[1],
                            udpRow.localPort[0] }, 0), udpRow.owningPid));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(udpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(udpTableRecordPtr);
            }
            return udpTableRecords != null ? udpTableRecords.Distinct()
                .ToList() : new List<UdpProcessRecord>();
        }

        private void SocketConnection_Load(object sender, EventArgs e)
        {
            tscProtocolType.SelectedIndex = (int)Protocol.TCP;
        }

        private void TscProtocolType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gdvSocketConnections.Items.Refresh();
            gdvSocketConnections.ItemsSource = null;
        }

        private async void TmrDataRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (tscProtocolType.SelectedIndex == (int)Protocol.TCP)
            {
                if (tmrDataRefreshTimer.IsEnabled) TcpActiveConnections = await GetAllTcpConnectionsTask();

                gdvSocketConnections.Items.Refresh();
                gdvSocketConnections.ItemsSource = TcpActiveConnections;
                tslTotalRecords.Content = ((tmrDataRefreshTimer.IsEnabled) ? 
                    $"Aktywne połączenia TCP: {gdvSocketConnections.Items.Count.ToString()}": "Zatrzymano");
            }

            else if (tscProtocolType.SelectedIndex == (int)Protocol.UDP)
            {
                if (tmrDataRefreshTimer.IsEnabled) UdpActiveConnections = await GetAllUdpConnectionsTask();

                gdvSocketConnections.Items.Refresh();
                gdvSocketConnections.ItemsSource = UdpActiveConnections;
                tslTotalRecords.Content = ((tmrDataRefreshTimer.IsEnabled) ? 
                    $"Aktywne połączenia UDP: {gdvSocketConnections.Items.Count.ToString()}" : "Zatrzymano");
            }
            view = (CollectionView)CollectionViewSource.GetDefaultView(gdvSocketConnections.ItemsSource);
            index = 0;
        }

        private void GdvConnections_SelectionChanged(object sender, EventArgs e) => gdvSocketConnections.UnselectAll();   

        private void TsbStartCapture_Click(object sender, EventArgs e)
        {
            tmrDataRefreshTimer.Start();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                tsbStopCapture.Background = Brushes.Red;
                tsbStartCapture.Background = Brushes.Green;
            }));
        }

        private void TsbStopCapture_Click(object sender, EventArgs e)
        {
            tmrDataRefreshTimer.Stop();
            tsbStopCapture.Background = Brushes.Green;
            tsbStartCapture.Background = Brushes.Red;
        }

        private void TsbCopyData_Click(object sender, EventArgs e)
        {
            if (!tmrDataRefreshTimer.IsEnabled)
            {
                switch (MessageBox.Show(messageBoxText: "Czy chcesz aby plik znalazł się na pulpicie? ",
                         caption: "Zapytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        switch (tscProtocolType.SelectedIndex)
                        {
                            case 0:
                                Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TabelaTcp.txt", true);
                                break;
                            case 1:
                                Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TabelaUdp.txt", false);
                                break;
                            default:
                                break;
                        }
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show(messageBoxText: "Zostaniesz poproszony o podanie nowego położenia pliku",
                            caption: "Informacja", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        switch (tscProtocolType.SelectedIndex)
                        {
                            case 0:
                                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                                { FileName = "TabelaTcp", DefaultExt = ".text", Filter = "Text documents (.txt)|*.txt" };
                                if (Dialog(dlg) == true) Save(dlg.FileName, true);
                                break;
                            case 1:
                                dlg = new Microsoft.Win32.SaveFileDialog
                                { FileName = "TabelaUdp", DefaultExt = ".text", Filter = "Text documents (.txt)|*.txt" };
                                if (Dialog(dlg) == true) Save(dlg.FileName, false);
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
            else
            {
                switch (MessageBox.Show(messageBoxText: "Zapisywanie dostępne tylko po zatrzymaniu\nCzy chcesz zatrzymać?",
                    caption: "Pytanie", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        break;
                    case MessageBoxResult.Yes:
                        TsbStopCapture_Click(sender: new object(), e: new EventArgs());
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Save(string path, bool tcp)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Adres lokalny\tPort lokalny\tID procesu\tStan\tAdres zdalny\tPort zdalny\tNazwa procesu");
                if (tcp)
                {
                    for (int i = 0; i < TcpActiveConnections.Count; i++)
                    {
                        writer.WriteLine($"{TcpActiveConnections[i].LocalAddress}\t{TcpActiveConnections[i].LocalPort}\t" +
                            $"{TcpActiveConnections[i].ProcessId}\t{TcpActiveConnections[i].State}\t" +
                            $"{TcpActiveConnections[i].RemoteAddress}\t{TcpActiveConnections[i].RemotePort}\t" +
                            $"{TcpActiveConnections[i].ProcessName}");
                    }
                }
                else
                {
                    for (int i = 0; i < UdpActiveConnections.Count; i++)
                    {
                        writer.WriteLine($"{UdpActiveConnections[i].LocalAddress}\t{UdpActiveConnections[i].LocalPort}\t" +
                            $"{UdpActiveConnections[i].ProcessId}\t{UdpActiveConnections[i].State}\t" +
                            $"{UdpActiveConnections[i].RemoteAddress}\t{UdpActiveConnections[i].RemotePort}\t" +
                            $"{UdpActiveConnections[i].ProcessName}");
                    }
                }
            }
        }

        private static bool? Dialog(Microsoft.Win32.SaveFileDialog dlg) => dlg.ShowDialog();
    }

    public enum Protocol
    {
        TCP,
        UDP
    }

    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    public enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public int owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class TcpProcessRecord
    {
        public int Lp { get; set; }
        public string LocalAddress { get; set; }
        public uint LocalPort { get; set; }
        public string RemoteAddress { get; set; }
        public ushort RemotePort { get; set; }
        public MibTcpState State { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state)
        {
            Lp = TcpUdpConnections.index++;
            LocalAddress = localIp.ToString();
            RemoteAddress = remoteIp.ToString();
            LocalPort = localPort;
            RemotePort = remotePort;
            State = state;
            ProcessId = pId;
            if (Process.GetProcesses().Any(process => process.Id == pId))
            {
                ProcessName = Process.GetProcessById(ProcessId).ProcessName;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public int owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public UdpProcessRecord[] table;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UdpProcessRecord
    {
        public int Lp { get; set; }
        public string LocalAddress { get; set; }
        public uint LocalPort { get; set; }
        public string RemoteAddress { get; set; }
        public ushort RemotePort { get; set; }
        public string State { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public UdpProcessRecord(IPAddress localAddress, uint localPort, int pId)
        {
            Lp = TcpUdpConnections.index++;
            LocalAddress = localAddress.ToString();
            RemoteAddress = "Brak";
            LocalPort = localPort;
            ProcessId = pId;
            State = "Brak";
            if (Process.GetProcesses().Any(process => process.Id == pId))
                ProcessName = Process.GetProcessById(ProcessId).ProcessName;
        }
    }
}
