using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VoxelGame.Client.UI
{
	public class TextRenderer
	{
		public const char DelimeterChar = '¶';
		public const string DelimeterCharASCII = "\\u00b6";

		/// <summary>
		/// Replaces all instances of '\u00b6' with '¶' in the given string. The replace function is case insensitive.
		/// </summary>
		/// <param name="inString"></param>
		/// <returns></returns>
		public static string FixASCIIDelimeterChar(string inString)
		{
			//Thanks https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
			return Regex.Replace(
				inString,
				Regex.Escape(DelimeterCharASCII),
				DelimeterChar.ToString().Replace("$", "$$"),
				RegexOptions.IgnoreCase
			);
		}

		//TODO: Render text method
		//TODO: Get text size (vector2int) given properties
	}
}
