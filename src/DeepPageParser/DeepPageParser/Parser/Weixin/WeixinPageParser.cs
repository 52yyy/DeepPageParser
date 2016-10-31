using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

using DeepPageParser;
using DeepPageParser.Dom;
using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		微信页面解析类
	/// </summary>
	public class WeixinPageParser : UnstructuredContentPageParser
	{
		private static NoiseModel _weChatSubscriptionNoises;
		private static readonly string _noiseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeepPageParserModels", "WeiXin", "Noise.Model");

	    private static HashSet<string> FilterTags;
	    private List<string> FilterRules;
        protected WeixinParserConfig parserConfig;

        public WeixinPageParser( )
        {

            this.parserConfig = new WeixinParserConfig();
            FilterTags = parserConfig.FilterTags;
			_weChatSubscriptionNoises = NoiseModel.LoadNoiseModel(_noiseFilePath);
		}



		protected override IDeepParseExecuteResult SaveResult(PendingAnalysisPageExtractedInfo data)
		{
			// to parse meta informations
			HtmlNode body = data.Page.Body.RawNode;
			HtmlNode titleNode = GetUsefulNode(body, "h2", "rich_media_title", "");
			HtmlNode publishTimeNode = GetUsefulNode(body, "em", "", "post-date");
			HtmlNode authorNode = GetUsefulNode(body, "a", "", "post-user");
			WrappedNode maincontentNode =  new WrappedNode(GetUsefulNode(body, "div", "rich_media_content", "js_content"));
           
            
            
            //WrappedNode maincontentNode = data.Page.MainBlockNode ?? new WrappedNode(GetUsefulNode(body, "div", "rich_media_content", "js_content"));

		    
			if (authorNode == null)
			{
				return DeepParseExecuteResult.Fail(201, "Weixin Parsed Failed. Do not found author.");
			}

			//	to get noise context
			NoiseContext noise;
			if (_weChatSubscriptionNoises.TryGetValue(authorNode.InnerText, out noise))
			{
			}
			else
			{
				noise = new NoiseContext();
			}

			// to parse maincontent with images
			List<MainImage> images = new List<MainImage>();
			StringBuilder sb = new StringBuilder();
            bool hit = false;

			DocHelper.TravelOnRawNode(
				maincontentNode.RawNode,
				null,
				delegate(HtmlNode wrappedNode)
				{
				    string text = wrappedNode.InnerText.Trim();
				    if (isFilterTag(text))
				    {
                        hit = true;
				    }
				    if (wrappedNode.Name == "img"&& !hit)
					{
						string tmp = NodeHelper.GetAttribute(wrappedNode, "src");
						string src = string.IsNullOrEmpty(tmp) ? NodeHelper.GetAttribute(wrappedNode, "data-src") : tmp;
						sb.Append("\r\n");
						images.Add(new MainImage() { Src = src, IndexOfMainContent = sb.Length });
					}


					if (wrappedNode.InnerHtml.Length != 0)
					{
						if ((wrappedNode.ChildNodes == null || wrappedNode.ChildNodes.Count == 0))
						{
							int sentenceHash = wrappedNode.InnerText.GetHashCode();
							if (noise.NoiseHashSentences.Contains(sentenceHash))
							{
								return false;
							}
						    string str = WebUtility.HtmlDecode(wrappedNode.InnerText.Trim());
                            if (str.Length > 0 && !hit)
                            {
                                sb.Append(str);
                                
                            }
						}
					}
				    if (wrappedNode.Name == "p"&& !hit)
				    {
                        sb.Append("\r\n");
				    }
					return true;
				});

			// to save
			var result = new WeixinDeepParsedInfo();
			result.ArticleTitleNode = titleNode;
			result.PublishTime = new PublishTime() { TimeValue = DateTime.Parse(publishTimeNode.InnerText) };
			result.Author = authorNode.InnerText;
		    result.MainContent = sb.ToString();

			result.MainImages = images;
			return DeepParseExecuteResult.Succeed(result);
		}

		private static HtmlNode GetUsefulNode(HtmlNode node, string tagName, string className, string idName)
		{
			HtmlNode result = null;
			bool isMatch = false;
			DocHelper.TravelOnRawNode(
				node,
				delegate(HtmlNode wrappedNode)
				{
					if (wrappedNode.Name == tagName && wrappedNode.GetAttributeValue("class","").Contains(className) && wrappedNode.GetAttributeValue("id","").Contains(idName))
					{
						result = wrappedNode;
						isMatch = true;
					}
					return !isMatch;
				},
				null);
			return result;
		}
       
	    private bool isFilterTag(string text)
	    {
	        return FilterTags.Contains(text);
	    }



       

	}
}