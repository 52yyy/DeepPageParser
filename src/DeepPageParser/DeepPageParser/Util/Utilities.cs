using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DeepPageParser.Util
{
	public static class Utilities
	{
		private static Encoding[] sniffableEncodings = new Encoding[] { Encoding.UTF32, Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF8 };


		public static Encoding GetEntityBodyEncoding(this WebHeaderCollection oHeaders, byte[] oBody)
		{
			if (oHeaders != null)
			{
				string tokenValue = oHeaders.GetTokenValue("Content-Type", "charset");
				if (tokenValue != null)
				{
					try
					{
						return GetTextEncoding(tokenValue);
					}
					catch (Exception)
					{
					}
				}
			}
			Encoding oHeaderEncoding = Encoding.Default;
			if ((oBody != null) && (oBody.Length >= 2))
			{
				foreach (Encoding encoding2 in sniffableEncodings)
				{
					byte[] preamble = encoding2.GetPreamble();
					if (oBody.Length >= preamble.Length)
					{
						bool flag = preamble.Length > 0;
						for (int i = 0; i < preamble.Length; i++)
						{
							if (preamble[i] != oBody[i])
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							oHeaderEncoding = encoding2;
							break;
						}
					}
				}
				if ((oHeaders != null) && oHeaders.AllKeys.Contains("Content-Type"))
				{
					if (oHeaders.ExistsAndContains("Content-Type", "multipart/form-data"))
					{
						string str2 = oHeaderEncoding.GetString(oBody, 0, Math.Min(0x2000, oBody.Length));
						MatchCollection matchs = new Regex(".*Content-Disposition: form-data; name=\"_charset_\"\\s+(?<thecharset>[^\\s'&>\\\"]*)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase).Matches(str2);
						if ((matchs.Count > 0) && (matchs[0].Groups.Count > 0))
						{
							try
							{
								oHeaderEncoding = GetTextEncoding(matchs[0].Groups[1].Value);
							}
							catch (Exception)
							{
							}
						}
					}
					if (oHeaders.ExistsAndContains("Content-Type", "application/x-www-form-urlencoded"))
					{
						string str4 = oHeaderEncoding.GetString(oBody, 0, Math.Min(0x1000, oBody.Length));
						MatchCollection matchs2 = new Regex(".*_charset_=(?<thecharset>[^'&>\\\"]*)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase).Matches(str4);
						if ((matchs2.Count > 0) && (matchs2[0].Groups.Count > 0))
						{
							try
							{
								oHeaderEncoding = GetTextEncoding(matchs2[0].Groups[1].Value);
							}
							catch (Exception)
							{
							}
						}
					}

					string input = oHeaderEncoding.GetString(oBody, 0, Math.Min(0x1000, oBody.Length));
					MatchCollection matchs3 = new Regex("<meta\\s.*charset\\s*=\\s*['\\\"]?(?<thecharset>[^'>\\\"]*)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase).Matches(input);
					if ((matchs3.Count <= 0) || (matchs3[0].Groups.Count <= 0))
					{
						return oHeaderEncoding;
					}
					string sEncoding = null;
					try
					{
						sEncoding = matchs3[0].Groups[1].Value;
						Encoding textEncoding = GetTextEncoding(sEncoding);
						if (textEncoding == oHeaderEncoding)
						{
							return oHeaderEncoding;
						}
						if (((oHeaderEncoding == Encoding.UTF8) && (((textEncoding == Encoding.BigEndianUnicode) || (textEncoding == Encoding.Unicode)) || (textEncoding == Encoding.UTF32))) || ((textEncoding == Encoding.UTF8) && (((oHeaderEncoding == Encoding.BigEndianUnicode) || (oHeaderEncoding == Encoding.Unicode)) || (oHeaderEncoding == Encoding.UTF32))))
						{
							return oHeaderEncoding;
						}
						oHeaderEncoding = textEncoding;
					}
					catch (Exception)
					{
					}
				}
			}
			return oHeaderEncoding;
		}

		public static bool ExistsAndContains(this WebHeaderCollection httpHeaders, string key, string value)
		{
			if (httpHeaders.AllKeys.Contains(key))
			{
				string[] values = httpHeaders.GetValues(key);

				return values != null && values.Any(item => item == value);
			}

			return false;
		}


		public static Encoding GetTextEncoding(string sEncoding)
		{
			if (sEncoding.OICEquals("utf8"))
			{
				sEncoding = "utf-8";
			}
			return Encoding.GetEncoding(sEncoding);
		}

		public static bool OICEquals(this string inStr, string toMatch)
		{
			return string.Equals(inStr, toMatch, StringComparison.OrdinalIgnoreCase);
		}



		internal static string ExtractAttributeValue(string sFullValue, string sAttribute)
		{
			string str = null;
			Match match = new Regex(Regex.Escape(sAttribute) + "\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)", RegexOptions.IgnoreCase).Match(sFullValue);
			if (match.Success && (match.Groups["TokenValue"] != null))
			{
				str = match.Groups["TokenValue"].Value;
			}
			return str;
		}


		public static string GetTokenValue(this WebHeaderCollection headers, string sHeaderName, string sTokenName)
		{
			string str = headers.GetValues(sHeaderName).FirstOrDefault();
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}

			return Utilities.ExtractAttributeValue(str, sTokenName);
		}

	}
}