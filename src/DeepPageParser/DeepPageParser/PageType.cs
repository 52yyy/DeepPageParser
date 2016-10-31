namespace DeepPageParser
{
	/// <summary>
	///		ҳ�����ͣ�ʶ��ҳ������Ϳ����ж���
	/// </summary>
	public abstract class PageType
	{
		/// <summary>
		///		�ڲ�����һ���µ�ʵ��
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		protected PageType(string name, int stateCode)
		{
			this.Name = name;
			this.TypeCode = stateCode;
		}

		/// <summary>
		///		�������
		/// </summary>
		public abstract string Name { get; protected set; }

		/// <summary>
		///		������
		/// </summary>
		public abstract int TypeCode { get; protected set; }
	}

	/// <summary>
	///		����ҳҳ�����
	/// </summary>
	public class ContentPageType : PageType
	{
		private static PageType _news = new ContentPageType("News", 101);  // ����ҳ����101
		private static PageType _forum = new ContentPageType("Forum", 102);  // ��̳ҳ����102
		private static PageType _weixin = new ContentPageType("Weixin", 103);  // ΢��ҳ����103
		private static PageType _structure = new ContentPageType("Structure", 104);  // �ṹ������ṹ��ҳ����104
		
		/// <summary>
		///		�ⲿ���Թ����µ�����ҳҳ������ʵ��
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		private ContentPageType(string name, int stateCode)
			: base(name, stateCode)
		{
		}

		/// <summary>
		///		����News����
		/// </summary>
		public static PageType News
		{
			get
			{
				return _news;
			}
		}

		/// <summary>
		///		����Forum����
		/// </summary>
		public static PageType Forum
		{
			get
			{
				return _forum;
			}
		}

		/// <summary>
		///		����Weixin����
		/// </summary>
		public static PageType Weixin
		{
			get
			{
				return _weixin;
			}
		}

		/// <summary>
		///		����Structure����
		/// </summary>
		public static PageType Structure
		{
			get
			{
				return _structure;
			}
		}

		public override string Name { get; protected set; }

		public override int TypeCode { get; protected set; }
	}

	/// <summary>
	///		������ҳҳ�����
	/// </summary>
	internal class NonContentPageType : PageType
	{
		private static PageType _unknown = new NonContentPageType("Unknown", 0);
		private static PageType _catalog = new NonContentPageType("Catalog", 1);

		/// <summary>
		///		�ڲ������µ�ʵ��
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		private NonContentPageType(string name, int stateCode)
			: base(name, stateCode)
		{
		}

		/// <summary>
		///		����Unknown����
		/// </summary>
		public static PageType Unknown
		{
			get
			{
				return _unknown;
			}
		}

		/// <summary>
		///		����Catalog����
		/// </summary>
		public static PageType Catalog
		{
			get
			{
				return _catalog;
			}
		}

		/// <summary>
		///		�������
		/// </summary>
		public override string Name { get; protected set; }

		/// <summary>
		///		������
		/// </summary>
		public override int TypeCode { get; protected set; }
	}
}