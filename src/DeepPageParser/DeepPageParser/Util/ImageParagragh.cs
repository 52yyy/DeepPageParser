
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

