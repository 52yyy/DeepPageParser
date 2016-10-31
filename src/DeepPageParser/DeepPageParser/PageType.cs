namespace DeepPageParser
{
	/// <summary>
	///		页面类型，识别页面的类型可能有多种
	/// </summary>
	public abstract class PageType
	{
		/// <summary>
		///		内部构造一个新的实例
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		protected PageType(string name, int stateCode)
		{
			this.Name = name;
			this.TypeCode = stateCode;
		}

		/// <summary>
		///		类别名称
		/// </summary>
		public abstract string Name { get; protected set; }

		/// <summary>
		///		类别编码
		/// </summary>
		public abstract int TypeCode { get; protected set; }
	}

	/// <summary>
	///		内容页页面类别
	/// </summary>
	public class ContentPageType : PageType
	{
		private static PageType _news = new ContentPageType("News", 101);  // 新闻页面编号101
		private static PageType _forum = new ContentPageType("Forum", 102);  // 论坛页面编号102
		private static PageType _weixin = new ContentPageType("Weixin", 103);  // 微信页面编号103
		private static PageType _structure = new ContentPageType("Structure", 104);  // 结构化、半结构化页面编号104
		
		/// <summary>
		///		外部可以构造新的内容页页面类型实例
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		private ContentPageType(string name, int stateCode)
			: base(name, stateCode)
		{
		}

		/// <summary>
		///		返回News类型
		/// </summary>
		public static PageType News
		{
			get
			{
				return _news;
			}
		}

		/// <summary>
		///		返回Forum类型
		/// </summary>
		public static PageType Forum
		{
			get
			{
				return _forum;
			}
		}

		/// <summary>
		///		返回Weixin类型
		/// </summary>
		public static PageType Weixin
		{
			get
			{
				return _weixin;
			}
		}

		/// <summary>
		///		返回Structure类型
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
	///		非内容页页面类别
	/// </summary>
	internal class NonContentPageType : PageType
	{
		private static PageType _unknown = new NonContentPageType("Unknown", 0);
		private static PageType _catalog = new NonContentPageType("Catalog", 1);

		/// <summary>
		///		内部构造新的实例
		/// </summary>
		/// <param name="name"></param>
		/// <param name="stateCode"></param>
		private NonContentPageType(string name, int stateCode)
			: base(name, stateCode)
		{
		}

		/// <summary>
		///		返回Unknown类型
		/// </summary>
		public static PageType Unknown
		{
			get
			{
				return _unknown;
			}
		}

		/// <summary>
		///		返回Catalog类型
		/// </summary>
		public static PageType Catalog
		{
			get
			{
				return _catalog;
			}
		}

		/// <summary>
		///		类别名称
		/// </summary>
		public override string Name { get; protected set; }

		/// <summary>
		///		类别编码
		/// </summary>
		public override int TypeCode { get; protected set; }
	}
}