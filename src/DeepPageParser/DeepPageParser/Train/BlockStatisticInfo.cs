using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using DeepPageParser.Dom;
using DeepPageParser.Util;

namespace DeepPageParser
{
	public static class TrainHelper
	{
		public static PageBlockStatisticInfoCollection GetPageBlockStatisticInfoCollection(WrappedNode node)
		{
			PageBlockStatisticInfoCollection collection = new PageBlockStatisticInfoCollection();
			Dictionary<string, BlockStatisticInfo> tmp = new Dictionary<string, BlockStatisticInfo>();
			DocHelper.Travel(
				node,
				delegate(WrappedNode n)
				{
					return true;
				},
				delegate(WrappedNode n)
				{
					if (n.TagName == "div")
					{
						if (string.IsNullOrEmpty(n.Class))
						{
							return true;
						}
						if (tmp.ContainsKey(n.Class))
						{
							tmp[n.Class].Frequence++;
							tmp[n.Class].TotalTextLength += n.InnerText.Length;
						}
						else
						{
							BlockStatisticInfo block = new BlockStatisticInfo();
							block.NodeTag = n.Class;
							block.Frequence = 1;
							block.TotalTextLength = n.InnerText.Length;
							tmp[n.Class] = block;
						}
					}
					return true;
				});
			tmp = tmp.Where(i => i.Value.Frequence > 1).ToDictionary(key => key.Key, value => value.Value);
			collection.BlockStatisticInfos = tmp;
			return collection;
		}
	}

	public class MultiPageBlockStatisticInfoCollection
	{
		public MultiPageBlockStatisticInfoCollection()
		{
			this.PageBlockStatisticInfoCollections = new List<PageBlockStatisticInfoCollection>();
		}

		public List<PageBlockStatisticInfoCollection> PageBlockStatisticInfoCollections { get; set; } 
	}

	public class PageBlockStatisticInfoCollection
	{
		public PageBlockStatisticInfoCollection()
		{
			this.BlockStatisticInfos = new Dictionary<string,BlockStatisticInfo>();
		}

		public Dictionary<string,BlockStatisticInfo> BlockStatisticInfos { get; set; } 
	}

	public class BlockStatisticInfo
	{
		public string NodeTag { get; set; }

		public int TotalTextLength { get; set; }

		public int Frequence { get; set; }

		public bool IsMatch
		{
			get
			{
				return Frequence > 0;
			}
		}

		public override int GetHashCode()
		{
			return this.NodeTag.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType()!=this.GetType())
			{
				return false;
			}
			BlockStatisticInfo other = obj as BlockStatisticInfo;
			if (other.NodeTag == this.NodeTag)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override string ToString()
		{
			return this.Frequence.ToString();
		}
	}
}
