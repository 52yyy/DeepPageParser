using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

namespace DeepPageParser.Util
{
	public class ListPageCheckHelper
	{
		public static bool CheckIsListPage(HtmlDocument document)
		{
			var commonText = new List<string> { "Copyright", "ICP" };

			List<NodeElement> allTextElements =
				document.DocumentNode.SelectNodes(@"//text()")
					.Select(item => new NodeElement(item))
					.Where(
						item =>
						item.InnerText.Length > 4 && item.XPath.Contains("style") == false && item.XPath.Contains("script") == false
						&& !commonText.Any(s => item.InnerText.Contains(s)))
					.ToList();
			List<NodeElement> allNotLinkedTextElentment =
				allTextElements.Where(item => item.XPath.Contains(@"/a/") == false && item.InnerText.Length > 30).ToList();
			var tmp =
				allNotLinkedTextElentment.GroupBy(item => item.LinePosition)
					.OrderByDescending(item => item.Count())
					.FirstOrDefault();
			List<NodeElement> maxRepeatElements = tmp == null ? null : tmp.ToList();

			if (maxRepeatElements != null)
			{
				// 找到重复元素最多的
				double allTextCount = allTextElements.Where(item => item.InnerText.Length > 12).Sum(item => item.InnerText.Length);
				int maxTextLength = allNotLinkedTextElentment.Max(item => item.InnerText.Length);
				double allNotLinkedTextCount = allNotLinkedTextElentment.Sum(item => item.InnerText.Length);
				double allRepeatElementsTextCount =
					maxRepeatElements.Sum(item => item.InnerText.Length * (item.IsInListElement() ? 1.8 : 0.4));

				if (!maxRepeatElements.Any(item => item.InnerText.Length == maxTextLength))
				{
					allNotLinkedTextCount += maxTextLength;
				}

				if (maxRepeatElements.Count <= 2)
				{
					if (allTextCount / allNotLinkedTextCount > 8)
					{
						return true;
					}

					return false;
				}

				var lineDiffs = new Dictionary<int, int>();

				lineDiffs = GetRepeatElementDic(maxRepeatElements);

				double rate = 1;

				lineDiffs = lineDiffs.Where(item => item.Value > 1).ToDictionary(item => item.Key, item => item.Value);
				if (lineDiffs.Count==0)
				{
					return false;
				}
				int max = lineDiffs.Keys.Max();
				int min = lineDiffs.Keys.Min();

				if (min == 1 || max < 2 || (maxRepeatElements.Count / lineDiffs.Count <= 3))
				{
					// 如果重复元算很接近, 那么很可能是正文的内容很接近.
					// 多样的间距也可能是正文.
					rate = 0.2;
				}
				else if (min > 2 && (max - min) <= 4)
				{
					rate = 1.4;
				}

				double textrate = allNotLinkedTextCount / (allRepeatElementsTextCount * rate);

				//((allNotLinkedTextElentment.Count+1.0) / (s.Count+1.0)) /(allTextCount/allRepeatElementsTextCount - 1);
				if (textrate < 2)
				{
					//Console.WriteLine("list page");
					return true;
				}

				return false;
			}

			// 当前页面中的所有文本都是有链接的; 或者有文本但是内容少于30字.
			return true;
		}

		public static bool CheckIsListPage(string url)
		{
			var document = new HtmlDocument();
			var webclient = new WebClient(); //http://www.cnblogs.com/

			byte[] data = webclient.DownloadData(url);
			Encoding encoding = webclient.ResponseHeaders.GetEntityBodyEncoding(data);

			string html = encoding.GetString(data);

			//思路: 如果是包含a很多的网站, 可以从非连接文本与链接文本的比来看.  注意xpath中style跟Javascripte要移除统计.
			//如果是有摘要的, 统计p标签看段落间的行间距是否等距.  如果path中包li等元素就更好了.
			document.LoadHtml(html);
			return CheckIsListPage(document);
		}

		public static Dictionary<int, int> GetRepeatElementDic(List<NodeElement> maxRepeatElements)
		{
			var lineDiffs = new Dictionary<int, int>();

			for (int i = 1; i < maxRepeatElements.Count; i++)
			{
				int linediff = 0;
				if (maxRepeatElements[i - 1].NextSibling == maxRepeatElements[i].CurrentNode)
				{
					linediff = 1;
				}
				else
				{
					linediff = maxRepeatElements[i].Line - maxRepeatElements[i - 1].Line;
				}

				if (!lineDiffs.ContainsKey(linediff))
				{
					lineDiffs.Add(linediff, 1);
				}
				else
				{
					lineDiffs[linediff] += 1;
				}
			}
			return lineDiffs;
		}

		public class NodeElement
		{
			public NodeElement(HtmlNode htmlNode)
			{
				this.XPath = htmlNode.XPath;
				this.InnerText = WebUtility.HtmlDecode(htmlNode.InnerText).Trim();
				this.Line = htmlNode.Line;
				this.LinePosition = htmlNode.LinePosition;
				this.ParentNode = htmlNode.ParentNode;
				this.NextSibling = htmlNode.NextSibling;
				this.CurrentNode = htmlNode;
			}

			public HtmlNode CurrentNode { get; set; }

			public string InnerText { get; set; }

			public int Line { get; set; }

			public int LinePosition { get; set; }

			public HtmlNode NextSibling { get; set; }

			public HtmlNode ParentNode { get; set; }

			public string XPath { get; set; }

			public bool IsInListElement()
			{
				if (this.XPath.Contains("li") || this.XPath.Contains("ol"))
				{
					return true;
				}

				return false;
			}
		}
	}
}