using System;

namespace DeepPageParser
{
	/// <summary>
	///		空页面解析类
	/// </summary>
	public class EmptyPageParser : PageParser
	{
		public override IDeepParseExecuteResult Parse(PendingAnalysisPage pendingAnalysisPage)
		{
			return DeepParseExecuteResult.Fail();
		}
	}
}