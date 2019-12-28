using System.Text;

namespace IpScanner
{
    class GetPublicDataClass
    {
        public static string GetIp(string data)
        {
            string ip = data.Substring(data.IndexOf("IP Address:") + 64);
            ip = ip.Substring(startIndex: 0, length: ip.IndexOf("<"));
            return ip;
        }

        public static string GetIsp(string data)
        {
            string isp = data.Substring(data.IndexOf("ISP:") + 16);
            isp = isp.Substring(0, isp.IndexOf("<"));
            return isp;
        }

        public static string GetLocal(string data)
        {
            string local = data.Substring(data.IndexOf("Local") + 18);
            local = local.Substring(0, length: local.IndexOf("<"));
            local = local.Replace("&nbsp;-&nbsp;", ", ");
            return local;
        }

        public static string GetLocation(string data)
        {
            string location = data.Substring(data.IndexOf("Location:") + 21);
            location = location.Substring(0, location.IndexOf("<"));
            location = Encoding.UTF8.GetString(Encoding.Default.GetBytes(location));
            return location;
        }

        public static string GetHostname(string data)
        {
            string hostname = data.Substring(data.IndexOf("Hostname:") + 25);
            hostname = hostname.Substring(0, hostname.IndexOf("\""));
            return hostname;
        }

        public static string GetCoords(string data)
        {
            string coords = data.Substring(data.IndexOf("Coord") + 19);
            coords = coords.Substring(0, coords.IndexOf("<"));
            return coords;
        }
    }
}