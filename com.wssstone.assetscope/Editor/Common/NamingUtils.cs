using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text.RegularExpressions;

using UnityEngine;

namespace AssetScope
{ 
	public class NamingUtils
	{
		public static bool CheckSuffix(Texture2D texture, string suffix)
		{
			return texture.name.EndsWith(suffix);
		}

		public static bool CheckSuffix(Texture2D texture, string[] suffixs)
		{
			foreach (var suffix in suffixs)
			{
				if (CheckSuffix(texture, suffix)) return true;
			}
			return false;
		}

		public static string LCSubstring(string str1, string str2)
		{
			if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
			{
				return string.Empty;
			}

			int[,] dp = new int[str1.Length + 1, str2.Length + 1];
			int maxLen = 0;
			int endIndex = 0;

			for (int i = 1; i <= str1.Length; i++)
			{
				for (int j = 1; j <= str2.Length; j++)
				{
					if (str1[i - 1] == str2[j - 1])
					{
						dp[i, j] = dp[i - 1, j - 1] + 1;
						if (dp[i, j] > maxLen)
						{
							maxLen = dp[i, j];
							endIndex = i;
						}
					}
					else
					{
						dp[i, j] = 0;
					}
				}
			}

			return maxLen > 0 ? str1.Substring(endIndex - maxLen, maxLen) : string.Empty;
		}

		public static string RemovePrefix(string[] Prefixs, string Name)
		{
			var newname = string.Copy(Name);
			foreach (var prefix in Prefixs)
			{
				if (newname.StartsWith(prefix))
				{
					newname = newname.Substring(prefix.Length);
					break;
				}
			}
			return newname;
		}

		public static string RemoveSuffix(string[] Suffixs, string Name)
		{
			var newname = string.Copy(Name);
			foreach (var suffix in Suffixs)
			{
				string pattern = suffix;
				if (Regex.IsMatch(newname, pattern))
				{
					newname = Regex.Replace(newname, pattern, "");
					break;
				}
			}
			return newname;
		}

		public static (string mainName, string number) ExtractMainNameAndNumber(string name, string defaultSuffix = "0001")
		{
			var match = Regex.Match(name, @"(_?)(\d+)$");
			string main = name;
			string tailNumber = null;
			if (match.Success)
			{
				main = name.Substring(0, match.Index);
				tailNumber = match.Groups[2].Value;
			}

			var numMatch = Regex.Match(main, @"(\d+)$");
			if (numMatch.Success)
			{
				string mainName = main.Substring(0, numMatch.Index);
				string number = numMatch.Groups[1].Value.PadLeft(4, '0');
				return (mainName, number);
			}
			else if (tailNumber != null)
			{
				return (main, tailNumber.PadLeft(4, '0'));
			}
			else
			{
				return (name, defaultSuffix);
			}
		}
	}
}