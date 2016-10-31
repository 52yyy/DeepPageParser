
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

