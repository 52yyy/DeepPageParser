namespace DeepPageParser
{
	/// <summary>
	///		失败的页面解析结果
	/// </summary>
	public class FailDeepParseExecuteResult : DeepParseExecuteResult
	{
		/// <summary>
		///		失败的页面解析结果
		/// </summary>
		/// <param name="errorId">内部提供的错误编号</param>
		/// <param name="error">错误信息</param>
		public FailDeepParseExecuteResult(int errorId, string error = null)
			: base(ExecuteStatus.Failed, null, error)
		{
			this.ErrorId = errorId;
		}
	}
}