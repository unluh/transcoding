using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Proxy.ConsoleApp
{
    public class CssBundler : IHtmlProcessor
    {
        private readonly HttpClient httpClient;
        public CssBundler(HttpClient httpClient)
        {

            this.httpClient = httpClient;
        }

        void Log(string s)
        {
            System.IO.File.AppendAllText("/var/www/html/log/log", $"\n{s}");
        }
        public void Process(HtmlDocument document, bool isMinified)
        {

            var links = document.DocumentNode.Descendants("link");
            var result = links.Where(x => x.Attributes.Contains("href") && x.Attributes["rel"].Value.Equals("stylesheet")).ToList();

            StringBuilder contentBuilder = new StringBuilder();
            Log($"CSS {result.Count}");
            if (result.Count <= 1)
            {
                return;
            }
            foreach (var match in result)
            {
                var url = match.Attributes["href"].Value;
                if (url.StartsWith("//"))
                {
                    url = $"http:{url}";
                }

                using (var response = httpClient.GetAsync(url).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Log( $"CSS {url} successfull {response.RequestMessage.RequestUri}");
                        var cssContent = httpClient.GetStringAsync(url).Result;
                        contentBuilder.AppendLine(cssContent);
                        match.Remove();
                    }
                    else
                    {
                        Log($"CSS {url} unsuccessfull {response.RequestMessage.RequestUri}");
                    }

                }
            }

            Log( $"CSS len {contentBuilder.Length}");
            string id = Guid.NewGuid().ToString();
            string fileName = $"{id}.css";
            string path = $"/var/www/html/{fileName }";
            string lastCSSLocation = $"http://194.170.10.101/{fileName}";
            System.IO.File.WriteAllText(path, contentBuilder.ToString());

            if (isMinified)
            {
                string minPath = $"/var/www/html/{id}.min.css";
                lastCSSLocation = $"http://194.170.10.101/{id}.min.css";

                ProcessStartInfo startInfo = new ProcessStartInfo("yui-compressor", $"-o {minPath} {path}")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var proc = System.Diagnostics.Process.Start(startInfo);
                proc.WaitForExit();
                int x = proc.ExitCode;
            }


            var head = document.DocumentNode.Descendants("head").FirstOrDefault();
            HtmlNode node = new HtmlNode(HtmlNodeType.Element, document, 1)
            {
                Name = "link",
                Id = id
            };
            node.Attributes.Add("rel", "stylesheet");
            node.Attributes.Add("type", "text/css");
            node.Attributes.Add("href", lastCSSLocation);
            head.AppendChild(node);
        }


    }
}