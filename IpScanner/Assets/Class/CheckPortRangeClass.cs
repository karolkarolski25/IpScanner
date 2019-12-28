using System;
using System.Net;

namespace IpScanner
{
    public class CheckPortRangeClass
    {
        private static bool CheckSpace(string data)
        {
            if (data.IndexOf(' ') > 0) return false;
            else return true;
        }

        private static bool CheckDash(string data)
        {
            if (data.IndexOf('-') > 0) return true;
            else return false;
        }

        private static bool CheckLength(string data)
        {
            if (data.Length >= 3) return true;
            else return false;
        }

        private static bool CheckIp(string data)
        {
            try
            {
                if (IPAddress.TryParse(data, out IPAddress iPAddress)) return true;
                else throw new Exception();
            }
            catch (Exception) { return false; }
        }

        private static bool CheckDigits(string data, bool port)
        {
            try
            {
                int left = Convert.ToInt32(data.Substring(0, data.IndexOf('-')));
                int right = Convert.ToInt32(data.Substring(data.IndexOf('-') + 1));

                if (port)
                {
                    if (!(left < right)) throw new Exception();
                    if (left > 65535 || right > 65535) throw new Exception();                
                }

                else
                {
                    if (!(left < right)) throw new Exception();
                    if (left > 254 || right > 254) throw new Exception();
                }

                if (left < 0 || right < 0) throw new Exception();

                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool ResultIpScanner(string data, string ipData)
        {
            if (CheckLength(data) && CheckSpace(data) && CheckDash(data) && CheckDigits(data, false) && CheckIp(ipData))
                return true;
            else return false;
        }

        public static bool Result(string data, string ipData)
        {
            if (CheckLength(data) && CheckSpace(data) && CheckDash(data) && CheckDigits(data, true) && CheckIp(ipData))
                return true;
            else return false;
        }
    }
}