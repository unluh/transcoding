using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Proxy.ConsoleApp
{
	internal class JsBundler : IHtmlProcessor
	{

		private readonly HttpClient httpClient;

		public JsBundler(HttpClient client)
		{
			this.httpClient = client;
		}


		public void Process(HtmlDocument document, bool isMinified)
		{
			var links = document.DocumentNode.Descendants("script");
			var result = links.Where(x => x.Attributes.Contains("src")).ToList();

			StringBuilder contentBuilder = new StringBuilder();

			if (result.Count <= 1)
			{
				return;
			}

			foreach (var match in result)
			{

				var url = match.Attributes["src"].Value;
				if (url.StartsWith("//"))
				{
					url = $"http:{url}";
				}

				using (var response = httpClient.GetAsync(url).Result)
				{
					if (response.IsSuccessStatusCode)
					{
						var content = response.Content.ReadAsStringAsync().Result;

						contentBuilder.AppendLine(content);
						match.Remove();
					}
				}
			}
			string id = Guid.NewGuid().ToString();
			string fileName = $"{id}.js";
			string path = $"/var/www/html/{fileName }";
			string lastLocation = $"http://194.170.10.101/{fileName}";
			System.IO.File.WriteAllText(path, contentBuilder.ToString());


			if (isMinified)
			{
				string minPath = $"/var/www/html/{id}.min.js";
				lastLocation = $"http://194.170.10.101/{id}.min.js";

				ProcessStartInfo startInfo = new ProcessStartInfo("python", $" -m jsmin {path}")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				var proc = System.Diagnostics.Process.Start(startInfo);
				string output = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit();
				if (proc.ExitCode == 0)
				{
					System.IO.File.WriteAllText(minPath, output);
				}
			}
			var head = document.DocumentNode.Descendants("head").FirstOrDefault();
			HtmlNode node = new HtmlNode(HtmlNodeType.Element, document, 0)
			{
				Name = "script",
				Id = id
			};
			node.Attributes.Add("src", lastLocation);
			head.AppendChild(node);
		}
	}
}
