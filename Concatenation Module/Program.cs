using HtmlAgilityPack;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net.Http;

namespace Proxy.ConsoleApp
{
    public class Program
    {
        private static HttpClient httpClient = null;
        private static void Main(string[] args)
        {

            System.Diagnostics.Debugger.Break();
            var app = new CommandLineApplication
            {
                Name = "Huseyin's Proxy",
                Description = "Huseyin Proxy"
            };

            app.HelpOption("-?|-h|--help");

            var optionUrl = app.Option("-u| --url<value>;", "URL", CommandOptionType.SingleValue);
            var optionFile = app.Option("-f| --file<value>;", "File", CommandOptionType.SingleValue);

            var optionJs = app.Option("-js| --JavaScript;", "JS", CommandOptionType.NoValue);
            var optionCss = app.Option("-css| --CSS;", "CSS", CommandOptionType.NoValue);
            var optionMinJs = app.Option("-mjs | --minifyjs", "JS Minification", CommandOptionType.NoValue);
            var optionMinCss = app.Option("-mcss | --minifycss", "CSS Minification", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                string baseUrl = optionUrl.HasValue() ? optionUrl.Value() : "http://194.170.10.2/data/6.qq/";
                string file = optionFile.Value();
                try
                {
                    Run(baseUrl, file, optionCss.HasValue(), optionMinCss.HasValue(), optionJs.HasValue(), optionMinJs.HasValue());

                    return 0;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    System.IO.File.WriteAllText(file, ex.Message);
                    return -1;
                }

            });

            app.Execute(args);
        }
        static void Log(string s)
        {
            System.IO.File.AppendAllText("/var/www/html/log/log", $"\n{s}");
        }
        private static void Run(string baseUrl, string file, bool css, bool cssMin, bool js, bool jsMin)
        {
            Log("hello");
            Log(baseUrl);
            var httpClientHandler = new HttpClientHandler
            {
                UseProxy = false
            };

            httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            string body;
            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("Downloading");
                body = httpClient.GetStringAsync(baseUrl).Result;
            }
            else
            {
                Console.WriteLine("Reading from file");
                body = System.IO.File.ReadAllText(file);
            }


            var document = new HtmlDocument();
            document.LoadHtml(body);

            if (js)
            {
                var jsEngine = new JsBundler(httpClient);
                jsEngine.Process(document, jsMin);
            }
            if (css)
            {
                var cssEngine = new CssBundler(httpClient);
                cssEngine.Process(document, cssMin);
            }

            body = document.DocumentNode.OuterHtml + "\n<!-- PROXY -->";

            if (!string.IsNullOrWhiteSpace(file))
            {
                System.IO.File.WriteAllText(file, body);
            }
            else
            {
                Console.WriteLine(body);
            }
        }
    }
}