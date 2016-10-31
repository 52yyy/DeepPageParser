using System;
using System.Collections.Generic;

using DeepPageParser.Util;

using HtmlAgilityPack;

namespace DeepPageParser.Dom
{
    /// <summary>
    /// 文本数据节点，解析器基本数据单元
    /// </summary>
    public class WrappedNode
    {
        #region Constructor

        public WrappedNode(HtmlNode rawNode)
        {
            this.Init(rawNode);
        }

        #endregion

        #region Members

        private string _innerHtml;
        public Dictionary<ParallelFeature, List<WrappedNode>> ParallelNodeDictionary = new Dictionary<ParallelFeature, List<WrappedNode>>();

        #endregion


        #region Attributes

        /// <summary>
        /// A标签索引
        /// </summary>
        public int Atag { get; set; }

        /// <summary>
        /// 块索引
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// 孩子节点
        /// </summary>
        public int BfsIndex { get; set; } //按照广度优先遍历给的节点id

        public List<WrappedNode> Children { get; set; }

        public List<WrappedNode> ChildrenBak { get; set; }

        /// <summary>
        /// html类名
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// 初始化节点
        /// </summary>
        public HtmlNode ClonedRawNode { get; set; }

        /// <summary>
        /// 节点有意义文本长度，包含子节点
        /// </summary>
        public int ElementNodeMeaningFulCharsLength { get; set; }

        public WrappedNode FirstBlockParent { get; set; }

        /// <summary>
        /// 获取第一个叶子节点
        /// </summary>
        public WrappedNode FirstPreTraversalLeafChild
        {
            get
            {
                WrappedNode node = this;
                while (node.Children.Count != 0)
                {
                    node = node.Children[0];
                }

                return node;
            }
        }

        public WrappedNode HeadingNode { get; set; }

        public string Href { get; set; }

        /// <summary>
        /// 节点字体大小
        /// </summary>
        public int Hx { get; set; }

        public string Id { get; set; }

        /// <summary>
        /// 节点索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 内部html文本
        /// </summary>
        public string InnerHtml
        {
            get
            {
                if ((this._innerHtml == null) && (this.RawNode != null))
                {
                    try
                    {
                        this._innerHtml = this.RawNode.InnerHtml;
                    }
                    catch
                    {
                        this._innerHtml = string.Empty;
                    }
                }

                return this._innerHtml;
            }
        }

        /// <summary>
        /// 内部文本
        /// </summary>
        public string InnerText { get; set; }

        public bool IsCenter { get; set; }

        /// <summary>
        /// 是否是标题字段
        /// </summary>
        public bool IsHeadingNode
        {
            get
            {
                return (((this.TagName == "h1") || (this.TagName == "h2")) || ((this.TagName == "h3") || (this.Id.IndexOf("newsheading", StringComparison.OrdinalIgnoreCase) > -1))) || (this.Class.IndexOf("newsheading", StringComparison.OrdinalIgnoreCase) > -1);
            }
        }

        public bool IsNavBlock { get; set; }

        /// <summary>
        /// 节点可见
        /// </summary>
        public bool IsNodeVisible { get; set; }

        /// <summary>
        /// 孩子节点数量
        /// </summary>
		public int ChildrenCount { get; set; }

        /// <summary>
        /// 是否是文本节点
        /// </summary>
        public bool IsTextNode
        {
            get
            {
                return this.NodeType == HtmlNodeType.Text;
            }
        }

        public bool IsUnderNavBlock
        {
            get
            {
                return (this.FirstBlockParent != null) && this.FirstBlockParent.IsNavBlock;
            }
        }

        public double Jac { get; set; }

        /// <summary>
        /// 获取最后一个叶子节点
        /// </summary>
        public WrappedNode LastPreTraversalLeafChild
        {
            get
            {
                WrappedNode node = this;
                while (node.Children.Count != 0)
                {
                    node = node.Children[node.Children.Count - 1];
                }

                return node;
            }
        }

        public int Level { get; set; }

        /// <summary>
        /// 链接索引
        /// </summary>
        public int LinkIndex { get; set; }

        /// <summary>
        /// 节点中有意义的文本位置
        /// </summary>
        public int MeaningFulCharsPos { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public HtmlNodeType NodeType { get; set; }

        public List<WrappedNode> OriginalChildren { get; set; }

        public int OriginalIndex { get; set; }

        public int OriginalTextIndex { get; set; }

        public WrappedNode Parent { get; set; }

        /// <summary>
        /// 普通文本索引
        /// </summary>
        public int PlainTextIndex { get; set; }

        /// <summary>
        /// 普通文本位置
        /// </summary>
        public int PlainTextPos { get; set; }

        /// <summary>
        /// 获取文本中标点和数字数量
        /// </summary>
        public int PunctualAndDigitNumber { get; set; }

        /// <summary>
        /// 获取标点符号个数
        /// </summary>
        public int PunctualNumber { get; set; }

        public HtmlNode RawNode { get; set; }

        /// <summary>
        /// 结果类型
        /// </summary>
        public ResultType ResultType { get; set; }

        public string Src { get; set; }

        /// <summary>
        /// 链接深度层级
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 节点innertext
        /// </summary>
        public string Text
        {
            get
            {
                return this.InnerText;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int TextIndex { get; set; }

        public int TextLen
        {
            get
            {
                return string.IsNullOrEmpty(this.InnerText) ? 0 : this.InnerText.Length;
            }
        }

        /// <summary>
        /// 节点中有意义文本长度
        /// </summary>
        public int TextNodeMeaningFulCharsLength { get; set; }

        internal BlockInfo NavBlockInfo { get; set; }

        #endregion

        #region Methods

        public void Init(HtmlNode rawNode)
        {
            this.Id = rawNode.Id;//预处理，解码，替换空格，
            this.InnerText = StringHelper.GetMeaningFulChars(rawNode.InnerText);
            this._innerHtml = null;
            this.TagName = rawNode.Name;
            this.Parent = null;
            this.Children = new List<WrappedNode>();
            this.ChildrenBak = null;
            this.NodeType = rawNode.NodeType;
            this.Class = NodeHelper.GetAttribute(rawNode, "class");
            this.Href = NodeHelper.GetAttribute(rawNode, "href");
            this.Src = NodeHelper.GetAttribute(rawNode, "src");
            this.RawNode = rawNode;
            this.ClonedRawNode = null;
            this.OriginalIndex = -1;
            this.Index = -1;
            this.OriginalTextIndex = -1;
            this.TextIndex = -1;
            this.TextNodeMeaningFulCharsLength = (this.NodeType == HtmlNodeType.Text) ? this.InnerText.Length : 0;
            this.MeaningFulCharsPos = -1;
            this.Level = -1;
            this.ResultType = ResultType.None;
            this.Atag = -1;
            this.HeadingNode = null;
            this.IsNodeVisible = true;
            this.Jac = -1.0;
            this.PunctualNumber = StringHelper.GetPuncuationNum(this.InnerText);
            this.BlockIndex = -1;
            this.BfsIndex = -1;
            this.IsCenter = false;
            this.PlainTextPos = -1;
            this.PlainTextIndex = -1;
            this.IsNavBlock = false;
            this.NavBlockInfo = new BlockInfo();
            this.FirstBlockParent = null;
            this.PunctualAndDigitNumber = (this.NodeType == HtmlNodeType.Text) ? StringHelper.GetPuncuationAndDigitNum(this.InnerText) : 0;
            this.LinkIndex = -1;
        }
            #endregion
    }

    public class ParallelFeature
    {
        public ParallelFeature(WrappedNode node)
        {
            this.TagName = node.TagName;
            this.ClassName = node.Class;
            this.ChildrenCount = node.ChildrenCount;
            this.TextLenRate = (node.TextLen * 1.0 / node.ChildrenCount > 5) && (node.TextLen > 50);
        }

        public string TagName { get; set; }

        public string ClassName { get; set; }

        public bool TextLenRate { get; set; }

        public int ChildrenCount { get; set; }

        public override string ToString()
        {
            return TagName + ClassName + ChildrenCount;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            ParallelFeature objBag = (ParallelFeature)obj;
            if (objBag.TagName != this.TagName)
            {
                return false;
            }
            if (objBag.ClassName != this.ClassName)
            {
                return false;
            }
            if (objBag.TextLenRate != this.TextLenRate)
            {
                return false;
            }
            if (objBag.ChildrenCount != this.ChildrenCount)
            {
                return false;
            }
            return true;
        }
    }
}

