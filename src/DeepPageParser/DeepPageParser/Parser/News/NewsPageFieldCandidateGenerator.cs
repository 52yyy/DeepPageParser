using System.Collections.Generic;

using DeepPageParser.Dom;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		新闻页面字段候选节点生成类
	/// </summary>
	public class NewsPageFieldCandidateGenerator
	{
		/// <summary>
		/// data config
		/// </summary>
		protected NewsParserConfig ParserConfig;

		public NewsPageFieldCandidateGenerator()
		{
			
		}

		public NewsPageFieldCandidateGenerator(NewsParserConfig parserConfig)
		{
			this.ParserConfig = parserConfig;
		}

      
        //新增加函数，专门用于挑选发布时间的候选。
        //其实可以和找文章来源用同一个函数，但是担心彼此的修改影响对方的功能，基于单一职责，新增加函数。
        //有些依赖标题和正文解析是否正确，但是不完全依赖。
        public List<int> GetCandidateTimeNodeIndex(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
        {
            var titleNode = data.Page.Title;
            var bodyNode = data.Page.MainBlockNode;

            int titleNodeIdx = 0;
            int bodyNodeIdx = 0;
            List<int> candidateNodeIdx = new List<int>();

            if (titleNode != null && titleNode.OriginalIndex >= 0
                && bodyNode != null && bodyNode.OriginalIndex >= 0)
            {
                titleNodeIdx = titleNode.OriginalIndex;
                bodyNodeIdx = bodyNode.OriginalIndex;
                //Temporary processing, need to be modified in the future
                // in case body parsing errors such as body including title
                //Unsafe code but only affects the body and title parsing errors' url
                if (bodyNodeIdx < titleNodeIdx)
                {
                    bodyNodeIdx = titleNodeIdx + 40 < data.OriginalNodeList.Count ? titleNodeIdx + 40 : data.OriginalNodeList.Count - 1;
                }
                // 正文节点的起始位置和正文节点真正的开始不一定一样，需要以真正的开始（第一个孩子）为准
                else if (bodyNode.Children != null &&bodyNode.Children.Count>0&& bodyNode.Children[0] != null)
                {
                    bodyNodeIdx = bodyNode.Children[0].OriginalIndex;
                }
                //assume source appear between title and main body
                //there are too many empty text node some times, so only count non-text nodes
                int nNode = 0;
                //标题前的节点。
                for (int i = titleNodeIdx - 1; i >= 0; i--)
                {
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
                    {
                        nNode++;
                    }
                    if (nNode > this.ParserConfig.NumNodeBeforeTitle) //标题前最多找50个节点。
                    {
                        break;
                    }
                    candidateNodeIdx.Insert(0, i);
                }
                //标题和内容中间的节点。
                for (int i = titleNodeIdx + 1; i < data.OriginalNodeList.Count - 1 && i < bodyNodeIdx; i++)
                {
                    candidateNodeIdx.Add(i);
                }
   
                int lastContentIndex = bodyNodeIdx;
                for (int i = bodyNodeIdx; i < data.OriginalNodeList.Count - 1; i++)
                {
                    if (data.OriginalNodeList[i].ResultType > ResultType.None)
                    {
                        lastContentIndex = i;
                    }
                }
                //防止标题和正文解析错误，正文中包含了标题和发布时间的内容，取正文的前10个位置
                //例如http://oil.fx678.com/news/detail/id/201609221106051583.html
                //如果提高了标题和正文定位的正确率，这部分可以删掉啦。
                nNode = 0;
                for (int i = bodyNodeIdx; i < data.OriginalNodeList.Count - 1; i++)
                {
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
                    {
                        nNode++;
                    }
                    //if (nNode > this.ParserConfig.NumNodeAfterMainContent)
                    if (nNode > 10)
                    {
                        break;
                    }

                    candidateNodeIdx.Add(i);
                }
                //  正文后面的节点要10吧，太多容易到相关新闻等地方。
                nNode = 0;
                for (int i = lastContentIndex; i < data.OriginalNodeList.Count - 1; i++)
                {
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
                    {
                        nNode++;
                    }
                    //if (nNode > this.ParserConfig.NumNodeAfterMainContent)
                    if (nNode >10)
                    {
                        break;
                    }

                    candidateNodeIdx.Add(i);
                }
            }
            else
            {
                for (int i = 0; i < data.OriginalNodeList[data.OriginalNodeList.Count - 1].OriginalIndex; i++)
                {
                    candidateNodeIdx.Add(i);
                }
            }

            return candidateNodeIdx;

        }

		public List<int> GetCandidateNodeIndex(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
		{
			var titleNode = data.Page.Title;
			var bodyNode = data.Page.MainBlockNode;

			int titleNodeIdx = 0;
			int bodyNodeIdx = 0;
			List<int> candidateNodeIdx = new List<int>();

			if (titleNode != null && titleNode.OriginalIndex >= 0
				&& bodyNode != null && bodyNode.OriginalIndex >= 0)
			{
				titleNodeIdx = titleNode.OriginalIndex;
				bodyNodeIdx = bodyNode.OriginalIndex;
                //Temporary processing, need to be modified in the future
                // in case body parsing errors such as body including title
                //Unsafe code but only affects the body and title parsing errors' url
                if (bodyNodeIdx < titleNodeIdx)
                {
                    bodyNodeIdx = titleNodeIdx + 40 < data.OriginalNodeList.Count ? titleNodeIdx + 40 : data.OriginalNodeList.Count - 1;
                }
				// 正文节点的起始位置和正文节点真正的开始不一定一样，需要以真正的开始（第一个孩子）为准
                else if (bodyNode.Children != null && bodyNode.Children.Count > 0 && bodyNode.Children[0] != null)
				{
					bodyNodeIdx = bodyNode.Children[0].OriginalIndex;
				}
				//assume source appear between title and main body
				//there are too many empty text node some times, so only count non-text nodes
				int nNode = 0;
				for (int i = titleNodeIdx - 1; i >=0; i--)
				{
                    if (data.OriginalNodeList[i] == null)
				    {
                        continue;

				    }
					if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
					{
						nNode++;
					}
					if (nNode > this.ParserConfig.NumNodeBeforeTitle)
					{
						break;
					}
					candidateNodeIdx.Insert(0, i);
				}

				for (int i = titleNodeIdx + 1; i < data.OriginalNodeList.Count - 1 && i < bodyNodeIdx; i++)
				{
					candidateNodeIdx.Add(i);
				}

				int lastContentIndex = bodyNodeIdx;
				for (int i = bodyNodeIdx; i < data.OriginalNodeList.Count - 1; i++)
				{
					if (data.OriginalNodeList[i].ResultType > ResultType.None)
					{
						lastContentIndex = i;
					}
				}

				nNode = 0;
				for (int i = lastContentIndex; i < data.OriginalNodeList.Count - 1; i++)
				{
					if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
					{
						nNode++;
					}
					if (nNode > this.ParserConfig.NumNodeAfterMainContent)
					{
						break;
					}

					candidateNodeIdx.Add(i);
				}
			}
			else
			{
				for (int i = 0; i < data.OriginalNodeList[data.OriginalNodeList.Count - 1].OriginalIndex; i++)
				{
					candidateNodeIdx.Add(i);
				}
			}

			return candidateNodeIdx;

		}
	}
}