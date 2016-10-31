using System;
using System.Collections.Generic;

using DeepPageParser;
using DeepPageParser.Dom;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		来源提取类，使用状态模式实现
	/// </summary>
	public class SourceExtractor : NewsPageFieldExtractor
	{
		public SourceExtractor(NewsParserConfig parserConfig)
			: base(parserConfig)
		{
		}

		/// <summary>
		/// extract article source from a node
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		protected ArticleSource GetSourceFromNode(WrappedNode node, string url)
		{
			PageFieldDebug debug;
			string text;
			ArticleSource source;
			if (node.NodeType == HtmlNodeType.Text)
			{
				text = node.Text;
				debug = TryExtractField(text, PageFieldType.Source, true);
				source = new ArticleSource();
				source.Name = debug.FieldValue;
				source.Link = string.Empty;
				return source;
			}

			//a link node
			text = node.InnerText;
			debug = TryExtractField(text, PageFieldType.Source, true);
			source = new ArticleSource();
			source.Name = debug.FieldValue;

			if (string.Compare(node.TagName, "a", true) == 0)
			{
				//add link information                
				source.Link = node.Href ?? string.Empty;
				return source;
			}
			else if (node.OriginalChildren.Count > 0)
			{
				//get the first link node to extract the source link
				var linkNodes = node.RawNode.SelectNodes(".//a[@href]");
				if (linkNodes != null && linkNodes.Count > 0)
				{
					//add link information                
					string link = linkNodes[0].GetAttributeValue("href", string.Empty);
					string linkAbs = ToAbsoluteUrl(url, link);
					source.Link = linkAbs;
					return source;
				}
			}
			return source;
		}

		public static string ToAbsoluteUrl(string currentUrl, string href)
		{
			Uri url;
			var baseUrl = new Uri(currentUrl);
			try
			{
				url = new Uri(baseUrl, href);
				return url.AbsoluteUri;
			}
			catch (UriFormatException)
			{
				return string.Empty;
			}

		}

		public override void GetFieldValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, List<int> candidateNodeIdx)
		{
			if (data.OriginalNodeList != null && data.OriginalNodeList.Count > 0)
			{
				//evaluate each candidate
				foreach (var i in candidateNodeIdx)
				{
					string text = data.OriginalNodeList[i].Text.Trim(ParserConfig.UselessChars);
					string id = data.OriginalNodeList[i].Id;
					string itemProp = string.Empty;
					string bosszone = string.Empty;
					if (data.OriginalNodeList[i].RawNode.Attributes.Contains("itemprop"))
					{
						itemProp = data.OriginalNodeList[i].RawNode.Attributes["itemprop"].Value;
					}

					if (data.OriginalNodeList[i].RawNode.Attributes.Contains("bosszone"))
					{
						bosszone = data.OriginalNodeList[i].RawNode.Attributes["bosszone"].Value;
					}

					if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Text)
					{
						if (text.Length > 0)
						{
							var debug = TryExtractField(text, PageFieldType.Source, false);
							if (debug.ContainsClue && debug.FieldValue.Length > 0)
							{
								var source = new ArticleSource();
								source.Name = debug.FieldValue;
								source.Link = string.Empty;
								data.Page.Source = source;
								break;
							}
							else if (debug.ContainsClue && debug.FieldValue.Length == 0
								&& i < data.OriginalNodeList.Count - 1)
							{
								int j = i + 1;
								bool getNext = false;
								for (; j < data.OriginalNodeList.Count; j++)
								{
									//find next nonempty node
									if ((data.OriginalNodeList[j].TagName != "#text"
										|| data.OriginalNodeList[j].Text.Trim(ParserConfig.UselessChars).Length > 0)
										&& (data.OriginalNodeList[j].Parent == data.OriginalNodeList[i].Parent)
										)
									{
										getNext = true;
										break;
									}
								}
								if (getNext)
								{
									var nextNode = data.OriginalNodeList[j];
									data.Page.Source = GetSourceFromNode(nextNode, data.Page.Url);
									break;
								}
							}
						}
					}
					else
					{
						bool matched = false;
						foreach (string condidateId in ParserConfig.SourceControlIds)
						{
							if (string.Compare(condidateId, id, true) == 0)
							{
								data.Page.Source = GetSourceFromNode(data.OriginalNodeList[i], data.Page.Url);
								matched = true;
								break;
							}
							if (string.Compare(condidateId, itemProp, true) == 0)
							{
								data.Page.Source = GetSourceFromNode(data.OriginalNodeList[i], data.Page.Url);
								matched = true;
								break;
							}
							if (string.Compare(condidateId, bosszone, true) == 0)
							{
								data.Page.Source = GetSourceFromNode(data.OriginalNodeList[i], data.Page.Url);
								matched = true;
								break;
							}
						}
						if (matched)
						{
							break;
						}
					}
				}
			}
		}
	}
}