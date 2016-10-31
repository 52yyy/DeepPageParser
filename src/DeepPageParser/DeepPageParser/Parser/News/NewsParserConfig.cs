using System.Collections.Generic;

namespace DeepPageParser
{
    /// <summary>
    /// some data configurations that are used in the news parser
    /// </summary>
    public class NewsParserConfig
    {//http://www.cnautonews.com/xw/qy/201609/t20160910_492003.htm date 中并不是时间，去掉date
        public static string[] DefaultPubTimeControlIds ={ "pubTime","pubtime", 
															"pubtime_baidu","p_publishtime", "article-time", "articleTime", "datePublished","post_time_source","spanDateTime","story-time","addtime","conDate","timestamp","data-datetime","date date--v2","date-display-single","publish-online","pubdate"};
        public static string[] DefaultSourceControlIds = { "source", "jgname", "source_baidu", "laiy", "publisher" };

	    public static string[] DefaultPubTimeClueWords =
	    {
			"发表时间", "发稿时间", "发布时间", "时间:", "时间：", "发布日期", "日期：", "日期:","发表于",
			"发布于", "发布：","Last Updated","Updated:","時間","更新于","更新於","日期 "
		};
        public static string[] DefaultSourceClueWords = { "来源", "來源", "稿源" };
        public static string[] DefaultOtherClueWords = { " 作者", " 简介", " 编辑", " 责编", "字体:", "字体：", " 浏览次数", " 字号", " 浏览次数", "阅读次数：", "字体大小", "点击率", "点击量" };

        public static char[] DefaultUselessChars = { ';', '；', ':', '：', ' ', '|' ,'(','[',')',']'};
        public static char[] DefaultTimeChars = { ' ', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '/', '月', '日', '年', ':', '：', '分', '时', '秒', '点', 'p', 'm', 'a', 'P', 'M', 'A', ',', ',', '一', '二', '三', '四', '五', '六', '七', '八', '九', '十' };

        public static string[] DefaultEnglishiTimeWords =
        {
            "January", "February", "March", "April", "May",
            "June", "July ", "August", "September",  "October","November", "December"
        };

        public static string[] DefaultEnglishiTimeWordsAbbreviations =
        {
            "Jan", "Feb", "Mar", "Apr", "Sept", "Oct", "Nov",
            "Dec"
        };

        //==============添加==========================================
        private static readonly string[] DefaultCommentHeadWords = { "我来说两句", "来源：", "我要分享", "我要评论",
                                                                       "评论(","评论（", "参与评论(","分享到：","我有话说（" };
        private static readonly string[] DefaultCommentTailWords = { "评论", "条评论", "位网友发表评", "人参与", "回帖",
                                                                       "回复:", "位网友评论", "位网友参与评论" ,"人跟贴","評論"};
        private static readonly string[] DefaultVisitHeadWords = { "浏览数", "阅读", "阅读(", "阅读:", "查看: ", "点击数：",
                                                                     "关注:", "浏览次数："," 阅读：",
                                                                     "访问：", "阅读量:", "关注" ,"点击：", "查看数: "," 点击量："};
        private static readonly string[] DefaultVisitTailWords = { "参与", "收藏成功", "人表态", "浏览", "人浏览" };

        public List<string> HeadClueWords { get; set; }
        public List<string> TailClueWords { get; set; } 
        public string[] CommentTailWords { get; set; }
        public string[] CommentHeadWords { get; set; }
        public string[] VisitHeadWords { get; set; }
        public string[] VisitTailWords { get; set; }


        //=============================================================

        /// <summary>
        /// possible ids of the controls that may contain the publish time
        /// </summary>
        public string[] PubTimeControlIds { get; set; }

        /// <summary>
        /// possible ids of the controls that may contain the article source
        /// </summary>
        public string[] SourceControlIds { get; set; }

        /// <summary>
        /// the clue words that may indicate that start of the publish time
        /// </summary>
        public string[] PubTimeClueWords { get; set; }

        /// <summary>
        /// the clue words that may indicate the start of the article source
        /// </summary>
        public string[] SourceClueWords { get; set; }

        /// <summary>
        /// the clue words that may indicate the end of the publish time or the article source
        /// </summary>
        public string[] OtherClueWords { get; set; }

        /// <summary>
        /// useless characters that can be directly removed
        /// </summary>
        public char[] UselessChars { get; set; }

        public char[] TimeChars;

        public string[] EnglishTimeStrings;
        public string[] EnglishTimeStringsAbbr;

        public int NumNodeAfterMainContent = 50;
        public int NumNodeBeforeTitle = 50;

        public NewsParserConfig()
        {
            this.PubTimeControlIds = DefaultPubTimeControlIds;
            this.SourceControlIds = DefaultSourceControlIds;
            this.PubTimeClueWords = DefaultPubTimeClueWords;
            this.SourceClueWords = DefaultSourceClueWords;
            this.OtherClueWords = DefaultOtherClueWords;
            this.UselessChars = DefaultUselessChars;
            this.TimeChars = DefaultTimeChars;

            this.EnglishTimeStrings = DefaultEnglishiTimeWords;
            this.EnglishTimeStringsAbbr = DefaultEnglishiTimeWordsAbbreviations;
           
            ///++++++++++++++++++++++++++++++++++++++++++++
            this.CommentHeadWords = DefaultCommentHeadWords;
            this.CommentTailWords = DefaultCommentTailWords;
            this.VisitHeadWords = DefaultVisitHeadWords;
            this.VisitTailWords = DefaultVisitTailWords;
            this.HeadClueWords = new List<string>();
            this.TailClueWords = new List<string>();
            this.HeadClueWords.AddRange(DefaultCommentHeadWords);
            this.HeadClueWords.AddRange(DefaultVisitHeadWords);
            this.TailClueWords.AddRange(DefaultCommentTailWords);
            this.TailClueWords.AddRange(DefaultVisitTailWords);
            ///++++++++++++++++++++++++++++++++++++++++++++
        }
    }
}