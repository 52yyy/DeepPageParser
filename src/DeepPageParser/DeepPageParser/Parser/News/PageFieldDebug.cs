namespace DeepPageParser
{
	/// <summary>
	///		页面字段提取调试类
	/// </summary>
	public class PageFieldDebug
	{
		/// <summary>
		///		关键区域，提示词之间的区域
		/// </summary>
		public string FieldValue { get; set; }

		/// <summary>
		///		尾巴区域，结束提示词后面的区域
		/// </summary>
		public string TailValue { get; set; }

		/// <summary>
		///		包含提示词
		/// </summary>
		public bool ContainsClue { get; set; }

		/// <summary>
		///		包含结束提示词
		/// </summary>
		public bool ContainsEndClue { get; set; }

        /// <summary>
        ///		包含其他提示词
        /// </summary>
        public bool ContainsOtherClue { get; set; }
	}
}