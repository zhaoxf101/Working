using LinqToSqlShared.DbmlObjectModel;
using System;
using System.Collections.Generic;

namespace LinqToSqlShared.Generator
{
	internal interface IMappingCodeGenerator
	{
		IEnumerable<ValidationMessage> ValidateModel(Database db);

		GenerationResult Generate(Database db, GenerateOptions options);
	}
}
