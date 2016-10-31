using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepPageParser
{
	/// <summary>
	///		页面解析抽象类
	/// </summary>
	public abstract class PageParser
	{
		private static readonly Dictionary<int, PageParser> pageParsers = new Dictionary<int, PageParser>();
	    protected static bool IsParseVisitInfo = false;
	    protected static bool IsParsePublicTime = false;
	    protected static bool IsParseSource = false;

		public static PageParser GetParser(PageType type)
		{
			PageParser parser;
			lock (pageParsers)
			{
				if (pageParsers.TryGetValue(type.TypeCode, out parser))
				{
				}
				else
				{
					switch (type.TypeCode)
					{
						case 1:
							parser = new EmptyPageParser();
							break;
						case 101:
							parser = new NewsPageParser();
							break;
						case 102:
							parser = new ForumPageParser();
							break;
						case 103:
							parser = new WeixinPageParser();
							break;
						default:
							parser = new EmptyPageParser();
							break;
					}
					pageParsers[type.TypeCode] = parser;
				}
			}
			return parser;
		}

		public abstract IDeepParseExecuteResult Parse(PendingAnalysisPage pendingAnalysisPage);

	    public PageParser EnableParseVisitInfo(bool enable)
	    {
	        IsParseVisitInfo = enable;
	        return this;
	    }

	    public PageParser EnableParsePublicTime(bool enable)
	    {
	        IsParsePublicTime = enable;
	        return this;
	    }

	    public PageParser EnableParseSource(bool enable)
	    {
	        IsParseSource = enable;
	        return this;
	    }
	}
}
