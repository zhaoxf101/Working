using System;

namespace SqlMetal
{
	[Flags]
	internal enum ExtractTypes
	{
		Tables = 1,
		Views = 2,
		Functions = 4,
		StoredProcedures = 8,
		Relationships = 16,
		All = 31
	}
}
