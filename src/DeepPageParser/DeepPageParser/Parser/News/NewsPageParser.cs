using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DeepPageParser;
using DeepPageParser.Classifier;
using DeepPageParser.Dom;
using DeepPageParser.NewVersion.Parser.News;
using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///     新闻页面解析类
	/// </summary>
	public class NewsPageParser : UnstructuredContentPageParser
	{        
		/// <summary>
		/// data config
		/// </summary>
		protected NewsParserConfig ParserConfig;
		private NewsPageFieldExtractor _sourceExtractor;
		private NewsPageFieldExtractor _publishTimeExtractor;
        private NewsPageFieldExtractor _visitInfoExtractor;
		private NewsPageFieldParser _newsPageFieldParser;

		public NewsPageParser()
		{
			this.ParserConfig = new NewsParserConfig();
			this._newsPageFieldParser = new NewsPageFieldParser(this.ParserConfig);
			this._publishTimeExtractor = new PublishTimeExtractor(this.ParserConfig);
			this._sourceExtractor = new SourceExtractor(this.ParserConfig);
		}

		protected sealed override IDeepParseExecuteResult SaveResult(PendingAnalysisPageExtractedInfo data)
		{
			if (data == null)
			{
				return DeepParseExecuteResult.Fail(255, "Null");
			}

			if (!string.IsNullOrEmpty(data.Page.Warning))
			{
				return DeepParseExecuteResult.Fail(210, data.Page.Warning);
			}

			//after get title and main block node

			//rule-based extraction for specific websites
			Uri uri;
			string host = string.Empty;
			if (Uri.TryCreate(data.Page.Url, UriKind.Absolute, out uri))
			{
				host = uri.Host.ToLower();
			}
			string pubTimePattern = string.Empty;
			string sourcePattern = string.Empty;
			string linkPattern = string.Empty;
			string postSourcePattern = string.Empty;
		//    string titlePattern = string.Empty;

			string html = data.Page.SourceHtml;
			switch (host)
			{
				case "www.chinalaw.gov.cn":
					pubTimePattern = "var tm = \"(?<pubTime>[^\"]+)\";";
					sourcePattern = "var author = \"(?<source>[^\"]+)\";";
					linkPattern = "var author = \"(?<source>[^\"]+)\";";
					break;
				case "www.gxzf.gov.cn":
					sourcePattern = "var from='(?<source>[^']+)';";
					break;
				case "www.drc.gov.cn":
					postSourcePattern = "《(?<source>.+)》";
					break;
			}
			if (host.EndsWith(".cyol.com") || host.EndsWith("yn.gov.cn"))
			{
				postSourcePattern = "(?<source>[\\S]+)";
			}
			else if (host.EndsWith("jl.gov.cn"))
			{
				sourcePattern = "var laiyuan = \"(?<source>[^\"]+)\";";
			}

			if (!string.IsNullOrEmpty(pubTimePattern)&& IsParsePublicTime)
			{
				Regex regexPubTime = new Regex(pubTimePattern);

				PublishTime dtPubTime = null;
				var match = regexPubTime.Match(html);
				if (match.Success)
				{
					string pubTimeStr = match.Groups["pubTime"].Value;
					dtPubTime = ParseTime(pubTimeStr);
				}

				if (dtPubTime != null)
				{
					data.Page.PublishTime = dtPubTime;
				}
			}
			if ((!string.IsNullOrEmpty(sourcePattern) ||
				!string.IsNullOrEmpty(linkPattern))&&
                IsParseSource)
			{
				string linkName = ExtractByPattern(html, sourcePattern, "source");
				string linkHref = ExtractByPattern(html, linkPattern, "link");
				bool found = linkName != null || linkHref != null;
				if (found)
				{
					ArticleSource source = new ArticleSource();
					source.Name = linkName ?? string.Empty;
					source.Link = linkHref ?? string.Empty;
					data.Page.Source = source;
				}
			}

			if (data.Page.Source == null && IsParseSource)
			{
				//get article source information
//				GetSource(data);
				_newsPageFieldParser.SetNewsPageFieldExtractor(_sourceExtractor);
				_newsPageFieldParser.GetFieldValue(data);
			}

			if (data.Page.PublishTime == null && IsParsePublicTime)
			{
				//get publish time            
//				GetPublishTime(data);
				_newsPageFieldParser.SetNewsPageFieldExtractor(_publishTimeExtractor);
				_newsPageFieldParser.GetFieldTimeValue(data);
			}

            //++++++++++++++++++++++++++++++++++++++++
		    if (data.Page.CommentNumber == null&& data.Page.VisitNumber==null && IsParseVisitInfo)
		    {      
                _newsPageFieldParser.GetFieldValue2(data);
		    }

            //++++++++++++++++++++++++++++++++++++++++

			//post process
			if (!string.IsNullOrEmpty(postSourcePattern)
				&& data.Page.Source != null && data.Page.Source.Name != null
                && IsParseSource
				)
			{
				string linkName = ExtractByPattern(data.Page.Source.Name, postSourcePattern, "source");
				data.Page.Source.Name = linkName;
			}      

			DeepPageParsedInfo info = new NewsDeepParsedInfo
			{
				ExtractionResultFeature = data.ExtractionResultFeature,
				ArticleBodyNode = data.Page.MainBlock,
				DominatedImage = data.Page.MainImage,
				NextPageLink = data.NextPageLink,
				ArticleTitleNode = (data.Page.Title == null) ? null : data.Page.Title.RawNode,
				Url = data.Page.Url,
				PublishTime = IsParsePublicTime ? data.Page.PublishTime : null,
				Source = IsParseSource ? data.Page.Source : null,
				Warning = data.Page.Warning,
                VisitNumber = IsParseVisitInfo ? data.Page.VisitNumber : null,
                CommentNumber = IsParseVisitInfo ? data.Page.CommentNumber : null
			};
			return DeepParseExecuteResult.Succeed(info);
		}


		public string ExtractByPattern(string text, string sourcePattern, string groupName)
		{
			string linkName = null;
			if (!string.IsNullOrEmpty(sourcePattern))
			{
				Regex regexSource = new Regex(sourcePattern);

				var match = regexSource.Match(text);
				if (match.Success)
				{
					var g = match.Groups[groupName];
					if (g != null)
					{
						linkName = g.Value;
					}
				}
			}
			return linkName;
		}

		public PublishTime ParseTime(string text)
		{
			text = text.Trim();

			PublishTime publishTime = new PublishTime();

			int iEnd = 0;
			int nSpace = 0;
			bool prevspace = false;
			//at most two spaces are allowed.
			for (int i = 0; i < text.Length; i++)
			{
				if (!ParserConfig.TimeChars.Contains<char>(text[i]))
				{
					iEnd = i;
					break;
				}

				if (char.IsWhiteSpace(text[i]))
				{
					if (!prevspace)
					{
						nSpace++;
						prevspace = true;
					}
				}
				else
				{
					prevspace = false;
				}
				if (nSpace == 2)
				{
					iEnd = i;
					break;
				}
			}
			if (iEnd > 0)
			{
				text = text.Substring(0, iEnd);
			}

			publishTime.TimeString = text;
			DateTime dt;
			if (DateTime.TryParse(text, out dt))
			{
				publishTime.TimeValue = dt;
				publishTime.ValidTime = true;
			}
			return publishTime;
		}
	}
}