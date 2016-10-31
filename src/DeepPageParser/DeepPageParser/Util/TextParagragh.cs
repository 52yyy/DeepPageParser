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
using System.Net;

namespace DeepPageParser.Util
{
    public class TextParagragh : Paragragh
    {
        public TextParagragh(string text)
        {
            this.Text = text;
            this.Text = WebUtility.HtmlEncode(this.Text);
            this.IsCenter = this.IsStrong = false;
        }

        public override string HtmlBlock
        {
            get
            {
                string text = this.Text;
                if (this.IsStrong)
                {
                    text = string.Format(CultureInfo.InvariantCulture, "<strong>{0}</strong>", new object[] { text });
                }

                if (this.IsCenter)
                {
                    text = string.Format(CultureInfo.InvariantCulture, "<div sytle='text-align:center'>{0}</div>", new object[] { text });
                }

                if (this.IsCaption)
                {
                    text = string.Format(CultureInfo.InvariantCulture, "<i>{0}</i>", new object[] { text });
                }

                if (this.IsTitle)
                {
                    return string.Format(CultureInfo.InvariantCulture, "<h1>{0}</h1>", new object[] { text });
                }

                if (this.TagName == "pre")
                {
                    return string.Format(CultureInfo.InvariantCulture, "<pre>{0}</pre>", new object[] { text });
                }

                return string.Format(CultureInfo.InvariantCulture, "<p>{0}</p>", new object[] { text });
            }
        }

        public bool IsCaption { get; internal set; }

        public bool IsCenter { get; internal set; }

        public bool IsStrong { get; internal set; }

        public bool IsTitle { get; internal set; }

        public string TagName { get; internal set; }

        public string Text { get; internal set; }
    }
}

