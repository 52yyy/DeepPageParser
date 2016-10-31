using System.Collections.Generic;

using DeepPageParser;
using DeepPageParser.Dom;
using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	public class PendingAnalysisPage
	{
		public PendingAnalysisPage()
		{
		}

        #region Attributes
        
        /// <summary>
		///		页面类型
		/// </summary>
		public PageType PageType { get; set; }

		public WrappedNode Body { get; set; }

		public HtmlDocument Document { get; set; }

		public HtmlNode MainBlock { get; set; }

		public WrappedNode MainBlockNode { get; set; }

		public string MainImage { get; set; }

		public HtmlNode RawBody { get; set; }

		public PublishTime PublishTime { get; set; }

		public ArticleSource Source { get; set; }

        //+++++++++++++++++++++++++++++++++++++++++
        public VisitNumber  VisitNumber { get; set; }
        public CommentNumber CommentNumber { get; set; }
        //+++++++++++++++++++++++++++++++++++++++++

		/// <summary>
		///		HTML的源代码
		/// </summary>
		public string SourceHtml { get; set; }
        //主域名
        public string Domain { get; set; }
        // 特殊主域名编号
        public int SpecialDomainId { get; set; }

		public WrappedNode Title { get; set; }

		public string Url { get; set; }

		public string Warning { get; set; }

        #endregion

        public string GetBlockHtml()
		{
			List<Paragragh> blockListWithTitle = this.GetBlockListWithTitle();
			return BlockHelper.GetBlockHtml(this.Url, this.Warning, blockListWithTitle);
		}

		public List<Paragragh> GetBlockListWithTitle()
		{
			HtmlNode mainTitle = (this.Title == null) ? null : this.Title.RawNode;
			return BlockHelper.ToBlockList(mainTitle, this.MainBlock);
		}

		public string GetExtractedText()
		{
			HtmlNode title = (this.Title == null) ? null : this.Title.RawNode;
			return StringHelper.GetExtractedText(title, this.MainBlock);
		}
	}
}
