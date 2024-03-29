﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;

namespace HttpNet
{
	/// <summary>
	/// Collection of utility methods relevant to the web server
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Translates the value of the ContentType enum to its official value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetContentType(ContentType type)
		{
			switch (type)
			{
				case ContentType.Css:
					return "text/css";

				case ContentType.Html:
					return "text/html";

				case ContentType.Javascript:
					return "application/javascript";

				case ContentType.Json:
					return "application/json";

				case ContentType.Plain:
					return "text/plain";

				case ContentType.Zip:
					return "application/zip";

				case ContentType.Image:
					return "image/";

				default:
					goto case ContentType.Plain;
			}
		}

		internal static string GetLogLevelTag(LogLevels level)
		{
			switch (level)
			{
				case LogLevels.Debug:
					return "DEBUG";

				case LogLevels.Error:
					return "ERROR";

				case LogLevels.Info:
					return "INFO";

				case LogLevels.Warning:
					return "WARNING";

				default:
					return "";
			}
		}

		public static Dictionary<string, string> GetUrlParams(string rawUrl)
		{
			Dictionary<string, string> GET = new Dictionary<string, string>();

			string[] urlParts = rawUrl.Split('?');

			if (urlParts.Length < 2 || urlParts[1] == "")
				return GET;

			NameValueCollection vals = HttpUtility.ParseQueryString(urlParts[1]);

			foreach (string key in vals.AllKeys)
			{
				GET.Add(key, vals.Get(key));
			}

			return GET;
		}

		/// <summary>
		/// Replace url-friendly wildcard patterns with the regex equivalent
		/// </summary>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static string WildcardRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
		}
	}
}
