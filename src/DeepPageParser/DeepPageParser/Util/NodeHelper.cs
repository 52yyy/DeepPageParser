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

using System;
using System.Collections.Generic;

using DeepPageParser.Dom;

using HtmlAgilityPack;

namespace DeepPageParser.Util
{
    internal static class NodeHelper
    {
        internal static CaptionInfo FindCaptions(WrappedNode image, WrappedNode box)
        {
            CaptionInfo info = new CaptionInfo();
            List<WrappedNode> list = new List<WrappedNode>();
            int num = 0;
            int count = box.OriginalChildren.Count;
            while (num < count)
            {
                WrappedNode node = box.OriginalChildren[num];
                bool flag = false;
                if (!node.IsTextNode)
                {
                    if (!(!IsElementIgnorable(node) && node.IsNodeVisible))
                    {
                        flag = true;
                    }

                    if (node.NodeType == HtmlNodeType.Comment)
                    {
                        flag = true;
                    }
                }
                else if (node.ElementNodeMeaningFulCharsLength <= 0)
                {
                    flag = true;
                }

                if (!flag)
                {
                    list.Add(node);
                }

                num++;
            }

            info.NumberChildren = list.Count;
            if (info.NumberChildren == 0)
            {
                return null;
            }

            if (info.NumberChildren != 1)
            {
                num = 0;
                while ((num < list.Count) && (list[num].Index != image.Index))
                {
                    num++;
                }

                if (num >= list.Count)
                {
                    return null;
                }

                if ((num + 1) < list.Count)
                {
                    info.Caption1 = list[num + 1];
                    info.Conf1 = IsCaptionOrCredit(info.Caption1);
                }

                if ((num + 2) < list.Count)
                {
                    info.Caption2 = list[num + 2];
                    info.Conf2 = IsCaptionOrCredit(info.Caption2);
                }
            }

            return info;
        }

        /// <summary>
        /// 获取节点指定属性
        /// </summary>
        /// <param name="rawNode">节点</param>
        /// <param name="property">属性名</param>
        /// <returns>属性值</returns>
        internal static string GetAttribute(HtmlNode rawNode, string property)
        {
            HtmlAttributeCollection attributes = rawNode.Attributes;
            if ((attributes != null) && attributes.Contains(property))
            {
                return attributes[property].Value.Trim();
            }

            return string.Empty;
        }

        internal static bool GetDimensionInStyle(string style, ref WidthHeight wh)
        {
            wh.Width = -1;
            wh.Height = -1;
            int res = -1;
            int num2 = -1;
            string[] strArray = style.Split(new[] { ';' });
            foreach (string str in strArray)
            {
                string[] strArray2 = str.Split(new[] { ':' });
                if (strArray2.Length == 2)
                {
                    if (string.Equals(strArray2[0].Trim(), "width", StringComparison.OrdinalIgnoreCase) && GetInt(strArray2[1], ref res))
                    {
                        wh.Width = res;
                    }

                    if (string.Equals(strArray2[0].Trim(), "height", StringComparison.OrdinalIgnoreCase) && GetInt(strArray2[1], ref num2))
                    {
                        wh.Height = num2;
                    }
                }
            }

            if ((res != -1) && (num2 != -1))
            {
                return true;
            }

            wh.Width = -1;
            wh.Height = -1;
            return false;
        }

        //https://www.washingtonpost.com/politics/inside-donald-trumps-echo-chamber-of-conspiracies-grievances-and-vitriol/2016/10/16/1c3c6a72-921e-11e6-9c85-ac42097b8cc0_story.html?hpid=hp_hp-top-table-main_echochamber-810pm%3Ahomepage%2Fstory
        //图片一定有维度吗?
        internal static WidthHeight GetElementDimension(WrappedNode node)
        {
            int num;
            int num2;
            WidthHeight wh = new WidthHeight {
                Width = -1, 
                Height = -1
            };
            string s = GetAttribute(node.RawNode, "width").ToUpperInvariant().Replace("px", string.Empty).Replace("pt", string.Empty);
            string str2 = GetAttribute(node.RawNode, "height").ToUpperInvariant().Replace("px", string.Empty).Replace("pt", string.Empty);
            bool flag = false;
            if (int.TryParse(s, out num) && int.TryParse(str2, out num2))
            {
                wh.Width = num;
                wh.Height = num2;
                flag = true;
            }

            if (!flag)
            {
                GetDimensionInStyle(GetAttribute(node.RawNode, "style"), ref wh);
            }

            return wh;
        }

        internal static HyperRatioInfo GetHyperRatios(WrappedNode child)
        {
            DocHelper.ActionOnWrappedNode visitor = null;
            HyperRatioInfo result = new HyperRatioInfo();
            int textLen = child.TextLen;
            int hyperlen = 0;
            result.Ratio = 0.0;
            result.Count = 0;
            if (!child.IsTextNode)
            {
                if (visitor == null)
                {
                    visitor = delegate (WrappedNode node) {
                        if (node.TagName == "a")
                        {
                            result.Count++;
                            hyperlen += node.TextLen;
                        }

                        return true;
                    };
                }

                DocHelper.Travel(child, null, visitor);
                if (textLen != 0)
                {
                    result.Ratio = (1.0 * hyperlen) / ((double) textLen);
                }
                else
                {
                    result.Ratio = 0.0;
                }

                return result;
            }

            if (child.Parent.TagName == "a")
            {
                result.Ratio = 1.0;
                result.Count = 1;
                return result;
            }

            result.Ratio = 0.0;
            result.Count = 0;
            return result;
        }

        internal static int GetListNumber(WrappedNode child)
        {
            DocHelper.ActionOnWrappedNode visitor = null;
            int listCount = 0;
            if (!child.IsTextNode)
            {
                if (visitor == null)
                {
                    visitor = delegate (WrappedNode node) {
                        if (node.TagName == "li")
                        {
                            listCount++;
                        }

                        return true;
                    };
                }

                DocHelper.Travel(child, null, visitor);
                return listCount;
            }

            if (child.Parent.TagName == "li")
            {
                return 1;
            }

            return 0;
        }

        internal static bool IsBlockQuoteNode(WrappedNode root)
        {
            int atext = 0;
            int btext = 0;
            int bnode = 0;
            int btag = 0;
            if (root.TagName == "blockquote")
            {
                return true;
            }

            DocHelper.Travel(root, delegate (WrappedNode node) {
                if (node.TagName == "blockquote")
                {
                    btag++;
                    bnode++;
                }

                return true;
            }, delegate (WrappedNode node) {
                if (node.TagName == "blockquote")
                {
                    btag--;
                }

                if (node.TextIndex != -1)
                {
                    atext += node.TextLen;
                    if (btag > 0)
                    {
                        btext += node.TextLen;
                    }
                }

                return true;
            });
            return (bnode > 0) && (((bnode > 0) && (atext > 0)) && ((btext / atext) > 0.7));
        }

        internal static int IsCaptionOrCredit(WrappedNode node)
        {
            if (!node.IsTextNode)
            {
                if (!string.IsNullOrWhiteSpace(node.Class))
                {
                    string str = node.Class.ToUpperInvariant();
                    if ((str.IndexOf("caption", StringComparison.OrdinalIgnoreCase) >= 0) || (str.IndexOf("credit", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return 2;
                    }
                }

                if (!string.IsNullOrWhiteSpace(node.Id))
                {
                    string str2 = node.Id.ToUpperInvariant();
                    if ((str2.IndexOf("caption", StringComparison.OrdinalIgnoreCase) >= 0) || (str2.IndexOf("credit", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return 2;
                    }
                }

                if ((node.ElementNodeMeaningFulCharsLength < 5) || (node.ElementNodeMeaningFulCharsLength > 400))
                {
                    return 0;
                }

                if (DocHelper.NumOfImages(node) > 0)
                {
                    return 0;
                }

                return 1;
            }

            return 0;
        }

        internal static bool IsDataTable(WrappedNode node)
        {
            int num = -1;
            int num2 = 0;
            int num3 = -1;
            int num4 = 0;
            if (DocHelper.Tags(node, "table").Count > 1)
            {
                return false;
            }

            if (((DocHelper.Tags(node, "th").Count > 0) || (DocHelper.Tags(node, "tfoot").Count > 0)) || (DocHelper.Tags(node, "thead").Count > 0))
            {
                return true;
            }

            List<WrappedNode> list = DocHelper.Tags(node, "tr");
            for (int i = 0; i < list.Count; i++)
            {
                int count = DocHelper.Tags(list[i], "td").Count;
                if ((num == -1) || (num == count))
                {
                    num = count;
                    num2++;
                }
                else if ((num3 == -1) || (num3 == count))
                {
                    num3 = count;
                    num4++;
                }
                else
                {
                    return false;
                }
            }

            return (((num >= 2) && (num3 == -1)) || ((num2 > num4) && (num >= 2))) || ((num2 <= num4) && (num3 >= 2));
        }

        internal static bool IsElementIgnorable(WrappedNode node)
        {
            string text = node.Text;
            if (StringHelper.IsBadTagsForImage(node.TagName))
            {
                return true;
            }

            if (node.TagName == "img")
            {
                return false;
            }

            if (DocHelper.NumOfImages(node) > 0)
            {
                return false;
            }

            return ((node.TextNodeMeaningFulCharsLength <= 0) || StringHelper.IsEnlargeButtonText(text)) || StringHelper.IsJavaScriptNotice(text);
        }

        internal static bool IsVideoImage(ImageInfo imgInfo)
        {
            string id;
            string str2;
            WrappedNode image = imgInfo.Image;
            if (imgInfo.Box != null)
            {
                image = imgInfo.Box;
            }

            while (image != null)
            {
                id = image.Id;
                str2 = image.Class;
                if ((id.IndexOf("video", StringComparison.OrdinalIgnoreCase) != -1) || (str2.IndexOf("video", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    return true;
                }

                image = image.Parent;
            }

            image = imgInfo.Image;
            if (imgInfo.Box != null)
            {
                image = imgInfo.Box;
            }

            List<WrappedNode> children = new List<WrappedNode>();
            DocHelper.TravelInOrginalNodes(image, delegate (WrappedNode n) {
                children.Add(n);
                return true;
            }, null);
            int count = children.Count;
            for (int i = 0; i < count; i++)
            {
                WrappedNode node2 = children[i];
                id = node2.Id;
                str2 = node2.Class;
                if ((id.IndexOf("video", StringComparison.OrdinalIgnoreCase) != -1) || (str2.IndexOf("video", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    return true;
                }

                if (node2.NodeType == HtmlNodeType.Element)
                {
                    string tagName = node2.TagName;
                    string attribute = GetAttribute(node2.RawNode, "title");
                    string innerHtml = node2.InnerHtml;
                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        tagName = tagName.ToUpperInvariant();
                    }

                    if (!string.IsNullOrWhiteSpace(attribute))
                    {
                        attribute = attribute.ToUpperInvariant();
                    }

                    if ((string.Equals(tagName, "div", StringComparison.OrdinalIgnoreCase) && ((attribute.IndexOf("video", StringComparison.OrdinalIgnoreCase) != -1) || (attribute.IndexOf("play", StringComparison.OrdinalIgnoreCase) != -1))) && (StringHelper.GetMeaningFulChars(innerHtml).Length == 0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static ImageInfo SearchForImageCaptions(WrappedNode image)
        {
            WrappedNode parent = image.Parent;
            WrappedNode node2 = image;
            ImageInfo info = new ImageInfo(image, null, image, null, null);
            while (parent != null)
            {
                CaptionInfo info2 = FindCaptions(node2, parent);
                if (info2 == null)
                {
                    return info;
                }

                if (((info2.NumberChildren == 3) && (info2.Conf1 > 0)) && (info2.Conf2 > 0))
                {
                    info.Image = node2;
                    info.Box = parent;
                    info.Caption1 = info2.Caption1;
                    info.Caption2 = info2.Caption2;
                    return info;
                }

                if (((info2.NumberChildren == 2) && (info2.Conf1 > 0)) && (info2.Caption2 == null))
                {
                    info.Image = node2;
                    info.Box = parent;
                    info.Caption1 = info2.Caption1;
                    info.Caption2 = null;
                    return info;
                }

                if (info2.Conf1 == 2)
                {
                    info.Image = node2;
                    info.Box = null;
                    info.Caption1 = info2.Caption1;
                    info.Caption2 = (info2.Conf2 == 2) ? info2.Caption2 : null;
                    return info;
                }

                if (info2.Conf2 == 2)
                {
                    info.Image = node2;
                    info.Box = null;
                    info.Caption1 = info2.Caption2;
                    info.Caption2 = null;
                    return info;
                }

                if (info2.NumberChildren > 1)
                {
                    return info;
                }

                node2 = parent;
                parent = parent.Parent;
            }

            return info;
        }

        /// <summary>
        /// 如果节点中不包含属性值，则设置为指定值
        /// </summary>
        /// <param name="rawNode">节点</param>
        /// <param name="property">属性名</param>
        /// <param name="value">值</param>
        internal static void SetAttribute(HtmlNode rawNode, string property, string value)
        {
            if ((rawNode != null) && string.IsNullOrWhiteSpace(GetAttribute(rawNode, property)))
            {
                rawNode.SetAttributeValue(property, value);
            }
        }

        private static bool GetInt(string str, ref int res)
        {
            str = str.ToUpperInvariant().Trim();
            int length = 0;
            while (length < str.Length)
            {
                if (!char.IsDigit(str[length]))
                {
                    break;
                }

                length++;
            }

            if (length <= 0)
            {
                return false;
            }

            str = str.Substring(0, length);
            return int.TryParse(str, out res);
        }
    }
}

