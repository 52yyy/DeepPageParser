// ======================================================================
// 
//      Copyright (C) 北京国双科技有限公司        
//                    http://www.gridsum.com
// 
//      保密性声明：此文件属北京国双科技有限公司所有，仅限拥有由国双科技
//      授予了相应权限的人所查看和所修改。如果你没有被国双科技授予相应的
//      权限而得到此文件，请删除此文件。未得国双科技同意，不得查看、修改、
//      散播此文件。
// 
// 
// ======================================================================

using System.Collections.Generic;

using HtmlAgilityPack;

namespace DeepPageParser.Util
{
    public static class BlockHelper
    {
        private static readonly HashSet<string> NewLinetags = new HashSet<string> { "div", "p", "pre", "li", "td", "br", "h1", "h2", "h3", "h4" };
        private static readonly HashSet<string> StrongTags = new HashSet<string> { "strong", "b", "bold", "h1", "h2", "h3", "h4", "h5" };

        public static List<Paragragh> ToBlockList(HtmlNode mainTitle, HtmlNode root)
        {
            List<Paragragh> list = new List<Paragragh>();
            List<HtmlNode> curBlock = new List<HtmlNode>();
            bool newline = false;
            string curLine = string.Empty;
            if (mainTitle != null)
            {
                string str = StringHelper.TrimLeadingSpace(mainTitle.InnerText);
                if (string.IsNullOrWhiteSpace(str))
                {
                }

                TextParagragh paragragh = new TextParagragh(str) {
                                                                     IsTitle = true
                                                                 };
                list.Add(paragragh);
            }

            DocHelper.TravelOnRawNode(root, delegate (HtmlNode node) {
                                                                         if (node == null)
                                                                         {
                                                                             return false;
                                                                         }

                                                                         if ((node.NodeType == HtmlNodeType.Text) && !string.IsNullOrEmpty(node.InnerText))
                                                                         {
                                                                             curLine = curLine + node.InnerText;
                                                                             curBlock.Add(node);
                                                                             newline = false;
                                                                         }

                                                                         if ((node.NodeType != HtmlNodeType.Element) && (node.NodeType != HtmlNodeType.Document))
                                                                         {
                                                                             return false;
                                                                         }

                                                                         if (node.Name == "script")
                                                                         {
                                                                             return false;
                                                                         }

                                                                         return true;
            }, delegate (HtmlNode node) {
                                            if (!string.IsNullOrEmpty(node.Name))
                                            {
                                                if (NewLinetags.Contains(node.Name))
                                                {
                                                    curLine = StringHelper.TrimLeadingSpace(curLine);
                                                    if ((curLine.Length != 0) || !newline)
                                                    {
                                                        TextParagragh item = new TextParagragh(curLine) {
                                                                                                            TagName = node.Name, 
                                                                                                            IsStrong = !string.IsNullOrWhiteSpace(curLine) && IsTags(curBlock, StrongTags), 
                                                                                                            IsCenter = !string.IsNullOrWhiteSpace(curLine) && (NodeHelper.GetAttribute(node, "isCenter") == "true")
                                                                                                        };
                                                        if (NodeHelper.GetAttribute(node, "isCaption") == "true")
                                                        {
                                                            item.IsCaption = true;
                                                        }

                                                        list.Add(item);
                                                        curLine = string.Empty;
                                                        curBlock.Clear();
                                                        newline = true;
                                                    }
                                                }

                                                if ((node.NodeType == HtmlNodeType.Element) && (node.Name == "img"))
                                                {
                                                    int num;
                                                    int num2;
                                                    ImageParagragh paragragh3 = new ImageParagragh(NodeHelper.GetAttribute(node, "src"));
                                                    string attribute = NodeHelper.GetAttribute(node, "width");
                                                    string s = NodeHelper.GetAttribute(node, "height");
                                                    if (!(int.TryParse(attribute, out num) && int.TryParse(s, out num2)))
                                                    {
                                                        num = -1;
                                                        num2 = -1;
                                                    }

                                                    if ((num <= 0) || (num2 <= 0))
                                                    {
                                                        num = 0;
                                                        num2 = 0;
                                                    }

                                                    paragragh3.Width = num;
                                                    paragragh3.Height = num2;
                                                    list.Add(paragragh3);
                                                }
                                            }

                                            return true;
            });
            if (list.Count <= 1)
            {
            }

            return list;
        }

        internal static string GetBlockHtml(string url, string warning, List<Paragragh> list)
        {
            string str = "<head><meta charset=\"UTF-8\"/></head><body>";
            if (!string.IsNullOrWhiteSpace(warning))
            {
                string str2 = "<p style=\"background-color:yellow;\"> " + warning + "</p>";
                str = str + str2;
            }

            foreach (Paragragh paragragh in list)
            {
                str = str + paragragh.HtmlBlock;
            }

            return (str + "<p><a href=\"" + url + "\">Original Page</a></p>") + "</body>";
        }

        internal static bool IsTags(List<HtmlNode> textNodes, HashSet<string> tags)
        {
            foreach (HtmlNode node in textNodes)
            {
                if (string.IsNullOrWhiteSpace(StringHelper.TrimLeadingSpace(node.InnerText)))
                {
                    continue;
                }

                HtmlNode parentNode = node.ParentNode;
                while (parentNode != null)
                {
                    if (tags.Contains(parentNode.Name) || NewLinetags.Contains(parentNode.Name))
                    {
                        break;
                    }

                    parentNode = parentNode.ParentNode;
                }

                if (!((parentNode != null) && tags.Contains(parentNode.Name)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

