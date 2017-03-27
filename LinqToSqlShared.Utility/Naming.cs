using LinqToSqlShared.DbmlObjectModel;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LinqToSqlShared.Utility
{
	internal static class Naming
	{
		private delegate bool IsValidDelegate(StringBuilder name);

		private static class StringUtil
		{
			internal static bool EqualValue(string str1, string str2)
			{
				return Naming.StringUtil.EqualValue(str1, str2, false);
			}

			private static bool EqualValue(string str1, string str2, bool caseInsensitive)
			{
				if (str1 == null)
				{
					throw Error.ArgumentNull("str1");
				}
				if (str2 == null)
				{
					throw Error.ArgumentNull("str2");
				}
				if (caseInsensitive)
				{
					return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
				}
				return string.Compare(str1, str2, StringComparison.Ordinal) == 0;
			}

			internal static bool ListContains(IEnumerable<string> array, string value, bool caseInsensitive)
			{
				foreach (string current in array)
				{
					if (Naming.StringUtil.EqualValue(current, value, caseInsensitive))
					{
						return true;
					}
				}
				return false;
			}
		}

		private const int maxIdentifierLength = 128;

		private const int maxNameLength = 120;

		private const int maxGenerationAttempts = 9999;

		private const string emptyIdentifierBase = "Name";

		private static string[] vbInvalidKeywords = new string[]
		{
			"FROM",
			"WHERE",
			"ORDER",
			"GROUP",
			"BY",
			"ASCENDING",
			"DESCENDING",
			"DISTINCT"
		};

		private static CodeDomProvider csharpProvider = new CSharpCodeProvider();

		private static CodeDomProvider vbProvider = new VBCodeProvider();

		internal static string MakeSingularName(string name)
		{
			if (string.Compare(name, "series", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return name;
			}
			if (string.Compare(name, "wines", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return name.Remove(name.Length - 1, 1);
			}
			if (name.Length > 3 && name.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
			{
				if (!Naming.IsVowel(name[name.Length - 4]))
				{
					name = name.Remove(name.Length - 3, 3);
					name += 'y';
				}
			}
			else if (name.EndsWith("ees", StringComparison.OrdinalIgnoreCase))
			{
				name = name.Remove(name.Length - 1, 1);
			}
			else if (name.EndsWith("ches", StringComparison.OrdinalIgnoreCase) || name.EndsWith("xes", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sses", StringComparison.OrdinalIgnoreCase))
			{
				name = name.Remove(name.Length - 2, 2);
			}
			else
			{
				if (string.Compare(name, "gas", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return name;
				}
				if (name.Length > 1 && name.EndsWith("s", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) && !name.EndsWith("us", StringComparison.OrdinalIgnoreCase))
				{
					name = name.Remove(name.Length - 1, 1);
				}
			}
			return name;
		}

		internal static string MakePluralName(string name)
		{
			if (name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
			{
				name += "es";
			}
			else if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && name.Length > 1 && !Naming.IsVowel(name[name.Length - 2]))
			{
				name = name.Remove(name.Length - 1, 1);
				name += "ies";
			}
			else if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
			{
				name += "s";
			}
			return name;
		}

		private static bool IsVowel(char c)
		{
			if (c <= 'Y')
			{
				if (c <= 'I')
				{
					if (c != 'A' && c != 'E' && c != 'I')
					{
						return false;
					}
				}
				else if (c != 'O' && c != 'U' && c != 'Y')
				{
					return false;
				}
			}
			else if (c <= 'i')
			{
				if (c != 'a' && c != 'e' && c != 'i')
				{
					return false;
				}
			}
			else if (c != 'o' && c != 'u' && c != 'y')
			{
				return false;
			}
			return true;
		}

		internal static string MakeCSharpLegalName(StringBuilder name)
		{
			return Naming.MakeValidIdentifierLangDependent(name, Naming.csharpProvider).ToString();
		}

		internal static string MakeVBLegalName(StringBuilder name)
		{
			return Naming.MakeValidIdentifierLangDependent(name, Naming.vbProvider).ToString();
		}

		private static StringBuilder MakeValidIdentifierLangDependent(StringBuilder name, CodeDomProvider codeProvider)
		{
			return Naming.MakeValidIdentifier(name, (StringBuilder s) => Naming.IsValidIdentifierLangDependent(s.ToString(), codeProvider), codeProvider);
		}

		internal static StringBuilder MakeLegalNameLangIndependent(StringBuilder name)
		{
			return Naming.MakeValidIdentifier(name, (StringBuilder s) => Naming.IsValidIdentifierLangIndependentInternal(s.ToString()), null);
		}

		internal static string LegalizeKeyword(string name, CodeDomProvider provider)
		{
			if (name == null)
			{
				return null;
			}
			if (provider.GetType() == typeof(CSharpCodeProvider))
			{
				return Naming.LegalizeCSharpKeywords(name);
			}
			return Naming.LegalizeVBKeywords(name);
		}

		private static string LegalizeVBKeywords(string name)
		{
			CodeDomProvider codeDomProvider = new VBCodeProvider();
			if (Array.IndexOf<string>(Naming.vbInvalidKeywords, name.ToUpper(CultureInfo.InvariantCulture)) >= 0 || !codeDomProvider.IsValidIdentifier(name))
			{
				name = Naming.EscapeVBKeyword(name).ToString();
			}
			return name;
		}

		private static string LegalizeCSharpKeywords(string name)
		{
			CodeDomProvider codeDomProvider = new CSharpCodeProvider();
			if (!codeDomProvider.IsValidIdentifier(name))
			{
				name = Naming.EscapeCSKeyword(name).ToString();
			}
			return name;
		}

		private static bool IsValidIdentifierStart(char c)
		{
			return char.IsLetter(c) || c == '_';
		}

		private static bool IsValidIdentifierLangIndependentInternal(string name)
		{
			if (string.IsNullOrEmpty(name) || name.Length > 120)
			{
				return false;
			}
			if (!Naming.IsValidIdentifierStart(name[0]))
			{
				return false;
			}
			for (int i = 1; i < name.Length; i++)
			{
				if (!Naming.IsValidIdentifierRest(name[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal static string CapitalizeFirstLettersOfWords(string name)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_')
				{
					if (flag)
					{
						c = char.ToUpper(c, CultureInfo.InvariantCulture);
					}
					flag = false;
					stringBuilder.Append(c);
				}
				else
				{
					flag = true;
					stringBuilder.Append(c);
				}
			}
			name = stringBuilder.ToString();
			return name;
		}

		internal static bool IsValidIdentifierLangIndependent(string name)
		{
			return Naming.IsValidIdentifierLangIndependentInternal(name);
		}

		private static string GetLanguageExtension(CodeDomProvider codeProvider)
		{
			if (codeProvider == null)
			{
				return string.Empty;
			}
			string text = "." + codeProvider.FileExtension;
			if (text.StartsWith("..", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(1);
			}
			return text;
		}

		private static bool IsVbCodeDomProvider(CodeDomProvider codeProvider)
		{
			return Naming.StringUtil.EqualValue(Naming.GetLanguageExtension(codeProvider), ".vb");
		}

		internal static bool IsValidIdentifierLangDependent(string name, CodeDomProvider provider)
		{
			return Naming.IsValidIdentifierLangIndependent(name) && (!Naming.IsVbCodeDomProvider(provider) || !Naming.StringUtil.ListContains(Naming.vbInvalidKeywords, name, true)) && provider.IsValidIdentifier(name);
		}

		private static bool IsValidIdentifierRest(char c)
		{
			if (char.IsLetterOrDigit(c) || c == '_')
			{
				return true;
			}
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
			return unicodeCategory == UnicodeCategory.ConnectorPunctuation || unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark;
		}

		private static StringBuilder MakeValidIdentifier(StringBuilder name, Naming.IsValidDelegate IsValidId, CodeDomProvider provider)
		{
			if (name.Length > 120)
			{
				name.Remove(120, name.Length - 120);
			}
			if (IsValidId(name))
			{
				return name;
			}
			name.Replace(" ", "");
			name.Replace("(", "");
			name.Replace(")", "");
			if (!IsValidId(name))
			{
				if (!Naming.IsValidIdentifierStart(name[0]))
				{
					if (Naming.IsValidIdentifierRest(name[0]))
					{
						name.Insert(0, "_");
					}
					else
					{
						name.Replace(name[0], '_', 0, 1);
					}
				}
				for (int i = 1; i < name.Length; i++)
				{
					if (!Naming.IsValidIdentifierRest(name[i]))
					{
						name.Replace(name[i], '_');
					}
				}
			}
			if (IsValidId(name))
			{
				return name;
			}
			if (provider == null)
			{
				name.Append('_');
				return name;
			}
			if (Naming.IsVbCodeDomProvider(provider))
			{
				return Naming.EscapeVBKeyword(name.ToString());
			}
			return Naming.EscapeCSKeyword(name.ToString());
		}

		private static StringBuilder EscapeVBKeyword(string name)
		{
			return new StringBuilder("[" + name + "]");
		}

		private static StringBuilder EscapeCSKeyword(string name)
		{
			return new StringBuilder("@" + name);
		}

		internal static bool IsSameName(string name1, string name2)
		{
			return name1 != null && name2 != null && (string.Compare(name1.ToLower(CultureInfo.CurrentCulture), name2.ToLower(CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase) == 0 || string.Compare(name1.ToUpper(CultureInfo.CurrentCulture), name2.ToUpper(CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase) == 0);
		}

		internal static bool MatchingColumnMemberExists(string legalLanguageName, Table table)
		{
			return table.Type.Columns.Exists((Column c) => Naming.IsSameName(c.Member, legalLanguageName) || (c.Member == null && Naming.IsSameName(c.Name, legalLanguageName)));
		}

		internal static bool MatchingAssociationMemberExists(string legalLanguageName, Table table)
		{
			return table.Type.Associations.Exists((Association a) => Naming.IsSameName(a.Member, legalLanguageName));
		}

		internal static bool MatchingParameterNameExists(string legalLanguageName, Function f)
		{
			return f.Parameters.Exists((Parameter p) => Naming.IsSameName(p.ParameterName, legalLanguageName));
		}

		internal static bool IsUniqueTableClassMemberName(string legalLanguageName, Table table)
		{
			return !Naming.IsSameName(table.Type.Name, legalLanguageName) && !Naming.MatchingColumnMemberExists(legalLanguageName, table) && !Naming.MatchingAssociationMemberExists(legalLanguageName, table);
		}

		internal static bool IsUniqueParameterName(string legalLanguageName, Function f)
		{
			return !Naming.MatchingParameterNameExists(legalLanguageName, f);
		}

		internal static string GetUniqueName(string candidateLegalLanguageName, Predicate<string> isUniquename)
		{
			string arg = candidateLegalLanguageName;
			for (int i = 1; i < 1000; i++)
			{
				candidateLegalLanguageName = arg + i;
				if (isUniquename(candidateLegalLanguageName))
				{
					return candidateLegalLanguageName;
				}
			}
			throw Error.CouldNotMakeUniqueName(candidateLegalLanguageName);
		}

		internal static string GetUniqueTableMemberName(Table table, string suggestedName)
		{
			Predicate<string> isUniquename = (string s) => Naming.IsUniqueTableClassMemberName(s, table);
			return Naming.GetUniqueName(suggestedName, isUniquename);
		}

		internal static string GetUniqueParameterName(Function f, string suggestedName)
		{
			Predicate<string> isUniquename = (string s) => Naming.IsUniqueParameterName(s, f);
			return Naming.GetUniqueName(suggestedName, isUniquename);
		}
	}
}
