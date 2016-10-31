namespace DeepPageParser
{
	/// <summary>
	///		���ҳ��������
	/// </summary>
	public class DeepParseExecuteResult : IDeepParseExecuteResult
	{
		private static readonly IDeepParseExecuteResult _failedResult = new FailDeepParseExecuteResult(255);
		private readonly DeepPageParsedInfo result;

		/// <summary>
		///		���ҳ��������
		/// </summary>
		/// <param name="status">ִ��״̬</param>
		/// <param name="resultObj">���ؽ���Ķ���</param>
		/// <param name="error">������Ϣ</param>
		public DeepParseExecuteResult(ExecuteStatus status, DeepPageParsedInfo resultObj, string error = null)
		{
			this.Status = status;
			this.Error = error;
			this.result = resultObj;
		}

		/// <summary>
		///		��ȡ�ڲ�ϵͳ�Ĵ�����
		/// </summary>
		public int ErrorId { get; set; }

		/// <summary>
		///		��ȡ���ҳ�����ִ��״̬
		/// </summary>
		public ExecuteStatus Status { get; private set; }

		/// <summary>
		///		��ȡ������Ϣ
		/// </summary>
		public string Error { get; private set; }

		/// <summary>
		///		��ȡ���ҳ�������Ϣ
		/// </summary>
		/// <typeparam name="T">���������ҳ�������Ϣ��</typeparam>
		/// <returns></returns>
		public T GetResult<T>() where T : DeepPageParsedInfo
		{
			return (T)this.result;
		}

		/// <summary>
		///     ����һ���µ�ִ�н��
		/// </summary>
		/// <param name="state">ִ�н����״̬</param>
		/// <param name="resultObj">�������</param>
		/// <returns>���ش������ִ�н������</returns>
		public static IDeepParseExecuteResult Create(ExecuteStatus state, DeepPageParsedInfo resultObj)
		{
			return new DeepParseExecuteResult(state, resultObj);
		}

		/// <summary>
		///     ����һ���µĳɹ�ִ�н��
		/// </summary>
		/// <param name="resultObj">�������</param>
		/// <returns>���ش�����ĳɹ�ִ�н������</returns>
		public static IDeepParseExecuteResult Succeed(DeepPageParsedInfo resultObj)
		{
			return new DeepParseExecuteResult(ExecuteStatus.Succeed, resultObj);
		}

		/// <summary>
		///     ����һ��ʧ�ܵ�ִ�н��
		/// </summary>
		/// <returns>����ʧ�ܵ�ִ�н������</returns>
		public static IDeepParseExecuteResult Fail()
		{
			return _failedResult;
		}

		/// <summary>
		///     ����һ��ʧ�ܵ�ִ�н��
		/// </summary>
		/// <exception cref="errorId">�ڲ�ϵͳ������</exception>
		/// <exception cref="reason">ʧ��ԭ��</exception>
		/// <returns>����ʧ�ܵ�ִ�н������</returns>
		public static IDeepParseExecuteResult Fail(byte errorId, string reason)
		{
			return new FailDeepParseExecuteResult(errorId, reason);
		}
	}
}