using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;

namespace IpScanner
{
    class TracertPingClass
    {
        static Ping pinger = new Ping();

        static float czas = 0;

        public int HopID { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public long ReplyTime { get; set; }
        public IPStatus ReplyStatus { get; set; }

        public override string ToString() => $"{HopID}.   " +
            $"{(string.IsNullOrEmpty(Hostname) ? Address : Hostname + "  [ " + Address + " ]")} " +
                $"  {(ReplyStatus == IPStatus.TimedOut ? "Upłynął limit czasu żądania" : ReplyTime.ToString() + " ms")}";

        public static string DoGetHostEntry(string hostname)
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(hostname);
                string ajpi = "";
                foreach (IPAddress ip in host.AddressList) ajpi += ip;
                return ajpi;              
            }
            catch (Exception) { return hostname; }
        }

        public static bool CheckIp(string ip)
        {
            IPAddress iPAddress;
            if (!IPAddress.TryParse(DoGetHostEntry(ip), out iPAddress)) return false;
            else return true;
        }

        public static Tuple<string, double> Ping(string host)
        {
            PingReply pingReply = null;
            pingReply = pinger.Send(host);
            czas = pingReply.RoundtripTime;
            return Tuple.Create($"Odpowiedz od: {pingReply.Address}: Czas: {czas} ms, Status: {pingReply.Status}", 
                Convert.ToDouble(czas));
        }

        public static bool CheckData(string data)
        {
            try
            {
                int var = Convert.ToInt32(data);
                return true;
            }
            catch (Exception) { return false; }
        }

        public static IEnumerable<TracertPingClass> Tracert(string ipAddress, int maxHops, int timeout)
        {
            PingOptions pingOptions = new PingOptions(1, true);
            Stopwatch pingReplyTime = new Stopwatch();
            PingReply reply = null;
            do
            {
                pingReplyTime.Start();
                try { reply = pinger.Send(ipAddress, timeout, new byte[] { 0 }, pingOptions); }
                catch (Exception ex)
                {
                    MessageBox.Show(messageBoxText: $"Wystąpił błąd podczas procesu\nTreść błędu: " +
                    $"{ex.Message}", caption: "ERROR", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                    break;
                }

                pingReplyTime.Stop();

                string hostname = string.Empty;
                if (reply.Address != null)
                {
                    try { hostname = Dns.GetHostEntry(reply.Address).HostName; }
                    catch (SocketException) { }
                }

                yield return new TracertPingClass()
                {
                    HopID = pingOptions.Ttl,
                    Address = reply.Address == null ? "N/A" : reply.Address.ToString(),
                    Hostname = hostname,
                    ReplyTime = pingReplyTime.ElapsedMilliseconds,
                    ReplyStatus = reply.Status
                };

                pingOptions.Ttl++;
                pingReplyTime.Reset();
            }
            while (reply.Status != IPStatus.Success && pingOptions.Ttl <= maxHops);
        }
    }
}