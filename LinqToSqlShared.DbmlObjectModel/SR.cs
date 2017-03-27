using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal sealed class SR
	{
		internal const string OwningTeam = "OwningTeam";

		internal const string SchemaRequirementViolation = "SchemaRequirementViolation";

		internal const string SchemaDuplicateIdViolation = "SchemaDuplicateIdViolation";

		internal const string SchemaInvalidIdRefToNonexistentId = "SchemaInvalidIdRefToNonexistentId";

		internal const string SchemaOrRequirementViolation = "SchemaOrRequirementViolation";

		internal const string SchemaUnexpectedElementViolation = "SchemaUnexpectedElementViolation";

		internal const string SchemaUnexpectedAdditionalAttributeViolation = "SchemaUnexpectedAdditionalAttributeViolation";

		internal const string InvalidBooleanAttributeValueViolation = "InvalidBooleanAttributeValueViolation";

		internal const string InvalidEnumAttributeValueViolation = "InvalidEnumAttributeValueViolation";

		internal const string RequiredElementMissingViolation = "RequiredElementMissingViolation";

		internal const string ElementMoreThanOnceViolation = "ElementMoreThanOnceViolation";

		internal const string RequiredAttributeMissingViolation = "RequiredAttributeMissingViolation";

		internal const string SchemaRecursiveTypeReference = "SchemaRecursiveTypeReference";

		internal const string SchemaUnrecognizedAttribute = "SchemaUnrecognizedAttribute";

		internal const string SchemaExpectedEmptyElement = "SchemaExpectedEmptyElement";

		internal const string DatabaseNodeNotFound = "DatabaseNodeNotFound";

		internal const string TypeNameNotUnique = "TypeNameNotUnique";

		internal const string Bug = "Bug";

		private static SR loader;

		private ResourceManager resources;

		private static CultureInfo Culture
		{
			get
			{
				return null;
			}
		}

		public static ResourceManager Resources
		{
			get
			{
				return SR.GetLoader().resources;
			}
		}

		internal SR()
		{
			this.resources = new ResourceManager("LinqToSqlShared.DbmlObjectModel", base.GetType().Assembly);
		}

		private static SR GetLoader()
		{
			if (SR.loader == null)
			{
				SR value = new SR();
				Interlocked.CompareExchange<SR>(ref SR.loader, value, null);
			}
			return SR.loader;
		}

		public static string GetString(string name, params object[] args)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			string @string = sR.resources.GetString(name, SR.Culture);
			if (args != null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					string text = args[i] as string;
					if (text != null && text.Length > 1024)
					{
						args[i] = text.Substring(0, 1021) + "...";
					}
				}
				return string.Format(CultureInfo.CurrentCulture, @string, args);
			}
			return @string;
		}

		public static string GetString(string name)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			return sR.resources.GetString(name, SR.Culture);
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return SR.GetString(name);
		}

		public static object GetObject(string name)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			return sR.resources.GetObject(name, SR.Culture);
		}
	}
}
