using System;
using System.Collections.Generic;

using DeepPageParser.Dom;
using DeepPageParser.Util;

namespace DeepPageParser
{
	/// <summary>
	///		噪音统计缓存区
	/// </summary>
	public class NoiseStatisticalBuffer
	{
		public NoiseStatisticalBuffer()
		{
			this.SentenceCounter = new Dictionary<int, Counter>();
		}

		/// <summary>
		///		统计文章数
		/// </summary>
		public int NodeCount { get; set; }

		/// <summary>
		///		句子计数器
		/// </summary>
		public Dictionary<int, Counter> SentenceCounter { get; set; }

		public void Clear()
		{
			this.NodeCount = 0;
			this.SentenceCounter.Clear();
		}

		public class Counter
		{
			public int Value { get; set; }
		}

		public void AddNode(WrappedNode node)
		{
			int childrenCount = node.ChildrenCount;
			string className = node.Class;
			string idName = node.Id;
			List<WrappedNode> nodes = new List<WrappedNode>();
			DocHelper.Travel(
				node,
				delegate(WrappedNode wrappedNode)
				{
					if (wrappedNode.TextLen == 0)
					{
						return false;
					}
					if (wrappedNode.ChildrenCount == childrenCount && wrappedNode.Class == className && wrappedNode.Id == idName)
					{
						return true;
					}
					WrappedNode parent = wrappedNode.Parent;
					if (parent != null && parent.ChildrenCount == childrenCount && parent.Class == className && parent.Id == idName)
					{
						return true;
					}
					return false;
				},
				delegate(WrappedNode wrappedNode)
				{
					nodes.Add(wrappedNode);

					return true;
				});

			if (nodes.Count > 3)
			{
				UpdateSentenceCounter(nodes[0]);
				UpdateSentenceCounter(nodes[nodes.Count - 2]);
				this.NodeCount++;
			}
		}

		private void UpdateSentenceCounter(WrappedNode node)
		{
			//int sentenceHashcode = node.InnerText.GetHashCode();
			//if (this.SentenceCounter.ContainsKey(sentenceHashcode))
			//{
			//	this.SentenceCounter[sentenceHashcode].Value++;
			//}
			//else
			//{
			//	this.SentenceCounter[sentenceHashcode] = new Counter() { Value = 1 };
			//}
			DocHelper.Travel(
				node,
				delegate(WrappedNode wrappedNode)
				{
					if (wrappedNode.TextLen == 0)
					{
						return false;
					}

					return true;
				},
				delegate(WrappedNode wrappedNode)
				{
					if (wrappedNode.Children == null || wrappedNode.ChildrenCount == 0)
					{
						int sentenceHashcode = wrappedNode.InnerText.GetHashCode();
						if (this.SentenceCounter.ContainsKey(sentenceHashcode))
						{
							this.SentenceCounter[sentenceHashcode].Value++;
						}
						else
						{
							this.SentenceCounter[sentenceHashcode] = new Counter() { Value = 1 };
						}
					}
					return true;
				});
		}

		/// <summary>
		///		更新噪音句集合
		/// </summary>
		/// <returns></returns>
		public Noise UpdateNoise()
		{
			Noise noises = new Noise();
			foreach (KeyValuePair<int, Counter> dictionaryEntry in this.SentenceCounter)
			{
				int sentenceCount = dictionaryEntry.Value.Value;
				if (sentenceCount == this.NodeCount)
				{
					noises.Add(dictionaryEntry.Key);
				}
			}
			return noises;
		}

		/// <summary>
		///		更新噪音句集合
		/// </summary>
		/// <returns></returns>
		public Noise UpdateNoise(Noise olderOne)
		{
			Noise noises = olderOne;
			foreach (KeyValuePair<int, Counter> dictionaryEntry in this.SentenceCounter)
			{
				int sentenceCount = dictionaryEntry.Value.Value;
				if (sentenceCount == this.NodeCount)
				{
					noises.Add(dictionaryEntry.Key);
				}
			}
			return noises;
		}
	}
}