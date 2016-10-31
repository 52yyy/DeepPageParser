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
    ///		����ʱ����ȡ�࣬ʹ��״̬ģʽʵ��
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
        //����������ַ��ʱ�䣬ϣ�����ǣ��Ժ��ܹ�֪������ַ���Ƿ�Ϊ������ַ���ڵ��ú���ʱ��������ַʹ��ParseEnglishTime��������ַʹ�� ParseTime��
        public PublishTime ParseEnglishTime(string text)
        {
            int monthstart = 0;
            int monthend = 0;
            int timestart = 0;
            int timeend = 0;
            PublishTime publishTime = new PublishTime();
            bool hasenglishmonth = false;
            //�������Ŀ�ʼ�ң����û����ȥ����д�������д�������ķ���һ��October�����ҳ���д�����´���
            foreach (string clue in ParserConfig.EnglishTimeStrings)
            {
                monthstart = text.IndexOf(clue);
                //	��ʱ��
                if (monthstart>= 0)
                {
                    hasenglishmonth = true;
                    monthend = monthstart + clue.Length;
                    break;
                }
            }
            //����û���ҵ�������д��
            if (!hasenglishmonth)
            {
                foreach (string clue in ParserConfig.EnglishTimeStringsAbbr)
                {
                    monthstart = text.IndexOf(clue);
                    //	��ʱ��
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
                    //ֻ֧��ȥ��һ��
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
            //���һ�����ܹ��ǵ� ����2016-09-22 10:36 ����� �ᱻȡ��2016-09-22 10:36 ��
            if (textLength > 1 && text[textLength - 1] == '��')
            {
                text = text.Substring(0, textLength - 1);
            }
            //���һ��������ǡ��֡���DateTime.TryParse(text, out dt)���ܹ���ȷ������ʱ�䣬�磺2016��9��21�� 17��25�� ����Ҫת��Ϊ2016��9��21�� 17:25
            //����publishTime.TimeString���ʱ����Ȼ�Ǵ�ŵ�2016��9��21�� 17��25�֣���Ϊ������Ȼ�Ӱ�����Ŷȵļ���
            // ������ַ��http://www.financeun.com/News/2016921/2013cfn/172610126800.shtml
            //yyyy��MM��dd�ո�ʽ  �����Բ���������գ�
            //XXʱXX��XX���ʽ���ɲ����룩

            //�볢�����򣬵���ʧ�ܣ��ñȽϲڵķ�ʽʵ��
            if (textLength > 15 && text.Contains("��") && text.Contains("��") && !text.Contains("��"))//������16���ַ����ſ������˴����硰34�֡��������Ķ��ַ���û�б�Ҫ�����ķ�ȥ������Ϊ����������������һ����Ч��ʱ��
            {
                text = text.Replace("��", ":").Replace("��", "");
            }
            //Ϊ��������, ���� 18, 2016 �����ҳɡ���, ���� 18, 2016�������ߡ�������  ���� 18, 2016 �����ҳɡ��� ���� 18, 2016�����߼� ȥ���ո���߶���ǰ������ݡ�
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
            //Ϊʱ�䣺���� 18, 2016 �����˿���Ϊʱ��Ĵ�"��". ���ǿ��ܳ��֣������� 18, 2016�������� ��������������������� 18, 2016������������Ҫ��ȥһ�����õĴʣ��ѡ�����ȥ����
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
                    if (text[iStart + 1] == ':' || text[iStart + 1] == '��')
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
            publishTime.TimeString = text;   //����publishTime.TimeString���ʱ����Ȼ�Ǵ�ŵ�2016��9��21�� 17��25�֣���Ϊ������Ȼ�Ӱ�����Ŷȵļ��㣬���Ǻ���������ַ�����һЩ���⴦��
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
                if (tmp == ' ' || tmp == '��' || tmp == 160)
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


                  //  text = "2016��9��21�� 17��25��";

                    //   text = " August 10, 2016";

                    // �����ݽڵ��ҷǿ�
                    if (data.OriginalNodeList[i].NodeType == HtmlNodeType.Text && !String.IsNullOrEmpty(text))
                    {

                        //���������ʲ���
                        var debug = TryExtractTimeField(text, PageFieldType.PublishTime, false);
                        // ��ǰƥ�䣬��ֵ
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
                        // ��ǰƥ�䣬�к�ƥ�䣬��ǰ�����β����Ϣ��ʱ��
                        else if (debug.ContainsEndClue && (!string.IsNullOrEmpty(debug.FieldValue) ||!string.IsNullOrEmpty(debug.TailValue)))
                        {
                            //���������Ż���վ��www.gov.cn���� 2012��07��12�� 08ʱ56�֡��� ��Դ������Ժ�칫��
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
                        //��ǰƥ�䣬�޺�ƥ�䣬�����������ʣ�һ��һ���ַ�����Ϣ
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
                        //�н�β�����������ʣ�����û���ҵ�ʱ����Ϣ����ǰ�Ҹ��ı��ڵ㣬�����ҵ�������������ݣ����ŶȺ��ҵ�λ���й�ϵ

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
                        //�������ʣ�����û���ҵ�ʱ����Ϣ���Һ����4���ǿ��ı���㣬�п����ҵ������еĴʣ����Ŷ��������ҵ�λ���й�,�����ֳ��ȴ���һ��ֵʱ(����ֵ30)����Ϊ�ҹ��ˣ��ҵ��������е����ݡ�
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
                        //�ҿ�����©�ĵ����Ŷȵ�ʱ��
                        //	���β�ͻ�����Ϣ������ʱ���������β
                        //Ӧ�ÿ���ɾ��������Ϊֻ����endclue����β�� ��Ϣ�������ǲ��ֽ����˴���
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
                                    if (debug.TailValue[idx] == ' ' || debug.TailValue[idx] == '��')
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

                                if (!pt.ValidTime)//��������Ѱ����Ч������Ӣ�Ĳ���
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
                                for (int nexti = i + 1; i < data.OriginalNodeList.Count; nexti++) //����ʱ���Ƿ���������ڵ���,ƴ��ʱ�� ʱ��Ϊ��ʱ�䣨û�г�ʱ����������ʣ�
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
                        else //������û�к�ѡ�ľ����У�����ʱ��
                        {
                           var pt = Findtimebychar(text);
                            if (pt != null && pt.ValidTime)
                            {
                                pt.Confidence = 1.0 * pt.TimeString.Length;
                                times.Add(pt);
                                matched = true;
                            }
                        }
                        if (!matched)//���水�����ĵķ�ʽû���ҵ�ʱ�䣬��Ӣ����
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