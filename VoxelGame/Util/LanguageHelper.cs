using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelGame.Util
{
	public class LanguageHelper
	{
		static Dictionary<string, string> languageLookup = new Dictionary<string, string>();
		public static string LoadedLanguage = "";

		public static void Reset()
		{
			languageLookup.Clear();
			LoadedLanguage = "";
		}

		public static void SetLanguage(string language_name)
		{
			LoadLanguage(FileUtil.GetPath("lang/" + language_name + ".lang"));
			LoadedLanguage = language_name;
		}

		static void LoadLanguage(string filePath)
		{
			if (File.Exists(filePath))
			{
				string[] languageContents = File.ReadAllLines(filePath);
				for (int i = 0; i < languageContents.Length; i++)
				{
					//Ignore whitespace and comments
					if (!(languageContents[i].Trim() == "" || languageContents[i].Trim().StartsWith("#")))
					{
						string argument, value;
						argument = languageContents[i].Substring(0, languageContents[i].IndexOf('=')).Trim().ToLower();
						value = languageContents[i].Substring(languageContents[i].IndexOf('=') + 1).Trim();

						//Unique keys. If duplicates are found, we warn
						if (!languageLookup.ContainsKey(argument))
						{
							languageLookup.Add(argument, value);
						}
						else
						{
							Logger.warn(-1, "Duplicate language key \"" + argument + "\" with value \"" + value + "\" will not be loaded! (Line " + (i + 1) + ", file " + filePath + ")");
						}
					}
				}
			}
			else
			{
				Logger.fatal(0, "Language file \"" + filePath + "\" doesn't exist!");
			}
		}

		public static string GetLanguageString(string internalName)
		{
			//language keys are lowercase
			internalName = internalName.ToLower();
			if (languageLookup.ContainsKey(internalName))
			{
				return languageLookup[internalName];
			}
			return internalName;
		}

		public static string ToString(double number, string format)
		{
			return number.ToString(format, new System.Globalization.CultureInfo("en-US")).Replace('.', '_').Replace(",", GetLanguageString("lang.misc.numberthousanddelimeter")).Replace("_", GetLanguageString("lang.misc.decimalpoint"));
		}
		
		public static string ToString(long number, string format)
		{
			return number.ToString(format, new System.Globalization.CultureInfo("en-US")).Replace('.', '_').Replace(",", GetLanguageString("lang.misc.numberthousanddelimeter")).Replace("_", GetLanguageString("lang.misc.decimalpoint"));
		}
	}
}
