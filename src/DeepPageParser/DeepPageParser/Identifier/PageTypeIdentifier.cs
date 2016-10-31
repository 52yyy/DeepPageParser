using System;
using System.Collections.Generic;
using System.Linq;

using DeepPageParser;
using DeepPageParser.Classifier;
using DeepPageParser.Dom;
using DeepPageParser.Util;

namespace DeepPageParser
{
	/// <summary>
	///     页面类型识别抽象类
	/// </summary>
	public class PageTypeIdentifier
	{
		public PageTypeIdentifier()
		{
			this.Config = new AlgoConfig();
		}

		private AlgoConfig Config { get; set; }

		public static bool IsNodeMatchCluster(WrappedNode node, Cluster cluster)
		{
			return ((node.PunctualNumber > 0) && (node.Level == cluster.Level))
					&& (((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 12)
						|| ((((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 0x18)
							&& (node.FirstBlockParent != null)) && (node.FirstBlockParent.TagName == "p")));
		}

		private ArticleFeature GetArticleFeature(PendingAnalysisPageExtractedInfo data)
		{
			var feature = new ArticleFeature { IsUrlSafe = ArticleChecker.IsSafeUrl(data.Page.Url) ? 1 : 0 };
			this.ReIndex(data);
			this.GenerateTextFeature(data);
			int lastTextIndex = data.TextList.Count - 1;
			List<Cluster> source = this.CreateTextClusters(data, 0, lastTextIndex, null);
			if (source.Count <= 0)
			{
				feature.MaxClusterNodesCount = 0;
				return feature;
			}

			source.Sort((a, b) => (-a.TextLengthBak + b.TextLengthBak));
			feature.MaxClusterBlockScore = source[0].BlockScore;
			feature.MaxClusterLength = source[0].TextLengthBak;
			feature.MaxClusterNodesCount = source[0].Nodes.Count;
			feature.MaxClusterParagraphNumber = source[0].ParagraphNumber;
			feature.MaxClusterPosScore = source[0].PositionScore;
			feature.MaxClusterPunctualNumber = source[0].PunctualNumberSum;
			int num2 = source.Count - 1;
			feature.MinClusterLength = source[num2].TextLengthBak;
			feature.MinClusterNodesCount = source[num2].Nodes.Count;
			int num3 = (source.Count > 1) ? 1 : 0;
			feature.CountSecondToFirst = source[0].Nodes.Count - source[num3].Nodes.Count;
			feature.LengthSecondToFirst = source[0].TextLengthBak - source[num3].TextLengthBak;
			feature.AvgClusterNodesCount = source.Average(x => x.Nodes.Count);
			feature.AvgClusterNodesLength = source.Average(x => x.TextLengthBak);
			Cluster cluster = source[0];
			source.Sort((a, b) => (-a.ParagraphNumber + b.ParagraphNumber));
			Cluster cluster2 = source[0];
			source.Sort((a, b) => (-a.PunctualNumberSum + b.PunctualNumberSum));
			Cluster cluster3 = source[0];
			feature.LengthMaxParagraphToMaxLength = Math.Abs(cluster2.TextLengthBak - cluster.TextLengthBak);
			feature.LengthMaxPunctualToMaxLength = Math.Abs(cluster3.TextLengthBak - cluster.TextLengthBak);
			feature.LengthMaxPunctualToMaxParagraph = Math.Abs(cluster2.TextLengthBak - cluster3.TextLengthBak);
			double num4 = source.Sum(x => x.TextLengthBak);
			feature.MaxClusterPlainTextRatio = (1.0 * cluster.TextLengthBak) / data.PlainTextLength;
			feature.MaxClusterToAllClustersPlainTextRatio = (1.0 * cluster.TextLengthBak) / num4;
			feature.MaxClusterLinkScore = cluster.LinkScore;
			feature.MaxClusterAvgLength = cluster.Nodes.Average(x => x.TextLen);
			feature.MaxClusterAvgDistance = -1.0;
			feature.MaxClusterMaxDistance = -1;
			double num5 = 0.0;
			for (int i = 0; i < (cluster.Nodes.Count - 1); i++)
			{
				int num7 = cluster.Nodes[i + 1].TextIndex - cluster.Nodes[i].TextIndex;
				feature.MaxClusterMaxDistance = Math.Max(feature.MaxClusterMaxDistance, num7);
				num5 += num7;
			}

			if (cluster.Nodes.Count > 1)
			{
				feature.MaxClusterAvgDistance = num5 / (cluster.Nodes.Count - 1);
			}

			return feature;
		}

		/// <summary>
		///     识别页面类别
		/// </summary>
		/// <param name="pendingAnalysisPage"></param>
		/// <returns></returns>
		public PageType Identify(PendingAnalysisPage pendingAnalysisPage)
		{
			var pageExtractedInfo = new PendingAnalysisPageExtractedInfo(pendingAnalysisPage);
			if (pageExtractedInfo != null)
			{
				// 计算页面的特征项，并判断页面是不是内容页
				if (this.Config.EnableArticleClassifier && this.GetArticleFeature(pageExtractedInfo).IsNotArticle())
				{
					pendingAnalysisPage.Warning = "This is likely not an article.";
					return NonContentPageType.Catalog;
				}

				if (ListPageCheckHelper.CheckIsListPage(pendingAnalysisPage.Document))
				{
					pendingAnalysisPage.Warning = "This is likely not an article in list page checker.";
					return NonContentPageType.Catalog;
				}
				return ContentPageType.News;
			}
			return NonContentPageType.Unknown;
		}

		private List<Cluster> CreateTextClusters(
			PendingAnalysisPageExtractedInfo data,
			int firstTextIndex,
			int lastTextIndex,
			WrappedNode title)
		{
			var source = new List<Cluster>();
			for (int i = firstTextIndex + 1; i <= lastTextIndex; i++)
			{
				Func<Cluster, bool> predicate = null;
				WrappedNode node = data.TextList[i];
				if ((node.Text.Length > 0) && node.IsNodeVisible)
				{
					bool flag = false;
					if (this.Config.EnableShortParagraph)
					{
						flag = (node.Text.Length <= 50) && !node.Text.TrimEnd(new char[0]).EndsWith("。");
					}
					else
					{
						flag = node.Text.Length <= 50;
					}

					if ((!flag && (node.Atag == 0)) && (node.PunctualNumber >= 1))
					{
						if (predicate == null)
						{
							predicate = cluster => IsNodeMatchCluster(node, cluster);
						}

						Cluster item = source.FirstOrDefault(predicate);
						if (item == null)
						{
							if ((title != null) && Condition.IsNodeFarFromTitle(node, title, data.PlainTextLength))
							{
								continue;
							}

							item = new Cluster { Level = node.Level, MinTextIndex = i };
							source.Add(item);
						}

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

		private void GenerateTextFeature(PendingAnalysisPageExtractedInfo data)
		{
			int level = 0;
			int atag = 0;
			int plainTextIndex = 0;
			int plainTextPos = 0;
			DocHelper.Travel(
				data.Page.Body,
				delegate(WrappedNode node)
				{
					node.Level = level;
					if (!StringHelper.NoLevelTags.Contains(node.TagName))
					{
						level++;
					}

					if (node.TagName == "a")
					{
						atag++;
					}

					node.Hx = StringHelper.DefaultFontSize.ContainsKey(node.TagName)
								? StringHelper.DefaultFontSize[node.TagName]
								: StringHelper.DefaultFontSize["other"];
					if ((node.Hx == 0) && (node.Parent != null))
					{
						node.Hx = node.Parent.Hx;
					}

					return true;
				},
				delegate(WrappedNode node)
				{
					if (node.TagName == "a")
					{
						atag--;
					}

					if (!StringHelper.NoLevelTags.Contains(node.TagName))
					{
						level--;
					}

					node.Atag = atag;
					node.PlainTextPos = plainTextPos;
					node.PlainTextIndex = plainTextIndex;
					if (((node.Atag == 0) && node.IsTextNode) && (node.TextNodeMeaningFulCharsLength > 0))
					{
						plainTextPos += node.TextNodeMeaningFulCharsLength;
						plainTextIndex++;
					}

					return true;
				});
			data.PlainTextLength = plainTextPos;
			data.PlainTextNumber = plainTextIndex;
		}

		private void ReIndex(PendingAnalysisPageExtractedInfo data)
		{
			data.TextList = new List<WrappedNode>();
			data.NodeList = new List<WrappedNode>();
			int index = 0;
			int textIndex = 0;
			int meaningfulcharPos = 0;
			int blockIndex = 0;
			int linkIndex = 0;
			DocHelper.Travel(
				data.Page.Body,
				delegate(WrappedNode node)
				{
					node.Index = index;
					node.BlockIndex = blockIndex;
					node.LinkIndex = linkIndex;
					data.NodeList.Add(node);
					index++;
					if (StringHelper.IsBlockTag(node.TagName))
					{
						blockIndex++;
					}

					if (node.TagName == "a")
					{
						linkIndex++;
					}

					if (node.IsTextNode)
					{
						node.MeaningFulCharsPos = meaningfulcharPos;
						if (node.TextNodeMeaningFulCharsLength > 0)
						{
							meaningfulcharPos += node.TextNodeMeaningFulCharsLength;
							node.TextIndex = textIndex;
							data.TextList.Add(node);
							textIndex++;
						}

						node.ElementNodeMeaningFulCharsLength = node.TextNodeMeaningFulCharsLength;
					}

					return true;
				},
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

		public class AlgoConfig
		{
			public AlgoConfig()
			{
				this.EnableArticleClassifier = true;
				this.EnableShortParagraph = true;
			}

			public bool EnableArticleClassifier { get; set; }

			public bool EnableShortParagraph { get; set; }
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

		public class PendingAnalysisPageExtractedInfo
		{
			public PendingAnalysisPageExtractedInfo(PendingAnalysisPage page)
			{
				this.Page = page;
				this.TextList = null;
				this.NodeList = null;
				this.PlainTextLength = 0;
			}

			public List<WrappedNode> NodeList { get; set; }

			public PendingAnalysisPage Page { get; set; }

			public int PlainTextLength { get; set; }

			public int PlainTextNumber { get; set; }

			public List<WrappedNode> TextList { get; set; }
		}
	}
}