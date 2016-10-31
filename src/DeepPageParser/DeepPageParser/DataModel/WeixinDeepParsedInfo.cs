using System.Collections.Generic;

namespace DeepPageParser
{
	/// <summary>
	///		微信页面深度解析结果
	/// </summary>
	public class WeixinDeepParsedInfo : DeepPageParsedInfo
	{
		public WeixinDeepParsedInfo()
		{
			this.MainImages = new List<MainImage>();
		}

		public IEnumerable<MainImage> MainImages { get; set; }

		public string MainContent { get; set; }
	}
}