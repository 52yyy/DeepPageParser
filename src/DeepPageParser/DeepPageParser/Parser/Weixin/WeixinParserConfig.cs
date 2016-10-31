using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepPageParser
{
   public class WeixinParserConfig
    {
        private static readonly HashSet<string> DefaultFilterTags = new HashSet<string>()
	    {
	        "小编有话说",
	        "大家都在看",
	        "推荐阅读",
	        "微信大号公布",
	        "阅读原文",
	        "（本文内容来源于网络 版权归原作者所有）",
	        "版权归原创者所有，如有侵权请及时联系！",
	        "本文版权系原作者所有，如有侵权，请联系我们。"
	        ,
	        "点击下方“阅读原文”查看更多",
	        "长按图片识别二维码加微信：",
	        "▲长按二维码“识别”关注",
	        "版权归原作者 如有侵权请与我们联系",
           "版权归作者所有 如有侵权请与我们联系",
           "更多原创新车试驾（视频）" // 总觉得这样加规则不是个办法呀。
	    };

        //private static readonly List<string> DefaultRules1 = new List<string>() { "版权", "侵权", "联系" };

        //private static readonly List<string> DefaultRules2 = new List<string>() { "长按", "二维码" };

        //private static readonly List<string> DefaultRules3 = new List<string>() { "点击", "查看更多" };


        public HashSet<string> FilterTags { get; set; }

        //public List<List<string>> FilterRules { get; set; }

        public WeixinParserConfig()
        {
            this.FilterTags = DefaultFilterTags;
            //this.FilterRules = new List<List<string>>();
            //this.FilterRules.Add(DefaultRules1);
            //this.FilterRules.Add(DefaultRules2);
            //this.FilterRules.Add(DefaultRules3);
        }
    }
}
