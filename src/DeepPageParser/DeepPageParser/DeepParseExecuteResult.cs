namespace DeepPageParser
{
	/// <summary>
	///		深度页面解析结果
	/// </summary>
	public class DeepParseExecuteResult : IDeepParseExecuteResult
	{
		private static readonly IDeepParseExecuteResult _failedResult = new FailDeepParseExecuteResult(255);
		private readonly DeepPageParsedInfo result;

		/// <summary>
		///		深度页面解析结果
		/// </summary>
		/// <param name="status">执行状态</param>
		/// <param name="resultObj">返回结果的对象</param>
		/// <param name="error">错误信息</param>
		public DeepParseExecuteResult(ExecuteStatus status, DeepPageParsedInfo resultObj, string error = null)
		{
			this.Status = status;
			this.Error = error;
			this.result = resultObj;
		}

		/// <summary>
		///		获取内部系统的错误编号
		/// </summary>
		public int ErrorId { get; set; }

		/// <summary>
		///		获取深度页面解析执行状态
		/// </summary>
		public ExecuteStatus Status { get; private set; }

		/// <summary>
		///		获取错误信息
		/// </summary>
		public string Error { get; private set; }

		/// <summary>
		///		获取深度页面解析信息
		/// </summary>
		/// <typeparam name="T">派生自深度页面解析信息类</typeparam>
		/// <returns></returns>
		public T GetResult<T>() where T : DeepPageParsedInfo
		{
			return (T)this.result;
		}

		/// <summary>
		///     创造一个新的执行结果
		/// </summary>
		/// <param name="state">执行结果的状态</param>
		/// <param name="resultObj">结果对象</param>
		/// <returns>返回创建后的执行结果对象</returns>
		public static IDeepParseExecuteResult Create(ExecuteStatus state, DeepPageParsedInfo resultObj)
		{
			return new DeepParseExecuteResult(state, resultObj);
		}

		/// <summary>
		///     创造一个新的成功执行结果
		/// </summary>
		/// <param name="resultObj">结果对象</param>
		/// <returns>返回创建后的成功执行结果对象</returns>
		public static IDeepParseExecuteResult Succeed(DeepPageParsedInfo resultObj)
		{
			return new DeepParseExecuteResult(ExecuteStatus.Succeed, resultObj);
		}

		/// <summary>
		///     创造一个失败的执行结果
		/// </summary>
		/// <returns>返回失败的执行结果对象</returns>
		public static IDeepParseExecuteResult Fail()
		{
			return _failedResult;
		}

		/// <summary>
		///     创造一个失败的执行结果
		/// </summary>
		/// <exception cref="errorId">内部系统错误编号</exception>
		/// <exception cref="reason">失败原因</exception>
		/// <returns>返回失败的执行结果对象</returns>
		public static IDeepParseExecuteResult Fail(byte errorId, string reason)
		{
			return new FailDeepParseExecuteResult(errorId, reason);
		}
	}
}