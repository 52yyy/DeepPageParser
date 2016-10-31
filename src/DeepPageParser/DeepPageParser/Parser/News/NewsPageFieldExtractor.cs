using System.Collections.Generic;

using DeepPageParser;

namespace DeepPageParser
{
    /// <summary>
    ///		新闻页面字段提取类
    /// </summary>
    public abstract class NewsPageFieldExtractor
    {
        /// <summary>
        /// data config
        /// </summary>
        protected NewsParserConfig ParserConfig;

        protected NewsPageFieldExtractor(NewsParserConfig parserConfig)
        {
            this.ParserConfig = parserConfig;
        }

        public abstract void GetFieldValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,
            List<int> candidateNodeIdx);

        /// <summary>
        /// get article source information from raw text
        /// </summary>
        /// <param name="textOri"></param>
        /// <returns></returns>
        protected PageFieldDebug TryExtractField(string textOri, PageFieldType field, bool force)
        {
            PageFieldDebug debug = new PageFieldDebug();
            debug.FieldValue = textOri ?? string.Empty;

            if (string.IsNullOrEmpty(textOri))
            {
                return debug;
            }

            string text = textOri.Trim(ParserConfig.UselessChars);


            string[] startClues;
            string[] endClues;
            string[] otherClues;
            otherClues = ParserConfig.OtherClueWords;
            if (field == PageFieldType.Source)
            {
                startClues = ParserConfig.SourceClueWords;
                endClues = ParserConfig.PubTimeClueWords;
            }
            else
            {
                startClues = ParserConfig.PubTimeClueWords;
                endClues = ParserConfig.SourceClueWords;
            }
            // check whether have other clue
            foreach (string clue in otherClues)
            {
                if (text.Contains(clue.Trim()))
                {
                    debug.ContainsOtherClue = true;
                    break;
                }

            }

            //get the position of the clue word and remove the preceding string
            foreach (string clue in startClues)
            {
                int iClue = text.IndexOf(clue);
                if (iClue >= 0)
                {
                    text = text.Substring(clue.Length + iClue);
                    debug.ContainsClue = true;
                    break;
                }
            }


            int iSourceEnd = int.MaxValue;

            //check possible end position of article source information
            foreach (string clue in endClues)
            {
                int iTime = text.IndexOf(clue);
                if (iTime >= 0)
                {
                    if (iSourceEnd > iTime)
                    {
                        iSourceEnd = iTime;
                    }
                }
            }

            foreach (string clue in ParserConfig.OtherClueWords)
            {
                int iTime = text.IndexOf(clue);
                if (iTime >= 0)
                {
                    if (iSourceEnd > iTime)
                    {
                        iSourceEnd = iTime;
                    }
                }
            }

            if (iSourceEnd < text.Length && iSourceEnd > 0)
            {
                //	如果该段落的长度比结束符号在30个字符以内
                if (text.Length - iSourceEnd < 30)
                {
                    debug.TailValue = text.Substring(iSourceEnd);
                }
                text = text.Substring(0, iSourceEnd);
                debug.ContainsEndClue = true;
            }
            else if (iSourceEnd == 0)
            {
                //	如果该段落的长度比结束符号在30个字符以内
                if (text.Length - iSourceEnd < 30)
                {
                    debug.TailValue = text.Substring(iSourceEnd);
                }
                //nothing is extracted for this field
                text = string.Empty;
                debug.ContainsEndClue = true;
            }

            text = text.Trim(ParserConfig.UselessChars);

            debug.FieldValue = text;

            return debug;
        }


        /// <summary>
        /// get article source information from raw text
        /// </summary>
        /// <param name="textOri"></param>
        /// <returns></returns>
        /// 主要用于根据候选，找时间所在的区域
        protected PageFieldDebug TryExtractTimeField(string textOri, PageFieldType field, bool force)
        {
            //priority startClues >endClues >otherClues , return according to the priority
            PageFieldDebug debug = new PageFieldDebug();

            debug.FieldValue = textOri ?? string.Empty;
            if (string.IsNullOrEmpty(textOri))
            {
                return debug;
            }

            string text = textOri.Trim(ParserConfig.UselessChars);

            string[] startClues = ParserConfig.PubTimeClueWords;
            string[] endClues = ParserConfig.SourceClueWords;
            string[] otherClues = ParserConfig.OtherClueWords;

            // check whether have start clue
            //get the position of the clue word and remove the preceding string, at the same time ,return result
            
            foreach (string clue in startClues)
            {
                // 去掉一些干扰词.如果包含干扰词，跳出整个循环
                if (text.Contains("交战时间"))
                {
                    break;
                }
                int iClue = text.IndexOf(clue);
                if (iClue >= 0)
                {
                    text = text.Substring(clue.Length + iClue);
                    debug.ContainsClue = true;
                    text = text.Trim(ParserConfig.UselessChars);
                    debug.FieldValue = text;
                    return debug;
                }
            }


            // check whether have end clue
            //get the position of the clue word and remove the posterior string, at the same time ,return result
            //time string behind the end words, it will miss, so save it in tailvale
            foreach (string clue in endClues)
            {
                int iClue = text.IndexOf(clue);
                //	线索词在开始，如果该段落的长度比结束符号在30个字符以内
                if (iClue == 0 && text.Length - iClue < 30)
                {
                    debug.TailValue = text.Substring(clue.Length + iClue);
                    debug.FieldValue = string.Empty;
                    debug.ContainsEndClue = true;
                    return debug;
                }
                else if (iClue < text.Length && iClue > 0)
                {
                    debug.ContainsEndClue = true;
                    debug.FieldValue = text.Substring(0, iClue).Trim(ParserConfig.UselessChars); ;
                    //	如果该段落的长度比结束符号在30个字符以内
                    if (text.Length - iClue < 30)
                    {
                        debug.TailValue = text.Substring(iClue);
                    }
                    return debug;
                }
            }

            // check whether have other clue
            foreach (string clue in otherClues)
            {
                if (text.Contains(clue.Trim()))
                {
                    debug.ContainsOtherClue = true;
                    text = text.Trim(ParserConfig.UselessChars);
                    debug.FieldValue = text;
                    return debug;
                }
            }
            return debug;
        }
    }

}