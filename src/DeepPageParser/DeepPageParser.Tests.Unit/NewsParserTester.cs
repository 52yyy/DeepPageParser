using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeepPageParser;

using NUnit.Framework;

namespace Gridsum.NLP.DeepPageParserLibTest.NewVersionDesign
{
    /// <summary>
    ///		用于测试新闻页面解析器
    /// </summary>
    /// 
    [TestFixture]
    public class NewsParserTester
    {
        public void contentparse(string url)
        {
            // init
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseVisitInfo(true);
            // parse
            PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
            IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

            // print
            // print
            Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
            if (result.Status == ExecuteStatus.Succeed)
            {
                var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
        //        Console.WriteLine(deepPageParsedInfo.ArticleTitle);
         //       Console.WriteLine(deepPageParsedInfo.PublishTime.TimeValue);
             //   Console.WriteLine(deepPageParsedInfo.Source.Name);
                Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                Console.WriteLine("==========");
            }
        }
	    [Test]
	    [Description("测试功能")]
	    public void Example()
	    {
			// init
		    PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();
		    PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseVisitInfo(true);

		    string url = "网页URL";
		    string html = "网页源码";

			// parse
		    PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url, html);
		    IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

			// print
		    if (result.Status == ExecuteStatus.Succeed)
		    {
			    var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
			    Console.WriteLine("阅读数：{0}", deepPageParsedInfo.VisitNumber.Value);
			    Console.WriteLine("评论数：{0}", deepPageParsedInfo.CommentNumber.Value);
		    }
	    }


	    [Test]
        [Description("测试功能")]
        public void CommentVisitNumCase()
        {
            string url = @"http://news.163.com/16/0328/20/BJ966H26000156PO.html";
            string[] files = Directory.GetFiles(Environment.CurrentDirectory+"/data");
            var builder = new PendingAnalysisPageBuilder();  
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseVisitInfo(true);

            foreach (string path in files)
            {
                var reader = new StreamReader(path, Encoding.UTF8);
                string htm = reader.ReadToEnd();

                PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url, htm);
                IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

                Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
                if (result.Status == ExecuteStatus.Succeed)
                {
                    var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                    Console.WriteLine("阅读数：{0}" ,deepPageParsedInfo.VisitNumber.Value);
                    Console.WriteLine("评论数：{0}", deepPageParsedInfo.CommentNumber.Value);
                }
            }
        }

	    [Test]
		[Description("测试功能")]
		public void MultiThreadCase()
		{
			// parser init 
			var builder = new PendingAnalysisPageBuilder();
			PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseVisitInfo(true);

			// data init
			string url = @"http://news.163.com/16/0328/20/BJ966H26000156PO.html";
			string[] files = Directory.GetFiles(Environment.CurrentDirectory + "/data");
			List<string> htmls = files.Select(path => new StreamReader(path, Encoding.UTF8)).Select(reader => reader.ReadToEnd()).ToList();
			//htmls.RemoveAt(0);

			// parse
			int[] visits = new int[htmls.Count];
			int[] comments = new int[htmls.Count];
			Parallel.For(
				0,
				htmls.Count,
				i =>
				{
					PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url, htmls[i]);
					IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);
					if (result.Status == ExecuteStatus.Succeed)
					{
						var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
						visits[i] = deepPageParsedInfo.VisitNumber.Value;
						comments[i] = deepPageParsedInfo.CommentNumber.Value;
					}
					else
					{
						visits[i] = -999;
						comments[i] = -999;
					}
				});

			// print
			for (int i = 0; i < htmls.Count; i++)
			{
				Console.WriteLine("阅读数：{0}", visits[i]);
				Console.WriteLine("评论数：{0}", comments[i]);
				Console.WriteLine();
			}
		}

        [Test]
        [Description("测试功能")]
        public void ToyCase()
        {
			//string url = @"http://www.pcauto.com.cn/qcbj/836/8367975.html";
			string url = @"http://news.ncnews.com.cn/money/";
			//string url = @"http://www.autohome.com.cn/news/201606/890002.html";
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParsePublicTime(true).EnableParseSource(true);  // 具体的页面解析器，全局应该只有一个

            // parse
            PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
            IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

            // print
            Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
            if (result.Status == ExecuteStatus.Succeed)
            {
                var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                Console.WriteLine(deepPageParsedInfo.PublishTime.TimeValue);
                Console.WriteLine(deepPageParsedInfo.Source.Name);
                Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                Console.WriteLine("==========");
            }
        }


        private void ContentParse(string url)
        {
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseSource(true).EnableParsePublicTime(true);  // 具体的页面解析器，全局应该只有一个
           // parse
                PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
                IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

                // print ExecuteStatus.Succeed
                Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
                if (result.Status == ExecuteStatus.Succeed)
            {
                var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                Console.WriteLine("==========");
                Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                Console.WriteLine("==========");
            }
        }

        private void ContentParse(string url ,string htmlfile)
        {
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParseSource(true).EnableParsePublicTime(true);  // 具体的页面解析器，全局应该只有一个
            // parse
            string html = File.ReadAllText(htmlfile);
            PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url,html);
            IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

            // print ExecuteStatus.Succeed
            Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
            if (result.Status == ExecuteStatus.Succeed)
            {
                var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                Console.WriteLine("==========");
                Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                Console.WriteLine("==========");
            }
        }

        private string ContentParsereturn(string url)
        {
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News);  // 具体的页面解析器，全局应该只有一个
            // parse
            string parsecontent = "wrong";
            try
            {
                PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
                IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

                if (result.Status == ExecuteStatus.Succeed)
                {
                    var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                    parsecontent = deepPageParsedInfo.ArticleMainBody;
                    //Assert.IsTrue(deepPageParsedInfo.ArticleMainBody.Length > 20 && Math.Abs(deepPageParsedInfo.ArticleMainBody.Length - content.Length) < 10);//简单的认为和原始的存的范围为10，并且正文的长度要大于20
                }
            }
            catch (Exception)
            {
                parsecontent = "wrong";
            }
            return parsecontent;
        }



        [Test]
        [Description("正文")]
        public void ContentCase()
        {
            //string url = @"http://www.pcauto.com.cn/qcbj/836/8367975.html";
           // string url = @"http://news.ncnews.com.cn/money/";
            string url = @"http://news.ncnews.com.cn/money/tt/2016-10/17/content_1668593.htm";
            //string url = @"http://www.autohome.com.cn/news/201606/890002.html";
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News).EnableParsePublicTime(true).EnableParseSource(true);  // 具体的页面解析器，全局应该只有一个

            // parse
            PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
            IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

            // print
            Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
            if (result.Status == ExecuteStatus.Succeed)
            {
                var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                Console.WriteLine(deepPageParsedInfo.PublishTime.TimeValue);
                Console.WriteLine(deepPageParsedInfo.Source.Name);
                Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                Console.WriteLine("==========");
            }
        }

        [Test]
        [Description("英文网站新闻页面解析")]
        public void ForeignSiteCase()
        {
            // init
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News);  // 具体的页面解析器，全局应该只有一个
            List<string> urls = new List<string>
								{
									@"http://www.globaltimes.cn/content/968681.shtml",
									@"http://www.shanghaidaily.com/metro/Cash-prize-amount-for-food-safety-whistleblowers-raised-to-300000-yuan/shdaily.shtml",
									@"http://www.chinadaily.com.cn/china/2016-02/16/content_23494443.htm",
									@"http://www.chinadaily.com.cn/business/2016-02/16/content_23504958.htm",
								};

            foreach (string url in urls)
            {
                // parse
                PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
                IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

                // print
                Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
                if (result.Status == ExecuteStatus.Succeed)
                {
                    var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                    Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                    Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                    Console.WriteLine("==========");
                }
            }
        }

        [Test]
        [TestCase("http://www.bbc.com/news/uk-england-birmingham-37625378","E:\\htmlcontenttest\\overseas1.html")]
        [Description("标题:Racing drivers jailed over '130mph' crash in Birmingham 文章结尾：Both men have also been disqualified from driving for two years. ")]
        public void overseas_case1f(string url,string filename )
        {
            ContentParse(url,filename);
        }

        [Test]
        [TestCase("http://www.shx.chinanews.com/news/2016/1024/58087.html")]
        public void overseas_case1(string url)
        {
            ContentParse(url);
        }



        [Test]
        [TestCase("https://www.washingtonpost.com/politics/as-trump-stumbles-clinton-weighs-a-striking-choice-expand-the-map-or-stick-to-the-plan/2016/10/16/f0f77470-93a7-11e6-bb29-bf2701dbe0a3_story.html?hpid=hp_hp-top-table-main_electoralmap-7pm%3Ahomepage%2Fstory", "E:\\htmlcontenttest\\overseas_case2.html")]
        [Description("标题:As Trump stumbles, Clinton weighs a striking choice: Expand the map or stick to the plan 文章结尾：Clinton in Cleveland on Friday. ")]
        public void overseas_case2f(string url, string filename)
        {
            ContentParse(url, filename);
        }

        [Test]
        [TestCase("https://www.washingtonpost.com/politics/inside-donald-trumps-echo-chamber-of-conspiracies-grievances-and-vitriol/2016/10/16/1c3c6a72-921e-11e6-9c85-ac42097b8cc0_story.html?hpid=hp_hp-top-table-main_echochamber-810pm%3Ahomepage%2Fstory", "E:\\htmlcontenttest\\overseas3.html")]
        [Description("标题:Inside Donald Trump’s echo chamber of conspiracies, grievances and vitriol 文章结尾：Carson said. “They saw them as one establishment, and they put the media together with it.” ")]
        public void overseas_case3f(string url, string filename)
        {
            ContentParse(url, filename);
        }


        [Test]
        [TestCase("http://news.sohu.com/20161014/n470226128.shtml")]
        public void common_case1(string url)
        {
            ContentParse(url);
        }
       
        [Test]
        [TestCase(" https://xueqiu.com/4043855103/75954905")]
        public void xueqiu_case1(string url)
        {
            ContentParse(url);
        }
        
        [Test]
        [TestCase(" https://xueqiu.com/S/SZ300104/76004368")]
        public void xueqiu_case2(string url)
        {
            ContentParse(url);
        }

        [Test]
        [TestCase(" https://xueqiu.com/p/ZH190715")]
        [Description("不应该被解出来")]
        public void xueqiu_case3(string url)
        {
            ContentParse(url);
        }

        [Test]
        [TestCase(" https://xueqiu.com/5124430882/75986212")]
        public void xueqiu_case4(string url)
        {
            ContentParse(url);
        }

       
         [Test]
         [TestCase(" https://xueqiu.com/8152922548/76190320")]
        public void xueqiu_case5(string url)
        {
            ContentParse(url);
        }

       
         [Test]
         [TestCase(" https://xueqiu.com/today")]
        [Description("不应该被解出来，按照原来的逻辑会被解出来")]
        public void xueqiu_case6(string url)
        {
            ContentParse(url);
        }

         [Test]
         [TestCase(" https://xueqiu.com/8584666563/76181691")]
         public void xueqiu_case7(string url)
         {
             ContentParse(url);
         }

         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\test.html")]
         [Description("自己写的简单html")]
         public void simple_test(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }


         [Test]
         [TestCase("http://news.cnfol.com/huiyihuodong/20140708/18338502.shtml", "E:\\newsparser\\database\\database7777.html")]
         public void database7777(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://bbs.cnhubei.com/thread-4112449-1-2.html", "E:\\newsparser\\database\\database1494.html")]
         public void database1494(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://zgsc.china.com.cn/gz/2016-10-18/541523.html", "E:\\newsparser\\database\\database12109.html")]
         public void database12109(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://legal.china.com.cn/fzgc/2016-10/18/content_39512255.htm", "E:\\newsparser\\database\\database3620.html")]
         public void database3620(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://bbs.cnhubei.com/forum.php?mod=viewthread&tid=4114794", "E:\\newsparser\\database\\database12624.html")]
         public void database12624(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://dealer.chexun.com/zajbqcmy/News-2/article-837061.html", "E:\\newsparser\\database\\database6011.html")]
         public void database6011(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://www.fxrbs.cn/portal.php?mod=view&aid=61935")]
         public void database1141(string url)
         {
             ContentParse(url);
         }


         [Test]
         [TestCase("http://bbs.cnhubei.com/thread-4112925-1-5.html", "E:\\newsparser\\database\\database12951.html")]
         public void database12951(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://book.163.com/09/1126/10/5P1P9B9400923IN9.html", "E:\\newsparser\\database\\database12247.html")]
         public void database12247(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://stock.caijing.com.cn/20161018/4187555.shtml", "E:\\newsparser\\database\\database9924.html")]
         public void database9924(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://fund.sohu.com/20161018/n470565002.shtml", "E:\\newsparser\\database\\database5156.html")]
        [Description("乱码")]
         public void database5156(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://mil.sohu.com/20160718/n461082827.shtml", "E:\\newsparser\\database\\database8187.html")]
        [Ignore("下载下来就会错")]
         public void database8187(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://mil.sohu.com/20160718/n461082827.shtml")]
         public void database8187nofile(string url)
         {
             ContentParse(url);
         }

         [Test]
         [TestCase("http://blog.chinadaily.com.cn/thread-1873359-1-1.html", "E:\\newsparser\\database\\database45.html")]
        [Ignore("英文论坛 删掉吧")]
         public void database45(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://mil.sohu.com/20160718/n461082827.shtml", "E:\\newsparser\\database\\database6934.html")]
        [Ignore("没有下载")]
         public void database6934(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://bbs.cnhubei.com/thread-4106206-1-3.html", "E:\\newsparser\\database\\database7141.html")]
        [Ignore("bbs")]
         public void database7141(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://lvyou.yuduxx.com/cjy/486800.html", "E:\\newsparser\\database\\database14115.html")]
        [Ignore("没有解决，文章中短的不一定有标点符号，会去掉一些内容 待解决")]
         public void database14115(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://lvyou.yuduxx.com/cjy/486800.html")]
         public void database14115nofile(string url)
         {
             ContentParse(url);
         }
      
         [Test]
         [TestCase("http://you.ctrip.com/travels/jiaohe2471/3184106.html")]
        [Ignore("待解决 图片，还有一段的文字解决")]
         public void database14827nofile(string url)
         {
             ContentParse(url);
         }

         [Test]
         [TestCase("http://bank.cngold.org/c/2016-10-18/c4491915.html")]
         public void database643nofile(string url)
         {
             ContentParse(url);
         }
         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newoverseas1.html")]
         [Description("布鲁金斯-话题-中国-博文 标题：陆克：英国脱欧后的中欧关系	结尾：被包括在内。可以解析")]
         public void new_overseas1(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newoverseas2.html")]
         [Description("德国之声-在线报道-经济纵横 可能找不到 标题：会着火的三星 赤裸裸的“烧钱” 尾部勉勉强强吧，感觉去的不是特别好")]
         public void new_overseas2(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }


         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newsoverseas3.html")]
         [Description("德国之声-在线报道-评论分析 可以啦")]
         public void new_overseas3(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }



         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newsoverseas4.html")]
         [Description("德国之声-在线报道-时政风云 标题：家藏2亿现金 “能源五虎”之首被判死缓 结尾勉勉强强。")]
         public void new_overseas4(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newsoverseas5.html")]
         [Description("标题：After the first two debates, what do Chinese people think about Clinton and Trump? 正文的头把发布时间什么的包括进去啦。还是那个合并超链接的问题，测试下吧 ")]
         public void new_overseas5(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

 
         [Test]
         [TestCase("https://www.brookings.edu/research/the-trans-pacific-partnership-the-politics-of-openness-and-leadership-in-the-asia-pacific-2/", "E:\\newsparser\\overseas\\newsoverseas6.html")]
         [Description("布鲁金斯-话题-全球贸易， 可以啦，差不多啦。")]
         public void new_overseas6(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://carnegieindia.org/2016/10/18/raja-mandala-power-and-principle-pub-64888", "E:\\newsparser\\overseas\\newsoverseas7.html")]
         [Description("卡内基-地区-中国    标题：Raja Mandala: Power and Principle 尾部：above the presumed power of principle.")]
         public void new_overseas7(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("https://piie.com/experts/peterson-perspectives/chinas-reliance-state-owned-enterprises-poses-growth-risks", "E:\\newsparser\\overseas\\newsoverseas8.html")]
         [Ignore("国际经济研究所-研究-中国 错误 这是个视频啊")]
         public void new_overseas8(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("https://piie.com/blogs/china-economic-watch/chinas-rebalance-reflected-rising-wage-share-gdp", "E:\\newsparser\\overseas\\newsoverseas9.html")]
         [Description("国际经济研究所-博客-中国经济观察 标题：China’s Rebalance Reflected in Rising Wage Share of GDP 结尾：factor supporting rebalancing.  很奇怪开头加了超链接的文字居然被正确解出来啦")]
         public void new_overseas9(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://blogs.wsj.com/chinarealtime/2016/10/19/economists-react-as-china-growth-concerns-ease-risks-rise/", "E:\\newsparser\\overseas\\newsoverseas10.html")]
         [Ignore("华尔街日报-实时中国-分类-经济商业 看起来解不了，在time里面  没有这个文件？这个需要订阅，下不全")]
         public void new_overseas10(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://www.bbc.com/sport/football/37667541", "E:\\newsparser\\overseas\\newsoverseas11.html")]
         [Description("美国企业研究所-标签-美中关系 网页没有进去,没有下载 标题：Charlton v Coventry stopped after plastic pigs thrown on pitch 开头少了一句，结尾多了一句，是图片的备注")]
         public void new_overseas11(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://www.rand.org/pubs/external_publications/EP66675.html", "E:\\newsparser\\overseas\\newsoverseas12.html")]
         [Description("兰德公司-话题-中国 标题》China's Medical Savings Accounts  research clients and sponsors. 副标题及周围内容被解到了正文")]
         public void new_overseas12(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://www.pbs.org/newshour/bb/can-china-use-the-slowdown-to-change-its-economy/", "E:\\newsparser\\overseas\\newsoverseas13.html")]
         [Ignore("下载下来的有问题")]
         public void new_overseas13(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://www.pbs.org/newshour/bb/can-china-use-the-slowdown-to-change-its-economy/")]
         [Description("布鲁金斯-作者-李侃如-研究 new_overseas13直接网页抓取")]
         public void new_overseas13nofile(string url)
         {
             ContentParse(url);
         }

         [Test]
         [TestCase("http://www.rand.org/pubs/external_publications/EP66675.html", "E:\\newsparser\\overseas\\newsoverseas14.html")]
         [Description("布鲁金斯-作者-李侃如-文章 China's Medical Savings Accounts 副标题解到了标题中 副标题有subtitle")]
         public void new_overseas14(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("https://www.brookings.edu/experts/jeffrey-a-bader/", "E:\\newsparser\\overseas\\newsoverseas15.html")]
         [Description("布鲁金斯-作者-杰弗里拜德 没有时间 标题就为作者吧，也说得过去")]
         public void new_overseas15(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("https://www.brookings.edu/blog/future-development/2016/09/30/the-renminbi-rises-but-is-no-match-for-the-dollar/", "E:\\newsparser\\overseas\\newsoverseas16.html")]
         [Description("布鲁金斯-作者-普拉德-发文 开头多了一行，不影响，勉强算正文")]
         public void new_overseas16(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("https://www.bloomberg.com/view/articles/2016-10-19/who-wins-if-the-u-s-withdraws-china", "E:\\newsparser\\overseas\\newsoverseas17.html")]
         [Description("布鲁金斯-作者-普拉德-发文 可以")]
         public void new_overseas17(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://www.rand.org/blog/2016/09/central-bank-perversity.html", "E:\\newsparser\\overseas\\newsoverseas18.html")]
         [Description("兰德公司-人员-查尔斯·沃尔夫-评论文章 可以")]
         public void new_overseas18(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("http://belfercenter.ksg.harvard.edu/publication/26859/can_a_rebuked_china_manage_its_anger.html?breadcrumb=%2Ftopic%2F172%2Fchinas_economy", "E:\\newsparser\\overseas\\newsoverseas19.html")]
         [Description("哈佛贝尔弗尔研究中心-话题-中国经济 双引号和单引号编码错误")]
         public void new_overseas19(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }
         [Test]
         [TestCase("https://theconversation.com/despite-china-free-trade-agreement-australian-beef-producers-are-missing-out-64316", "E:\\newsparser\\overseas\\newsoverseas20.html")]
         [Description("对话-文档-詹姆斯-文章 把右边的东西又弄进去啦")]
         public void new_overseas20(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("https://theconversation.com/chinas-stock-market-is-so-unstable-even-the-government-cant-control-it-45457", "E:\\newsparser\\overseas\\newsoverseas21.html")]
         [Description("对话-文档-迈克尔-文章")]
         public void new_overseas21(string url, string htmlfile)
         {
             ContentParse(url, htmlfile);
         }

         [Test]
         [TestCase("http://dept3.jingmen.gov.cn/Html/jmlyj/jmlyj_yjbg/2010-5/6/0840481065.html")]
         [Description("待解决，时间，标题，正文")]
         public void linye(string url)
         {
             ContentParse(url);
         }

        [Test]
        [TestCase(" https://www.washingtonpost.com/national/the-white-flight-of-derek-black/2016/10/15/ed5f906a-8f3b-11e6-a6a3-d50061aa9fae_story.html?hpid=hp_hp-top-table-main_stormfront-658am%3Ahomepage%2Fstory")]
        [TestCase("https://www.washingtonpost.com/politics/inside-donald-trumps-echo-chamber-of-conspiracies-grievances-and-vitriol/2016/10/16/1c3c6a72-921e-11e6-9c85-ac42097b8cc0_story.html?hpid=hp_hp-top-table-main_echochamber-810pm%3Ahomepage%2Fstory")]
        public void temp(string url)
        {
            Uri uri;
            string host = string.Empty;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                host = uri.Host.ToLower();
            }
            Console.WriteLine(host);
            Assert.IsTrue(host == "www.washingtonpost.com");
            
        }

        [Test]
        [Description("英文网站新闻页面解析")]
        public void tempCase()
        {
            // init
            PendingAnalysisPageBuilder builder = new PendingAnalysisPageBuilder();  // Dom树创建器，应该只有一个
            PageParser pageParser = PageParser.GetParser(ContentPageType.News);  // 具体的页面解析器，全局应该只有一个
            List<string> urls = new List<string>
								{
									@"http://news.163.com/16/0922/09/C1IC90R900014PRF.html",
									@"http://news.163.com/16/0721/12/BSGHHVLK00011229.html",
								};

            foreach (string url in urls)
            {
                // parse
                PendingAnalysisPage pendingAnalysisPage = builder.BuildDomTree(url);
                IDeepParseExecuteResult result = pageParser.Parse(pendingAnalysisPage);

                // print
                Assert.IsTrue(result.Status == ExecuteStatus.Succeed);
                if (result.Status == ExecuteStatus.Succeed)
                {
                    var deepPageParsedInfo = result.GetResult<NewsDeepParsedInfo>();
                    Console.WriteLine(deepPageParsedInfo.ArticleTitle);
                    Console.WriteLine(deepPageParsedInfo.ArticleMainBody);
                    Console.WriteLine("==========");
                }
            }
        }

    }
}
