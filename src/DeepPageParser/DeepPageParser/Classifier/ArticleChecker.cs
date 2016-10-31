using System;
using System.IO;

namespace DeepPageParser.Classifier
{
	internal class ArticleChecker
	{
		/// <summary>
		/// 检查是否是安全url
		/// </summary>
		/// <param name="url">待检查url</param>
		/// <returns>是否是安全url</returns>
		public static bool IsSafeUrl(string url)
		{
			Uri uri = new Uri(url);
			string host = uri.Host;
			string pathAndQuery = uri.PathAndQuery;
			string fileName = string.Empty;
			if (uri.IsFile)
			{
				fileName = Path.GetFileName(uri.LocalPath);
			}

			if (((((((host.IndexOf("mail", StringComparison.OrdinalIgnoreCase) >= 0)
					|| (host.IndexOf("map.", StringComparison.OrdinalIgnoreCase) >= 0))
					|| ((host.IndexOf("maps.", StringComparison.OrdinalIgnoreCase) >= 0)
						|| (host.IndexOf("search.", StringComparison.OrdinalIgnoreCase) >= 0)))
					|| ((host.IndexOf("wiki", StringComparison.OrdinalIgnoreCase) >= 0)
						|| ((host.IndexOf("onenote", StringComparison.OrdinalIgnoreCase) >= 0)
							&& ((pathAndQuery.IndexOf("clipper/reader", StringComparison.OrdinalIgnoreCase) != -1)
								|| (pathAndQuery.IndexOf("clipper/onenote", StringComparison.OrdinalIgnoreCase) != -1)))))
				|| ((((host.IndexOf("amazon.", StringComparison.OrdinalIgnoreCase) >= 0)
					|| (host.IndexOf("twitter.", StringComparison.OrdinalIgnoreCase) >= 0))
					|| ((host.IndexOf("pinterest.", StringComparison.OrdinalIgnoreCase) >= 0) || (pathAndQuery == string.Empty)))
					|| (((pathAndQuery == "/") || (pathAndQuery == "/index.html"))
						|| ((pathAndQuery.IndexOf("/search/", StringComparison.OrdinalIgnoreCase) >= 0)
							|| (fileName.IndexOf("home", StringComparison.OrdinalIgnoreCase) == 0)))))
				|| (fileName.IndexOf("default", StringComparison.OrdinalIgnoreCase) == 0))
				|| ((fileName.IndexOf("search", StringComparison.OrdinalIgnoreCase) > 0) && (fileName.Length < 10)))
			{
				return false;
			}

			return true;
		}
	}
}

