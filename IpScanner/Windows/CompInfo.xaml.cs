using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace IpScanner.Windows
{
    public partial class CompInfo
    {
        List<Data> items = new List<Data>();
        CollectionView view;
        PropertyGroupDescription groupDescription;

        public CompInfo()
        {
            InitializeComponent();
            Init();
            cmbxOption.SelectedItem = "Win32_Processor";
        }

        private async void Init() => await FillTask();

        private Task FillTask() => Task.Run(() =>
         {
             Application.Current.Dispatcher.Invoke(() =>
             {
                 cmbxDeveloper.ItemsSource = FillCompInfoClass.FillcmbxDeveloper();
                 cmbxMemory.ItemsSource = FillCompInfoClass.FillcmbxMemory();
                 cmbxNetwork.ItemsSource = FillCompInfoClass.FillcmbxNetwork();
                 cmbxOption.ItemsSource = FillCompInfoClass.FillcmbxOption();
                 cmbxStorage.ItemsSource = FillCompInfoClass.FillcmbxStorage();
                 cmbxSystemInfo.ItemsSource = FillCompInfoClass.FillcmbxUserAccount();
                 cmbxUtility.ItemsSource = FillCompInfoClass.FillcmbxUtility();
                 cmbxUserAccount.ItemsSource = FillCompInfoClass.FillcmbxUserAccount();
             });
        });

        private void InsertInfo(string Key, ref ListView lst, bool DontInsertNull)
        {
            if (items.Count >= 0)
            {
                items.Clear();
                lst.ItemsSource = items;
                lst.Items.Refresh();
                try { view.GroupDescriptions.Clear(); }
                catch (Exception) { }
            }

            string Name = "", Value = "", HeaderName = "";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + Key);

            try
            {
                foreach (ManagementObject share in searcher.Get())
                {
                    try
                    {
                        HeaderName = share["Name"].ToString();
                    }
                    catch
                    {
                        HeaderName = share.ToString();
                    }

                    if (share.Properties.Count <= 0)
                    {
                        MessageBox.Show(messageBoxText: "No Information Available", caption: "No Info", 
                            button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                        return;
                    }

                    foreach (PropertyData PC in share.Properties)
                    {

                        ListViewItem item = new ListViewItem();

                        if (lst.Items.Count % 2 != 0)
                            item.Background = Brushes.White;
                        else
                            item.Background = Brushes.WhiteSmoke;

                        Name = PC.Name;

                        if (PC.Value != null && PC.Value.ToString() != "")
                        {
                            switch (PC.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] str = (string[])PC.Value;

                                    string str2 = "";
                                    foreach (string st in str)
                                        str2 += st + " ";

                                    Value = str2;

                                    break;
                                case "System.UInt16[]":
                                    ushort[] shortData = (ushort[])PC.Value;

                                    string tstr2 = "";
                                    foreach (ushort st in shortData)
                                        tstr2 += st.ToString() + " ";

                                    Value = tstr2;

                                    break;

                                default:
                                    Value = PC.Value.ToString();
                                    break;
                            }
                        }
                        else
                        {
                            if (!DontInsertNull)
                                Value = "No Information available";
                            else
                                continue;
                        }
                        items.Add(new Data { Name = Name, Value = Value, GroupName = $"  {HeaderName}" });
                    }
                }

                lst.ItemsSource = items;

                view = (CollectionView)CollectionViewSource.GetDefaultView(lst.ItemsSource);
                groupDescription = new PropertyGroupDescription("GroupName");
                view.GroupDescriptions.Add(groupDescription);
            }

            catch (Exception exp)
            {
                MessageBox.Show(messageBoxText: "can't get data because of the followeing error \n" + exp.Message, 
                    caption: "Error", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }
        }

        private void RemoveNullValue(ref ListView lst)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Value == "No Information available")
                {
                    items.RemoveAt(i);
                }
            }
        }

        #region Control events ...

        private void CmbxNetwork_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxNetwork.SelectedItem.ToString(), ref lstNetwork, (bool)chkNetwork.IsChecked);
        }

        private void CmbxSystemInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxSystemInfo.SelectedItem.ToString(), ref lstSystemInfo, (bool)chkSystemInfo.IsChecked);
        }

        private void CmbxUtility_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxUtility.SelectedItem.ToString(), ref lstUtility, (bool)chkUtility.IsChecked);
        }

        private void CmbxUserAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxUserAccount.SelectedItem.ToString(), ref lstUserAccount, (bool)chkUserAccount.IsChecked);
        }

        private void CmbxStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxStorage.SelectedItem.ToString(), ref lstStorage, (bool)chkDataStorage.IsChecked);
        }

        private void CmbxDeveloper_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxDeveloper.SelectedItem.ToString(), ref lstDeveloper, (bool)chkDeveloper.IsChecked);
        }

        private void CmbxMemory_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxMemory.SelectedItem.ToString(), ref lstMemory, (bool)chkMemory.IsChecked);
        }

        private void ChkHardware_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkHardware.IsChecked)
                RemoveNullValue(ref lstDisplayHardware);
            else
                InsertInfo(cmbxOption.SelectedItem.ToString(), ref lstDisplayHardware, (bool)chkHardware.IsChecked);
        }

        private void CmbxOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            InsertInfo(cmbxOption.SelectedItem.ToString(), ref lstDisplayHardware, (bool)chkHardware.IsChecked);
        }

        private void ChkDataStorage_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkDataStorage.IsChecked)
                RemoveNullValue(ref lstStorage);
            else
                InsertInfo(cmbxStorage.SelectedItem.ToString(), ref lstStorage, (bool)chkDataStorage.IsChecked);
        }

        private void ChkMemory_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkMemory.IsChecked)
                RemoveNullValue(ref lstMemory);
            else
                InsertInfo(cmbxMemory.SelectedItem.ToString(), ref lstStorage, false);
        }

        private void ChkSystemInfo_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkSystemInfo.IsChecked)
                RemoveNullValue(ref lstSystemInfo);
            else
                InsertInfo(cmbxSystemInfo.SelectedItem.ToString(), ref lstSystemInfo, false);
        }

        private void ChkNetwork_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkNetwork.IsChecked)
                RemoveNullValue(ref lstNetwork);
            else
                InsertInfo(cmbxNetwork.SelectedItem.ToString(), ref lstNetwork, false);
        }

        private void ChkUserAccount_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkUserAccount.IsChecked)
                RemoveNullValue(ref lstUserAccount);
            else
                InsertInfo(cmbxUserAccount.SelectedItem.ToString(), ref lstUserAccount, false);
        }

        private void ChkDeveloper_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkDeveloper.IsChecked)
                RemoveNullValue(ref lstDeveloper);
            else
                InsertInfo(cmbxDeveloper.SelectedItem.ToString(), ref lstDeveloper, false);
        }

        private void ChkUtility_CheckedChanged(object sender, EventArgs e)
        {
            if ((bool)chkUtility.IsChecked)
                RemoveNullValue(ref lstUtility);
            else
                InsertInfo(cmbxUtility.SelectedItem.ToString(), ref lstUtility, false);
        }

        #endregion
    }
}
