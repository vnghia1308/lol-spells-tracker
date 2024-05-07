using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LOL_Spells
{
    internal class ClientAPI
    {
        private string RiotPort = null;
        private string RiotPassword = null;

        public void Init()
        {
            foreach (var process in Process.GetProcessesByName("LeagueClientUX"))
            {
                try
                {
                    string cmd = GetCommandLine(process.Id);
                    Regex regex = new Regex("\"(.*?)\"");

                    var matches = regex.Matches(cmd);

                    foreach (Match match in matches)
                    {
                        string cl = match.Groups[1].Value;

                        if (cl.Contains("--app-port"))
                        {
                            RiotPort = cl.Replace("--app-port=", string.Empty);
                        }

                        if (cl.Contains("--remoting-auth-token"))
                        {
                            RiotPassword = cl.Replace("--remoting-auth-token=", string.Empty);
                        }
                    }
                }
                catch { }
            }

            if (RiotPort == null || RiotPassword == null)
            {
                throw new Exception("Cannot get LCU info");
            }
        }

        public string SendRiotRequest(string endpoint, string method = "GET", string value = null)
        {
            try
            {
                // @ref https://stackoverflow.com/questions/9145667/how-to-post-json-to-a-server-using-c
                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://127.0.0.1:{RiotPort}{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = method;
                httpWebRequest.Headers.Add("Authorization", "Basic " + Base64Encode($"riot:{RiotPassword}"));
                httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;


                if (value != null)
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = value;

                        streamWriter.Write(json);
                    }
                }

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

        // @ref https://stackoverflow.com/questions/2633628/can-i-get-command-line-arguments-of-other-processes-from-net-c
        private string GetCommandLine(int processId)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + processId))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }
        }

        // @ref https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
