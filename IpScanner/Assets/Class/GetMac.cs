using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace IpScanner
{
    class GetMac
    {
        //Poniżej stara metoda pobierania adresu. Pobiera ona rowniez MAC np. VirtualBoxa, ale nie dziala na wszystkich komputerach
        //public static string HostMac() //Adres MAC hosta
        //{
        //    string mac = "";
        //    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
        //        if (nic.OperationalStatus == OperationalStatus.Up)
        //        {
        //            for (int i = 0; i < nic.GetPhysicalAddress().GetAddressBytes().Length; i++)
        //            {
        //                mac += nic.GetPhysicalAddress().GetAddressBytes()[i].ToString(format: "X2");
        //                if (i != nic.GetPhysicalAddress().GetAddressBytes().Length - 1) mac += ':';
        //            }
        //        }
        //    }
        //    return mac.Substring(0, 17); //Zwraca tylko MAC maszyny pomijajac MAC np. z VirtualBox
        //}

        public static string HostMac()
        {
            string macAddr =
                            (
                            from nic in NetworkInterface.GetAllNetworkInterfaces()
                            where nic.OperationalStatus == OperationalStatus.Up
                            select nic.GetPhysicalAddress().ToString()
                            ).FirstOrDefault();

            for (int i = 0; i < macAddr.Length; i++) if (i % 3 == 0) macAddr = macAddr.Insert(i, ":");
            return macAddr.Remove(0, 1);
        }

        public static string Result(string ipAddress) //Adresy MAC poszczegolnych adresow IP
        {
            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));

            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = $"-a {ipAddress}";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                return $"{substrings[3].Substring(startIndex: Math.Max(0, val2: substrings[3].Length - 2))}".ToUpper() +
                    $":{substrings[4]}:{substrings[5]}:{substrings[6]}:{substrings[7]}:{substrings[8].Substring(0, 2)}".ToUpper();
            }
            else if (ipAddress == GetIp.Subnet()) return HostMac();
            else return "Nie znaleziono";
        }
    }
}
/*
 * Ta klasa pobiera adresy MAC poszczegolnych adresow ip
 * Jest taki myk, ze funkcja, ktora pobiera adresy MAC od innych nie daje adresu hosta
 * Wiec ta klasa pobiera rowniez adres ip hosta i gdy ta klasa dostanie adres ip hosta
 * Nie szuka adresu MAC tylko zwraca MAC maszyny w sensie jest on zwaracany nie przez funkcje Result
 * Korzysta ona z klasy GetIp, z ktorej pobiera adres ip hosta
 */