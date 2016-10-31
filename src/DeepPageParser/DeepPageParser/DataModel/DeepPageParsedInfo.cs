using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		深度页面解析结果
	/// </summary>
	public class DeepPageParsedInfo
	{
		public HtmlNode ArticleBodyNode { get; internal set; }

		public string ArticleInHtml
		{
			get
			{
				List<Paragragh> formatedParagraphList = this.FormatedParagraphList;
				if (string.IsNullOrWhiteSpace(this.Warning))
				{
					this.Warning = TryGetWarning(formatedParagraphList);
				}

				return BlockHelper.GetBlockHtml(this.Url, this.Warning, formatedParagraphList);
			}
		}

		public string ArticleInHtmlBody
		{
			get
			{
				var seed = new StringBuilder();
				this.FormatedParagraphList.Aggregate(seed, (builder, block) => builder.AppendLine(block.HtmlBlock));
				seed.AppendFormat("<p><a href='{0}'>Original Page</a></p>", this.Url);
				return seed.ToString();
			}
		}

		public string ArticleInText
		{
			get
			{
				return StringHelper.GetExtractedText(this.ArticleTitleNode, this.ArticleBodyNode);
			}
		}

		public string ArticleMainBody
		{
			get
			{
				return WebUtility.HtmlDecode(DocHelper.OutputText(this.ArticleBodyNode));
			}
		}

		public string ArticleTitle
		{
			get
			{
				return this.GetTitle();
			}
		}



		public string Author { get; set; }

		public HtmlNode ArticleTitleNode { get; internal set; }

		public string DominatedImage { get; internal set; }

		public ExtractionResultFeature ExtractionResultFeature { get; internal set; }

		public List<Paragragh> FormatedParagraphList
		{
			get
			{
				return BlockHelper.ToBlockList(this.ArticleTitleNode, this.ArticleBodyNode);
			}
		}

		public string NextPageLink { get; internal set; }

		public PublishTime PublishTime { get; set; }

		public ArticleSource Source { get; set; }

        /// <summary>
        /// add
        /// </summary>
        public VisitNumber VisitNumber { get; set; }
        public CommentNumber CommentNumber { get; set; }

		public string Url { get; internal set; }

		public string Warning { get; internal set; }

		internal static string TryGetWarning(List<Paragragh> list)
		{
			string str = string.Empty;
			if (list.Count <= 1)
			{
				return "Empty content";
			}

			if (!(list[0] is TextParagragh))
			{
				return "Empty Title";
			}

			var paragragh = list[0] as TextParagragh;
			if (!(paragragh.IsTitle && (paragragh.Text.Length > 0)))
			{
				str = "Empty Title";
			}

			return str;
		}

		private string GetTitle()
		{
			if (this.ArticleTitleNode != null)
			{
				string result = string.Empty;
				bool newline = false;
				var newLineTags = new HashSet<string> { "div", "p", "li", "td", "br", "h1", "h2", "h3" };
				DocHelper.TravelOnRawNode(
					this.ArticleTitleNode,
					delegate(HtmlNode node)
					{
						if (node == null)
						{
							return false;
						}

						// 只要是a标签的就不往下要了
						if (node.Name == "a")
						{
							string hrefLink = node.GetAttributeValue("href", "");
							if (!string.IsNullOrEmpty(hrefLink) && hrefLink.StartsWith("#"))
							{
								return false;
							}
						}

						if ((node.NodeType == HtmlNodeType.Text) && !string.IsNullOrEmpty(node.InnerText))
						{
							result = result + node.InnerText;
							newline = false;
						}

						if (node.NodeType != HtmlNodeType.Element)
						{
							return false;
						}

						if (node.Name == "script")
						{
							return false;
						}

						return true;
					},
					delegate(HtmlNode node)
					{
						if ((!string.IsNullOrEmpty(node.Name) && newLineTags.Contains(node.Name)) && !newline)
						{
							result = result + "\n";
							newline = true;
						}
						return true;
					});
				return StringHelper.GetMeaningFulChars(result);
			}
			return null;
		}
	}
}