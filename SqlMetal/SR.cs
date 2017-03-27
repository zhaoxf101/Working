using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace SqlMetal
{
	internal sealed class SR
	{
		internal const string OwningTeam = "OwningTeam";

		internal const string VersionString = "VersionString";

		internal const string CopyrightString = "CopyrightString";

		internal const string UsageHeader = "UsageHeader";

		internal const string ProgramDescription0 = "ProgramDescription0";

		internal const string ProgramDescription1 = "ProgramDescription1";

		internal const string ProgramDescription2 = "ProgramDescription2";

		internal const string ProgramDescription3 = "ProgramDescription3";

		internal const string UsageOptions = "UsageOptions";

		internal const string ServerUsage = "ServerUsage";

		internal const string ServerDescription = "ServerDescription";

		internal const string DatabaseUsage = "DatabaseUsage";

		internal const string DatabaseDescription = "DatabaseDescription";

		internal const string UserUsage = "UserUsage";

		internal const string UserDescription = "UserDescription";

		internal const string PasswordUsage = "PasswordUsage";

		internal const string PasswordDescription = "PasswordDescription";

		internal const string TimeOutUsage = "TimeOutUsage";

		internal const string TimeOutDescription = "TimeOutDescription";

		internal const string ConnectionStringUsage = "ConnectionStringUsage";

		internal const string ConnectionStringDescription = "ConnectionStringDescription";

		internal const string ViewsUsage = "ViewsUsage";

		internal const string ViewsDescription = "ViewsDescription";

		internal const string FunctionsUsage = "FunctionsUsage";

		internal const string FunctionDescription = "FunctionDescription";

		internal const string SprocsUsage = "SprocsUsage";

		internal const string SprocsDescription = "SprocsDescription";

		internal const string DbmlUsage = "DbmlUsage";

		internal const string DbmlDescription = "DbmlDescription";

		internal const string CodeUsage = "CodeUsage";

		internal const string CodeDescription = "CodeDescription";

		internal const string MapUsage = "MapUsage";

		internal const string MapDescription = "MapDescription";

		internal const string LanguageUsage = "LanguageUsage";

		internal const string LanguageDescription = "LanguageDescription";

		internal const string NamespaceUsage = "NamespaceUsage";

		internal const string NamespaceDescription = "NamespaceDescription";

		internal const string ContextUsage = "ContextUsage";

		internal const string ContextDescription = "ContextDescription";

		internal const string EntityBaseUsage = "EntityBaseUsage";

		internal const string EntityBaseDescription = "EntityBaseDescription";

		internal const string PluralizeUsage = "PluralizeUsage";

		internal const string PluralizeDescription = "PluralizeDescription";

		internal const string SerializationUsage = "SerializationUsage";

		internal const string SerializationDescription = "SerializationDescription";

		internal const string ProviderUsage = "ProviderUsage";

		internal const string ProviderDescription = "ProviderDescription";

		internal const string InputFileUsage = "InputFileUsage";

		internal const string InputFileDescription = "InputFileDescription";

		internal const string DirectGenerationDescription = "DirectGenerationDescription";

		internal const string DirectGenerationExample = "DirectGenerationExample";

		internal const string ExtractDbmlDescription = "ExtractDbmlDescription";

		internal const string ExtractDbmlExample = "ExtractDbmlExample";

		internal const string GenerateFromDbmlDescription = "GenerateFromDbmlDescription";

		internal const string GenerateFromDbmlExample = "GenerateFromDbmlExample";

		internal const string ExtractFromSdfDescription = "ExtractFromSdfDescription";

		internal const string ExtractFromSdfExample = "ExtractFromSdfExample";

		internal const string ExtractFromSqlExpressDescription = "ExtractFromSqlExpressDescription";

		internal const string ExtractFromSqlExpressExample = "ExtractFromSqlExpressExample";

		internal const string ExtractUsingConnectionStringDescription = "ExtractUsingConnectionStringDescription";

		internal const string ExtractUsingConnectionStringExample = "ExtractUsingConnectionStringExample";

		internal const string UnknownLanguage = "UnknownLanguage";

		internal const string UnknownProvider = "UnknownProvider";

		internal const string UnknownSerializationMode = "UnknownSerializationMode";

		internal const string UnknownOption = "UnknownOption";

		internal const string InputFileDoesNotExist = "InputFileDoesNotExist";

		internal const string MultipleInputFiles = "MultipleInputFiles";

		internal const string InvalidTimeoutFormat = "InvalidTimeoutFormat";

		internal const string TimeoutMustNotBeNegative = "TimeoutMustNotBeNegative";

		internal const string OutputFileIOError = "OutputFileIOError";

		internal const string OutputFileNameFormatError = "OutputFileNameFormatError";

		internal const string EmptyNameError = "EmptyNameError";

		internal const string UserIsMissing = "UserIsMissing";

		internal const string DatabaseIsNotSpecified = "DatabaseIsNotSpecified";

		internal const string UnableToExtractTable = "UnableToExtractTable";

		internal const string UnableToExtractView = "UnableToExtractView";

		internal const string UnableToExtractSproc = "UnableToExtractSproc";

		internal const string UnableToExtractFunction = "UnableToExtractFunction";

		internal const string ServerAndConnectionConflict = "ServerAndConnectionConflict";

		internal const string UserAndConnectionConflict = "UserAndConnectionConflict";

		internal const string PasswordAndConnectionConflict = "PasswordAndConnectionConflict";

		internal const string DatabaseAndConnectionConflict = "DatabaseAndConnectionConflict";

		internal const string SprocResultColumnsHaveSameName = "SprocResultColumnsHaveSameName";

		internal const string SprocResultMultipleAnonymousColumns = "SprocResultMultipleAnonymousColumns";

		internal const string SprocParameterTypeNotSupported = "SprocParameterTypeNotSupported";

		internal const string UnableToExtractColumnBecauseOfUDT = "UnableToExtractColumnBecauseOfUDT";

		internal const string CouldNotMakePropertyNameForAssociation = "CouldNotMakePropertyNameForAssociation";

		internal const string DbmlOrMapNotBoth = "DbmlOrMapNotBoth";

		internal const string DbmlOrCodeNotBoth = "DbmlOrCodeNotBoth";

		internal const string MapWithoutCode = "MapWithoutCode";

		internal const string ProviderNotInstalled = "ProviderNotInstalled";

		internal const string CouldNotIdentifyPrimaryKeyColumn = "CouldNotIdentifyPrimaryKeyColumn";

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
			this.resources = new ResourceManager("SqlMetal", base.GetType().Assembly);
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
