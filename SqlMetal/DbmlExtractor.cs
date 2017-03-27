using LinqToSqlShared.DbmlObjectModel;
using System;

namespace SqlMetal
{
	internal class DbmlExtractor : IDatabaseExtractor
	{
		public static DbmlExtractor Create()
		{
			return new DbmlExtractor();
		}

		private DbmlExtractor()
		{
		}

		public Database Extract(string connection, string database, ExtractOptions options)
		{
			Extractor extractor = new Extractor(connection, options);
			return extractor.Extract(database);
		}
	}
}
