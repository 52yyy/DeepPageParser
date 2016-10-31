namespace DeepPageParser
{
	/// <summary>
	///		深度页面解析结果接口
	/// </summary>
	public interface IDeepParseExecuteResult
	{
		/// <summary>
		///		获取深度页面解析执行状态
		/// </summary>
		ExecuteStatus Status { get; }

		/// <summary>
		///		获取内部系统的错误编号
		/// </summary>
		int ErrorId { get; set; }

		/// <summary>
		///		获取错误信息
		/// </summary>
		string Error { get; }

		/// <summary>
		///		获取深度页面解析信息
		/// </summary>
		/// <typeparam name="T">派生自深度页面解析信息类</typeparam>
		/// <returns></returns>
		T GetResult<T>() where T : DeepPageParsedInfo;
	}
}