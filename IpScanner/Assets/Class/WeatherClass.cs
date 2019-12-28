using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IpScanner.Class
{
    class WeatherClass
    {
        private static List<string> AvailLangs;

        public static string API_KEY { get; } = "5bf3dfd23ebc1b9ca655e79ca30982c5";
        public static string CurrentUrl { get; } = "http://api.openweathermap.org/data/2.5/weather?" +
            "@QUERY@=@LOC@&mode=xml&units=metric&lang=pl&APPID=" + API_KEY;
        public static string ForecastUrl { get; } = "http://api.openweathermap.org/data/2.5/forecast?" +
            "@QUERY@=@LOC@&mode=xml&units=metric&lang=pl&APPID=" + API_KEY;
        
        public static string[] QueryCodes { get; } = { "q", "zip", "id", };

        public static IRestResponse RequestService(string strUrl)
        {
            var client = new RestClient()
            {
                BaseUrl = new Uri(strUrl)
            };
            var request = new RestRequest()
            {
                Method = Method.GET
            };
            return client.Execute(request);
        }

        private static void DetectLang(string data)
        {
            var response = RequestService(string.Format(AppCache.UrlGetAvailableLanguages, AppCache.API, data));

            var dict = JsonConvert.DeserializeObject<IDictionary>(response.Content);

            foreach (DictionaryEntry entry in dict)
            {
                if (entry.Key.Equals("langs"))
                {
                    var availableConverts = (JObject)entry.Value;
                    AvailLangs = new List<string>();
                    foreach (var lang in availableConverts)
                    {
                        if (!lang.Equals(data))
                        {
                            if (lang.Value.ToString() == "Polish")
                            {
                                AvailLangs.Add(lang.Key);
                            }
                        }
                    }
                }
            }
        }

        public static string Translate(string data)
        {
            DetectLang(data);
            var response = RequestService(string.Format(AppCache.UrlTranslateLanguage, AppCache.API, data, AvailLangs[0]));
            var dict = JsonConvert.DeserializeObject<IDictionary>(response.Content);
            var statusCode = dict["code"].ToString();

            if (statusCode.Equals("200"))
            {
                string translated = string.Join(",", dict["text"]);
                translated = translated.Substring(translated.IndexOf("\"") + 1);
                translated = translated.Substring(0, translated.IndexOf("\""));
                return translated;
            }
            return string.Empty;
        }

        public static string Encode(string data) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(data));

        public static Tuple<string, string> GetCoords(string data)
        {
            string lon, lat;

            lat = lon = data;

            if (lat.IndexOf(',') > 0) lat.Replace(',', '.');
            if (lon.IndexOf(',') > 0) lon.Replace(',', '.');

            lat = lat.Substring(0, lat.IndexOf(' '));
            lon = lon.Substring(lon.IndexOf(' ') + 1);

            return Tuple.Create(lat, lon);
        }
    }

    class AppCache
    {
        public static string API { get; } = @"trnsl.1.1.20190510T063524Z.8a34747f7701986f.7d81990f0ed5d67901a9c2d2d588a0b85e144e82";
        public static string UrlGetAvailableLanguages { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/getLangs?key={0}&ui={1}";
        public static string UrlDetecSrcLanguage { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/detect?key={0}&text={1}";
        public static string UrlTranslateLanguage { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/translate?key={0}&text={1}&lang={2}";
    }
}