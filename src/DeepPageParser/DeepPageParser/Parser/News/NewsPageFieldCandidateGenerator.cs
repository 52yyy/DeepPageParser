using System.Collections.Generic;

using DeepPageParser.Dom;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///		����ҳ���ֶκ�ѡ�ڵ�������
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

      
        //�����Ӻ�����ר��������ѡ����ʱ��ĺ�ѡ��
        //��ʵ���Ժ���������Դ��ͬһ�����������ǵ��ı˴˵��޸�Ӱ��Է��Ĺ��ܣ����ڵ�һְ�������Ӻ�����
        //��Щ������������Ľ����Ƿ���ȷ�����ǲ���ȫ������
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
                // ���Ľڵ����ʼλ�ú����Ľڵ������Ŀ�ʼ��һ��һ������Ҫ�������Ŀ�ʼ����һ�����ӣ�Ϊ׼
                else if (bodyNode.Children != null &&bodyNode.Children.Count>0&& bodyNode.Children[0] != null)
                {
                    bodyNodeIdx = bodyNode.Children[0].OriginalIndex;
                }
                //assume source appear between title and main body
                //there are too many empty text node some times, so only count non-text nodes
                int nNode = 0;
                //����ǰ�Ľڵ㡣
                for (int i = titleNodeIdx - 1; i >= 0; i--)
                {
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Element)
                    {
                        nNode++;
                    }
                    if (nNode > this.ParserConfig.NumNodeBeforeTitle) //����ǰ�����50���ڵ㡣
                    {
                        break;
                    }
                    candidateNodeIdx.Insert(0, i);
                }
                //����������м�Ľڵ㡣
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
                //��ֹ��������Ľ������������а����˱���ͷ���ʱ������ݣ�ȡ���ĵ�ǰ10��λ��
                //����http://oil.fx678.com/news/detail/id/201609221106051583.html
                //�������˱�������Ķ�λ����ȷ�ʣ��ⲿ�ֿ���ɾ������
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
                //  ���ĺ���Ľڵ�Ҫ10�ɣ�̫�����׵�������ŵȵط���
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
				// ���Ľڵ����ʼλ�ú����Ľڵ������Ŀ�ʼ��һ��һ������Ҫ�������Ŀ�ʼ����һ�����ӣ�Ϊ׼
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