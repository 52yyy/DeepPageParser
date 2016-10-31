using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DeepPageParser.Dom;
using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser
{
	/// <summary>
	///     非结构化内容页面解析类
	/// </summary>
	public abstract class UnstructuredContentPageParser : PageParser
	{
		public UnstructuredContentPageParser()
		{
			this.Config = new AlgoConfig();
		}

		public AlgoConfig Config { get; set; }

        /// <summary>
        /// 当前节点是否属于当前集群
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cluster"></param>
        /// <returns></returns>
		public static bool IsNodeMatchCluster(WrappedNode node, Cluster cluster)
        {
            //必需满足：标点符号个数大于0，节点层级与集群层级相等，（节点块与集群中节点的距离小于12 或者 节点与集群的距离小于24且节点有父块，且父块为<p>节点），
			return ((node.PunctualNumber > 0) && (node.Level == cluster.Level))
					&& (((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 12)
						|| ((((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 0x18)
							&& (node.FirstBlockParent != null)) && (node.FirstBlockParent.TagName == "p")));
		}

        /// <summary>
        /// 页面内容解析函数
        /// </summary>
        /// <param name="pendingAnalysisPage">待分析页面</param>
        /// <returns>分析结果</returns>
	    public override IDeepParseExecuteResult Parse(PendingAnalysisPage pendingAnalysisPage)
	    {
            var data = new PendingAnalysisPageExtractedInfo(pendingAnalysisPage);
            if (pendingAnalysisPage.SpecialDomainId==0)
	        {
                return GeneralParse(pendingAnalysisPage);
	        }
	        else
	        {
                return SpecialParse(pendingAnalysisPage);
	        }
            return this.SaveResult(data);
	    }

        public IDeepParseExecuteResult SpecialParse(PendingAnalysisPage pendingAnalysisPage)
	    {
            var data = new PendingAnalysisPageExtractedInfo(pendingAnalysisPage);
            HtmlNode node2 = null;
            WrappedNode body = null;
            HtmlNode contentnode = null;
            HtmlNode tittlenode = null;
            switch (pendingAnalysisPage.SpecialDomainId)
            {
                case 1://xueqiu.com
                    tittlenode = pendingAnalysisPage.Document.DocumentNode.SelectSingleNode("//h1[starts-with(@class,'status-title')]");
                    contentnode = pendingAnalysisPage.Document.DocumentNode.SelectSingleNode("//div[starts-with(@class,'detail')]");
                    break;
                case 2://www.washingtonpost.com
                    contentnode = pendingAnalysisPage.Document.DocumentNode.SelectSingleNode("//div[starts-with(@id,'article-body')]");
                    break;
            }
             if (tittlenode != null)
             {
                 data.Page.Title = new WrappedNode(tittlenode);
             }
            if (contentnode != null)
            {
                node2 = contentnode;
                body = new WrappedNode(contentnode);
            }
            data.Page.MainBlock = node2;
            data.Page.MainBlockNode = body;
            return this.SaveResult(data);
	    }
		public  IDeepParseExecuteResult  GeneralParse(PendingAnalysisPage pendingAnalysisPage)
		{
            //将待分析页面转化成方法内部使用的富结构体
			var data = new PendingAnalysisPageExtractedInfo(pendingAnalysisPage);
            if (data != null) // 这个地方data可能为空吗？这个判断条件永远都是正确的呀？倒是data.Page.Body可能为空，当data.Page.Body为空时DocHelper.GetOriginalList(data.Page.Body)会报错。
			{
                //获取深度遍历节点序列
				data.OriginalNodeList = DocHelper.GetOriginalList(data.Page.Body);
                //如果开启了段落合并
				if (this.Config.EnableParagraghCollaption)
				{
					data.Page.Body = DocHelper.MergeTextChildren(data.Page.Body, data.Page.Document);
                
                    
                   
				}

                //遍历节点，设置可见性属性和中央对齐属性
				data.Page.Body = DocHelper.SetVisibleAndCenter(data.Page.Body); // Travel

                //修剪节点，去除不需要的节点并去除冗余嵌套
				data.Page.Body = DocHelper.TrimByTags(data.Page.Body);

               
                //应该在这个时候聚，不然后面块之间距离又会大，换了好多次，其实还是不确定
                data.Page.Body = DocHelper.MergeLinkInBlock(data.Page.Body, data.Page.Document);
                //更新跳转节点信息
				UpdateNavBlockInfo(data.Page.Body);
                //获得meta节点中的标题信息以及部分去除无用信息的标题信息
				data.MetaTitles = TitleHelper.SearchMetaTitles(data.Page.Document, data.Page.Url);
                //建立索引
                //生成文本相关信息
				//又index了一次，需要检查之前的index是否被使用。
                this.ReIndex(data);
                //增加层次遍历的索引
			    this.GetsBfsIndex(data);

				this.GenerateTextFeature(data);
                //更新文本节点层级关系
				this.UpdateTextLevel(data);
                //设置每个节点的父标题节点
				DocHelper.SetHeadingNode(data.Page.Body);
                //筛选出标题候选
				data.TitleCandidates = TitleHelper.GetTitleCandidates(data.TextList, data.MetaTitles);
                //选出正文和标题
				this.FindTitleAndMainContent(data);
				if (data.Page.Title != null)
				{
					if (this.Config.EnableImageExtraction)
					{
                        //抽取图片
						this.ExtractImage(data);
					}

					if (this.Config.EnableNextPageLinkExtraction)
					{
                        //抽下一页链接
						data.NextPageLink = NextPageUtil.ExtractNextPageLink(
							data.Page.Body,
							data.OriginalNodeList,
							data.Page.Url,
							data.Page.Title);
					}
                    //重设标题节点，在上一步选取标题时可以做，但是没有做。建议放到之前的函数中
					data.Page.Title = this.SelectTitleNode(data, data.Page.Title);
				}

				WrappedNode body = DocHelper.TrimByResultType(data.Page.Body);
                //更新NavBlock信息
				UpdateNavBlockInfo(body);
                HtmlNode node2 = null;
				if (this.Config.EnableHighPrecision)
				{
					body = DocHelper.ImprovePrecision(body);
					UpdateNavBlockInfo(body);
				}

				if (this.Config.EnableLowPrecisionWarning)
				{
					this.CheckContent(data, body);
				}

				if (this.Config.EnableConfidenceFeatureExtraction)
				{
					this.ExtractConfidenceFeature(body, data);
				}

				if (this.Config.EnableParagraghCollaption)
				{
					body = DocHelper.SeparateTextChild(body);
				}

				
				if (body != null)
				{
					node2 = DocHelper.Clone(body);
				}


				data.Page.MainBlock = node2;
				data.Page.MainBlockNode = body;
			}

			return this.SaveResult(data);
		}

	    

		protected abstract IDeepParseExecuteResult SaveResult(PendingAnalysisPageExtractedInfo data);

        /// <summary>
        /// 根据文本集群中的最大特征选取最佳集群
        /// </summary>
        /// <param name="candidates"></param>
        /// <returns></returns>
		private static Cluster ChooseBestCluster(BestClusterHelper candidates)
		{
			Cluster cluster = null;
			if (!candidates.AllFound)
			{
				return cluster;
			}

            //只有一个节点
			if (candidates.BiggestSize == 1)
			{
				return candidates.MaxTextLengthCluster;
			}

            //返回平均文本最长的集群
			if ((((candidates.MaxAverageTextLengthCluster != candidates.MaxPunctNumberCluster)
				&& ((candidates.MaxPunctNumberCluster.MinTextIndex - candidates.MaxAverageTextLengthCluster.MinTextIndex) > 50))
				&& ((candidates.MaxPCluster.MinTextIndex - candidates.MaxAverageTextLengthCluster.MinTextIndex) > 50))
				&& (candidates.MaxAverageTextLengthCluster.Nodes.Count > 1))
			{
				return candidates.MaxAverageTextLengthCluster;
			}

            //返回标点最多的集群
			if (candidates.MaxPunctNumberCluster == candidates.MaxTextLengthCluster)
			{
				return candidates.MaxPunctNumberCluster;
			}

			if (candidates.MaxPunctNumberCluster == candidates.BiggestSizeCluster)
			{
                //返回文本总长最长的集群
				if (((candidates.LargestP > 0) && (candidates.MaxPCluster == candidates.MaxTextLengthCluster))
					&& (candidates.MaxPunctNumberCluster.ParagraphNumber == 0))
				{
					return candidates.MaxTextLengthCluster;
				}

                //返回标点最多的集群
				return candidates.MaxPunctNumberCluster;
			}

            //返回标点最多的集群
			if ((candidates.MaxPunctNumberCluster.TextLength > 300)
				&& ((candidates.MaxPunctNumberCluster.MinTextIndex < (candidates.BiggestSizeCluster.MinTextIndex - 40))
					|| (candidates.MaxPunctNumberCluster.MinTextIndex < (candidates.MaxTextLengthCluster.MinTextIndex - 40))))
			{
				return candidates.MaxPunctNumberCluster;
			}

            //返回标点最多的集群
			if ((candidates.MaxPunctNumberCluster.TextLength > 0x3e8)
				&& (candidates.MaxTextLengthCluster.TextLength < (8 * candidates.MaxPunctNumberCluster.TextLength)))
			{
				return candidates.MaxPunctNumberCluster;
			}

            //返回段落最多的集群
			if (((candidates.LargestP > 0) && (candidates.MaxPCluster.TextLength > 300))
				&& (candidates.MaxTextLengthCluster.TextLength < (2 * candidates.MaxPCluster.TextLength)))
			{
				return candidates.MaxPCluster;
			}

            //返回文本总长最长的集群
			if (candidates.MaxTextLengthCluster.MinTextIndex < candidates.BiggestSizeCluster.MinTextIndex)
			{
				return candidates.MaxTextLengthCluster;
			}

            //返回节点数最多的集群
			return candidates.BiggestSizeCluster;
		}

		private static double GetExtractedRatio(WrappedNode root, ClusterCompleteHelper mapHelper)
		{
			int extractedLen = 0;
			int textLen = 0;
			int extractedNodes = 0;
			int nodes = 0;
			DocHelper.Travel(
				root,
				null,
				delegate(WrappedNode node)
				{
					if (node.Children.Count == 0)
					{
						int length = 0;
						if (node.IsTextNode)
						{
							length = node.Text.Length;
						}

						if (mapHelper.ExtractedSet.Contains(node.Index))
						{
							extractedLen += length;
							extractedNodes++;
							if (!mapHelper.CheckedSet.Contains(node.Index))
							{
								mapHelper.CheckedSet.Add(node.Index);
							}
						}

						textLen += length;
						nodes++;
					}

					return true;
				});
			if (textLen > 0)
			{
				return (1.0 * extractedLen) / textLen;
			}

			if (nodes > 0)
			{
				return (1.0 * extractedNodes) / nodes;
			}

			return 0.0;
		}

        /// <summary>
        /// 选取集群中最大特征
        /// </summary>
        /// <param name="testClusters"></param>
        /// <returns></returns>
		private static BestClusterHelper SelectBestClusterCandidate(IEnumerable<Cluster> testClusters)
		{
			var helper = new BestClusterHelper();
			foreach (Cluster cluster in testClusters)
			{
				int count = cluster.Nodes.Count;
				if (count > helper.BiggestSize)
				{
					helper.BiggestSizeCluster = cluster;
				}

				if ((count > 0) && ((cluster.TextLength / ((double)count)) > helper.MaxAverageTextLength))
				{
					helper.MaxAverageTextLengthCluster = cluster;
				}

				if (cluster.TextLength > helper.MaxTextLength)
				{
					helper.MaxTextLengthCluster = cluster;
				}

				if (cluster.ParagraphNumber > helper.LargestP)
				{
					helper.MaxPCluster = cluster;
				}

				if (cluster.PunctualNumberSum > helper.MaxPunctNumber)
				{
					helper.MaxPunctNumberCluster = cluster;
				}
			}

			return helper;
		}

        /// <summary>
        /// 选取最佳集群，找出文本最长的索引和长度
        /// </summary>
        /// <param name="clusters"></param>
        /// <param name="maxTextLengthTextIndex"></param>
        /// <param name="maxTextLength"></param>
		private static void SelectBestClusterWithDifferentTitles(
			List<Cluster> clusters,
			out int maxTextLengthTextIndex,
			out int maxTextLength)
		{
			maxTextLength = -1;
			maxTextLengthTextIndex = -1;
			int punctualNumberSum = -1;
			int num2 = -1;
            //遍历集群，找到文本最长和标点最多的集群
			for (int i = 0; i < clusters.Count; i++)
			{
				if (clusters[i].MainTitleTextIndex >= (clusters[i].MinTextIndex - 500))
				{
					if ((clusters[i].TextLength > maxTextLength)
						|| (((clusters[i].TextLength == maxTextLength)
							&& (clusters[i].TitleNode.Jac > (clusters[maxTextLengthTextIndex].TitleNode.Jac + 0.2)))
							&& (clusters[maxTextLengthTextIndex].TitleNode.Jac < 0.1)))
					{
						maxTextLength = clusters[i].TextLength;
						maxTextLengthTextIndex = i;
					}

					if ((clusters[i].PunctualNumberSum > punctualNumberSum)
						|| (((clusters[i].PunctualNumberSum == punctualNumberSum)
							&& (clusters[i].TitleNode.Jac > (clusters[num2].TitleNode.Jac + 0.2))) && (clusters[num2].TitleNode.Jac < 0.1)))
					{
						punctualNumberSum = clusters[i].PunctualNumberSum;
						num2 = i;
					}
				}
			}
            //将文本最多的节点设为标点最多的节点，（标点所占比例最大）
			if ((maxTextLengthTextIndex >= 0)
				&& ((((maxTextLengthTextIndex != num2) && (clusters[num2].TextLength > 500))
					&& ((clusters[num2].TextLength > 0) && (clusters[maxTextLengthTextIndex].TextLength > 0)))
					&& ((clusters[num2].PunctualNumberSum / clusters[num2].TextLength)
						> (clusters[maxTextLengthTextIndex].PunctualNumberSum / clusters[maxTextLengthTextIndex].TextLength))))
			{
				maxTextLengthTextIndex = num2;
			}
		}

        /// <summary>
        /// 选取最优集群
        /// </summary>
        /// <param name="textClusters"></param>
        /// <returns></returns>
		protected static Cluster SelectBestClusterWithSameTitle(List<Cluster> textClusters)
		{
			Cluster cluster = ChooseBestCluster(SelectBestClusterCandidate(textClusters));
            //设置集群文本长度
			if (cluster != null)
			{
				cluster.TextLength = cluster.TextLengthBak;
			}

			return cluster;
		}

		protected static void UpdateClusterHelper(WrappedNode root, ClusterCompleteHelper clusterHelper)
		{
			while ((root.Parent != null) && (root.Parent.Children.Count == 1))
			{
				root = root.Parent;
			}
            //这个访问？
			DocHelper.Travel(
				root,
				null,
				delegate(WrappedNode node)
				{
					if (!clusterHelper.DoneSet.Contains(node.Index))
					{
						clusterHelper.DoneSet.Add(node.Index);
					}

					return true;
				});
		}

		private static void UpdateImageInfo(ImageInfo info)
		{
			WrappedNode rawImage = info.RawImage;
			rawImage.RawNode.SetAttributeValue("width", info.Width.ToString(CultureInfo.InvariantCulture));
			rawImage.RawNode.SetAttributeValue("height", info.Height.ToString(CultureInfo.InvariantCulture));
		}


        /// <summary>
        /// 更新跳转节点信息
        /// </summary>
        /// <param name="body"></param>
        //临时变量可以不要。是引用类型。
		protected static void UpdateNavBlockInfo(WrappedNode body)
		{
            //递归
			DocHelper.Travel(
				body,
                //初始化NavBlock
				delegate(WrappedNode node)
				{
					node.NavBlockInfo = new BlockInfo();
					return true;
				},
                //递归调用对跳转节点信息赋值
				delegate(WrappedNode node)
				{
					node.NavBlockInfo.InnerTextLen = 0;
					node.NavBlockInfo.InnerHtmlLength = node.InnerHtml.Length;
					if (node.TagName == "a")
					{
						BlockInfo info1 = node.NavBlockInfo;
						info1.LinkNum++;
						BlockInfo info2 = node.NavBlockInfo;
						info2.LinkTextLen += node.InnerText.Length;
					}

					if (node.TagName == "img")
					{
						BlockInfo info3 = node.NavBlockInfo;
						info3.ImgNum++;
					}

					if (node.TagName == "p")
					{
						BlockInfo info4 = node.NavBlockInfo;
						info4.ParagraphNumber++;
					}

					if (node.NodeType == HtmlNodeType.Text)
					{
						node.NavBlockInfo.InnerTextLen = node.InnerText.Length;
						node.NavBlockInfo.PunctualAndDigitNumber = node.PunctualAndDigitNumber;
					}

					if (!(node.IsTextNode && (node.InnerText.Length <= 0)))
					{
						BlockInfo info5 = node.NavBlockInfo;
						info5.NodeNum++;
					}

                    //将孩子节点的信息添加到该节点上
					foreach (WrappedNode node2 in node.Children)
					{
						BlockInfo info6 = node.NavBlockInfo;
						info6.LinkTextLen += node2.NavBlockInfo.LinkTextLen;
						BlockInfo info7 = node.NavBlockInfo;
						info7.NodeNum += node2.NavBlockInfo.NodeNum;
						BlockInfo info8 = node.NavBlockInfo;
						info8.LinkNum += node2.NavBlockInfo.LinkNum;
						BlockInfo info9 = node.NavBlockInfo;
						info9.ImgNum += node2.NavBlockInfo.ImgNum;
						BlockInfo info10 = node.NavBlockInfo;
						info10.InnerTextLen += node2.NavBlockInfo.InnerTextLen;
						BlockInfo info11 = node.NavBlockInfo;
						info11.ParagraphNumber += node2.NavBlockInfo.ParagraphNumber;
						BlockInfo info12 = node.NavBlockInfo;
						info12.PunctualAndDigitNumber += node2.NavBlockInfo.PunctualAndDigitNumber;
					}

                    //判断是否是跳转节点
					BlockInfo navBlockInfo = node.NavBlockInfo;
					node.IsNavBlock = ((((navBlockInfo.LinkNum > 2) && (navBlockInfo.InnerTextLen < 150))              //链接数大于2并且内部文本数小于150
										&& (navBlockInfo.NodeNum > 10))                                                //节点数大于10
										&& (((navBlockInfo.LinkTextLen / navBlockInfo.LinkNum) < 10)                   //平均每个链接中文本字数小于10
                                            || ((navBlockInfo.InnerTextLen / navBlockInfo.NodeNum) < 10)))             //平均每个节点中文本字数小于10
									&& ((navBlockInfo.InnerHtmlLength / (navBlockInfo.InnerTextLen + 1)) > 10);        //文本数只占html文本字数的十分之一以下
					return true;
				});
		}

        /// <summary>
        /// 调整标题，为最优集群修改最优标题
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clusters"></param>
        /// <param name="maxTextLenTextIndex"></param>
        /// <param name="maxTextLen"></param>
		protected void AdjustTitle(
			PendingAnalysisPageExtractedInfo data,
			List<Cluster> clusters,
			ref int maxTextLenTextIndex,
			ref int maxTextLen)
		{
			int minTextIndex = clusters[maxTextLenTextIndex].MinTextIndex;
			int mainTitleTextIndex = clusters[maxTextLenTextIndex].MainTitleTextIndex;
			int num3 = maxTextLenTextIndex;
			for (int i = 0; i < clusters.Count; i++)
			{
                //具体判断条件，选择最优集群以及最优标题
				int num5 = clusters[i].MainTitleTextIndex;
				if ((((num5 > (mainTitleTextIndex + 1)) && (num5 < minTextIndex)) && (clusters[i].TextLength == maxTextLen))
					&& (((data.TextList[num5].Hx > (data.TextList[mainTitleTextIndex].Hx * 0.5))
						&& (data.TextList[num5].HeadingNode != null))
						|| ((data.TextList[mainTitleTextIndex].InnerText.Length < 0x19)
							&& ((num5 > (mainTitleTextIndex + 10)) || (data.TextList[num5].Hx > data.TextList[mainTitleTextIndex].Hx)))))
				{
					mainTitleTextIndex = num5;
					num3 = i;
				}
				else if ((((clusters[i].TextLength > (0.8 * maxTextLen)) && (mainTitleTextIndex < 5))
						&& (data.TextList[num5].HeadingNode != null)) && (data.TextList[mainTitleTextIndex].HeadingNode == null))
				{
					mainTitleTextIndex = num5;
					num3 = i;
				}
			}

            //更新最优集群标题索引
			maxTextLenTextIndex = num3;
			if ((mainTitleTextIndex > 0)
				&& ((data.TextList[mainTitleTextIndex - 1].Hx > data.TextList[mainTitleTextIndex].Hx)
					&& (data.TextList[mainTitleTextIndex].Parent.Index < (data.TextList[mainTitleTextIndex - 1].Parent.Index + 2))))
			{
				clusters[maxTextLenTextIndex].MainTitleTextIndex = mainTitleTextIndex - 1;
				clusters[maxTextLenTextIndex].SubTitleTextIndex = mainTitleTextIndex;
			}
		}

        /// <summary>
        /// 检查内容，设置失败信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mb"></param>
		protected void CheckContent(PendingAnalysisPageExtractedInfo data, WrappedNode mb)
		{
            //为了对特殊网址的处理
            Uri uri;
            string host = string.Empty;
            if (Uri.TryCreate(data.Page.Url, UriKind.Absolute, out uri))
            {
                host = uri.Host.ToLower();
            }
			try
			{
				if (data.CommonParentOfContentNodes == null || data.TextNodesInBiggestCluster.Count < 2)
				{
                    throw ExceptionFactory.LowConfidenceAboutPrecisionOnClusterNodeNumber;
                }
				if (data.TextNodesInBiggestCluster[0].PlainTextPos * 2 > data.PlainTextLength)
				{
					throw ExceptionFactory.LowConfidenceAboutPrecisionTooMuchPlainText;
				}
				if (data.TextNodesInBiggestCluster[0].BlockIndex > data.Page.Title.BlockIndex + 40)
				{
					throw ExceptionFactory.LowConfidenceAboutPrecisionFirstNodeFarFromTitle;
				}
				var finalNodeList = new List<WrappedNode>();
				WrappedNode wrappedNode1 = null;
                //相邻两个块不能够离得太远。
				foreach (WrappedNode wrappedNode2 in data.TextNodesInBiggestCluster)
				{
                    // p div p 如果第一个p 是10 第二个p为12
					if (wrappedNode1 != null && wrappedNode2.BlockIndex > wrappedNode1.BlockIndex + 12&&(wrappedNode2.BfsIndex-wrappedNode1.BfsIndex)>3)
					{
						throw ExceptionFactory.LowConfidenceAboutPrecisionBigGapBetweenNeighbourNodes;
					}
					wrappedNode1 = wrappedNode2;
				}
                //这个好像是遍历把HtmlNodeType.Text 或者node.TagName == "img"的节点加到finalNodeLis中。
				DocHelper.Travel(
					mb,
					node =>
					{
						if (node.NodeType == HtmlNodeType.Text || node.TagName == "img")
						{
							finalNodeList.Add(node);
						}
						return true;
					},
					null);
                //?
				if (finalNodeList.Where(node => node.IsTextNode).Sum(node => node.InnerText.Length) < 107)
				{
					throw ExceptionFactory.LowConfidenceAboutPrecisionFinalLengthTooShort;
				}
			}
			catch (ArgumentException ex)
			{
                //增加对雪球网的特殊处理，当雪球网时，不考虑上面这些规则。
				if (!string.IsNullOrWhiteSpace(data.Page.Warning)||host=="xueqiu.com")
				{
					return;
				}
				data.Page.Warning = ex.Message;
			}
		}

        /// <summary>
        /// 过滤正文节点
        /// </summary>
        /// <param name="data"></param>
        /// <param name="root"></param>
        /// <param name="bestCluster"></param>
        /// <param name="mapHelper"></param>
        /// <returns></returns>
		protected bool CompleteAndFilterNode(
			PendingAnalysisPageExtractedInfo data,
			WrappedNode root,
			Cluster bestCluster,
			ClusterCompleteHelper mapHelper)
		{
            //TODO:解析这个函数的具体逻辑
			WrappedNode node;
			double extractedRatio;
			int length;
			int num10;
			double num11;
			WrappedNode parent;
			if (root.Children.Count == 0)
			{
				if (mapHelper.ExtractedSet.Contains(root.Index))
				{
					mapHelper.MainContentIndexSet.Add(root.Index);
					if (!mapHelper.CheckedSet.Contains(root.Index))
					{
						mapHelper.CheckedSet.Add(root.Index);
					}
				}
                //这个函数是不是总会在这里返回？
				return false;
			}

			var list = new List<int>();
			var list2 = new List<double>();
			int num = 0;
			int num2 = 0;
			var dictionary = new Dictionary<string, int>();
			bool flag = false;
			string str = string.Empty;
			int num3 = 0;
			int index = data.TextList[bestCluster.MainTitleTextIndex].Index;
			int num5 = 0;
			int count = root.Children.Count;
			while (num5 < count)
			{
				if (mapHelper.ExtractedSetCount == mapHelper.CheckedSetCount)
				{
					break;
				}

				list.Add(0);
				list2.Add(0.0);
				node = root.Children[num5];
				if (!mapHelper.DoneSet.Contains(node.Index))
				{
					bool flag2 = false;
					bool flag3 = false;
					extractedRatio = GetExtractedRatio(node, mapHelper);
					list2[num5] = extractedRatio;
					if (node.Index < index)
					{
						flag3 = true;
					}
					else if (extractedRatio > 0.7)
					{
						flag2 = true;
					}
					else if (extractedRatio < 0.2)
					{
						flag3 = true;
					}
					else if (NodeHelper.GetListNumber(node) > 2)
					{
						flag3 = true;
					}
					else
					{
						HyperRatioInfo hyperRatios = NodeHelper.GetHyperRatios(node);
						if (hyperRatios.Ratio > 0.48)
						{
							flag3 = true;
						}
						else if ((hyperRatios.Count > 2) && (node.TextLen == 0))
						{
							flag3 = true;
						}
					}

					if (flag3)
					{
						list[num5] = -1;
					}
					else if (flag2)
					{
						list[num5] = 1;
						if (!string.IsNullOrWhiteSpace(node.TagName))
						{
							if (dictionary.ContainsKey(node.TagName))
							{
								Dictionary<string, int> dictionary2;
								string str4;
								(dictionary2 = dictionary)[str4 = node.TagName] = dictionary2[str4] + 1;
							}
							else
							{
								dictionary[node.TagName] = 1;
							}
						}
						else
						{
							num2++;
						}

						num++;
					}
					else
					{
						list[num5] = 0;
					}
				}

				num5++;
			}

			foreach (string str2 in dictionary.Keys)
			{
				if (dictionary[str2] > num3)
				{
					num3 = dictionary[str2];
					str = str2;
				}
			}

			num5 = 0;
			count = list.Count;
			while (num5 < count)
			{
				node = root.Children[num5];
				if (!string.IsNullOrWhiteSpace(node.TagName) && (node.Index >= index))
				{
					extractedRatio = list2[num5];
					length = node.InnerHtml.Length;
					num10 = node.InnerHtml.Length;
					num11 = 0.0;
					if (length != 0)
					{
						num11 = (1.0 * num10) / length;
					}

					string tagName = node.TagName;
					if ((((tagName == str) && (num3 > 1)) && (extractedRatio > 0.0)) && (num11 > 0.8))
					{
						list[num5] = 1;
						num++;
						num3++;
					}
					else if (((tagName == str) && (tagName == "p")) && (num3 > 1))
					{
						list[num5] = 1;
						num++;
						num3++;
					}
					else if (NodeHelper.IsBlockQuoteNode(node))
					{
						list[num5] = 1;
						num++;
					}
					else
					{
						switch (tagName)
						{
							case "br":
								list[num5] = 1;
								num++;
								if (((num2 > (0.5 * num)) && !flag) && ((mapHelper.ExtractedSetCount * 0.8) < mapHelper.CheckedSetCount))
								{
									flag = true;
								}

								goto Label_0581;

							case "table":
								if (NodeHelper.IsDataTable(node))
								{
									list[num5] = 1;
									num++;
								}

								goto Label_0581;
						}

						if ((tagName == "p") && (num10 == 0))
						{
							list[num5] = 1;
							num++;
						}
					}
				}

				Label_0581:
				num5++;
			}

			num5 = 1;
			count = list.Count - 1;
			while (num5 < count)
			{
				if (list[num5] == 0)
				{
					node = root.Children[num5];
					if (node.Index >= index)
					{
						num11 = 0.0;
						if (node.TagName != null)
						{
							GetExtractedRatio(node, mapHelper);
							length = node.InnerHtml.Length;
							num10 = node.InnerHtml.Length;
							if (length != 0)
							{
								num11 = (1.0 * num10) / length;
							}
						}
						else if (node.IsTextNode)
						{
							num11 = 1.0;
						}

						if (((list[num5 - 1] == 1) && (list[num5 + 1] == 1)) && (num11 > 0.8))
						{
							list[num5] = 1;
							num++;
						}
					}
				}

				num5++;
			}

			if (!(str == "tr"))
			{
				if (!(str == "li"))
				{
					if (num == root.Children.Count)
					{
						mapHelper.MainContentIndexSet.Add(root.Index);
					}
					else
					{
						num5 = 0;
						count = list.Count;
						while (num5 < count)
						{
							if (list[num5] == 1)
							{
								mapHelper.MainContentIndexSet.Add(root.Children[num5].Index);
							}

							num5++;
						}
					}

					return flag;
				}

				for (parent = root.Parent; parent != null; parent = parent.Parent)
				{
					if ((parent.TagName == "ul") || (parent.TagName == "ol"))
					{
						break;
					}
				}
			}
			else
			{
				parent = root.Parent;
				while (parent != null)
				{
					if (parent.TagName == "table")
					{
						break;
					}

					parent = parent.Parent;
				}

				if (parent == null)
				{
					parent = root;
				}

				mapHelper.MainContentIndexSet.Add(parent.Index);
				return flag;
			}

			if (parent == null)
			{
				parent = root;
			}

			mapHelper.MainContentIndexSet.Add(parent.Index);
			return flag;
		}

        /// <summary>
        /// 选出最优集群，并完善内部参数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cluster"></param>
        /// <returns></returns>
		protected Cluster CompleteCluster(PendingAnalysisPageExtractedInfo data, Cluster cluster)
		{
			string str;
			WrappedNode node5;
			WrappedNode firstBlockParent;
			var cluster2 = new Cluster();
			int textIndex = cluster.Nodes[0].TextIndex;
            //集群内最大的文本序列索引
			int num3 = cluster.Nodes[cluster.Nodes.Count - 1].TextIndex;
            //集群的层级
			int level = cluster.Level;
            //集群的最大字体
			int maxHx = cluster.MaxHx;
            //返回结果字段
			cluster2.Level = level;
			cluster2.MainTitleTextIndex = cluster.MainTitleTextIndex;
			cluster2.SubTitleTextIndex = cluster.SubTitleTextIndex;
			WrappedNode commonParentOfContentNodes = data.CommonParentOfContentNodes;
            //没有备选标题节点则找一个标题备选节点
			if (cluster.SubTitleTextIndex == -1)
			{
                //num6为从标题所在节点开始，按照深度遍历顺序找到第一个父节点不同的旁支节点
				int num6 = cluster.MainTitleTextIndex + 1;
				while ((num6 < data.TextList.Count)
						&& (data.TextList[num6].Parent == data.TextList[cluster.MainTitleTextIndex].Parent))
				{
					num6++;
				}

                //如果找到了，则可以通过判断选取这个节点作为标题备选
				if (num6 < data.TextList.Count)
				{
					WrappedNode node2 = data.TextList[cluster.MainTitleTextIndex];
					WrappedNode node3 = data.TextList[num6];
					WrappedNode parent = node3.Parent;
                    //如果父节点之间距离小于4
					if ((parent.Index - node2.Parent.Index) < 4)
					{
						str = node3.InnerText.ToUpperInvariant();
						if ((((node3.Hx < node2.Hx) && ((node3.Hx > maxHx) || (node3.HeadingNode != null)))
							&& ((node3.TagName != "a") && (str.Length > 20)))
							&& ((str.IndexOf("by", StringComparison.OrdinalIgnoreCase) < 0)
								|| (str.IndexOf("by", StringComparison.OrdinalIgnoreCase) > 3)))
						{
							cluster2.SubTitleTextIndex = node3.TextIndex;
						}
						else if ((parent.Class.Length > 0) && (parent.Class == "subheadline"))
						{
							cluster2.SubTitleTextIndex = node3.TextIndex;
						}
					}
				}
			}

			int num7 = cluster2.MainTitleTextIndex + 1;
			if (cluster2.SubTitleTextIndex > -1)
			{
				num7 = cluster2.SubTitleTextIndex + 1;
			}

            //计算各种标签的数量
			var tagCount = new Dictionary<string, int>();
			DocHelper.CountTags(cluster.Nodes, ref tagCount);
			int num = num7;
            //从标题之后开始遍历，寻找正文节点
			while (num < (num3 + 1))
			{
				if (StringHelper.ContainEndWord(data.TextList[num].InnerText)
					&& ((cluster.TextLength <= 0) || ((cluster2.TextLength / (1f * cluster.TextLength)) > 0.7)))
				{
					break;
				}

				node5 = data.TextList[num];
				if (((((node5.FirstBlockParent == null) || !node5.FirstBlockParent.IsNavBlock) && (node5.TextLen > 0))
					&& ((((commonParentOfContentNodes == null) || DocHelper.IsChild(node5, commonParentOfContentNodes))
						|| ((((node5.Parent == null) || (node5.Parent.TagName == "p"))
							|| ((node5.Parent.TagName == "h1") || (node5.Parent.TagName == "h2"))) || (node5.Parent.TagName == "h3")))
						&& ((node5.PunctualNumber > 0)
							|| ((commonParentOfContentNodes == null) || DocHelper.IsChild(node5, commonParentOfContentNodes)))))
					&& (((Math.Abs(node5.Level - level) < 2) || ((node5.TagName == "p") && (num <= textIndex)))
						|| ((node5.TextLen >= 100) && (num <= textIndex))))
				{
					if (cluster2.Nodes.Count == 0)
					{
						str = node5.InnerText.ToUpperInvariant();
						if ((str.IndexOf("by", StringComparison.OrdinalIgnoreCase) < 0)
							|| (str.IndexOf("by", StringComparison.OrdinalIgnoreCase) > 3))
						{
							cluster2.Nodes.Add(data.TextList[num]);
							cluster2.TextLength += node5.TextLen;
						}
					}
					else
					{
						cluster2.Nodes.Add(data.TextList[num]);
						cluster2.TextLength += node5.TextLen;
					}
				}

				num++;
			}

			var set = new HashSet<WrappedNode>();
			if (cluster2.Nodes != null)
			{
				foreach (WrappedNode node6 in cluster2.Nodes)
				{
					firstBlockParent = node6.FirstBlockParent;
					if ((firstBlockParent != null) && (firstBlockParent.TagName == "p"))
					{
						set.Add(firstBlockParent);
					}
				}
			}

            //在集群范围外查找正文节点
			if (num == (num3 + 1))
			{
				while (num < data.TextList.Count)
				{
					if (StringHelper.ContainEndWord(data.TextList[num].InnerText))
					{
						break;
					}

					node5 = data.TextList[num];
					bool flag = false;
					firstBlockParent = node5.FirstBlockParent;
					if (set.Contains(firstBlockParent))
					{
						flag = true;
					}

					if (!flag
						&& (((((node5.FirstBlockParent != null) && node5.FirstBlockParent.IsNavBlock) || (node5.TextLen <= 0))
							|| ((node5.PunctualNumber <= 0) || (Math.Abs(node5.Level - level) >= 2)))
							|| (((cluster2.Nodes != null)
								&& ((data.TextList[num].OriginalIndex - cluster2.Nodes[cluster2.Nodes.Count - 1].OriginalIndex) > 30))
								|| !((commonParentOfContentNodes == null) || DocHelper.IsChild(data.TextList[num], commonParentOfContentNodes)))))
					{
						break;
					}

					if (cluster2.Nodes != null)
					{
						cluster2.Nodes.Add(data.TextList[num]);
					}

					cluster2.TextLength += node5.TextLen;
					num++;
				}
			}

            //设置最下文本索引
			if ((cluster2.Nodes != null) && (cluster2.Nodes.Count > 0))
			{
				cluster2.MinTextIndex = cluster2.Nodes[0].TextIndex;
			}

            //返回cluster2
			cluster2.TextLength = cluster2.TextLength;
			return cluster2;
		}

		protected void CompleteandFilterExtraction(PendingAnalysisPageExtractedInfo data, Cluster cluster)
		{
			var clusterCompleteHelper = new ClusterCompleteHelper();
			var commonRoots = new Dictionary<int, int>();
            //添加内容节点
			if (cluster.Nodes.Count < 2)
			{
                //没有节点则返回
				if (cluster.Nodes.Count != 1)
				{
					return;
				}
				cluster.Nodes[0].ResultType = ResultType.Content;
				data.ContentNodes = new List<WrappedNode> { cluster.Nodes[0] };
			}
			else
			{
				int index1 = 0;
				for (int count = cluster.Nodes.Count; index1 < count; ++index1)
				{
					clusterCompleteHelper.ExtractedSet.Add(cluster.Nodes[index1].Index);
				}
				int index2 = 0;
				for (int index3 = cluster.Nodes.Count - 1; index2 < index3; ++index2)
				{
					int index4 = DocHelper.CommonRootOf2Nodes(cluster.Nodes[index2], cluster.Nodes[index2 + 1]).Index;
					if (commonRoots.ContainsKey(index4))
					{
						Dictionary<int, int> dictionary;
						int index5;
						(dictionary = commonRoots)[index5 = index4] = dictionary[index5] + 1;
					}
					else
					{
						commonRoots[index4] = 1;
					}
				}

				var list = commonRoots.Keys.Select(
					i =>
					{
						var fAnonymousType0 = new { Index = i, Count = commonRoots[i] };
						return fAnonymousType0;
					}).ToList();

				//先按照Index排序再按照Count排序
				list.Sort(
					(a, b) =>
					{
						if (b.Count == a.Count)
						{
							return b.Index - a.Index;
						}
						return b.Count - a.Count;
					});
				int index6 = 0;
				for (int count = list.Count; index6 < count; ++index6)
				{
					WrappedNode root = data.NodeList[list[index6].Index];
					if (!clusterCompleteHelper.DoneSet.Contains(root.Index))
					{
						bool flag = this.CompleteAndFilterNode(data, root, cluster, clusterCompleteHelper);
						UpdateClusterHelper(root, clusterCompleteHelper);
						if (flag || clusterCompleteHelper.ExtractedSetCount == clusterCompleteHelper.CheckedSetCount)
						{
							break;
						}
					}
				}

				data.ContentNodes = new List<WrappedNode>();
				foreach (int index3 in clusterCompleteHelper.MainContentIndexSet)
				{
					data.NodeList[index3].ResultType = ResultType.Content;
					data.ContentNodes.Add(data.NodeList[index3]);
				}
			}
		}

        /// <summary>
        /// 根据标题创建文本集群
        /// </summary>
        /// <param name="data"></param>
        /// <param name="firstTextIndex">标题节点的index值</param>
        /// <param name="lastTextIndex"></param>
        /// <param name="title"></param>
        /// <returns></returns>
		protected List<Cluster> CreateTextClusters(
			PendingAnalysisPageExtractedInfo data,
			int firstTextIndex,
			int lastTextIndex,
			WrappedNode title)
		{
			var source = new List<Cluster>();
            //遍历所有节点，根据节点建立集群，如果节点远离标题，则不计算
			for (int i = firstTextIndex + 1; i <= lastTextIndex; i++)
			{
				Func<Cluster, bool> predicate = null;
				WrappedNode node = data.TextList[i];
                //如果节点可见并且有文本
				if ((node.Text.Length > 0) && node.IsNodeVisible)
				{
                    //节点中文字是否太短
					bool discardflag = false;
					if (this.Config.EnableShortParagraph)
					{
                        discardflag = (node.Text.Length <= 50) && !node.Text.TrimEnd(new char[0]).EndsWith("。") && !node.Text.TrimEnd(new char[0]).EndsWith(".") ;
					}
					else
					{
						discardflag = node.Text.Length <= 50;
					}


                    //如果节点不处于链接中，且文本足够长，文本中有标点符号

                    /*
					node.Atag == 1 && node.Parent.Parent.TagName == "p" && node.Parent.Parent.InnerText.Length > node.InnerText.Length，这里允许出现之前的discard的节点，his likability as a potential president 的节点，由于结尾没有句号，会被discard掉。 允许的是如下的节点：
                    <p>Americans are not the only ones abandoning Republican presidential candidate Donald Trump in the wake of two poor debate performances and a tape showing him bragging about sexually assaulting women. It seems that Chinese audiences are also re-evaluating both the likelihood that Trump will win and 
                    <a href="https://www.brookings.edu/blog/order-from-chaos/2016/07/22/what-do-chinese-people-have-to-say-about-donald-trump/" target="_blank">his likability as a potential president</a>.	</p>
                     中的his likability as a potential president
                    node.Parent.Parent 不会为空，因为当node.Atag == 1 时，node.Parent肯定是a标签在的节点。
					*/
                    //|| (node.Atag == 1 && node.Parent.Parent.TagName == "p" && (node.Parent.Parent.InnerText.Length - node.InnerText.Length >10)) 这个不要啦，因为在这之前就合并了这样的。
                    if ((!discardflag && node.Atag == 0 && node.PunctualNumber >= 1) )
					{
						if (predicate == null)
						{
							predicate = cluster => IsNodeMatchCluster(node, cluster);
						}

						Cluster item = source.FirstOrDefault(predicate);
                        //如果节点不属于任何一个集群，则自己建立一个集群
						if (item == null)
						{
                            //如果节点远离标题则不考虑
							if ((title != null) && Condition.IsNodeFarFromTitle(node, title, data.PlainTextLength))
							{
								continue;
							}

                            //根据节点构建集群
							item = new Cluster { Level = node.Level, MinTextIndex = i };
							source.Add(item);
						}

                        //在集群中添加当前节点，并更新集群信息
						item.Nodes.Add(node);
						item.TextLength += node.Text.Length;
						if ((node.Parent != null) && (node.Parent.TagName == "p"))
						{
							item.ParagraphNumber++;
						}

						item.MaxTextIndex = i;
						item.PunctualNumberSum += node.PunctualNumber;
						item.MaxHx = Math.Max(item.MaxHx, node.Hx);
					}
				}
			}

            //对每个集群更新相关信息
			if (source.Count > 0)
			{
				double meaningFulCharsPos = data.TextList[firstTextIndex].MeaningFulCharsPos;
				double endTextPos = data.TextList[lastTextIndex].MeaningFulCharsPos;
				foreach (Cluster cluster3 in source)
				{
					cluster3.UpdateClusterFeature(meaningFulCharsPos, endTextPos);
				}
			}

			return source;
		}

        //置信度怎么找的？
		protected void ExtractConfidenceFeature(WrappedNode mb, PendingAnalysisPageExtractedInfo data)
		{
			var feature = new ExtractionResultFeature
						{
							PlainTextLength = data.PlainTextLength,
							ClusterNodeNumber = (data.TextNodesInBiggestCluster != null) ? data.TextNodesInBiggestCluster.Count : 0,
                            HasContentParent = (data.CommonParentOfContentNodes != null) ? 1 : 0, //data.CommonParentOfContentNodes 怎么找出来的？
							TitlePlainTextIndexPosition =
								((data.Page.Title != null) && (data.PlainTextNumber > 0))
									? ((1.0 * data.Page.Title.PlainTextIndex) / data.PlainTextNumber)
                                    : 1.0 //所以，如果TitlePlainTextIndexPosition=1 表示data.Page.Title==null 或者data.PlainTextNumber <=0，或者两者都有。
						};
			data.ExtractionResultFeature = feature;
            //这怎么感觉又把TitlePlainTextIndexPosition 算了一遍呀？所以，这个应该是可以不要的吧?
            //PlainTextPos 这个是在文本中的位置吗？这样算合理吗？如果同样的界面，把html左边和右边的位置，在代码中的先后顺序放得不一样，结果就不一样了？
			data.ExtractionResultFeature.TitlePlainTextIndexPosition = ((data.Page.Title != null) && (data.PlainTextLength > 0))
																			? ((1.0 * data.Page.Title.PlainTextPos) / data.PlainTextLength)
																			: 1.0;
			data.ExtractionResultFeature.FirstClusterNodePlainTextPosition = (((data.ExtractionResultFeature.ClusterNodeNumber
																				> 0) && (data.TextNodesInBiggestCluster != null)) && (data.TextNodesInBiggestCluster.Count > 0))
																				? ((1.0 * data.TextNodesInBiggestCluster[0].PlainTextPos) / data.PlainTextLength)
																				: 0.0;
			int maxNodesPlainTextIndexDistance = 0;
			int maxNodesTextIndexDistance = 0;
			int maxNodesBlockIndexDistance = 0;
			int sumNodesPlainTextIndexDistance = 0;
			int extractedTextLength = 0;
			WrappedNode preTextNode = null;
			int textNodeNumber = 0;
			DocHelper.Travel(
				mb,
				delegate(WrappedNode node)
				{
					if (node.IsTextNode)
					{
						if (preTextNode != null)
						{
							maxNodesPlainTextIndexDistance = Math.Max(
								maxNodesPlainTextIndexDistance,
								node.PlainTextIndex - preTextNode.PlainTextIndex);
							maxNodesTextIndexDistance = Math.Max(maxNodesTextIndexDistance, node.TextIndex - preTextNode.TextIndex);
							maxNodesBlockIndexDistance = Math.Max(maxNodesBlockIndexDistance, node.BlockIndex - preTextNode.BlockIndex);
							sumNodesPlainTextIndexDistance += node.PlainTextIndex - preTextNode.PlainTextIndex;
						}

						preTextNode = node;
						textNodeNumber++;
						extractedTextLength += node.InnerText.Length;
					}

					return true;
				},
				null);
			data.ExtractionResultFeature.MaxNodesPlainTextIndexDistance = maxNodesPlainTextIndexDistance;
			data.ExtractionResultFeature.MaxNodesTextIndexDistance = maxNodesTextIndexDistance;
			data.ExtractionResultFeature.MaxNodesBlockIndexDistance = maxNodesBlockIndexDistance;
			data.ExtractionResultFeature.AvgNodesPlainTextIndexDistance = (textNodeNumber > 0)
																			? Convert.ToInt32((1.0 * sumNodesPlainTextIndexDistance) / textNodeNumber)
																			: 0x2710;
			data.ExtractionResultFeature.AvgNodesTextIndexDistance = (textNodeNumber > 0)
																		? Convert.ToInt32((1.0 * sumNodesPlainTextIndexDistance) / textNodeNumber)
																		: 0x2710;
			data.ExtractionResultFeature.AvgNodesBlockIndexDistance = (textNodeNumber > 0)
																		? Convert.ToInt32((1.0 * sumNodesPlainTextIndexDistance) / textNodeNumber)
																		: 0x2710;
			data.ExtractionResultFeature.ExtractedTextLength = extractedTextLength;
			data.ExtractionResultFeature.MainBlockRatio = DocHelper.GetMainBlockRatio(mb, data.TextNodesInBiggestCluster);
		}

		protected void ExtractImage(PendingAnalysisPageExtractedInfo data)
		{
			WrappedNode block1 = DocHelper.CommonRoot(data.Page.Body, block => block.ResultType > ResultType.None ? 1 : 0);
			int firstIndex = DocHelper.FirstIndex(block1);
			int lastIndex = DocHelper.LastIndex(block1);
			int titleIndex = data.Page.Title.Index;
			ImageHelper.UpdateImageSrc(data.Page.Body, data.Page.Url);
			List<ImageInfo> list =
				ImageHelper.GetImageFromCandidate(
					ImageHelper.ExtractImageCandidate(data.Page.Body, titleIndex, lastIndex),
					firstIndex,
					lastIndex,
					data.CommonParentOfContentNodes);
			if (list.Count(r => r.Image.Index < titleIndex) >= 5)
			{
				list = list.Where(r => r.Image.Index > titleIndex || r.Image.IsNodeVisible).ToList();
			}
			int index = 0;
			for (int count = list.Count; index < count; ++index)
			{
				if (list[index].RawImage.TagName == "img"
					&& (list[index].Width != -1 && list[index].Height != -1 && list[index].Width * list[index].Height < 10000))
				{
					list[index].IsSmall = true;
				}
				else
				{
					bool flag = NodeHelper.IsVideoImage(list[index]);
					list[index].IsVideo = flag;
					if (string.IsNullOrWhiteSpace(data.Page.MainImage) && list[index].IsLarge && list[index].RawImage.TagName == "img")
					{
						data.Page.MainImage = list[index].RawImage.Src;
					}
					list[index].Image.ResultType = ResultType.Image;
					data.ImageNodes.Add(list[index].Image);
					if (list[index].Caption1 != null)
					{
						list[index].Caption1.ResultType = ResultType.ImageCaption;
						list[index].Caption1.RawNode.SetAttributeValue("isCaption", "true");
					}

					data.ImageCaptionNodes.Add(list[index].Caption1);
					if (list[index].Caption2 != null)
					{
						list[index].Caption2.ResultType = ResultType.ImageCaption;
						list[index].Caption2.RawNode.SetAttributeValue("isCaption", "true");
					}

					data.ImageSubCaptionNodes.Add(list[index].Caption2);
				}
			}

			if (!string.IsNullOrWhiteSpace(data.Page.MainImage) || list.Count <= 0)
			{
				return;
			}
			ImageInfo imageInfo =
				list.FirstOrDefault(r => !r.IsSmall && r.RawImage.TagName.Equals("img", StringComparison.OrdinalIgnoreCase));
			if (imageInfo != null)
			{
				data.Page.MainImage = imageInfo.RawImage.Src;
			}
		}

        /// <summary>
        /// 查找最佳文本集群
        /// </summary>
        /// <param name="data"></param>
        /// <param name="title"></param>
        /// <returns></returns>
		protected Cluster FindBestCluster(PendingAnalysisPageExtractedInfo data, WrappedNode title)
		{
			int textIndex = title.TextIndex;
			int lastTextIndex = data.TextList.Count - 1;
            //选取最优集群
			Cluster cluster = SelectBestClusterWithSameTitle(this.CreateTextClusters(data, textIndex, lastTextIndex, title));
            //设置标题节点
			if (cluster != null)
			{
				cluster.TitleNode = title;
			}

			return cluster;
		}

        /// <summary>
        /// 选出正文和标题
        /// </summary>
        /// <param name="data"></param>
		protected void FindTitleAndMainContent(PendingAnalysisPageExtractedInfo data)
		{
            //最优集群索引
			int num;
            //最优集群文本长度
			int num2;
			var clusters = new List<Cluster>();
			foreach (WrappedNode node in data.TitleCandidates)
			{
                //寻找最佳文本集群
				Cluster item = this.FindBestCluster(data, node);
				if (item != null)
				{
                    //设置标题文本索引
					item.MainTitleTextIndex = node.TextIndex;
					clusters.Add(item);
				}
			}

            //选取文本最长的集群，num为索引，num2为长度
			SelectBestClusterWithDifferentTitles(clusters, out num, out num2);
            //找到了
			if ((num != -1) || (num2 != -1))
			{
                //调整标题，更改num，并将最优集群的标题赋值为最优标题
				this.AdjustTitle(data, clusters, ref num, ref num2);
                //clusters[num]集群为最优集群，找到最优集群的父节点
				if (clusters[num].Nodes.Count > 1)
				{
					data.CommonParentOfContentNodes = this.GetContentsParent(data, clusters[num].Nodes);
				}
				else
				{
					data.CommonParentOfContentNodes = clusters[num].Nodes[0].Parent;
				}

				if (((data.CommonParentOfContentNodes != null) && (data.CommonParentOfContentNodes.TagName == "p"))
					&& (data.CommonParentOfContentNodes.Parent != null))
				{
					data.CommonParentOfContentNodes = data.CommonParentOfContentNodes.Parent;
				}

                //根据最优集群，生成新的集群
				Cluster cluster = this.CompleteCluster(data, clusters[num]);
				data.TextNodesInBiggestCluster = new List<WrappedNode>();
				data.TextNodesInBiggestCluster.AddRange(clusters[num].Nodes);
                //添加data的正文节点
				this.CompleteandFilterExtraction(data, cluster);
                //选取标题节点
				this.SelectTitleNode(data, data.TextList[cluster.MainTitleTextIndex]).ResultType = ResultType.Title;

                //选取备选标题节点
				if ((cluster.SubTitleTextIndex >= 0)
					&& (data.TextList[cluster.SubTitleTextIndex].Parent.ResultType == ResultType.None))
				{
					data.TextList[cluster.SubTitleTextIndex].Parent.ResultType = ResultType.SubTitle;
				}

                //设置标题节点
				data.Page.Title = data.TextList[cluster.MainTitleTextIndex];
				int num3 = data.TextList.Count - 1;
				while (((data.TextList[num3].ResultType == ResultType.None) && (num3 > cluster.MainTitleTextIndex)) && (num3 > 0))
				{
					num3--;
				}

                //将最大正文节点和标题节点间的所有节点设置为正文节点
				while ((num3 > cluster.MainTitleTextIndex) && (num3 > 0))
				{
					if ((data.TextList[num3].HeadingNode != null) && (data.TextList[num3].Jac > 0.5))
					{
						data.TextList[num3].ResultType = ResultType.Content;
					}

					num3--;
				}
			}
		}

        /// <summary>
        /// 针对传入的节点，生成相关文本信息，包括文本长度、文本顺序索引、文本区块个数
        /// </summary>
        /// <param name="data">传入节点</param>
		protected void GenerateTextFeature(PendingAnalysisPageExtractedInfo data)
		{
			int level = 0;
			int atag = 0;
			int plainTextIndex = 0;
            int plainTextPos = 0;
            //遍历当前节点
			DocHelper.Travel(
                data.Page.Body,
                //过滤方法，递推调用
				delegate(WrappedNode node)
				{
                    node.Level = level;
                    //判断非层级标签块
					if (!StringHelper.NoLevelTags.Contains(node.TagName))
					{
						level++;
					}

                    //判断a标签，递推递增
					if (node.TagName == "a")
					{
						atag++;
					}

                    //赋值节点字体大小，如果当前节点没有显式标签能确定字体大小，则使用父节点的字体大小
					node.Hx = StringHelper.DefaultFontSize.ContainsKey(node.TagName)
								? StringHelper.DefaultFontSize[node.TagName]
								: StringHelper.DefaultFontSize["other"];
					if ((node.Hx == 0) && (node.Parent != null))
					{
						node.Hx = node.Parent.Hx;
					}

					return true;
				},
                //动作方法，递归调用，主要目的为建立文本索引以及确定文本所属位置信息
				delegate(WrappedNode node)
                {
                    //判断a标签,并递归递减
					if (node.TagName == "a")
					{
						atag--;
					}

                    //判断层级
					if (!StringHelper.NoLevelTags.Contains(node.TagName))
					{
						level--;
					}

                    //赋值a标签层级索引
                    node.Atag = atag;
                    //赋值普通文本位置
                    node.PlainTextPos = plainTextPos;
                    //赋值普通文本索引
                    node.PlainTextIndex = plainTextIndex;
                    //如果当前节点不在任何a标签中且文本长度大于0，则将文本位置及索引递增
                    //由于文本节点理论上都处于叶节点上，并且当前遍历顺序为深度遍历，所以递归调用是按照文本阅读顺序来标记的索引及位置的
					if (((node.Atag == 0) && node.IsTextNode) && (node.TextNodeMeaningFulCharsLength > 0))
					{
						plainTextPos += node.TextNodeMeaningFulCharsLength;
						plainTextIndex++;
					}

					return true;
                });
            //文本长度
            data.PlainTextLength = plainTextPos;
            //文本个数
			data.PlainTextNumber = plainTextIndex;
		}

		protected WrappedNode GetContentsParent(PendingAnalysisPageExtractedInfo data, List<WrappedNode> nodeList)
		{
			WrappedNode wrappedNode = null;
			if (nodeList != null && nodeList.Count >= 2)
			{
				wrappedNode = DocHelper.CommonRoot(data.Page.Body, node => nodeList.IndexOf(node) != -1 ? 1 : 0);
			}
			return wrappedNode;
		}

        /// <summary>
        /// 建立索引方法，将传入节点的子节点按照顺序索引
        /// </summary>
        /// <param name="data">传入节点</param>
	    protected void GetsBfsIndex(PendingAnalysisPageExtractedInfo data)
	    {
      
            int bfsIndex = 0;
            DocHelper.DfsTravel(
                data.Page.Body,
                delegate(WrappedNode node)
                {
                    node.BfsIndex = bfsIndex;
                 //   dfsIndex++;
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        bfsIndex++;
                    }
                    
                    return true;
                });
	    }

		protected void ReIndex(PendingAnalysisPageExtractedInfo data)
		{
			data.TextList = new List<WrappedNode>();
			data.NodeList = new List<WrappedNode>();
			int index = 0;
			int textIndex = 0;
			int meaningfulcharPos = 0;
			int blockIndex = 0;
            int linkIndex = 0;

            //对传入的数据节点进行遍历，累加文本长度，并赋值给ElementNodeMeaningFulCharsLength、meaningfulcharPos、TextNodeMeaningFulCharsLength字段
            //
            //利用过滤方法，递推遍历所有子节点，将以下值重新赋值
            //      index             --->    顺序标注索引
            //      textIndex         --->    文本区块顺序索引
            //      meaningfulcharPos --->    文本区块顺序位置
            //      blockIndex        --->    段落区块顺序索引
            //      linkIndex         --->    链接区块顺序索引
            //
            //与此同时，将所有文本区块顺序放入到TextList中，将所有子节点顺序放入NodeList中
			DocHelper.Travel(
                data.Page.Body,
                //过滤方法，一直返回true，递推调用
				delegate(WrappedNode node)
				{
					node.Index = index;
					node.BlockIndex = blockIndex;
                    node.LinkIndex = linkIndex;
                    //将当前节点加入到节点列表
					data.NodeList.Add(node);
                    index++;
                    //判断块标签
					if (StringHelper.IsBlockTag(node.TagName))
					{
						blockIndex++;
					}

                    //判断链接标签
					if (node.TagName == "a")
					{
						linkIndex++;
					}

                    //判断文本标签
					if (node.IsTextNode)
					{
						node.MeaningFulCharsPos = meaningfulcharPos;
						if (node.TextNodeMeaningFulCharsLength > 0)
						{
							meaningfulcharPos += node.TextNodeMeaningFulCharsLength;
                            node.TextIndex = textIndex;
                            //将当前文本节点加入到文本节点列表
							data.TextList.Add(node);
							textIndex++;
						}

						node.ElementNodeMeaningFulCharsLength = node.TextNodeMeaningFulCharsLength;
					}

					return true;
				},
                //动作方法，递归调用
				delegate(WrappedNode node)
				{
					if ((node.Children != null) && (node.Children.Count > 0))
					{
						int num = node.Children.Sum(child => child.ElementNodeMeaningFulCharsLength);
						node.ElementNodeMeaningFulCharsLength = num;
					}

					return true;
				});
		}

        /// <summary>
        /// 选取标题节点
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node">标题节点</param>
        /// <returns></returns>
		protected WrappedNode SelectTitleNode(PendingAnalysisPageExtractedInfo data, WrappedNode node)
		{
			WrappedNode parent = node.Parent;
			WrappedNode node3 = null;
			double num = -1.0;
			if (node.HeadingNode != null)
			{
				parent = node.HeadingNode;
			}

			while (node != parent.Parent)
			{
				int num2 = 0;
				int count = data.MetaTitles.Count;
				while (num2 < count)
				{
					double num4 = StringHelper.JaccardSimilarity(node.InnerText, data.MetaTitles[num2]);
					if ((num4 > num) || (Math.Abs(num4 - num) < 0.01))
					{
						num = num4;
						node3 = node;
					}

					num2++;
				}

				node = node.Parent;
			}

			return node3;
		}

        /// <summary>
        /// 更新文本层级关系
        /// </summary>
        /// <param name="data"></param>
		protected void UpdateTextLevel(PendingAnalysisPageExtractedInfo data)
		{
			int textNodeMeaningFulCharsLength;
			WrappedNode firstBlockParent;
            //字符长度字典
			var dictionary = new Dictionary<WrappedNode, int>();
            //层级字典
			var dictionary2 = new Dictionary<WrappedNode, int>();
            //在所有文本标签节点中寻找
			foreach (WrappedNode node in data.TextList)
			{
				textNodeMeaningFulCharsLength = node.TextNodeMeaningFulCharsLength;
                //第一个块标签父节点
				firstBlockParent = node.FirstBlockParent;
				if ((firstBlockParent != null) && (textNodeMeaningFulCharsLength > 0))
				{
					if (!dictionary2.ContainsKey(firstBlockParent))
					{
						dictionary[firstBlockParent] = 0;
						dictionary2[firstBlockParent] = 0;
					}
                    //为每一个块节点字典赋予文本最长的节点的层级
                    //TODO:是否应该为ElementNodeMeaningFulCharsLength？
					if (dictionary[firstBlockParent] < textNodeMeaningFulCharsLength)
					{
						dictionary[firstBlockParent] = textNodeMeaningFulCharsLength;
						dictionary2[firstBlockParent] = node.Level;
					}
				}
			}

            //对于每个节点，将层级信息修正为同一父节点下文本最长的节点的层级
			foreach (WrappedNode node in data.TextList)
			{
				textNodeMeaningFulCharsLength = node.TextNodeMeaningFulCharsLength;
				firstBlockParent = node.FirstBlockParent;
				if ((firstBlockParent != null) && (textNodeMeaningFulCharsLength > 0))
				{
					node.Level = dictionary2[firstBlockParent];
				}
			}
		}

        /// <summary>
        /// 信息抽取器配置信息
        /// </summary>
		public class AlgoConfig
        {
            #region Constructor

			public AlgoConfig()
			{
				this.EnableHighPrecision = true;
				this.EnableLowPrecisionWarning = true;
				this.EnableParagraghCollaption = true;
				this.EnableImageExtraction = true;
				this.EnableNextPageLinkExtraction = true;
				this.EnableConfidenceFeatureExtraction = true;
				this.EnableShortParagraph = true;
            }

            #endregion

            #region Attribute

            /// <summary>
            /// 开启高精度
            /// </summary>
            public bool EnableHighPrecision { get; set; }

            /// <summary>
            /// 开启图片抽取
            /// </summary>
            public bool EnableImageExtraction { get; set; }

            /// <summary>
            /// 开启下一页链接抽取
            /// </summary>
            public bool EnableNextPageLinkExtraction { get; set; }

            /// <summary>
            /// 开启短段落
            /// </summary>
            public bool EnableShortParagraph { get; set; }

            /// <summary>
            /// 开启关键特征抽取
            /// </summary>
            internal bool EnableConfidenceFeatureExtraction { get; set; }

            /// <summary>
            /// 开启低精度预警
            /// </summary>
            internal bool EnableLowPrecisionWarning { get; set; }

            /// <summary>
            /// 开启段落合并
            /// </summary>
            internal bool EnableParagraghCollaption { get; set; }

            #endregion
		}

		public class Cluster
		{
			public Cluster()
			{
				this.Level = -1;
				this.Nodes = new List<WrappedNode>();
				this.TextLength = 0;
				this.ParagraphNumber = 0;
				this.MaxHx = 0;
				this.MinTextIndex = -1;
				this.MaxTextIndex = -1;
				this.PunctualNumberSum = 0;
				this.SubTitleTextIndex = -1;
				this.TextLengthBak = -1;
				this.PositionScore = 0.0;
				this.BlockScore = 0.0;
				this.TitleNode = null;
			}

			public double BlockScore { get; set; }

			public int Level { get; set; }

			public double LinkScore { get; set; }

			public int MainTitleTextIndex { get; set; }

			public int MaxHx { get; set; }

			public int MaxTextIndex { get; set; }

			public int MinTextIndex { get; set; }

			public List<WrappedNode> Nodes { get; set; }

			public int ParagraphNumber { get; set; }

			public double PositionScore { get; set; }

			public int PunctualNumberSum { get; set; }

			public int SubTitleTextIndex { get; set; }

			public int TextLength { get; set; }

			public int TextLengthBak { get; set; }

			public WrappedNode TitleNode { get; set; }

            /// <summary>
            /// 更新集群信息
            /// </summary>
            /// <param name="beginTextPos"></param>
            /// <param name="endTextPos"></param>
			public void UpdateClusterFeature(double beginTextPos, double endTextPos)
			{
				double num = this.Nodes.Aggregate(0.0, (current, n) => current + n.MeaningFulCharsPos);
				this.PositionScore = (1.0 * (endTextPos - (num / this.Nodes.Count))) / ((endTextPos - beginTextPos) + 1.0);
				this.BlockScore = (1.0 * this.Nodes.Count)
								/ Math.Max(this.Nodes.Count, (this.Nodes[this.Nodes.Count - 1].BlockIndex - this.Nodes[0].BlockIndex) + 1);
				this.TextLengthBak = this.TextLength;
				this.TextLength = Convert.ToInt32((this.TextLength * this.PositionScore) * this.BlockScore);
				this.LinkScore = this.Nodes[this.Nodes.Count - 1].LinkIndex - this.Nodes[0].LinkIndex;
			}
		}

        /// <summary>
        /// 待分析页面富类型，用于记录更多的字段与信息
        /// </summary>
		public class PendingAnalysisPageExtractedInfo
		{
			public PendingAnalysisPageExtractedInfo(PendingAnalysisPage page)
			{
				this.Page = page;
				this.TextList = null;
				this.NodeList = null;
				this.CommonParentOfContentNodes = null;
				this.ContentNodes = null;
				this.ImageNodes = new List<WrappedNode>();
				this.ImageCaptionNodes = new List<WrappedNode>();
				this.ImageSubCaptionNodes = new List<WrappedNode>();
				this.PlainTextLength = 0;
				this.NextPageLink = string.Empty;
				this.TextNodesInBiggestCluster = null;
				this.ExtractionResultFeature = null;
			}

			public WrappedNode CommonParentOfContentNodes { get; set; }

            /// <summary>
            /// 正文节点
            /// </summary>
			public List<WrappedNode> ContentNodes { get; set; }

			public ExtractionResultFeature ExtractionResultFeature { get; set; }

			public List<WrappedNode> ImageCaptionNodes { get; set; }

			public List<WrappedNode> ImageNodes { get; set; }

			public List<WrappedNode> ImageSubCaptionNodes { get; set; }

			public List<string> MetaTitles { get; set; }

			public string NextPageLink { get; set; }

			public List<WrappedNode> NodeList { get; set; }

			public List<WrappedNode> OriginalNodeList { get; set; }

			public PendingAnalysisPage Page { get; set; }

			public int PlainTextLength { get; set; }

			public int PlainTextNumber { get; set; }

			public List<WrappedNode> TextList { get; set; }

			public List<WrappedNode> TextNodesInBiggestCluster { get; set; }

			public List<WrappedNode> TitleCandidates { get; set; }
		}

        /// <summary>
        /// 集群最大值集合
        /// </summary>
		protected class BestClusterHelper
		{
            /// <summary>
            /// 所有属性都找到了
            /// </summary>
			public bool AllFound
			{
				get
				{
					return (((this.BiggestSize > -1) && (this.MaxTextLength > -1))
							&& ((this.LargestP > -1) && (this.MaxPunctNumber > -1))) && (this.MaxAverageTextLength > -1.0);
				}
			}

            /// <summary>
            /// 集群中最大节点数
            /// </summary>
			public int BiggestSize
			{
				get
				{
					return (this.BiggestSizeCluster != null) ? this.BiggestSizeCluster.Nodes.Count : -1;
				}
			}

            /// <summary>
            /// 拥有最多节点的集群
            /// </summary>
			public Cluster BiggestSizeCluster { get; set; }

            /// <summary>
            /// 集群中最大段落数
            /// </summary>
			public int LargestP
			{
				get
				{
					return (this.MaxPCluster != null) ? this.MaxPCluster.ParagraphNumber : -1;
				}
			}

            /// <summary>
            /// 集群中最长文本数
            /// </summary>
			public double MaxAverageTextLength
			{
				get
				{
					return (this.MaxTextLengthCluster != null)
								? (this.MaxAverageTextLengthCluster.TextLength / (1.0 * this.MaxAverageTextLengthCluster.Nodes.Count))
								: -1.0;
				}
			}

            /// <summary>
            /// 拥有最长平均文本的集群
            /// </summary>
			public Cluster MaxAverageTextLengthCluster { get; set; }

            /// <summary>
            /// 拥有最大段落数的集群
            /// </summary>
			public Cluster MaxPCluster { get; set; }

            /// <summary>
            /// 集群中最大标点数
            /// </summary>
			public int MaxPunctNumber
			{
				get
				{
					return (this.MaxPunctNumberCluster != null) ? this.MaxPunctNumberCluster.PunctualNumberSum : -1;
				}
			}

            /// <summary>
            /// 拥有最多标点的集群
            /// </summary>
			public Cluster MaxPunctNumberCluster { get; set; }

            /// <summary>
            /// 集群中最长文本数
            /// </summary>
			public int MaxTextLength
			{
				get
				{
					return (this.MaxTextLengthCluster != null) ? this.MaxTextLengthCluster.TextLength : -1;
				}
			}

            /// <summary>
            /// 集群中拥有最长文本的集群
            /// </summary>
			public Cluster MaxTextLengthCluster { get; set; }
		}

        //这又是做什么的？分别存的什么？
		protected class ClusterCompleteHelper
		{
			public ClusterCompleteHelper()
			{
				this.ExtractedSet = new HashSet<int>();
				this.DoneSet = new HashSet<int>();
				this.CheckedSet = new HashSet<int>();
				this.MainContentIndexSet = new HashSet<int>();
			}

			public HashSet<int> CheckedSet { get; private set; }

			public int CheckedSetCount
			{
				get
				{
					return this.CheckedSet.Count;
				}
			}

			public HashSet<int> DoneSet { get; private set; }

			public HashSet<int> ExtractedSet { get; private set; }

			public int ExtractedSetCount
			{
				get
				{
					return this.ExtractedSet.Count;
				}
			}

			public HashSet<int> MainContentIndexSet { get; private set; }
		}
	}
}