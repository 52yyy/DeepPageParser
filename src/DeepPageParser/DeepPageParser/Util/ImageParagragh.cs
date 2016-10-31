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

using System.Globalization;

namespace DeepPageParser.Util
{
    public class ImageParagragh : Paragragh
    {
        public ImageParagragh(string link)
        {
            this.Link = link;
        }

        public override string HtmlBlock
        {
            get
            {
                if ((this.Height != 0) && (this.Width != 0))
                {
                    return string.Format(CultureInfo.InvariantCulture, "<img src='{0}' height='{1}' width='{2}' >", new object[] { this.Link, this.Height, this.Width });
                }

                return string.Format(CultureInfo.InvariantCulture, "<img src='{0}'>", new object[] { this.Link });
            }
        }

        public int Height { get; internal set; }

        public string Link { get; internal set; }

        public int Width { get; internal set; }
    }
}

