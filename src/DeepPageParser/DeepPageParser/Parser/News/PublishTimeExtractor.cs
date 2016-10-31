using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using DeepPageParser;
using DeepPageParser.Dom;
using HtmlAgilityPack;

namespace DeepPageParser
{
    /// <summary>
    ///		发布时间提取类，使用状态模式实现
    /// </summary>
    public class PublishTimeExtractor : NewsPageFieldExtractor
    {
        public PublishTimeExtractor(NewsParserConfig parserConfig)
            : base(parserConfig)
        {
        }

        public int FindEnglishMonth(string text)
        {
            int timestart = -1;

            return timestart;
        }
        //解析外文网址的时间，希望的是，以后能够知道该网址，是否为外文网址。在调用函数时，外文网址使用ParseEnglishTime，中文网址使用 ParseTime。
        public PublishTime ParseEnglishTime(string text)
        {
            int monthstart = 0;
            int monthend = 0;
            int timestart = 0;
            int timeend = 0;
            PublishTime publishTime = new PublishTime();
            bool hasenglishmonth = false;
            //从正常的开始找，如果没有再去找缩写。如果缩写和正常的放在一起。October可能找成缩写，导致错误。
            foreach (string clue in ParserConfig.EnglishTimeStrings)
            {
                monthstart = text.IndexOf(clue);
                //	有时间
                if (monthstart>= 0)
                {
                    hasenglishmonth = true;
                    monthend = monthstart + clue.Length;
                    break;
                }
            }
            //正常没有找到，从缩写找
            if (!hasenglishmonth)
            {
                foreach (string clue in ParserConfig.EnglishTimeStringsAbbr)
                {
                    monthstart = text.IndexOf(clue);
                    //	有时间
                    if (monthstart >= 0)
                    {
                        hasenglishmonth = true;
                        monthend = monthstart + clue.Length;
                        break;
                    }
                }
            }
            if (hasenglishmonth)
            {
                for (int i = monthstart - 1; i >= 0; i--)
                {
                    if (!ParserConfig.TimeChars.Contains<char>(text[i]))
                    {
                        timestart = i + 1;
                        break;
                    }
                }
                int removeindex = -1;
                for (int j = monthend; j < text.Length; j++)
                {
                    // Thu Oct 13, 2016 | 1:53am EDT
                    //只支持去掉一个
                    if (text[j] == '|')
                    {
                        removeindex = j;
                        
                    }
                    if (!ParserConfig.TimeChars.Contains<char>(text[j]) && text[j] != '|')
                    {
                        timeend = j;
                        break;
                    }
                }
                int timelength = 0;
                if (removeindex!=-1)
                {
                    text = text.Remove(removeindex, 1);
                    timelength = timeend - timestart-1;
                }
                else
                {
                    timelength = timeend - timestart;
                }
                if (timelength > 0)
                {
                    text = text.Substring(timestart, timelength);
                    text = text.Trim(ParserConfig.UselessChars);
                }
                publishTime.TimeString = text;
                DateTime dt;
                if (DateTime.TryParse(text, out dt))
                {
                    publishTime.TimeValue = dt;
                    publishTime.ValidTime = true;
                }
            }
            return publishTime;
        }


        public string SpecialTimeDeal(string text)
        {
            int textLength = text.Length;
            //最后一个不能够是点 ，如2016-09-22 10:36 点击量 会被取成2016-09-22 10:36 点
            if (textLength > 1 && text[textLength - 1] == '点')
            {
                text = text.Substring(0, textLength - 1);
            }
            //最后一个字如果是“分”，DateTime.TryParse(text, out dt)不能够正确解析出时间，如：2016年9月21日 17点25分 ，需要转换为2016年9月21日 17:25
            //但是publishTime.TimeString存放时，仍然是存放的2016年9月21日 17点25分，因为这个长度会影响置信度的计算
            // 测试网址：http://www.financeun.com/News/2016921/2013cfn/172610126800.shtml
            //yyyy年MM月dd日格式  （可以不包含年和日）
            //XX时XX分XX秒格式（可不含秒）

            //想尝试正则，但是失败，用比较糙的方式实现
            if (textLength > 15 && text.Contains("点") && text.Contains("分") && !text.Contains("秒"))//至少有16个字符，才考虑做此处理。如“34分”，这样的短字符，没有必要把他的分去掉，因为，反正他本来就是一个无效的时间
            {
                text = text.Replace("点", ":").Replace("分", "");
            }
            //为“星期四, 八月 18, 2016 ”会找成“四, 八月 18, 2016”，或者“星期四  八月 18, 2016 ”会找成“四 八月 18, 2016”的逻辑 去掉空格或者逗号前面的内容。
            int ispecialchar1 = text.IndexOf(',');
            int ispecialchar2 = text.IndexOf(' ');
            if (ispecialchar1>=0&&ispecialchar1<2)
            {
                text = text.Substring(ispecialchar1);
            }
            else if (ispecialchar2 >= 0 && ispecialchar2 < 2)
            {
                text = text.Substring(ispecialchar2);
            }
            //为时间：八月 18, 2016 增加了可能为时间的词"，". 但是可能出现：“八月 18, 2016，北京” 这样的情况，会抽出“八月 18, 2016，”，所以需要再去一遍无用的词，把“，”去掉。
            text = text.Trim(ParserConfig.UselessChars);
            return text;
        }
		
        public PublishTime ParseTime(string text)
        {
            text = text.Trim(ParserConfig.UselessChars);
            //text = text.TrimStart(new char[] { '(', '[' }).TrimEnd(new char[] { ')', ']' });
            PublishTime publishTime = new PublishTime();

            int iEnd = 0;
            int presentnSpace = 0;
            int iStart = 0;
            bool hasbreak = false;
            bool inthespace = false; //In the space area
            //at most two spaces are allowed.
            //To find effective time field
            for (int i = 0; i < text.Length; i++)
            {
                // Determine whether is empty
                // should find A continuous space
                if (char.IsWhiteSpace(text[i]))
                {
                    if (inthespace)
                    {
                        presentnSpace++;
                    }
                    else
                    {
                        presentnSpace = 1;
                        inthespace = true;
                    }
                }
                else
                {
                    inthespace = false;
                }
                if (!ParserConfig.TimeChars.Contains<char>(text[i]) && !inthespace)
                {
                    iEnd = i;
                    hasbreak = true;
                    break;
                }
                // When the blank area length more than 2, over
                if (presentnSpace == 2)
                {
                    iEnd = i;
                    break;
                }

            }
            if (iEnd > 0)
            {
                text = text.Substring(0, iEnd);
            }
            //time in end
            if (iEnd == 0 && hasbreak)
            {
                for (int j = text.Length - 1; j > -1; j--)
                {
                    if (!ParserConfig.TimeChars.Contains<char>(text[j]))
                    {
                        iStart = j;
                        break;
                    }
                }
                if (iStart != text.Length - 1 && (iStart + 2) < text.Length - 1)
                {
                    if (text[iStart + 1] == ':' || text[iStart + 1] == '：')
                    {
                        text = text.Substring(iStart + 2);
                    }
                    else
                    {
                        text = text.Substring(iStart + 1);
                    }
                }

            }

            text = text.Trim(ParserConfig.UselessChars);
            publishTime.TimeString = text;   //但是publishTime.TimeString存放时，仍然是存放的2016年9月21日 17点25分，因为这个长度会影响置信度的计算，但是后面会对这个字符进行一些特殊处理。
            text = SpecialTimeDeal(text);
            DateTime dt;
            if (DateTime.TryParse(text, out dt))
            {
                publishTime.TimeValue = dt;
                publishTime.ValidTime = true;
            }

            return publishTime;
        }

        protected PublishTime GetPublishTime(WrappedNode node)
        {
            PublishTime publishTime = new PublishTime();
            string text = node.Text;
      //    var debug = TryExtractField(text, PageFieldType.PublishTime, true);
            return Findtimebychar(text);
        }

        public PublishTime ParseTime(PageFieldDebug debug)
        {
            string text = debug.FieldValue;
            return ParseTime(text);
        }

        /// <summary>
        /// if Attributes contain (p0, v0) pair, than add (v0, v1) to metaTitles.
        /// </summary>
        private static void AddMetaInfo(HtmlAttributeCollection attributes,
            string p0, string v0, string p1, Dictionary<string, string> metaTitles)
        {
            var value = string.Empty;
            if (attributes.Contains(p0) && attributes[p0].Value == v0 && attributes.Contains(p1))
            {
                value = attributes[p1].Value.Trim();
            }
            if (string.IsNullOrWhiteSpace(value) == false)
            {
                metaTitles[v0] = value;
            }
        }

        /// <summary>
        /// extract publish time
        /// </summary>
        /// <param name="data"></param>
        public PublishTime Findtimebychar(string candidatetext)
        {
            //Determine whether can be parse directly
            PublishTime pt = ParseTime(candidatetext);
            if (pt.ValidTime)
            {
                return pt;
            }
            //deal special space
            candidatetext = candidatetext.Replace("&nbsp", " ");
            //real find by char
            for (int idx = 0; idx < candidatetext.Length; idx++)
            {
                char tmp = candidatetext[idx];
                if (tmp == ' ' || tmp == '　' || tmp == 160)
                {
                    string tempStr = candidatetext.Substring(idx + 1);
                    pt = ParseTime(tempStr);
                    if (pt.ValidTime)
                    {
                        return pt;
                    }
                }

            }
            return pt;
        }

        /// <summary>
        /// extract publish time
        /// </summary>
        /// <param name="data"></param>
        public override void GetFieldValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, List<int> candidateNodeIdx)
        {
            //var titleNode = data.Title;
            //var bodyNode = data.MainBlockNode;
            List<PublishTime> times = new List<PublishTime>();
            var doc = data.Page.Document;

            var metaPublishTime = new Dictionary<string, string>();
            var metas = doc.DocumentNode.SelectNodes("//meta/@content");

            if (metas != null)
            {
                foreach (var meta in metas)
                {
                    AddMetaInfo(meta.Attributes, "name", "publishdate", "content", metaPublishTime);
                }

                foreach (var kv in metaPublishTime)
                {
                    PublishTime t = new PublishTime();
                    t.TimeString = kv.Value;
                    t.Confidence = 1.0;
                    times.Add(t);
                }
            }

            if (data.OriginalNodeList != null && data.OriginalNodeList.Count > 0)
            {



                foreach (var i in candidateNodeIdx)
                {
                    string text = data.OriginalNodeList[i].Text.Trim();
                    string id = data.OriginalNodeList[i].Id;
                    string cls = data.OriginalNodeList[i].Class;

                    bool matched = false;


                  //  text = "2016年9月21日 17点25分";

                    //   text = " August 10, 2016";

                    // 是内容节点且非空
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Text && !String.IsNullOrEmpty(text))
                    {

                        //根据线索词查找
                        var debug = TryExtractTimeField(text, PageFieldType.PublishTime, false);
                        // 有前匹配，有值
                        if (debug.ContainsClue && !string.IsNullOrEmpty(debug.FieldValue))
                        {
                            var pt = ParseTime(debug);
                            if (pt.ValidTime)
                            {
                                pt.Confidence = 5.0 * pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                        }
                        // 无前匹配，有后匹配，从前面或者尾部信息找时间
                        else if (debug.ContainsEndClue && (!string.IsNullOrEmpty(debug.FieldValue) ||!string.IsNullOrEmpty(debug.TailValue)))
                        {
                            //中央政府门户网站　www.gov.cn　　 2012年07月12日 08时56分　　 来源：国务院办公厅
                            //identify time from the string 
                            var pt = ParseTime(debug);
                            if (pt.ValidTime)
                            {
                                pt.Confidence = 4.0 * pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                            else 
                            {
                                pt = Findtimebychar(debug.FieldValue);
                                if (pt != null && pt.ValidTime)
                                {
                                    pt.Confidence = 3.5 * pt.TimeString.Length;
                                    times.Add(pt);
                                    matched = true;
                                }

                            }
                            if (!matched && debug.TailValue!=null)
                            {
                                pt = Findtimebychar(debug.TailValue);
                                if (pt != null && pt.ValidTime)
                                {
                                    pt.Confidence = 2.0 * pt.TimeString.Length;
                                    times.Add(pt);
                                    matched = true;
                                }
                            }
                  
                        }
                        //无前匹配，无后匹配，有其他线索词，一个一个字符找信息
                        else if (debug.ContainsOtherClue && debug.FieldValue.Length > 0)
                        {

                            var pt = ParseTime(debug);
                            if (pt.ValidTime)
                            {
                                pt.Confidence = 3.0 * pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                            else
                            {
                                pt = Findtimebychar(debug.FieldValue);
                                if (pt != null && pt.ValidTime)
                                {
                                    pt.Confidence = 2.0 * pt.TimeString.Length;
                                    times.Add(pt);
                                    matched = true;
                                }
                                
                            }

                        }
                        //有结尾或其他线索词，但是没有找到时间信息，往前找个文本节点，容易找到标题等其他内容，置信度和找的位置有关系

                        if (!matched && (debug.ContainsEndClue || debug.ContainsOtherClue))
                        {
                            int findcount = 0;
                            for (int prei = i-1; prei > 0; prei--)
                            {
                                string pretext = data.OriginalNodeList[prei].Text.Trim();
                                if (data.OriginalNodeList[prei].NodeType == HtmlNodeType.Text &&
                                    !String.IsNullOrEmpty(pretext))
                                {
                                    findcount = findcount + 1;
                                    PublishTime pt = ParseTime(pretext);
                                    if (pt.ValidTime)
                                    {
                                        pt.Confidence = (5.0-findcount) * pt.TimeString.Length;
                                        times.Add(pt);
                                        matched = true;
                                        break;
                                    }
                                }
                                if (findcount == 4)
                                {
                                    break;
                                }
                            }
                        }
                        //有线索词，但是没有找到时间信息，找后面的4个非空文本结点，有可能找到文章中的词，置信度与往后找的位置有关,当文字长度大于一定值时(经验值30)，认为找过了，找到了正文中的内容。
                        if (!matched && (debug.ContainsClue || debug.ContainsEndClue||debug.ContainsOtherClue))
                        {
                            int findcount = 0;

                            for (int nexti = i + 1; nexti < data.OriginalNodeList.Count; nexti++)
                            {
                                string nexttext = data.OriginalNodeList[nexti].Text.Trim();
                                if (data.OriginalNodeList[nexti].NodeType == HtmlNodeType.Text &&
                                    !String.IsNullOrEmpty(nexttext))
                                {
                                    findcount = findcount + 1;
                                    PublishTime pt = ParseTime(nexttext);
                                    if (pt.ValidTime)
                                    {
                                        pt.Confidence = (5.0-findcount)*pt.TimeString.Length;
                                        times.Add(pt);
                                        matched = true;
                                        break;
                                    }
                                }
                                if (findcount == 4 || nexttext.Length>30)
                                {
                                    break;
                                }
                            }
                        }
                        //找可能遗漏的低置信度的时间
                        //	如果尾巴还有信息，而且时间出现在最尾
                        //应该可以删掉啦，因为只有有endclue才有尾部 信息，可是那部分进行了处理。
                        if (!string.IsNullOrEmpty(debug.TailValue))
                        {
                            PublishTime ptTail = ParseTime(debug.TailValue);
                            if (ptTail.ValidTime)
                            {
                                ptTail.Confidence = 1.0*ptTail.TimeString.Length;
                                times.Add(ptTail);
                                matched = true;
                            }
                            else
                            {
                                for (int idx = 0; idx < debug.TailValue.Length; idx++)
                                {
                                    if (debug.TailValue[idx] == ' ' || debug.TailValue[idx] == '　')
                                    {
                                        string tempStr = debug.TailValue.Substring(idx + 1);
                                        ptTail = ParseTime(tempStr);
                                        if (ptTail.ValidTime)
                                        {
                                            ptTail.Confidence = 1.0*ptTail.TimeString.Length;
                                            times.Add(ptTail);
                                            matched = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string condidateId in ParserConfig.PubTimeControlIds)
                        {
                            if (string.Compare(condidateId, id, true) == 0)
                            {
                                
                                PublishTime pt = GetPublishTime(data.OriginalNodeList[i]);

                                if (!pt.ValidTime)//按照中文寻找无效，则按照英文查找
                                {
                                    pt = ParseEnglishTime(data.OriginalNodeList[i].InnerText);
                                }

                                pt.Confidence = 5.0*pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                            if (string.Compare(condidateId, cls, true) == 0)
                            {
                                PublishTime pt = GetPublishTime(data.OriginalNodeList[i]);
                                pt.Confidence = 4.0*pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                        }
                    }
                    if (!matched)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(text, out dt))
                        {
                            if (text.Length < 20 && text.Length > 9)
                            {
                                for (int nexti = i + 1; i < data.OriginalNodeList.Count; nexti++) //考虑时间是否放在两个节点中,拼接时间 时间为纯时间（没有除时间外的其他词）
                                {
                                    string nexttext = data.OriginalNodeList[nexti].Text.Trim();
                                    if (data.OriginalNodeList[nexti].NodeType == HtmlNodeType.Text && !String.IsNullOrEmpty(nexttext) && !(text == nexttext))
                                    {
                                        DateTime nextTime;
                                        if (DateTime.TryParse(nexttext, out nextTime))
                                        {
                                            text = text + " " + nexttext;
                                        }
                                        break;
                                    }
                                }
                            }
                            DateTime dtTemp;
                            if (DateTime.TryParse(text, out dtTemp))
                            {
                                PublishTime pt = new PublishTime();
                                pt.TimeString = text;
                                pt.TimeValue = dtTemp;
                                pt.ValidTime = true;
                                pt.Confidence = 2.0*pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                        }
                        else //单个，没有候选的句子中，包含时间
                        {
                           var pt = Findtimebychar(text);
                            if (pt != null && pt.ValidTime)
                            {
                                pt.Confidence = 1.0 * pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                        }
                        if (!matched)//上面按照中文的方式没有找到时间，用英文找
                        {
                           PublishTime pt = ParseEnglishTime(data.OriginalNodeList[i].InnerText);
                            if (pt.ValidTime)
                            {
                                pt.Confidence = 1.0 * pt.TimeString.Length;
                                times.Add(pt);
                            }
                        }
                    }
                }
            }

            if (times.Count > 0)
            {
                var ordered = times.OrderBy(v => -v.Confidence);
                data.Page.PublishTime = ordered.First<PublishTime>();
            }
        }
    }
}