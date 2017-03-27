using LinqToSqlShared.DbmlObjectModel;
using System;

namespace SqlMetal
{
	internal interface IDatabaseExtractor
	{
		Database Extract(string connection, string database, ExtractOptions options);
	}
}
