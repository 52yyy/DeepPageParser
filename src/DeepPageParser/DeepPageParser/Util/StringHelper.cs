
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace DeepPageParser.Util
{
    public static class StringHelper
    {
        private static readonly HashSet<string> BlockTags;

        private static readonly HashSet<string> badTagsForImage;

		private static readonly char[] _char = new[] { '\t', ' ', '\v', '\n', '\r' };
	    //private static readonly Regex _reg = new Regex(@"\s+", RegexOptions.Compiled);

        //结束词
        private static readonly List<string> contentEnd = new List<string> { "filed under", "votes:", "related stories:", "related articles:", "more on", "related posts", "subscribe now" };
        private static readonly Dictionary<string, int> defaultFontSize;
        private static readonly HashSet<string> divTags;
        //非层级标签
        private static readonly HashSet<string> noLevelTags = new HashSet<string> { 
            "em", "strong", "dfn", "code", "samp", "kbd", "var", "cite", "abbr", "acronym", "tt", "i", "b", "big", "small", "strike", 
            "s", "u", "font", "basefont", "sub", "sup", "ins", "del", "td", "tr", "th", "span"
         };

        static StringHelper()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary.Add("h1", 6);
            dictionary.Add("h2", 5);
            dictionary.Add("h3", 4);
            dictionary.Add("h4", 3);
            dictionary.Add("h5", 2);
            dictionary.Add("h6", 1);
            dictionary.Add("other", 0);
            defaultFontSize = dictionary;
            //块标签名列表
	        BlockTags = new HashSet<string>
						{
							"address",
							"blockquote",
							"center",
							"dir",
							"div",
							"dl",
							"fieldset",
							"form",
							"h1",
							"h2",
							"h3",
							"h4",
							"h5",
							"h6",
							"hr",
							"isindex",
							"menu",
							"noframes",
							"noscript",
							"ol",
							"p",
							"pre",
							"table",
							"td",
							"ul"
						};
            divTags = new HashSet<string> { 
                "address", "article", "blockquote", "center", "dir", "div", "dl", "fieldset", "form", "hr", "isindex", "menu", "noframes", "noscript", "ol", "table", 
                "td", "ul"
             };
            badTagsForImage = new HashSet<string> { "script", "!", "meta", "br" };
        }

        /// <summary>
        /// 结束单词列表
        /// </summary>
        internal static List<string> ContentEnd
        {
            get
            {
                return contentEnd;
            }
        }

        internal static Dictionary<string, int> DefaultFontSize
        {
            get
            {
                return defaultFontSize;
            }
        }

        internal static HashSet<string> NoLevelTags
        {
            get
            {
                return noLevelTags;
            }
        }

        /// <summary>
        /// 判断字符串中是否包含结束单词
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainEndWord(string text)
        {
            Func<string, bool> predicate = null;
            bool flag = false;
            if (text.Length > 0)
            {
                text = text.ToUpperInvariant();
                int length = text.Length;
                text = text.Substring(0, Math.Min(30, text.Length));
                if (predicate == null)
                {
                    predicate = str => text.IndexOf(str, StringComparison.OrdinalIgnoreCase) > -1;
                }

                if (ContentEnd.Any<string>(predicate))
                {
                    flag = true;
                }

                if (((text.IndexOf("copyright", 0, StringComparison.OrdinalIgnoreCase) > -1) || (text.IndexOf("&copy;", 0, StringComparison.OrdinalIgnoreCase) > -1)) && (length < 300))
                {
                    flag = true;
                }
            }

            return flag;
        }

		/// <summary>
		///		抓取URL以及识别网页编码
		/// </summary>
		/// <param name="url"></param>
		/// <param name="userAgent"></param>
		/// <param name="proxy"></param>
		/// <param name="retry"></param>
		/// <returns></returns>
        public static string FetchAndDetectEncoding(string url, string userAgent, string proxy, int retry = 2)
        {
            string str = string.Empty;
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
                if (!string.IsNullOrWhiteSpace(proxy))
                    webClient.Proxy = (IWebProxy)new WebProxy(proxy);
                byte[] bytes = (byte[])null;
                for (int index = 0; index <= retry; ++index)
                {
                    bool flag = false;
                    try
                    {
                        bytes = webClient.DownloadData(url);
                        flag = true;
                    }
                    catch
                    {
                        if (index == retry)
                            return string.Empty;
                    }

                    if (flag)
                        break;
                }

                if (bytes != null)
                {
                    string name = Regex.Match(Encoding.Default.GetString(bytes), "<meta([^<]*)charset=[\"]*([-|\\s|a-z|A-Z|0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups[2].Value.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        name = "UTF-8";
                    str = Encoding.GetEncoding(name).GetString(bytes);
                }
            }

            return str;
        }

        /// <summary>
        /// 获取有意义的字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetMeaningFulChars(string text)
        {
            //解码两次，有一些页面会编码一次以上
			text = WebUtility.HtmlDecode(text);
			text = WebUtility.HtmlDecode(text);
            //去掉连续的空格、制表符以及换行符
			return string.Join(" ", text.Split(_char, StringSplitOptions.RemoveEmptyEntries)).Trim();
			//return _reg.Replace(text, " ").Trim();
        }
		
        /// <summary>
        /// 获取文本中标点和数字数量
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetPuncuationAndDigitNum(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            return str.Count<char>(ch => (((char.IsPunctuation(ch) || char.IsDigit(ch)) && ((ch != '.') && (ch != '\''))) && (ch != ',')));
        }

        /// <summary>
        /// 获取字符串中标点符号个数
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>标点个数</returns>
        public static int GetPuncuationNum(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            return str.Count<char>(ch => ((((char.IsPunctuation(ch) && (ch != ':')) && ((ch != '-') && (ch != '('))) && ((ch != ')') && (ch != '['))) && (ch != ']')));
        }

        public static bool IsBadTagsForImage(string tag)
        {
            return badTagsForImage.Contains(tag);
        }

        /// <summary>
        /// 确认一个标签是否是块标签，块标签中的文字独立成段
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <returns></returns>
        public static bool IsBlockTag(string tag)
        {
            return BlockTags.Contains(tag);
        }

        public static bool IsDivTags(string tag)
        {
            return divTags.Contains(tag);
        }

        /// <summary>
        /// 将url拆分，获得目录字段和文件字段
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Tuple<string, string> SplitUrl(string url)
        {
            string str = string.Empty;
            string str2 = string.Empty;
            int length = url.LastIndexOf('/');
            if (length > 7)
            {
                str = url.Substring(0, length);
                str2 = url.Substring(length + 1);
            }

            str = TransString(str.ToUpperInvariant()).Trim();
            return new Tuple<string, string>(str, TransString(str2.ToUpperInvariant()).Trim());
        }

        /// <summary>
        /// 去除非英文字母和数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TransString(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9]", " ");
        }

        /// <summary>
        /// 修正url字符串信息,获取url目录字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string TrimUrl(string url)
        {
            int length = url.LastIndexOf('/');
            string str = url.Substring(length + 1);
            if (length > 7)
            {
                if ((((str.IndexOf("index", StringComparison.OrdinalIgnoreCase) > -1) || (str.IndexOf("default", StringComparison.OrdinalIgnoreCase) > -1)) || ((str.IndexOf("home", StringComparison.OrdinalIgnoreCase) > -1) || (str.IndexOf("hp", StringComparison.OrdinalIgnoreCase) > -1))) || (length == (url.Length - 1)))
                {
                    url = url.Substring(0, length);
                    return url;
                }

                string str2 = url.Substring(0, length);
                length = str2.LastIndexOf('/');
                //倒数第二个'/'
                string str3 = str2.Substring(length + 1);
                //TODO:这个判断真的能找到吗？
                if ((str.IndexOf('-') < 0) && (str3.IndexOf('-') > -1))
                {
                    url = str2;
                }
            }

            return url;
        }

        internal static double CommonCharRatio(string a, string b, int maxCharsToCompare)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b))
            {
                return 1.0;
            }

            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            {
                return 0.0;
            }

            int length = a.Length;
            int num2 = b.Length;
            double[] numArray = new double[maxCharsToCompare];
            length = Math.Min(length, maxCharsToCompare);
            num2 = Math.Min(num2, maxCharsToCompare);
            int index = 0;
            while (index < length)
            {
                numArray[index] = (a[index] == b[0]) ? 1.0 : 0.0;
                index++;
            }

            for (int i = 1; i < num2; i++)
            {
                for (index = 0; index < length; index++)
                {
                    if (a[index] == b[i])
                    {
                        numArray[index] = 1.0 + numArray[Math.Max(0, index - 1)];
                    }
                    else
                    {
                        numArray[index] = Math.Max(numArray[index], numArray[Math.Max(0, index - 1)]);
                    }
                }
            }

            double num5 = numArray[length - 1];
            return (1.0 * num5) / ((length + num2) - num5);
        }

        internal static string GetExtractedText(HtmlNode title, HtmlNode mainBlock)
        {
            string str = string.Empty;
            if ((title != null) && (mainBlock != null))
            {
                str = GetMeaningFulChars(title.InnerText) + "\n" + WebUtility.HtmlDecode(DocHelper.OutputText(mainBlock));
            }

            return str;
        }

        internal static List<string> GetPropertyKeys(object obj)
        {
            return (from p in obj.GetType().GetProperties() select p.Name).ToList<string>();
        }

        internal static List<string> GetPropertyValues(object obj)
        {
            return (from p in obj.GetType().GetProperties() select p.GetValue(obj, null).ToString()).ToList<string>();
        }

        internal static bool IsEnlargeButtonText(string str)
        {
            return ((str.ToUpperInvariant().IndexOf("enlarge", StringComparison.OrdinalIgnoreCase) >= 0) && (str.Length < 20)) || (((str.ToUpperInvariant().IndexOf("launch", StringComparison.OrdinalIgnoreCase) >= 0) && (str.ToUpperInvariant().IndexOf("viewer", StringComparison.OrdinalIgnoreCase) >= 0)) && (str.Length < 0x19));
        }

        internal static bool IsJavaScriptNotice(string str)
        {
            return str.ToUpperInvariant().IndexOf("please turn on javascript", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool IsNotLetterString(string str)
        {
            return str.Any<char>(ch => (((((ch < 'a') || (ch > 'z')) && ((ch < 'A') || (ch > 'Z'))) && (!char.IsNumber(ch) && !char.IsPunctuation(ch))) && !char.IsWhiteSpace(ch)));
        }

        internal static bool IsStrongFontTag(string tagName)
        {
            return ((tagName == "b") || (tagName == "strong")) || (tagName == "big");
        }

        /// <summary>
        /// 笛卡尔相似度计算
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        internal static double JaccardSimilarity(string s1, string s2)
        {
            int num6;
            string str = s1.ToUpperInvariant();
            string str2 = s2.ToUpperInvariant();
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            double num5 = 0.0;
            List<string> list = str.Split(new char[0]).ToList<string>();
            List<string> list2 = str2.Split(new char[0]).ToList<string>();
            if ((((list.Count <= 3) || (list2.Count <= 3)) && IsNotLetterString(str)) && IsNotLetterString(str2))
            {
                return CommonCharRatio(str, str2, 500);
            }

            for (num6 = 0; num6 < list.Count; num6++)
            {
                if (!string.IsNullOrWhiteSpace(list[num6]))
                {
                    if (list2.IndexOf(list[num6]) > -1)
                    {
                        num++;
                    }

                    num3++;
                }
            }

            if ((num3 > 0) && (num > 0))
            {
                for (num6 = 0; num6 < list2.Count; num6++)
                {
                    if (!string.IsNullOrWhiteSpace(list2[num6]))
                    {
                        if (list.IndexOf(list2[num6]) > -1)
                        {
                            num2++;
                        }

                        num4++;
                    }
                }

                if ((num4 > 0) && (num2 > 0))
                {
                    num5 = (((double) num2) / (2.0 * num4)) + (((double) num) / (2.0 * num3));
                }
            }

            return num5;
        }

        internal static void LogError(string msg)
        {
            throw new ArgumentException(msg);
        }

        internal static string TrimLeadingSpace(string content)
        {
            return WebUtility.HtmlDecode(content).TrimStart("　\r\n\t  ".ToCharArray());
        }
    }
}

