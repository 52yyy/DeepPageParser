
using System;
using System.Collections.Generic;
using System.Linq;

using DeepPageParser.Dom;
using HtmlAgilityPack;

namespace DeepPageParser.Util
{
    internal static class TitleHelper
    {
        /// <summary>
        /// 生成标题特征
        /// </summary>
        /// <param name="textList"></param>
        /// <param name="metaTitles"></param>
        /// <returns></returns>
        public static List<WrappedNode> GenerateTitleFeature(List<WrappedNode> textList, List<string> metaTitles)
        {
            List<WrappedNode> list = new List<WrappedNode>();
            List<WrappedNode> list2 = new List<WrappedNode>();
            //文本总长度
            int num = (textList.Count > 0) ? (textList[textList.Count - 1].PlainTextPos + 1) : 0;
            bool flag = false;
            using (List<WrappedNode>.Enumerator enumerator = textList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<string, double> selector = null;
                    WrappedNode node = enumerator.Current;
                    //文本位于前六分之一 并且 长度大于三
                    if ((((1.0 * node.PlainTextPos) / ((double) num)) <= 0.6) && (node.TextNodeMeaningFulCharsLength >= 3))
                    {
                        if (selector == null)
                        {
                            //计算节点与标题的相似度
                            selector = metaTitle => StringHelper.JaccardSimilarity(node.InnerText, metaTitle);
                        }

                        //选取一个最大相似度
                        double num2 = metaTitles.Select<string, double>(selector).Concat<double>(new double[1]).Max();
                        node.Jac = num2;
                        //如果节点满足条件则添加到list中
                        if ((num2 >= 0.05) || ((node.Hx >= 1) && (node.InnerText.Length >= 20)))
                        {
                            list.Add(node);
                            if ((node.HeadingNode != null) && (node.Atag == 0))
                            {
                                flag = true;
                            }
                        }

                        list2.Add(node);
                    }
                }
            }

            if (!flag)
            {
                list.AddRange(from node in list2
                    where node.HeadingNode != null
                    select node);
            }

            if (list.Count == 0 )
            {
                list = list2;
            }

            bool seemcredible = false;
            // 当所有候选的相似度都低于0.3（经验值），认为按相似度的逻辑找的标题不可信。
            //候选与meta标题高度相似，但是文本过短时，认为该标题不可信，可能为新闻来源。如：汽车之家。
            //针对上述情况，利用另一种逻辑寻找标题：利用时间的位置查找标题。认为在时间上面，离时间最近的字符串为标题。
            foreach (WrappedNode item in list)
            {
                if (item.Jac > 0.3 && !(item.Jac > 0.9 && item.InnerText.Length < 5))
                {
                    seemcredible = true;//list 中有可信的标题
                    break;
                }
            }
            if (!seemcredible)
            {
                NewsParserConfig parserConfig = new NewsParserConfig();
                int id = 0;
                bool timeflage = false;
                foreach (WrappedNode item in list2)
                {
                    string text = item.InnerText.Trim();
                    int timecount = 0;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (parserConfig.TimeChars.Contains<char>(text[i]))
                        {
                            //完善的逻辑应该调用解析时间，并且判断是否为有效时间，这里比较暴力的认为有5个以上时间字符即为日期。
                            timecount = timecount + 1;
                            if (timecount > 6)
                            {
                                timeflage = true;
                                break;
                            }
                        }
                    }
                    if (timeflage)
                    {
                        break;
                    }
                    id = id + 1;
                }
                if(timeflage)
                {
                    for (int j = id - 1; j >= 0; j--)
                    {
                            if (list2[j].InnerText.Length > 5 &&! (list2[j].InnerText.Contains("www"))) //暴力的认为在时间上面，离时间最近的长于5个字符的，且不包括网址链接，即为标题。
                            {
                                list = new List<WrappedNode>();
                                list.Add(list2[j]);
                                break;
                            }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 根据meta标题获取候选标题
        /// </summary>
        /// <param name="textList"></param>
        /// <param name="metaTitles"></param>
        /// <returns></returns>
        public static List<WrappedNode> GetTitleCandidates(List<WrappedNode> textList, List<string> metaTitles)
        {
            HashSet<WrappedNode> source = new HashSet<WrappedNode>();
            //选取可信的标题节点
            List<WrappedNode> list = GenerateTitleFeature(textList, metaTitles);
            //按照标题大小优先排序
            list.Sort(delegate (WrappedNode a, WrappedNode b) {
                if (a.Hx != b.Hx)
                {
                    return b.Hx - a.Hx;
                }

                return ToInt(b.Jac - a.Jac);
            });
            //添加最可信的
            if ((list.Count > 0) && ((metaTitles.Count < 2) || (list[0].Jac > 0.2)))
            {
                source.Add(list[0]);
            }
            //添加第二可信的
            if ((list.Count > 1) && (list[1].Jac > 0.95))
            {
                source.Add(list[1]);
            }
            //按照相似度排序
            list.Sort(delegate (WrappedNode a, WrappedNode b) {
                if (Math.Abs((double) (b.Jac - a.Jac)) > 0.001)
                {
                    return ToInt(b.Jac - a.Jac);
                }

                return b.Hx - a.Hx;
            });
            if (list.Count > 0)
            {
                //添加相似度最高的
                source.Add(list[0]);
	            bool tmp = false;
	            if (((list.Count > 1) && ((list[0].HeadingNode == null) || (list[0].HeadingNode.IsHeadingNode == true)))
					&& (list[0].Jac < 0.8))
	            {
                    //如果可信度最高特征不是十分明显？则判断第二可信候选并添加
		            if (list[1].Hx > 3 && list[1].Jac > 0.3)
		            {
			            source.Add(list[1]);
			            tmp = true;
		            }
	            }

                //如果第二可信候选也很明显，则继续选择，并添加最后一个满足条件的候选？
				if (tmp && ((list.Count > 2) && (list[0].HeadingNode == null)) && (list[0].Jac < 0.7))
	            {
		            int num = 2;
		            while ((num < list.Count) && (list[num].HeadingNode == null) && list[num].Jac > 0.3)
		            {
			            num++;
		            }

		            if (num < list.Count)
		            {
			            source.Add(list[num]);
		            }
	            }
            }

            return source.ToList<WrappedNode>();
        }

        /// <summary>
        /// 获取Meta节点中的标题信息
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<string> SearchMetaTitles(HtmlDocument doc, string url)
        {
            //获取meta节点中的标题信息
            Dictionary<string, string> metaTitle = GetMetaTitle(doc);
            //去除url信息并返回列表，其中包括原有标题以及修剪过的标题
            return RemoveSite(url, metaTitle);
        }

        public static int ToInt(double x)
        {
            if (Math.Abs(x) < 0.001)
            {
                return 0;
            }

            if (x < 0.0)
            {
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// 添加Meta标题信息
        /// </summary>
        /// <param name="attributes">节点属性</param>
        /// <param name="p0">必需包含属性</param>
        /// <param name="v0">必需包含属性值，会成为标题字典的key</param>
        /// <param name="p1">需要提取的属性值</param>
        /// <param name="metaTitles">标题字典</param>
        private static void AddMetaInfo(HtmlAttributeCollection attributes, string p0, string v0, string p1, Dictionary<string, string> metaTitles)
        {
            string str = string.Empty;
            if ((attributes.Contains(p0) && (attributes[p0].Value == v0)) && attributes.Contains(p1))
            {
                str = attributes[p1].Value.Trim();
            }

            if (!string.IsNullOrWhiteSpace(str))
            {
                metaTitles[v0] = str;
            }
        }

        /// <summary>
        /// 从Meta节点中获取标题信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetMetaTitle(HtmlDocument doc)
        {
            Dictionary<string, string> metaTitles = new Dictionary<string, string>();
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//meta/@content");
            HtmlNodeCollection nodes2 = doc.DocumentNode.SelectNodes("//head//title");
            if (nodes != null)
            {
                foreach (HtmlNode node in (IEnumerable<HtmlNode>) nodes)
                {
                    AddMetaInfo(node.Attributes, "property", "og:title", "content", metaTitles);
                    AddMetaInfo(node.Attributes, "itemprop", "headline", "content", metaTitles);
                    AddMetaInfo(node.Attributes, "itemprop", "alternativeHeadline", "content", metaTitles);
                    AddMetaInfo(node.Attributes, "name", "title", "content", metaTitles);
                    AddMetaInfo(node.Attributes, "name", "twitter:title", "content", metaTitles);
                    AddMetaInfo(node.Attributes, "name", "fb_title", "content", metaTitles);
                }
            }

            if (nodes2 != null)
            {
	            int i = 0;
                foreach (HtmlNode node2 in (IEnumerable<HtmlNode>) nodes2)
                {
                    if (!((node2.InnerText == null) || string.IsNullOrWhiteSpace(node2.InnerText)))
                    {
                        metaTitles["title"+i] = node2.InnerText.Trim();
	                    i++;
                    }
                }
            }

            return metaTitles;
        }

        /// <summary>
        /// 将去除了某些不必要内容的标题以及原标题一起放入列表中
        /// </summary>
        /// <param name="url"></param>
        /// <param name="metaTitles"></param>
        /// <returns></returns>
        private static List<string> RemoveSite(string url, Dictionary<string, string> metaTitles)
        {
            url = StringHelper.TrimUrl(url);
            Tuple<string, string> tuple = StringHelper.SplitUrl(url);
            string path = tuple.Item1;
            string file = tuple.Item2;
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> pair in metaTitles)
            {
                string item = RemoveSite2(pair.Value.ToUpperInvariant(), path, file) ?? RemoveSite1(pair.Value.ToUpperInvariant());
                list.Add(item);
                list.Add(pair.Value);
            }

            return list;
        }

        /// <summary>
        /// 根据标题本身去除一些不必要字段
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RemoveSite1(string str)
        {
            int num = 0;
            int num2 = -1;
            int length = -1;
            int num4 = 0;
            int num5 = str.Length;
            while (num4 < num5)
            {
                //找到':'
                if (str[num4] == ':')
                {
                    num++;
                    num2 = num4;
                }
                //找到'-'
                else if (str[num4] == '-')
                {
                    num++;
                    length = num4;
                }

                //找到':'或者'-'两次以上，返回当前字符串
                if (num > 1)
                {
                    return str.Trim();
                }

                num4++;
            }

            //没有找到':'或者'-'，返回当前字符串
            if (num == 0)
            {
                return str.Trim();
            }

            //找到一个':'，截取冒号之前的文字
            if (num2 >= 0)
            {
                return str.Substring(num2 + 1).Trim();
            }

            //找到一个'-'，截取'-'之前的文字
            return str.Substring(0, length);
        }

        /// <summary>
        /// 根据url去除标题中的字段
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string RemoveSite2(string str, string path, string file)
        {
            string str2 = str.ToUpperInvariant();
            string str3 = string.Empty;
            //将标题分割
            string[] strArray = str2.Split(new[] { '|', '-', ':' });
            bool flag = true;
            bool flag2 = false;

            //遍历查找，字数足够长并且段落少于等于3则返回标题字段
            int index = 0;
            int length = strArray.Length;
            while (index < length)
            {
                if ((strArray[index].Length > 20) && (strArray.Length <= 3))
                {
                    return str;
                }

                index++;
            }

            //遍历查找
            index = 0;
            length = strArray.Length;
            while (index < length)
            {
                string str4 = StringHelper.TransString(strArray[index]).Trim();
                int num3 = 0;
                int num4 = 0;
                //按照空格分割
                string[] strArray2 = str4.Split(new[] { ' ' });

                //遍历查找
                int num5 = 0;
                int num6 = strArray2.Length;
                while (num5 < num6)
                {
                    string str5 = strArray2[num5];
                    int num7 = path.IndexOf(str5, 0, StringComparison.OrdinalIgnoreCase);
                    int num8 = file.IndexOf(str5, 0, StringComparison.OrdinalIgnoreCase);
                    //找到了路径字符串
                    if (num7 > -1)
                    {
                        num4++;
                    }

                    //找到了文件字符串
                    if (num8 > -1)
                    {
                        num3++;
                    }

                    num5++;
                }

                //没有找到文件字符串
                if (num3 == 0)
                {
                    flag = false;
                }

                //找到了路径字符串，并且比文件多
                if (num4 > num3)
                {
                    //不可能满足这个条件
                    if (num4 < 1)
                    {
                        str3 = str3 + strArray[index] + " ";
                    }
                    else
                    {
                        flag2 = true;
                        flag = false;
                    }
                }
                //都没有找到或者找到文件字符串个数大于等于路径个数
                else
                {
                    str3 = str3 + strArray[index] + " ";
                }

                index++;
            }

            str3 = str3.Trim();
            //找到了路径字符串并且路径出现次数大于文件出现次数
            if (flag2)
            {
                //将没有找到或者找到文件字符串个数大于等于路径个数的段落组合起来，并返回
                if (str3.Length > 0)
                {
                    return str3.ToUpperInvariant();
                }
                
                //如果路径在所有段落里面都出现，则返回空
                return null;
            }

            //如果都没有找到过文件字符串，则返回空，如果有找到文件字符串并且比路径多，则返回当前字符串
            return flag ? str.ToUpperInvariant() : null;
        }
    }
}

