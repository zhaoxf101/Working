using System;

namespace LinqToSqlShared.Utility
{
	internal static class Error
	{
		internal static Exception CouldNotMakeUniqueName(object p0)
		{
			return new InvalidOperationException(Strings.CouldNotMakeUniqueName(p0));
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
