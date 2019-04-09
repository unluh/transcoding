using HtmlAgilityPack;

namespace Proxy.ConsoleApp
{
	internal interface IHtmlProcessor
	{
		void Process(HtmlDocument htmlDocument, bool isMinified);
	}
}