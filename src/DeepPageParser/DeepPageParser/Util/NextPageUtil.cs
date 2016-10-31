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
using System.Linq;

using DeepPageParser.Dom;

namespace DeepPageParser.Util
{
    public static class NextPageUtil
    {
        private static string _title = string.Empty;

        public delegate bool CheckNodeFunc(WrappedNode node);

        public static string ExtractNextPageLink(WrappedNode body, List<WrappedNode> originalList, string currentUrl, WrappedNode titleNode)
        {
            string str = string.Empty;
            List<WrappedNode> oldList = FilterList(originalList, node => node.TagName == "a");
            int firstContentIndex = 0x1869f;
            int lastContentIndex = -1;
            DocHelper.TravelInOrginalNodes(body, delegate (WrappedNode node) {
                if (node.ResultType > ResultType.None)
                {
                    lastContentIndex = Math.Max(node.OriginalIndex, lastContentIndex);
                    firstContentIndex = Math.Min(node.OriginalIndex, firstContentIndex);
                }

                return true;
            }, null);
	        if (lastContentIndex == -1 && firstContentIndex == 0x1869f)
	        {
		        //防止由于列表页的nodetype导致的first 和last异常
				//尽量保留oldlist中的所有内容
		        firstContentIndex = 0;
		        lastContentIndex = 0x1869f;
	        }
            
			if (firstContentIndex > lastContentIndex)
            {
                return string.Empty;
            }

            oldList = FilterList(oldList, node => (node.OriginalIndex > firstContentIndex) && (node.OriginalIndex < (lastContentIndex + 500)));
            oldList = UpdateUrl(currentUrl, oldList);//将相对url变成绝对url
            List<int> curPageNoCans = GetCurPageNoCans(oldList, currentUrl);
            Dictionary<string, LinkInfo> dictionary = GroupLinksByUrl(FilterByHost(currentUrl, oldList));
            foreach (KeyValuePair<string, LinkInfo> pair in dictionary)
            {
                pair.Value.GetScore(currentUrl, curPageNoCans);
            }

            double score = -1.0;
            string absoluteUri = string.Empty;
            foreach (KeyValuePair<string, LinkInfo> pair in dictionary)
            {
                if ((pair.Value.Score > 0.0) && (pair.Value.Score > score))
                {
                    score = pair.Value.Score;
                    absoluteUri = pair.Value.LinkUri.AbsoluteUri;
                }
            }

            if (score > 0.0)
            {
                str = absoluteUri;
            }

            return str;
        }

        public static List<WrappedNode> FilterList(List<WrappedNode> oldList, CheckNodeFunc checkFunc)
        {
            return (from item in oldList
                where checkFunc(item)
                select item).ToList<WrappedNode>();
        }

        public static string ToAbsoluteUrl(string currentUrl, string href)
        {
            Uri uri;
            Uri baseUri = new Uri(currentUrl);
            try
            {
                uri = new Uri(baseUri, href);
				return uri.AbsoluteUri;
			}
            catch (UriFormatException)
            {
                return string.Empty;
            }
        }

        private static List<WrappedNode> FilterByHost(string currentUrl, List<WrappedNode> list)
        {
            Uri currentUri = new Uri(currentUrl);
            return (from link in list
                    let linkUri = new Uri(link.Href)
                    where currentUri.Host == linkUri.Host
                    select link).ToList<WrappedNode>();
        }

        private static List<int> GetCurPageNoCans(List<WrappedNode> list, string url)
        {
            List<int> list2 = new List<int>();
            Uri uri = new Uri(url);
            foreach (WrappedNode node in list)
            {
                int num;
                Uri uri2 = new Uri(node.Href);
                if ((uri.Host == uri2.Host) && ((((uri.AbsoluteUri == uri2.AbsoluteUri) || ((uri.LocalPath == uri2.LocalPath) && (uri.Query == uri2.Query))) && int.TryParse(node.InnerText.ToUpperInvariant().Trim(), out num)) && !list2.Contains(num)))
                {
                    list2.Add(num);
                }
            }

            return list2;
        }

        private static Dictionary<string, LinkInfo> GroupLinksByUrl(List<WrappedNode> linkList)
        {
            Dictionary<string, LinkInfo> dictionary = new Dictionary<string, LinkInfo>();
            foreach (WrappedNode node in linkList)
            {
                string href = node.Href;
                if (!dictionary.ContainsKey(href))
                {
                    dictionary[href] = new LinkInfo(href);
                }

                dictionary[href].NodeList.Add(node);
            }

            return dictionary;
        }

        private static List<WrappedNode> UpdateUrl(string currentUrl, List<WrappedNode> list)
        {
            return FilterList(list, delegate (WrappedNode item) {
                string href = NodeHelper.GetAttribute(item.RawNode, "href");
                string str2 = ToAbsoluteUrl(currentUrl, href);
                if (string.IsNullOrWhiteSpace(str2))
                {
                    return false;
                }

                NodeHelper.SetAttribute(item.RawNode, "href", str2);
                item.Href = str2;
                return true;
            });
        }

        private class LinkInfo
        {
            private static readonly HashSet<string> NextPageLinkHtmls;
            private static readonly HashSet<string> NextPageLinkTextExcludes;
            private static readonly HashSet<string> NextPageLinkTextP;
            private static readonly Dictionary<string, int> NextPageLinkTexts;

            static LinkInfo()
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                dictionary.Add("continue", 1);
                dictionary.Add("continue...", 1);
                dictionary.Add("continue.", 1);
                dictionary.Add("continue..", 1);
                dictionary.Add("continued", 1);
                dictionary.Add("next", 1);
                dictionary.Add("next...", 1);
                dictionary.Add("nextpage", 1);
                dictionary.Add("next page", 1);
                dictionary.Add("next page.", 1);
                dictionary.Add("next page..", 1);
                dictionary.Add("next page...", 1);
                dictionary.Add("next \x00bb", 1);
                dictionary.Add("next ›", 1);
                dictionary.Add("next page \x00bb", 1);
                dictionary.Add("next page ›", 1);
                dictionary.Add("\x00bb", 1);
                dictionary.Add("›", 1);
                dictionary.Add(">", 1);
                NextPageLinkTexts = dictionary;
                NextPageLinkHtmls = new HashSet<string> { "next page", "nextpage" };
                NextPageLinkTextP = new HashSet<string> { "continue", "next" };
                NextPageLinkTextExcludes = new HashSet<string> { "next post", "next article", "newer post", "older post", "print", "comments", "email", "tweet", "share" };
            }

            public LinkInfo(string linkUrl)
            {
                this.LinkUri = new Uri(linkUrl);
                this.NodeList = new List<WrappedNode>();
            }

            public Uri LinkUri { get; private set; }

            public List<WrappedNode> NodeList { get; private set; }

            public double Score { get; private set; }

            private int AltContain { get; set; }

            private int AltMatch { get; set; }

            private int ClassContain { get; set; }

            private int ClassMatch { get; set; }

            private int CurrUrlMatchTitleWords { get; set; }

            private int HtmlContain { get; set; }

            private bool IsPageUrl { get; set; }

            private int LinkPageNum { get; set; }

            private int TextContain { get; set; }

            private int TextContainExcludes { get; set; }

            private int TextIsNumber { get; set; }

            private int TextLength { get; set; }

            private int TextMatch { get; set; }

            private int TextNumberMatch { get; set; }

            private int TitleContain { get; set; }

            private int TitleMatch { get; set; }

            private int UrlMatchTitleWords { get; set; }

            private int UrlPageNum { get; set; }

            private int UrlPathMatch { get; set; }

            private int UrlSearchMatch { get; set; }

            private int UrlSearchStringMatch { get; set; }

            private int UrlpathDiffLen { get; set; }

            public void GetScore(string pageUrl, List<int> curPageNoCans)
            {
                this.Init();
                this.GenerateFeature(pageUrl, curPageNoCans);
                this.Score = -1.0;
                if (!this.IsPageUrl && (((this.TextContainExcludes == 0) && ((this.TextLength <= 20) || (this.TextMatch > 0))) && (((this.UrlpathDiffLen < 30) || ((this.UrlMatchTitleWords - this.CurrUrlMatchTitleWords) > 1)) || (this.TextNumberMatch > 0))))
                {
                    int num = ((((((((((((this.UrlPathMatch * 6) + (this.UrlSearchMatch * 6)) + (this.UrlSearchStringMatch * 6)) + (this.TextMatch * 6)) + (this.AltMatch * 4)) + (this.TitleMatch * 4)) + (this.ClassMatch * 3)) + (this.TextNumberMatch * 2)) + (this.TextContain * 2)) + (this.ClassContain * 2)) + (this.AltContain * 2)) + (this.TitleContain * 2)) + this.HtmlContain;
                    this.Score = num;
                }
            }

            private static int GetMatchedWordCount(string a, string b)
            {
                a = StringHelper.TransString(a).ToUpperInvariant();
                b = StringHelper.TransString(b).ToUpperInvariant();
                int num = 0;
                List<string> list = a.Split(new[] { ' ' }).ToList<string>();
                List<string> list2 = b.Split(new[] { ' ' }).ToList<string>();
                int num2 = 0;
                int count = list.Count;
                while (num2 < count)
                {
                    if (!string.IsNullOrWhiteSpace(list[num2]) && (list2.IndexOf(list[num2]) != -1))
                    {
                        num++;
                    }

                    num2++;
                }

                return num;
            }

            private static PathDiffInfo GetPathDiff(string urlPath, string linkPath)
            {
                int num4;
                if (string.IsNullOrEmpty(urlPath))
                {
                    urlPath = string.Empty;
                }

                if (string.IsNullOrEmpty(linkPath))
                {
                    linkPath = string.Empty;
                }

                PathDiffInfo info = new PathDiffInfo {
                                                         Url = string.Empty, 
                                                         Link = string.Empty
                                                     };
                int num = Math.Min(urlPath.Length, linkPath.Length);
                int startIndex = num;
                int num3 = num;
                for (num4 = 0; num4 < num; num4++)
                {
                    if (urlPath[num4] != linkPath[num4])
                    {
                        startIndex = num4;
                        break;
                    }
                }

                for (num4 = 0; num4 < num; num4++)
                {
                    if (urlPath[(urlPath.Length - num4) - 1] != linkPath[(linkPath.Length - 1) - num4])
                    {
                        num3 = num4;
                        break;
                    }
                }

                if (((urlPath.Length - num3) - 1) >= startIndex)
                {
                    string str = urlPath.Substring(startIndex, (((urlPath.Length - num3) - 1) - startIndex) + 1);
                    info.Url = str.Trim(new[] { ',', '-', '\\', '_' });
                }

                if (((linkPath.Length - num3) - 1) >= startIndex)
                {
                    info.Link = linkPath.Substring(startIndex, (((linkPath.Length - num3) - 1) - startIndex) + 1).Trim(new[] { ',', '-', '\\', '_' });
                }

                return info;
            }

            private static Dictionary<string, string> GetQueryVariable(string search)
            {
                if ((search.Length > 1) && (search[0] == '?'))
                {
                    search = search.Substring(1);
                }

                string[] strArray = search.Split(new[] { '&' });
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                int index = 0;
                int length = strArray.Length;
                while (index < length)
                {
                    if (strArray[index].IndexOf('=') == -1)
                    {
                        dictionary[strArray[index]] = "n/a";
                    }
                    else
                    {
                        string[] strArray2 = strArray[index].Split(new[] { '=' });
                        dictionary[strArray2[0]] = strArray2[1];
                    }

                    index++;
                }

                return dictionary;
            }

            private static int HtmlContained(string html)
            {
                if (!string.IsNullOrEmpty(html) && NextPageLinkHtmls.Any<string>(item => (html.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1)))
                {
                    return 1;
                }

                return 0;
            }

            private static int MatchPageNo(int urlPageNo, int linkPageNo, List<int> curPageNoCans, List<int> linkPageTextNos, bool pageRelated)
            {
                if (linkPageNo < 0)
                {
                    return -1;
                }

                if (urlPageNo < 0)
                {
                    int num = 0;
                    int count = curPageNoCans.Count;
                    while (num < count)
                    {
                        int num3 = 0;
                        int num4 = linkPageTextNos.Count;
                        while (num3 < num4)
                        {
                            if ((curPageNoCans[num] + 1) == linkPageTextNos[num3])
                            {
                                return linkPageTextNos[num3];
                            }

                            num3++;
                        }

                        num++;
                    }

                    if ((((linkPageTextNos != null) && (linkPageTextNos.Count > 0)) && linkPageTextNos.Contains(2)) && ((linkPageNo == 1) || (linkPageNo == 2)))
                    {
                        return 2;
                    }

                    if (pageRelated && ((linkPageNo == 1) || (linkPageNo == 2)))
                    {
                        return 2;
                    }
                }
                else if ((urlPageNo + 1) == linkPageNo)
                {
                    return linkPageNo + (linkPageTextNos.Contains(linkPageNo + 1) ? 1 : 0);
                }

                return 0;
            }

            private static int TextContained(string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Split(new char[0]).Length > 5)
                    {
                        return 0;
                    }

                    if (text.Length > 50)
                    {
                        return 0;
                    }

                    if (NextPageLinkTextP.Any<string>(item => text.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        return 1;
                    }
                }

                return 0;
            }

            private static int TextExcludesContained(string text)
            {
                if ((!string.IsNullOrEmpty(text) && (text.Length <= 50)) && NextPageLinkTextExcludes.Any<string>(item => (text.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1)))
                {
                    return 1;
                }

                return 0;
            }

            private static int TextMatched(string text)
            {
                if (!string.IsNullOrEmpty(text) && NextPageLinkTexts.ContainsKey(text))
                {
                    return 1;
                }

                return 0;
            }

            private static string TrimStartAndEnd(string str, string chars)
            {
                str = str.TrimStart(chars.ToCharArray());
                str = str.TrimEnd(chars.ToCharArray());
                return str;
            }

            private MatchedPage CheckSearch(string urlSearch, string linkSearch, List<int> curPageNoCans, List<int> linkPageTextNos)
            {
                MatchedPage page = new MatchedPage {
                                                       Matched = 0, 
                                                       UrlPage = -1, 
                                                       LinkPage = -1
                                                   };
                if (urlSearch != linkSearch)
                {
                    Dictionary<string, string> queryVariable = GetQueryVariable(linkSearch);
                    Dictionary<string, string> dictionary2 = GetQueryVariable(urlSearch);
                    foreach (KeyValuePair<string, string> pair in queryVariable)
                    {
                        int num2;
                        string key = pair.Key;
                        int result = -1;
                        if (!int.TryParse(queryVariable[key], out num2))
                        {
                            PathDiffInfo pathDiff = GetPathDiff(dictionary2.ContainsKey(key) ? dictionary2[key] : string.Empty, queryVariable[key]);
                            num2 = this.ConverToInt(pathDiff.Link);
                            result = this.ConverToInt(pathDiff.Url);
                        }
                        else if (dictionary2.ContainsKey(key) && !int.TryParse(dictionary2[key], out result))
                        {
                            result = -1;
                        }

                        int num3 = MatchPageNo(result, num2, curPageNoCans, linkPageTextNos, false);
                        if (num3 > 0)
                        {
                            page.Matched = 1;
                            page.UrlPage = num3 - 1;
                            page.LinkPage = num3;
                            return page;
                        }
                    }
                }

                return page;
            }

            private int ConverToInt(string str)
            {
                int num;
                if (!int.TryParse(str, out num))
                {
                    return -1;
                }

                return num;
            }

            private void GenerateFeature(string pageUrl, List<int> curPageNoCans)
            {
                PathDiffInfo pathDiff;
                int num2;
                int num3;
                int num4;
                List<int> linkPageTextNos = new List<int>();
                Uri uri = new Uri(pageUrl);
                foreach (WrappedNode node in this.NodeList)
                {
                    int num;
                    string html = node.InnerHtml.ToLower(CultureInfo.InvariantCulture);
                    string text = node.InnerText.ToLower(CultureInfo.InvariantCulture);
                    string str3 = node.Class.ToLower(CultureInfo.InvariantCulture);
                    string attribute = NodeHelper.GetAttribute(node.RawNode, "alt");
                    string str5 = NodeHelper.GetAttribute(node.RawNode, "title");
                    if ((this.LinkUri.AbsoluteUri == uri.AbsoluteUri) || ((this.LinkUri.LocalPath == uri.LocalPath) && (this.LinkUri.Query == uri.Query)))
                    {
                        this.IsPageUrl = true;
                    }

                    this.TextMatch += TextMatched(text);
                    this.TextLength = Math.Min(text.Length, this.TextLength);
                    this.TextContain += TextContained(text);
                    this.TextContainExcludes += TextExcludesContained(text);
                    this.ClassMatch += TextMatched(str3);
                    this.ClassContain += TextContained(str3);
                    this.AltMatch += TextMatched(attribute);
                    this.AltContain += TextContained(attribute);
                    this.TextContainExcludes += TextExcludesContained(attribute);
                    this.TitleMatch += TextMatched(str5);
                    this.TitleContain += TextContained(str5);
                    this.TextContainExcludes += TextExcludesContained(str5);
                    if (text.Length == 0)
                    {
                        this.HtmlContain += HtmlContained(html);
                    }

                    if (int.TryParse(TrimStartAndEnd(text, " |"), out num))
                    {
                        linkPageTextNos.Add(num);
                    }
                }

                string urlPath = TrimStartAndEnd(uri.LocalPath, @"/\ ");
                string linkPath = TrimStartAndEnd(this.LinkUri.LocalPath, @"/\ ");
                string str8 = TrimStartAndEnd(uri.Query, @"/\ ");
                string linkSearch = TrimStartAndEnd(this.LinkUri.Query, @"/\ ");
                if (urlPath != linkPath)
                {
                    pathDiff = GetPathDiff(urlPath, linkPath);
                    this.UrlMatchTitleWords = GetMatchedWordCount(pathDiff.Link, _title);
                    this.CurrUrlMatchTitleWords = GetMatchedWordCount(pathDiff.Url, _title);
                    num2 = -1;
                    num3 = -1;
                    this.UrlpathDiffLen = pathDiff.Link.Length + pathDiff.Url.Length;
                    if ((pathDiff.Url.Length > 0) && !int.TryParse(pathDiff.Url, out num2))
                    {
                        num2 = -1;
                    }

                    if ((pathDiff.Link.Length > 0) && !int.TryParse(pathDiff.Link, out num3))
                    {
                        num3 = -1;
                    }

                    num4 = MatchPageNo(num2, num3, curPageNoCans, linkPageTextNos, false);
                    if (num4 > 0)
                    {
                        this.UrlPathMatch = this.NodeList.Count;
                        this.UrlPageNum = num4 - 1;
                        this.LinkPageNum = num4;
                    }
                }
                else if (str8 != linkSearch)
                {
                    string str10 = linkSearch;
                    if (string.IsNullOrEmpty(str8))
                    {
                        str10 = str10.Replace("page", string.Empty).Replace(".", string.Empty).Replace("?", string.Empty).Replace("-", string.Empty).Replace("-", string.Empty);
                    }

                    pathDiff = GetPathDiff(str8, str10);
                    this.UrlpathDiffLen = pathDiff.Link.Length + pathDiff.Url.Length;
                    if (!int.TryParse(pathDiff.Url, out num2))
                    {
                        num2 = -1;
                    }

                    if (!int.TryParse(pathDiff.Link, out num3))
                    {
                        num3 = -1;
                    }

                    num4 = MatchPageNo(num2, num3, curPageNoCans, linkPageTextNos, false);
                    if (num4 > 0)
                    {
                        this.UrlSearchStringMatch = this.NodeList.Count;
                        this.UrlPageNum = num4 - 1;
                        this.LinkPageNum = num4;
                    }
                    else
                    {
                        MatchedPage page = this.CheckSearch(str8, linkSearch, curPageNoCans, linkPageTextNos);
                        if (page.Matched > 0)
                        {
                            this.UrlSearchMatch = this.NodeList.Count * page.Matched;
                            this.UrlPageNum = page.UrlPage;
                            this.LinkPageNum = page.LinkPage;
                        }
                    }
                }

                int num5 = 0;
                int count = linkPageTextNos.Count;
                while (num5 < count)
                {
                    this.TextIsNumber++;
                    if (linkPageTextNos[num5] == this.LinkPageNum)
                    {
                        this.TextNumberMatch++;
                    }

                    num5++;
                }
            }

            private void Init()
            {
                this.UrlPathMatch = 0;
                this.UrlSearchMatch = 0;
                this.UrlSearchStringMatch = 0;
                this.TextMatch = 0;
                this.AltMatch = 0;
                this.TitleMatch = 0;
                this.ClassMatch = 0;
                this.TextNumberMatch = 0;
                this.TextContain = 0;
                this.ClassContain = 0;
                this.AltContain = 0;
                this.TitleContain = 0;
                this.HtmlContain = 0;
                this.TextContainExcludes = 0;
                this.TextLength = 0;
                this.UrlpathDiffLen = 0;
                this.UrlMatchTitleWords = 0;
                this.CurrUrlMatchTitleWords = 0;
                this.UrlPageNum = -1;
                this.LinkPageNum = -1;
                this.TextIsNumber = 0;
                this.IsPageUrl = false;
            }
        }

        private class MatchedPage
        {
            public int LinkPage { get; set; }

            public int Matched { get; set; }

            public int UrlPage { get; set; }
        }

        private class PathDiffInfo
        {
            public string Link { get; set; }

            public string Url { get; set; }
        }
    }
}

