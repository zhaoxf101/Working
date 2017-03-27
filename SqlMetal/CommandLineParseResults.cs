using LinqToSqlShared.DbmlObjectModel;
using System;

namespace SqlMetal
{
	internal class CommandLineParseResults
	{
		public string Server;

		public string Database;

		public string Namespace;

		public string UserId;

		public string Password;

		public InputType InputType;

		public OutputType OutputType;

		public string InputFile;

		public string OutputFile;

		public string MapOutputFile;

		public int CommandTimeout;

		public string Provider;

		public string ContextClass;

		public LanguageType Lang;

		public SerializationMode? SerializationMode = null;

		public string EntityBase;

		public string ConnectionString;
	}
}
