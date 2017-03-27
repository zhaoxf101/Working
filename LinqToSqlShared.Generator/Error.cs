using System;

namespace LinqToSqlShared.Generator
{
	internal static class Error
	{
		internal static Exception CouldNotMakeUniqueTableName(object p0)
		{
			return new InvalidOperationException(Strings.CouldNotMakeUniqueTableName(p0));
		}

		internal static Exception CouldNotMakeUniqueVariableName(object p0)
		{
			return new InvalidOperationException(Strings.CouldNotMakeUniqueVariableName(p0));
		}

		internal static Exception CouldNotMakePropertyNameForAssociation(object p0)
		{
			return new InvalidOperationException(Strings.CouldNotMakePropertyNameForAssociation(p0));
		}

		internal static Exception MismatchedThisKeyOtherKey(object p0, object p1, object p2, object p3)
		{
			return new InvalidOperationException(Strings.MismatchedThisKeyOtherKey(p0, p1, p2, p3));
		}

		internal static Exception ArgumentNull(string paramName)
		{
			return new ArgumentNullException(paramName);
		}

		internal static Exception ArgumentOutOfRange(string paramName)
		{
			return new ArgumentOutOfRangeException(paramName);
		}

		internal static Exception NotImplemented()
		{
			return new NotImplementedException();
		}

		internal static Exception NotSupported()
		{
			return new NotSupportedException();
		}
	}
}
