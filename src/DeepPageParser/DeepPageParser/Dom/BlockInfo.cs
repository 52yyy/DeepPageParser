namespace DeepPageParser.Dom
{
    /// <summary>
    /// 块信息
    /// </summary>
    internal class BlockInfo
    {
        public BlockInfo()
        {
            this.InnerTextLen = 0;
            this.InnerHtmlLength = 0;
            this.LinkTextLen = 0;
            this.NodeNum = 0;
            this.LinkNum = 0;
            this.ImgNum = 0;
            this.ParagraphNumber = 0;
            this.PunctualAndDigitNumber = 0;
        }

        /// <summary>
        /// 图片数量
        /// </summary>
        public int ImgNum { get; set; }

        /// <summary>
        /// 内部html文本字数
        /// </summary>
        public int InnerHtmlLength { get; set; }

        /// <summary>
        /// 内部纯文本字数
        /// </summary>
        public int InnerTextLen { get; set; }

        /// <summary>
        /// 内部链接数量
        /// </summary>
        public int LinkNum { get; set; }

        /// <summary>
        /// 链接文本字数
        /// </summary>
        public int LinkTextLen { get; set; }

        /// <summary>
        /// 内部节点数量
        /// </summary>
        public int NodeNum { get; set; }

        /// <summary>
        /// 段落数量
        /// </summary>
        public int ParagraphNumber { get; set; }

        /// <summary>
        /// 标点和数字字符数量
        /// </summary>
        public int PunctualAndDigitNumber { get; set; }
    }
}

