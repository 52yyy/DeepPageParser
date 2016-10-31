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
using System.Globalization;

using DeepPageParser.Dom;

namespace DeepPageParser.Util
{
    internal static class ImageHelper
    {
        internal static List<WrappedNode> ExtractImageCandidate(WrappedNode body, int firstIndex, int lastIndex)
        {
            List<WrappedNode> imgCandidates = new List<WrappedNode>();
            DocHelper.CommonRoot(body, (DocHelper.FilterFunc)(block => block.ResultType > ResultType.None ? 1 : 0));
            WrappedNode lastNode = (WrappedNode)null;
            DocHelper.Travel(body, (DocHelper.ActionOnWrappedNode)(node =>
            {
                if (node.ResultType > ResultType.None)
                    lastNode = node;
                return true;
            }), (DocHelper.ActionOnWrappedNode)null);
            DocHelper.TravelInOrginalNodes(body, (DocHelper.ActionOnWrappedNode)(node =>
            {
                if (node.TagName.Equals("img", StringComparison.OrdinalIgnoreCase) && node.Index <= lastIndex && (lastNode != null && node.BlockIndex <= lastNode.BlockIndex + 5) && (node.FirstBlockParent != null && !node.FirstBlockParent.IsNavBlock) && node.Index > firstIndex)
                {
                    if (lastNode.BlockIndex < node.BlockIndex)
                        lastNode = node;
                    imgCandidates.Add(node);
                }

                return true;
            }), (DocHelper.ActionOnWrappedNode)null);
            return imgCandidates;
        }

        internal static List<ImageInfo> GetImageFromCandidate(IEnumerable<WrappedNode> candidates, int firstIndex, int lastIndex, WrappedNode commonRoot)
        {
            List<ImageInfo> list = new List<ImageInfo>();
            foreach (WrappedNode node in candidates)
            {
                WrappedNode node2 = node;
                WidthHeight elementDimension = NodeHelper.GetElementDimension(node2);
                int width = elementDimension.Width;
                int height = elementDimension.Height;
                WrappedNode compoundImage = DocHelper.GetCompoundImage(node2);
                if (DocHelper.IsChild(node2, commonRoot) || ((width > 250) || (height > 250))) //这个地方的是，图片一定有维度吗？如果格式写在css里面呢？
                {
                    if (compoundImage != null)
                    {
                        elementDimension = NodeHelper.GetElementDimension(compoundImage);
                        if ((elementDimension.Width >= width) && (elementDimension.Height >= height))
                        {
                            width = elementDimension.Width;
                            height = elementDimension.Height;
                            node2 = compoundImage;
                        }
                    }

                    double num3 = (1.0 * width) / ((double) height);
                    ImageInfo info = NodeHelper.SearchForImageCaptions(node2);
                    info.Width = width;
                    info.Height = height;
                    if ((node.Src.ToUpperInvariant().IndexOf("load", StringComparison.OrdinalIgnoreCase) == -1) || (node.Src.ToUpperInvariant().IndexOf(".gif", StringComparison.OrdinalIgnoreCase) == -1))
                    {
                        UpdateImageInfo(info);
                        if ((((width >= 400) && (num3 < 3.0)) && (num3 > 0.33333333333333331)) && (node2.Index <= lastIndex))
                        {
                            info.IsLarge = true;
                            list.Add(info);
                        }
                        else if (((node2.Index >= firstIndex) && (node2.Index <= lastIndex)) && (((width >= 120) && (height >= 120)) || ((width == -1) && (height == -1))))
                        {
                            info.IsLarge = false;
                            list.Add(info);
                        }
                    }
                }
            }

            return list;
        }

        internal static void UpdateImageInfo(ImageInfo info)
        {
            WrappedNode rawImage = info.RawImage;
            rawImage.RawNode.SetAttributeValue("width", info.Width.ToString(CultureInfo.InvariantCulture));
            rawImage.RawNode.SetAttributeValue("height", info.Height.ToString(CultureInfo.InvariantCulture));
        }

        internal static void UpdateImageSrc(WrappedNode body, string baseUrl)
        {
            Uri host = new Uri(baseUrl);
            DocHelper.TravelInOrginalNodes(body, delegate (WrappedNode node) {
                if (node.TagName.Equals("img", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(node.Src))
                {
                    Uri uri = new Uri(host, node.Src);
                    if (node.Src != uri.AbsoluteUri)
                    {
                        node.RawNode.SetAttributeValue("src", uri.AbsoluteUri);
                    }
                }

                return true;
            }, null);
        }
    }
}

