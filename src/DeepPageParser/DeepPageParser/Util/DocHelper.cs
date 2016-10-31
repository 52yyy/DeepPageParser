using System;
using System.Collections.Generic;
using System.Linq;

using DeepPageParser.Dom;

using HtmlAgilityPack;

namespace DeepPageParser.Util
{
    internal static class DocHelper
    {
        private static readonly HashSet<string> nltags = new HashSet<string> { "div", "p", "td", "br", "h1", "h2", "h3" };

        internal delegate bool ActionOnRawNode(HtmlNode node);

        internal delegate bool ActionOnWrappedNode(WrappedNode node);

        internal delegate int FilterFunc(WrappedNode node);

        public static WrappedNode CleanMainBlock(WrappedNode mainBlock)
        {
            List<WrappedNode> divList = GetDivList(mainBlock);
            HashSet<WrappedNode> nodesToTrim = new HashSet<WrappedNode>();
            foreach (WrappedNode node in divList)
            {
                if (IsNavBlock(node))
                {
                    nodesToTrim.Add(node);
                }
            }

            mainBlock = TrimByNodes(mainBlock, nodesToTrim);
            return mainBlock;
        }

        public static HtmlNode Clone(WrappedNode inputNode)
        {
            HtmlNode root = null;
            Travel(inputNode, null, delegate (WrappedNode n) {
                HtmlNode node = CloneNode(n);
                if (n.Children.Count > 0)
                {
                    foreach (WrappedNode node2 in n.Children)
                    {
                        node.AppendChild(node2.ClonedRawNode);
                    }
                }

                n.ClonedRawNode = node;
                root = node;
                return true;
            });
            return root;
        }

        public static HtmlNode CloneNode(WrappedNode node)
        {
            return node.RawNode.CloneNode(false);
        }

        public static WrappedNode CommonRoot(WrappedNode block, FilterFunc filter)
        {
            int num = filter(block);
            WrappedNode node = null;
            switch (num)
            {
                case 1:
                    return block;

                case -1:
                    return null;
            }

            int num2 = 0;
            int count = block.Children.Count;
            while (num2 < count)
            {
                WrappedNode node2 = CommonRoot(block.Children[num2], filter);
                if (node2 != null)
                {
                    if (node != null)
                    {
                        return block;
                    }

                    node = node2;
                }

                num2++;
            }

            return node;
        }

        public static WrappedNode CommonRootOf2Nodes(WrappedNode a, WrappedNode b)
        {
            HashSet<int> set = new HashSet<int>();
            while (a != null)
            {
                set.Add(a.Index);
                a = a.Parent;
            }

            while (!set.Contains(b.Index))
            {
                b = b.Parent;
            }

            return b;
        }

        /// <summary>
        /// 计算各类标签个数
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="tagCount"></param>
        public static void CountTags(List<WrappedNode> nodes, ref Dictionary<string, int> tagCount)
        {
            tagCount.Clear();
            foreach (WrappedNode node in nodes)
            {
                string tagName = node.TagName;
                if (tagCount.ContainsKey(tagName))
                {
                    Dictionary<string, int> dictionary;
                    string str2;
                    (dictionary = tagCount)[str2 = tagName] = dictionary[str2] + 1;
                }
                else
                {
                    tagCount[tagName] = 1;
                }
            }
        }

        /// <summary>
        /// 找到距离最近的块标签父节点
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns>父节点</returns>
        public static WrappedNode FirstBlockParent(WrappedNode node)
        {
            if (node != null)
            {
                while (node.Parent != null)
                {
                    //TODO:改变了node内容？
                    node = node.Parent;
                    string tagName = node.TagName;
                    if (!string.IsNullOrEmpty(tagName) && StringHelper.IsBlockTag(tagName))
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        public static int FirstIndex(WrappedNode block)
        {
            return block.Index;
        }

        public static WrappedNode GetCompoundImage(WrappedNode image)
        {
            WrappedNode node = image;
            for (WrappedNode node2 = image.Parent; node2 != null; node2 = node.Parent)
            {
                if (node2.ElementNodeMeaningFulCharsLength > 0)
                {
                    break;
                }

                node = node2;
            }

            if (NumOfImages(node) > 1)
            {
                return node;
            }

            return null;
        }

        public static HashSet<WrappedNode> GetDateBlock(WrappedNode root)
        {
            WrappedNode current;
            int num;
            HashSet<WrappedNode> set = new HashSet<WrappedNode>();
            List<WrappedNode> list = new List<WrappedNode>();
            string curLine = string.Empty;
            Travel(root, delegate (WrappedNode node) {
                if (node == null)
                {
                    return false;
                }

                if ((node.ResultType == ResultType.Title) || (node.ResultType == ResultType.SubTitle))
                {
                    return false;
                }

                if ((node.NodeType == HtmlNodeType.Text) && !string.IsNullOrEmpty(node.InnerText))
                {
                    curLine = curLine + node.InnerText;
                }

                if ((node.NodeType != HtmlNodeType.Element) && (node.NodeType != HtmlNodeType.Document))
                {
                    return false;
                }

                if (node.TagName == "script")
                {
                    return false;
                }

                return true;
            }, delegate (WrappedNode node) {
                if (!string.IsNullOrEmpty(node.TagName) && nltags.Contains(node.TagName))
                {
                    curLine = StringHelper.TrimLeadingSpace(curLine);
                    if (curLine.Length != 0)
                    {
                        list.Add(node);
                    }

                    curLine = string.Empty;
                }

                return true;
            });
            using (List<WrappedNode>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    if (current.NavBlockInfo.InnerTextLen > 50)
                    {
                        goto Label_00C9;
                    }

                    if (Condition.IsDateNode(current))
                    {
                        set.Add(current);
                    }
                    else if (Condition.IsLinkNode(current))
                    {
                        set.Add(current);
                        continue;
                    }
                }
            }

        Label_00C9:
            num = list.Count - 1;
            while (num >= 0)
            {
                current = list[num];
                if (current.NavBlockInfo.InnerTextLen > 50)
                {
                    return set;
                }

                if (Condition.IsDateNode(current))
                {
                    set.Add(current);
                }
                else if (Condition.IsLinkNode(current))
                {
                    set.Add(current);
                }

                num--;
            }

            return set;
        }

        public static WrappedNode GetMainBlock(WrappedNode body, List<WrappedNode> nodes)
        {
            if ((nodes != null) && (nodes.Count > 1))
            {
                Dictionary<WrappedNode, HashSet<WrappedNode>> dictionary = new Dictionary<WrappedNode, HashSet<WrappedNode>>();
                for (int i = 0; i < (nodes.Count - 1); i++)
                {
                    WrappedNode key = CommonRootOf2Nodes(nodes[i], nodes[i + 1]);
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = new HashSet<WrappedNode>();
                    }

                    dictionary[key].Add(nodes[i]);
                    dictionary[key].Add(nodes[i + 1]);
                }

                KeyValuePair<WrappedNode, HashSet<WrappedNode>> pair = new KeyValuePair<WrappedNode, HashSet<WrappedNode>>();
                foreach (KeyValuePair<WrappedNode, HashSet<WrappedNode>> pair2 in dictionary)
                {
                    if (pair.Key == null)
                    {
                        pair = pair2;
                    }
                    else if (pair2.Value.Count > pair.Value.Count)
                    {
                        pair = pair2;
                    }
                }

                List<WrappedNode> list = new List<WrappedNode>();
                int num2 = 0;
                foreach (KeyValuePair<WrappedNode, HashSet<WrappedNode>> pair2 in dictionary)
                {
                    if (pair2.Key != pair.Key)
                    {
                        if (IsChild(pair2.Key, pair.Key))
                        {
                            list.Add(pair2.Key);
                        }
                        else
                        {
                            num2 += pair2.Value.Count;
                        }
                    }
                }

                foreach (WrappedNode node2 in list)
                {
                    HashSet<WrappedNode> set = dictionary[node2];
                    foreach (WrappedNode node3 in set)
                    {
                        pair.Value.Add(node3);
                    }

                    dictionary.Remove(node2);
                }

                if (((pair.Value.Count > 4) && (pair.Value.Count > (num2 * 3))) && (body.NavBlockInfo.InnerTextLen < (pair.Key.NavBlockInfo.InnerTextLen * 2)))
                {
                    return pair.Key;
                }
            }

            return null;
        }

        public static double GetMainBlockRatio(WrappedNode mb, List<WrappedNode> textNodes)
        {
            WrappedNode mainBlock = GetMainBlock(mb, textNodes);
            if (mainBlock != null)
            {
                double extracted = 0.0;
                double main = 0.0;
                Travel(mb, delegate (WrappedNode node) {
                    if (node.IsTextNode && (node.Atag <= 0))
                    {
                        extracted += node.InnerText.Length;
                    }

                    return true;
                }, null);
                TravelInOrginalNodes(mainBlock, delegate (WrappedNode node) {
                    if (node.IsTextNode && (node.Atag <= 0))
                    {
                        main += node.InnerText.Length;
                    }

                    return true;
                }, null);
                return extracted / main;
            }

            return 1.0;
        }

        public static HashSet<WrappedNode> GetNodesOutsideMainDivToRemove(List<WrappedNode> divList, WrappedNode mainDiv, ref HashSet<WrappedNode> okDiv)
        {
            List<WrappedNode> list = (from div in divList
                where !IsChild(div, mainDiv)
                select div).ToList<WrappedNode>();
            HashSet<WrappedNode> set = new HashSet<WrappedNode>();
            foreach (WrappedNode node in list)
            {
                if (((node.NavBlockInfo.ImgNum > 0) && (node.NavBlockInfo.LinkTextLen < 5)) || ((node.NavBlockInfo.LinkTextLen * 3) < node.NavBlockInfo.InnerTextLen))
                {
                    okDiv.Add(node);
                }
                else if (IsChild(mainDiv, node) || ((!Condition.IsLinkNode(node) && ((node.NavBlockInfo.ParagraphNumber >= 1) || (node.NavBlockInfo.InnerTextLen >= 30))) && (((node.NavBlockInfo.InnerTextLen / Math.Max(node.NavBlockInfo.NodeNum - (2 * node.NavBlockInfo.ImgNum), 1)) >= 10) && (!Condition.IsDateNode(node) || (node.NavBlockInfo.InnerTextLen >= 100)))))
                {
                    okDiv.Add(node);
                }
            }

            foreach (WrappedNode node in list)
            {
                if (!okDiv.Contains(node))
                {
                    set.Add(node);
                }
            }

            return set;
        }

        /// <summary>
        /// 深度遍历建立原始节点序列
        /// </summary>
        /// <param name="body">根节点</param>
        /// <returns>节点序列</returns>
        public static List<WrappedNode> GetOriginalList(WrappedNode body)
        {
            List<WrappedNode> list = new List<WrappedNode>();
            int index = 0;
            //遍历树，并建立索引
            Travel(body, delegate (WrappedNode node) {
                node.OriginalIndex = index;
                index++;
                list.Add(node);
                return true;
            }, null);
            return list;
        }

        public static List<WrappedNode> GetTailNodesToRemove(List<WrappedNode> divList, WrappedNode mainDiv)
        {
            List<WrappedNode> list = new List<WrappedNode>();
            for (int i = divList.Count - 1; i >= 0; i--)
            {
                WrappedNode item = divList[i];
                if (item == mainDiv)
                {
                    return list;
                }

                if ((item.NavBlockInfo.ImgNum <= 0) && (item.NavBlockInfo.LinkTextLen > 0))
                {
                    if ((item.NavBlockInfo.ParagraphNumber < 1) && (item.NavBlockInfo.InnerTextLen < 0x19))
                    {
                        list.Add(item);
                    }

                    if ((item.NavBlockInfo.InnerTextLen / item.NavBlockInfo.NodeNum) < 10)
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 从HTML中的base节点获取url
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetUrlFromBaseTag(HtmlDocument doc)
        {
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("/base[1]");
            if ((nodes != null) && (nodes.Count > 0))
            {
                return NodeHelper.GetAttribute(nodes[0], "href");
            }

            return string.Empty;
        }

        /// <summary>
        /// 提高精度
        /// </summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public static WrappedNode ImprovePrecision(WrappedNode mb)
        {
            if (mb == null)
            {
                return null;
            }

            WrappedNode resulBodyRoot = GetResulBodyRoot(mb);
            List<WrappedNode> divList = GetDivList(resulBodyRoot);
            WrappedNode resultMainDiv = GetResultMainDiv(resulBodyRoot, divList);
            HashSet<WrappedNode> nodesOutsideMainDivToRemove = new HashSet<WrappedNode>();
            List<WrappedNode> tailNodesToRemove = new List<WrappedNode>();
            if (resultMainDiv != null)
            {
                HashSet<WrappedNode> okDiv = new HashSet<WrappedNode> {
                    resultMainDiv
                };
                tailNodesToRemove = GetTailNodesToRemove(divList, resultMainDiv);
                nodesOutsideMainDivToRemove = GetNodesOutsideMainDivToRemove(divList, resultMainDiv, ref okDiv);
            }

            HashSet<WrappedNode> dateBlock = GetDateBlock(mb);
            mb = SimplifyExtractionResult(mb, nodesOutsideMainDivToRemove, tailNodesToRemove, dateBlock);
            return mb;
        }

        public static bool IsChild(WrappedNode child, WrappedNode parent)
        {
            if (parent != null)
            {
                while (child != null)
                {
                    if (child == parent)
                    {
                        return true;
                    }

                    child = child.Parent;
                }
            }

            return false;
        }

        public static bool IsNavBlock(WrappedNode node)
        {
            if (node.NavBlockInfo.LinkNum > 0)
            {
                if (node.NavBlockInfo.ImgNum <= 0)
                {
                    if (node.NavBlockInfo.InnerHtmlLength > (node.NavBlockInfo.InnerTextLen * 8))
                    {
                        return true;
                    }

                    if ((node.NavBlockInfo.NodeNum > 5) && (node.NavBlockInfo.InnerTextLen < (node.NavBlockInfo.NodeNum * 5)))
                    {
                        return true;
                    }

                    if ((node.NavBlockInfo.NodeNum > 10) && (node.NavBlockInfo.InnerTextLen < (node.NavBlockInfo.NodeNum * 10)))
                    {
                        return true;
                    }
                }
                else if (((node.NavBlockInfo.LinkTextLen <= 0) && ((node.NavBlockInfo.LinkTextLen <= 0) || (node.NavBlockInfo.InnerHtmlLength > (node.NavBlockInfo.InnerTextLen * 8)))) || (node.NavBlockInfo.NodeNum < 5))
                {
                    return true;
                }
            }

            return false;
        }

        public static int LastIndex(WrappedNode block)
        {
            while (block.Children.Count > 0)
            {
                block = block.Children[block.Children.Count - 1];
            }

            return block.Index;
        }

        /// <summary>
        /// 合并子节点，主要为将内部文本信息组合成为新的文本节点，并替换原有的子节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="doc"></param>
        public static void MergeChildrenNode(WrappedNode node, HtmlDocument doc)
        {
            WrappedNode node3 = new WrappedNode(doc.CreateTextNode(node.InnerText));
            node.ChildrenBak = new List<WrappedNode>();
            node.ChildrenBak.AddRange(node.Children);
            node.Children = new List<WrappedNode> { node3 };
            node3.Parent = node;
        }

        /// <summary>
        /// 判断该节点，是否可以合并里面的链接
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="doc">HtmlDocument信息</param>
        /// <returns></returns>
        public static bool CanMergeChildrenLinkNode(WrappedNode node, HtmlDocument doc)
        {
            bool canmerge = false;
            int linkcount = 0;
            int linkcontent = 0;
            int linkchildcount = 0;
            if (node.Parent.TagName == "p")
            {
                foreach (WrappedNode chilNode in node.Children)
                {
                    if (chilNode.TagName=="a")
                    {
                        linkcount = linkcount + 1;
                        linkcontent = linkcontent + chilNode.InnerText.Length;
                        linkchildcount = linkchildcount + chilNode.Children.Count;
                    }
                }
                //所有的a都是一级标签，节点中文本的字大于链接中文本的字10个（经验），文本中链接个数小于5个
                if (linkchildcount == linkcount&&node.InnerText.Length-10>linkcontent&&linkcount<5)
                {
                    canmerge = true;
                }
                //
            }
            return canmerge;

        }
        /// <summary>
        /// 合并p节点中的链接
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="doc">HtmlDocument信息</param>
        /// <returns></returns>
        public static void MergeChildrenLinkNode(WrappedNode node, HtmlDocument doc)
        {
            
            WrappedNode nodeparent = node.Parent; //取 a的父亲节点。 a的父亲节点，如p。变成了新的p" // 将a与之前或之后的节点拼接
            if (CanMergeChildrenLinkNode(nodeparent,doc))
            {
                WrappedNode node3 = new WrappedNode(doc.CreateTextNode(nodeparent.InnerText));
                if (nodeparent.ChildrenBak != null) //只备份一次，如果一个段落中有多个带链接的文字
                {
                    nodeparent.ChildrenBak = new List<WrappedNode>();
                    nodeparent.ChildrenBak.AddRange(nodeparent.Children);
                }
                nodeparent.Children = new List<WrappedNode> { node3 };
                node3.Parent = nodeparent;
            }

        }

        //把block标签（如：p）下的a标签合并到bolck（如：p)下面。
        public static WrappedNode MergeLinkInBlock(WrappedNode root, HtmlDocument doc)
        {
            //node.Children.Count  才是真实的子节点数，node.ChildrenCount是trim后的。
            Travel(root, delegate(WrappedNode node)
            {
                if (node.TagName == "a" && node.Children.Count == 1 && node.Parent.TagName == "p") //现在只支持p，如果支持block标签用： StringHelper.IsBlockTag(node.Parent.TagName，感觉支持block标签可能有问题。
                {
                    MergeChildrenLinkNode(node, doc);
                    return false;
                }
                return true;
            }, null);
            return root;
        }

        /// <summary>
        /// 合并列表节点和预留文本节点的内容信息
        /// </summary>
        /// <param name="root">根节点</param>
        /// <param name="doc">HtmlDocument信息</param>
        /// <returns></returns>
        public static WrappedNode MergeTextChildren(WrappedNode root, HtmlDocument doc)
        {
            int blockIndex = 0;
            int linkIndex = 0;
            //递推遍历，将节点的块索引和链接索引赋值，如果发现块索引和链接，则相应的索引值递增加一
            Travel(root, delegate (WrappedNode node) {
                node.BlockIndex = blockIndex;
                node.LinkIndex = linkIndex;
                if (StringHelper.IsBlockTag(node.TagName))
                {
                    blockIndex++;
                }

                if (node.TagName == "a")
                {
                    linkIndex++;
                }

                return true;
            }, null);
            //合并子节点
            Travel(root, delegate (WrappedNode node) {
                WrappedNode lastPreTraversalLeafChild = node.LastPreTraversalLeafChild;
                WrappedNode firstPreTraversalLeafChild = node.FirstPreTraversalLeafChild;
                //如果满足下列条件之一，则合子节点
                //  *  节点为预留字段节点 
                //  *  节点为列表 && 节点不为叶子节点 && （ 该节点下的叶节点之间没有链接节点 || 叶节点之间出现的文本节点数量大于链接节点数量的三倍 ）
                if ((node.TagName == "pre") || ((((node.TagName == "ul") || (node.TagName == "ol")) && (node != lastPreTraversalLeafChild)) && ((firstPreTraversalLeafChild.LinkIndex == lastPreTraversalLeafChild.LinkIndex) || (((lastPreTraversalLeafChild.LinkIndex - firstPreTraversalLeafChild.LinkIndex) * 3) < (lastPreTraversalLeafChild.PlainTextIndex - firstPreTraversalLeafChild.PlainTextIndex)))))
                {
                    //合并子节点，并不再遍历孩子节点
                    MergeChildrenNode(node, doc);
                    return false;
                }

                return true;
            }, null);
            return root;
        }

        public static int NumOfImages(WrappedNode node)
        {
            if (node.TagName == "img")
            {
                return 1;
            }

            return TagsInOriginalDom(node, "img").Count;
        }

        public static string OutputText(HtmlNode root)
        {
            string result = string.Empty;
            bool newline = false;
            HashSet<string> newLineTags = new HashSet<string> { "div", "p", "li", "td", "br", "h1", "h2", "h3" };
            TravelOnRawNode(root, delegate (HtmlNode node) {
                if (node == null)
                {
                    return false;
                }

                if ((node.NodeType == HtmlNodeType.Text) && !string.IsNullOrEmpty(node.InnerText))
                {
                    result = result + node.InnerText;
                    newline = false;
                }

                if (node.NodeType != HtmlNodeType.Element)
                {
                    return false;
                }

                if (node.Name == "script")
                {
                    return false;
                }

                return true;
            }, delegate (HtmlNode node) {
                if ((!string.IsNullOrEmpty(node.Name) && newLineTags.Contains(node.Name)) && !newline)
                {
                    result = result + "\n";
                    newline = true;
                }

                return true;
            });
            return result;
        }

		/// <summary>
		///		把body里面不用的Node去掉
		/// </summary>
		/// <param name="body"></param>
		/// <returns></returns>
        public static HtmlNode RemoveComment(HtmlNode body)
        {
            List<HtmlNode> nonContentNodes = new List<HtmlNode>();
	        TravelOnRawNode(
		        body,
		        delegate(HtmlNode node)
		        {
					// 如果node是Comment且没有parent，或者node是element并且是应该被干掉的tag
					if (((node.NodeType == HtmlNodeType.Comment) && (node.ParentNode != null))
						|| ((node.NodeType == HtmlNodeType.Element) && TrimHelper.IsTrimmedTag(node.Name)))  // 有好些不要的节点名
					{
						nonContentNodes.Add(node);  // 把node放到要干掉的list里
						return false;
					}
					return true;
		        },
		        null);
			// 去掉不要的node
            foreach (HtmlNode node in nonContentNodes)
            {
                node.Remove();
            }

            return body;
        }

	    public static WrappedNode SeparateTextChild(WrappedNode root)
        {
            Travel(root, delegate (WrappedNode node) {
                if ((node.ChildrenBak != null) && (node.ChildrenBak.Count > 0))
                {
                    node.Children = node.ChildrenBak;
                }

                return true;
            }, null);
            return root;
        }

        /// <summary>
        /// 设置标题节点属性
        /// </summary>
        /// <param name="root"></param>
        public static void SetHeadingNode(WrappedNode root)
        {
            //按照标题节点层级将每个文字节点的HeadingNode都赋予距离最近的父标题节点
            Stack<WrappedNode> headingStack = new Stack<WrappedNode>();
            Travel(root, delegate (WrappedNode node) {
                if (node.IsHeadingNode)
                {
                    headingStack.Push(node);
                }
                else if (node.IsTextNode && (headingStack.Count > 0))
                {
                    node.HeadingNode = headingStack.Peek();
                }
                
                return true;
            }, delegate (WrappedNode node) {
                if (node.IsHeadingNode)
                {
                    headingStack.Pop();
                }
                return true;
            });
        }

        /// <summary>
        /// 递推设置节点的可见性属性和中间对其格式属性
        /// </summary>
        /// <param name="body">节点</param>
        /// <returns>修改后的节点</returns>
        public static WrappedNode SetVisibleAndCenter(WrappedNode body)
        {
            //顺序遍历并检查节点可见性属性和中间对齐的属性
            Travel(body, delegate (WrappedNode node) {
                string attribute = NodeHelper.GetAttribute(node.RawNode, "style");
                if (!string.IsNullOrEmpty(attribute) && (((attribute.IndexOf("display", StringComparison.OrdinalIgnoreCase) != -1) && (attribute.IndexOf("none", StringComparison.OrdinalIgnoreCase) != -1)) || ((attribute.IndexOf("visibility", StringComparison.OrdinalIgnoreCase) != -1) && (attribute.IndexOf("hidden", StringComparison.OrdinalIgnoreCase) != -1))))
                {
                    node.IsNodeVisible = false;
                }

                attribute = NodeHelper.GetAttribute(node.RawNode, "text-align");
                if (!string.IsNullOrEmpty(attribute))
                {
                    node.IsCenter = string.Equals(attribute.Trim(), "center", StringComparison.OrdinalIgnoreCase);
                }

                if (node.TagName == "center")
                {
                    node.IsCenter = true;
                }

                if (node.Parent != null)
                {
                    node.IsNodeVisible = node.IsNodeVisible && node.Parent.IsNodeVisible;
                    node.IsCenter = node.IsCenter || node.Parent.IsCenter;
                }

                if (node.IsCenter && (node.NodeType == HtmlNodeType.Element))
                {
                    NodeHelper.SetAttribute(node.RawNode, "isCenter", "true");
                }

                return true;
            }, null);
            return body;
        }

        public static WrappedNode SimplifyExtractionResult(WrappedNode mb, HashSet<WrappedNode> nodesOutsideMainDivToRemove, List<WrappedNode> tailNodesToRemove, HashSet<WrappedNode> dateBlocksToRemove)
        {
            mb = Trim(mb, delegate (WrappedNode node) {
                if (node.Children.Count == 0)
                {
                    return 1;
                }

                if (node.ResultType == ResultType.SubTitle)
                {
                    if ((node.NavBlockInfo.PunctualAndDigitNumber >= 10) && Condition.IsDateNode(node))
                    {
                        return -1;
                    }

                    return 1;
                }

                if (dateBlocksToRemove.Any<WrappedNode>(div => IsChild(node, div)))
                {
                    return -1;
                }

                if (tailNodesToRemove.Any<WrappedNode>(div => IsChild(node, div)))
                {
                    return -1;
                }

                if (nodesOutsideMainDivToRemove.Any<WrappedNode>(div => IsChild(node, div)))
                {
                    return -1;
                }

                return 0;
            });
            return mb;
        }

        public static List<WrappedNode> Tags(WrappedNode root, string tag)
        {
            List<WrappedNode> list = new List<WrappedNode>();
            Travel(root, delegate (WrappedNode node) {
                if (node.TagName == tag)
                {
                    list.Add(node);
                }

                return true;
            }, null);
            return list;
        }

        public static List<WrappedNode> TagsInOriginalDom(WrappedNode root, string tag)
        {
            List<WrappedNode> list = new List<WrappedNode>();
            TravelInOrginalNodes(root, delegate (WrappedNode node) {
                if (node.TagName == tag)
                {
                    list.Add(node);
                }

                return true;
            }, null);
            return list;
        }

        /// <summary>
        /// 递归遍历树节点,其中filter方法为递推调用，visitor方法为递归调用
        /// </summary>
        /// <param name="node">根节点</param>
        /// <param name="filter">过滤方法</param>
        /// <param name="visitor">动作方法</param>
        public static void Travel(WrappedNode node, ActionOnWrappedNode filter, ActionOnWrappedNode visitor)
        {
            if ((node != null) && ((filter == null) || filter(node)))
            {
                if (node.Children != null)
                {
                    foreach (WrappedNode node2 in node.Children)
                    {
                        Travel(node2, filter, visitor);
                    }
                }

                if (visitor != null)
                {
                    visitor(node);
                }
            }
        }

        public static void DfsTravel(WrappedNode node, ActionOnWrappedNode action) 
        {
            int level = 0;													// keeps track of level
            var frontiers = new List<WrappedNode>();									// keeps track of previous levels, i - 1
            var levels = new Dictionary<WrappedNode, int>(node.Children.Count);		// keeps track of visited nodes and their distances
            var parents = new Dictionary<WrappedNode, object>(node.Children.Count);	// keeps track of tree-nodes

            frontiers.Add(node);
            levels.Add(node, 0);
            parents.Add(node, null);

            // BFS VISIT CURRENT NODE
            action(node);

            // TRAVERSE GRAPH
            while (frontiers.Count > 0)
            {
                var next = new List<WrappedNode>();									// keeps track of the current level, i

                foreach (var node2 in frontiers)
                {
                    foreach (var adjacent in node2.Children)
                    {
                        if (!levels.ContainsKey(adjacent)) 				// not visited yet
                        {
                            // BFS VISIT NODE STEP
                            action(adjacent);

                            levels.Add(adjacent, level);					// level[node] + 1
                            parents.Add(adjacent, node2);
                            next.Add(adjacent);
                        }
                    }
                }

                frontiers = next;
                level = level + 1;
            }
        }

        /// <summary>
        /// 递归遍历树原始子节点
        /// </summary>
        /// <param name="node">根节点</param>
        /// <param name="filter">过滤方法</param>
        /// <param name="visitor">动作方法</param>
        public static void TravelInOrginalNodes(WrappedNode node, ActionOnWrappedNode filter, ActionOnWrappedNode visitor)
        {
            // 如果node是空，或者被过滤掉了，那么这个node的所有孩子都不找了
            if ((node != null) && ((filter == null) || filter(node)))
            {
                if (node.OriginalChildren != null)
                {
                    foreach (WrappedNode node2 in node.OriginalChildren)
                    {
                        Travel(node2, filter, visitor);
                    }
                }

                if (visitor != null)
                {
                    visitor(node);
                }
            }
        }

		/// <summary>
		///		遍历整个RawNode，执行相应动作
		/// </summary>
        /// <param name="node">根节点</param>
        /// <param name="filter">过滤方法</param>
        /// <param name="visitor">动作方法</param>
        public static void TravelOnRawNode(HtmlNode node, ActionOnRawNode filter, ActionOnRawNode visitor)
        {
			// 如果node是空，或者被过滤掉了，那么这个node的所有孩子都不找了
            if ((node != null) && ((filter == null) || filter(node)))
            {
                if (node.ChildNodes != null)
                {
                    foreach (HtmlNode node2 in (IEnumerable<HtmlNode>) node.ChildNodes)
                    {
                        TravelOnRawNode(node2, filter, visitor);
                    }
                }

                if (visitor != null)
                {
                    visitor(node);
                }
            }
        }

        /// <summary>
        /// 修剪节点
        /// </summary>
        /// <param name="root">待修剪节点</param>
        /// <param name="trimFunc">修剪节点方法，0为需要继续判断子节点，1为可以留下，-1为需要删除</param>
        /// <returns>修剪后的节点</returns>
        public static WrappedNode Trim(WrappedNode root, FilterFunc trimFunc)
        {
            WrappedNode node = DoTrim(root, trimFunc);
            if (node == null)
            {
                return null;
            }

            //去除冗余嵌套，获取第一个子节点个数不为1的节点
            while (node.Children.Count == 1)
            {
                node = node.Children[0];
            }

            return node;
        }

        public static WrappedNode TrimByNodes(WrappedNode root, HashSet<WrappedNode> nodesToTrim)
        {
            root.ResultType = ResultType.Content;
            Trim(root, delegate (WrappedNode node) {
                if (nodesToTrim.Any<WrappedNode>(div => IsChild(node, div)))
                {
                    return -1;
                }

                if (node.Children.Count == 0)
                {
                    return 1;
                }

                return 0;
            });
            return root;
        }

        /// <summary>
        /// 根据结果类型进行筛选
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static WrappedNode TrimByResultType(WrappedNode root)
        {
            return Trim(root, delegate (WrappedNode node) {
                if (node.ResultType != ResultType.None)
                {
                    if (node.ResultType > ResultType.Title)
                    {
                        return 1;
                    }

                    return -1;
                }

                if (node.Children.Count > 0)
                {
                    return 0;
                }

                return -1;
            });
        }

        public static WrappedNode TrimByTags(WrappedNode root)
        {
            return Trim(root, new FilterFunc(TrimHelper.TrimByTagFunc));
        }

        public static WrappedNode Wrap(HtmlNode rawNode)
        {
            int index = 0;
            int textIndex = 0;
            return Wrap(rawNode, null, ref index, ref textIndex);
        }

        internal static WrappedNode GetResultMainDiv(WrappedNode root, List<WrappedNode> divList)
        {
            WrappedNode node = null;
            foreach (WrappedNode node2 in divList)
            {
                if (node2.NavBlockInfo.InnerTextLen != root.NavBlockInfo.InnerTextLen)
                {
                    if (node == null)
                    {
                        node = node2;
                    }

                    if (node2.NavBlockInfo.InnerTextLen > node.NavBlockInfo.InnerTextLen)
                    {
                        node = node2;
                    }
                }
            }

            if ((node != null) && ((node.NavBlockInfo.InnerTextLen * 2) < root.NavBlockInfo.InnerTextLen))
            {
                node = root;
            }

            return node;
        }

        /// <summary>
        /// 修剪不需要的节点
        /// </summary>
        /// <param name="node">待修剪节点</param>
        /// <param name="trimFunc">判断是否需要修剪委托</param>
        /// <returns>修剪后的节点</returns>
        private static WrappedNode DoTrim(WrappedNode node, FilterFunc trimFunc)
        {
            switch (trimFunc(node))
            {
                case -1:
                    return null;

                case 1:
                    return node;
            }

            WrappedNode node2 = null;
            List<WrappedNode> list = new List<WrappedNode>();
            //递归修剪子节点
            foreach (WrappedNode node3 in node.Children)
            {
                WrappedNode item = DoTrim(node3, trimFunc);
                if (item != null)
                {
                    list.Add(item);
                    node2 = node;
                }
            }

            //一个子节点都没有找到，则返回null
            node.Children = list;
            return node2;
        }

        private static List<WrappedNode> GetDivList(WrappedNode root)
        {
            List<WrappedNode> divList = new List<WrappedNode>();
            Travel(root, delegate (WrappedNode node) {
                                                         if (StringHelper.IsDivTags(node.TagName) && (node.InnerHtml.Length > 0))
                                                         {
                                                             divList.Add(node);
                                                         }

                                                         return true;
            }, null);
            return divList;
        }

        private static WrappedNode GetResulBodyRoot(WrappedNode mb)
        {
            return CommonRoot(mb, delegate (WrappedNode node) {
                                                                  if (node.ResultType != ResultType.None)
                                                                  {
                                                                      if (node.ResultType > ResultType.SubTitle)
                                                                      {
                                                                          return 1;
                                                                      }

                                                                      return -1;
                                                                  }

                                                                  return 0;
            });
        }

        /// <summary>
        /// 递归构建WrappedNode
        /// </summary>
        /// <param name="rawNode">html节点</param>
        /// <param name="parent">父节点</param>
        /// <param name="index">节点索引</param>
        /// <param name="textIndex">文本索引</param>
        /// <returns>节点</returns>
        private static WrappedNode Wrap(HtmlNode rawNode, WrappedNode parent, ref int index, ref int textIndex)
        {
	        WrappedNode node = new WrappedNode(rawNode) { Parent = parent };
            //找到第一个块标签父节点
            node.FirstBlockParent = FirstBlockParent(node);
            //如果当前节点有内容，则将当前节点的原始文本序列值设置为传入值
            if ((node.NodeType == HtmlNodeType.Text) && (node.TextNodeMeaningFulCharsLength > 0))
            {
                node.OriginalTextIndex = textIndex;
            }

            index++;
            if (node.TextNodeMeaningFulCharsLength > 0)
            {
                textIndex++;
            }

            List<WrappedNode> list = new List<WrappedNode>();
            if (rawNode.ChildNodes != null)
            {
                //递归遍历所有子节点，并将FirstBlockParent和OriginalTextIndex赋值
                foreach (HtmlNode node2 in (IEnumerable<HtmlNode>) rawNode.ChildNodes)
                {
                    WrappedNode item = Wrap(node2, node, ref index, ref textIndex);
                    list.Add(item);
                }
            }
            //将子节点和原始子节点赋值，深拷贝
            node.Children = list;
            node.OriginalChildren = new List<WrappedNode>();
            node.OriginalChildren.AddRange(node.Children);
            return node;
        }

	    public static bool TravelToFindList(WrappedNode node)
	    {
			WrappedNode wrappedNode2 = CleanMainBlock(node);
		    List<WrappedNode> nodes = new List<WrappedNode>();
		    Travel(
			    wrappedNode2,
			    delegate(WrappedNode n)
			    {
					if (n.TextLen == 0)
					{
						return false;
					}
					if (n.TagName == "p" || n.TagName == "span" || n.TagName == "#text")
					{
						return false;
					}
					return true;
			    },
			    delegate(WrappedNode n)
			    {
					if (n.Parent != null)
					{
						n.Parent.ChildrenCount += 1 + n.ChildrenCount;
						ParallelFeature parallelFeature = new ParallelFeature(n);
						if (!parallelFeature.TextLenRate)
						{
							return false;
						}
						if (n.Parent.ParallelNodeDictionary.ContainsKey(parallelFeature))
						{
							n.Parent.ParallelNodeDictionary[parallelFeature].Add(n);
						}
						else
						{
							n.Parent.ParallelNodeDictionary.Add(parallelFeature, new List<WrappedNode>() { n });
						}

						foreach (KeyValuePair<ParallelFeature, List<WrappedNode>> keyValuePair in n.ParallelNodeDictionary)
						{
							if (keyValuePair.Value.Count > 3)
							{
								nodes.Add(n);
							}
						}
					}
					return true;
			    });
		    return nodes.Count != 0;
	    }

	    public static PageBlockStatisticInfoCollection GetPageBlockStatisticInfoCollection(WrappedNode node)
	    {
		    PageBlockStatisticInfoCollection collection = new PageBlockStatisticInfoCollection();
			Dictionary<string, BlockStatisticInfo> tmp = new Dictionary<string, BlockStatisticInfo>();
			Travel(
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
}

