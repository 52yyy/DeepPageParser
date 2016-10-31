using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;

using DeepPageParser.Dom;
using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	public class MainImageExtractor
	{
		public IEnumerable<string> Extract(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
		{
			Dictionary<int, string> srcs = new Dictionary<int, string>();
			foreach (WrappedNode imageNode in data.ImageNodes)
			{
				DocHelper.Travel(
					imageNode,
					null,
					delegate(WrappedNode node)
					{
						if (node.TagName == "img")
						{
							string src = string.IsNullOrEmpty(node.Src) ? NodeHelper.GetAttribute(node.RawNode, "data-src") : node.Src;
							if (!string.IsNullOrEmpty(src))
							{
								int srcHash = src.GetHashCode();
								if (!srcs.ContainsKey(srcHash))
								{
									srcs.Add(srcHash, src);
									return true;
								}
							}
						}
						return false;
					});
			}
			return srcs.Values;
		}	
	}
}