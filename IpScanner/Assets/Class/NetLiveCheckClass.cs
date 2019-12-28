using System;
using System.Net;
using System.Threading.Tasks;

namespace IpScanner
{
    public class NetLiveCheckClass
    { 
        public static Task<bool> CheckInternetConnection() => Task.Run(() =>
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        });

        public static Tuple <double, string> CheckDownloaded(double downloaded)
        {
            if (downloaded <= 1000) return Tuple.Create(downloaded, "Kb");
            else if (downloaded >= 1000 && downloaded < 1000000) return Tuple.Create(Math.Round(downloaded / 1024, 2), "Mb");
            else return Tuple.Create(Math.Round((downloaded / 1024)/ 1024, 2), "Gb");
        }

        public static Tuple<double, string> CheckUploaded(double uploaded)
        {
            if (uploaded <= 1000) return Tuple.Create(uploaded, "Kb");
            else if (uploaded >= 1000 && uploaded < 1000000) return Tuple.Create(Math.Round(uploaded / 1024, 2), "Mb");
            else return Tuple.Create(Math.Round((uploaded / 1024) / 1024, 2), "Gb");
        }

        public static Tuple<double, string> CheckDownload(double download)
        {
            if (download < 1000) return Tuple.Create(download, "Kb/s");
            else if (download > 10000) return Tuple.Create(Math.Round((download / 125) / 125, 2), "Gb/s");
            else if (download >= 1000 && download <= 10000) return Tuple.Create(Math.Round(download / 125, 2), "Mb/s");
            else return Tuple.Create(0.0, "inf");
        }

        public static Tuple<double, string> CheckUpload(double upload)
        {
            if (upload < 1000) return Tuple.Create(upload, "Kb/s");
            else if (upload > 10000) return Tuple.Create(Math.Round((upload / 125) / 125, 2), "Gb/s");
            else if (upload >= 1000 && upload <= 10000) return Tuple.Create(Math.Round(upload / 125, 2), "Mb/s");
            else return Tuple.Create(0.0, "inf");
        }
    }
}