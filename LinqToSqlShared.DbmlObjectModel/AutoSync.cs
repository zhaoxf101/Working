using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal enum AutoSync
	{
		Default,
		Always,
		Never,
		OnInsert,
		OnUpdate
	}
}
