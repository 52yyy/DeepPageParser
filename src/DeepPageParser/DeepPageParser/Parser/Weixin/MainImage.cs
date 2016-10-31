namespace DeepPageParser
{
	/// <summary>
	///		微信正文图片
	/// </summary>
	public class MainImage
	{
		/// <summary>
		///		图片地址，img节点的src属性值
		/// </summary>
		public string Src { get; set; }

		/// <summary>
		///		图片在正文中的索引位置
		/// </summary>
		public int IndexOfMainContent { get; set; }
	}
}