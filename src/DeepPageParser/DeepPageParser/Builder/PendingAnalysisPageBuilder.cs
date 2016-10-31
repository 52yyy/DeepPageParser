using System;

using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		待分析页面的建造者
	/// </summary>
	public class PendingAnalysisPageBuilder
	{
        public int GetSpecialDomainId(string domain)
        {
            int domainId = 0; //普通网址的为0
            if (domain == "xueqiu.com")
            {
                domainId = 1;
            }
            return domainId;
        }

		public PendingAnalysisPage BuildDomTree(string url)
		{
			string html = StringHelper.FetchAndDetectEncoding(url, string.Empty, string.Empty, 2);
			return this.BuildDomTree(url, html);
		}


		/// <summary>
		///		创建页面Dom树
		/// </summary>
		/// <param name="url"></param>
		/// <param name="html"></param>
		/// <returns></returns>
		public PendingAnalysisPage BuildDomTree(string url, string html)
		{
			PendingAnalysisPage extData;
			try
			{
				extData = new PendingAnalysisPage();

				// html过长导致45行出现内存溢出的错误，增加一个html长度的限制
				if (html.Length > 5000000)
				{
					return null;
				}
                //获取主域名
                Uri uri;
                string domain = string.Empty;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    domain = uri.Host.ToLower();
                }
                

				extData.SourceHtml = html;
                extData.Domain = domain;
                //根据主域名，编码特殊网址编号
			    extData.SpecialDomainId = GetSpecialDomainId(domain);
				HtmlDocument doc = new HtmlDocument();

				doc.LoadHtml(html);
                
                HtmlNode node = null;
                //根据特殊id的编号，获取不同的body
			    switch (extData.SpecialDomainId)
			    {
                    case 1:
                        node = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'status-bd')]");  //针对雪球网址的特殊处理
                        break;
                    default:
                        node = doc.DocumentNode.SelectSingleNode("//body");//获取body
                        break;
                }

				if (string.IsNullOrWhiteSpace(url))
				{
					url = DocHelper.GetUrlFromBaseTag(doc);
				}
				extData.Document = doc;
				extData.RawBody = node;
				if (extData.RawBody == null)
				{
					StringHelper.LogError("Parse html failed.");
				}

                //去除评论节点
				extData.RawBody = DocHelper.RemoveComment(extData.RawBody);
                extData.Url = url;
                // 网页正文解析，把HtmlNode解成WrappedNode
				if ((extData.RawBody != null) && !string.IsNullOrWhiteSpace(extData.Url))
				{
					extData.Body = DocHelper.Wrap(extData.RawBody);  
				}
			}
			catch (ArgumentException)
			{
				return null;
			}
			return extData;
		}
	}
}
