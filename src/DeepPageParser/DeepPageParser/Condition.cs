using DeepPageParser.Dom;

namespace DeepPageParser
{
    internal static class Condition
    {
        public static bool IsDateNode(WrappedNode div)
        {
            return ((div.NavBlockInfo.PunctualAndDigitNumber > 0) && (div.NavBlockInfo.InnerTextLen < 100)) && ((div.NavBlockInfo.InnerTextLen / div.NavBlockInfo.PunctualAndDigitNumber) < 5);
        }

        public static bool IsLinkNode(WrappedNode div)
        {
            return ((div.NavBlockInfo.LinkNum > 0) && (div.NavBlockInfo.LinkTextLen > 0)) && ((div.NavBlockInfo.LinkTextLen * 3) > div.NavBlockInfo.InnerTextLen);
        }

        /// <summary>
        /// 节点远离标题
        /// </summary>
        /// <param name="node"></param>
        /// <param name="title"></param>
        /// <param name="plainTextLength"></param>
        /// <returns></returns>
        public static bool IsNodeFarFromTitle(WrappedNode node, WrappedNode title, int plainTextLength)
        {
            return ((((title.PlainTextPos + 320) < node.PlainTextPos) && (((1.0 * node.PlainTextPos) / ((double) ((plainTextLength - title.PlainTextPos) + 1))) > 0.6)) && (plainTextLength < 0xbb8)) || ((((node.PlainTextPos - title.PlainTextPos) * 2.5) > plainTextLength) && (node.BlockIndex > (title.BlockIndex + 20)));
        }

        /// <summary>
        /// 判断当前节点是否可以放入到文本集群中
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="cluster">文本集群</param>
        /// <returns></returns>
        public static bool IsNodeMatchCluster(WrappedNode node, UnstructuredContentPageParser.Cluster cluster)
        {
            //必需满足：标点符号个数大于0，节点层级与集群层级相等，（节点块与集群中节点的距离小于12 或者 节点与集群的距离小于24且节点有父块，且父块为<p>节点），
            return ((node.PunctualNumber > 0) && (node.Level == cluster.Level)) && (((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 12) || ((((node.BlockIndex - cluster.Nodes[cluster.Nodes.Count - 1].BlockIndex) < 0x18) && (node.FirstBlockParent != null)) && (node.FirstBlockParent.TagName == "p")));
        }
    }
}

