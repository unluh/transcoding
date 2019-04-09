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


		public void Process(HtmlDocument document, bool isMinified)
		{

			var links = document.DocumentNode.Descendants("link");
			var result = links.Where(x => x.Attributes.Contains("href") && x.Attributes["rel"].Value.Equals("stylesheet")).ToList();


			if (result.Count < 1)
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
						var cssContent = httpClient.GetStringAsync(url).Result;
						string id = Guid.NewGuid().ToString();
						string fileName = $"{id}.css";
						string path = $"/var/www/html/{fileName }";
						System.IO.File.WriteAllText(path, cssContent);
						string minPath = $"/var/www/html/{id}.min.css";
						string lastCSSLocation = $"http://194.170.10.101/{id}.min.css";
						match.Attributes["href"].Value=$"{lastCSSLocation}";

						ProcessStartInfo startInfo = new ProcessStartInfo("yui-compressor", $"-o {minPath} {path}")
						{
							WindowStyle = ProcessWindowStyle.Hidden
						};
						var proc = System.Diagnostics.Process.Start(startInfo);
						proc.WaitForExit();
						int x = proc.ExitCode;
					}
				}

		
			}

		}


	}
}