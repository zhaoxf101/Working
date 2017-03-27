using System;

namespace LinqToSqlShared.Utility
{
	internal static class Strings
	{
		internal static string OwningTeam
		{
			get
			{
				return SR.GetString("OwningTeam");
			}
		}

		internal static string CouldNotMakeUniqueName(object p0)
		{
			return SR.GetString("CouldNotMakeUniqueName", new object[]
			{
				p0
			});
		}
	}
}
