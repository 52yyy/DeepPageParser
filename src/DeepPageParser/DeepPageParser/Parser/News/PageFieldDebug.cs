namespace DeepPageParser
{
	/// <summary>
	///		ҳ���ֶ���ȡ������
	/// </summary>
	public class PageFieldDebug
	{
		/// <summary>
		///		�ؼ�������ʾ��֮�������
		/// </summary>
		public string FieldValue { get; set; }

		/// <summary>
		///		β�����򣬽�����ʾ�ʺ��������
		/// </summary>
		public string TailValue { get; set; }

		/// <summary>
		///		������ʾ��
		/// </summary>
		public bool ContainsClue { get; set; }

		/// <summary>
		///		����������ʾ��
		/// </summary>
		public bool ContainsEndClue { get; set; }

        /// <summary>
        ///		����������ʾ��
        /// </summary>
        public bool ContainsOtherClue { get; set; }
	}
}