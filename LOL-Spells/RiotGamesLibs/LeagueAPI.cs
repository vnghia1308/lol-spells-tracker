using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace LOL_Spells
{
    internal class LeagueAPI
    {
        public static JArray GetCurrentMatchPlayerList()
        {
            string data = GetLeagueRequest("/liveclientdata/playerlist");

            if (data != null)
            {
                return JArray.Parse(data);
            }

            return null;
        }

        public static JObject GetCurrentMatchStats()
        {
            string data = GetLeagueRequest("/liveclientdata/gamestats");

            if (data != null)
            {
                return JObject.Parse(data);
            }

            return null;
        }

        public static string GetCurrentPlayerName()
        {
            string data = GetLeagueRequest("/liveclientdata/activeplayername");

            if (data != null)
            {
                return data.Replace("\"", string.Empty);
            }

            return null;
        }

        private static string GetLeagueRequest(string endpoint)
        {
            try
            {
                // @ref https://stackoverflow.com/questions/9145667/how-to-post-json-to-a-server-using-c
                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://127.0.0.1:2999{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
            catch
            {
                Console.WriteLine("[ERROR] Cannot connect to Riot Client API");
            }

            return null;
        }
    }
}
