using System;

namespace SqlMetal
{
	internal static class Error
	{
		internal static Exception CouldNotMakePropertyNameForAssociation(object p0)
		{
			return new InvalidOperationException(Strings.CouldNotMakePropertyNameForAssociation(p0));
		}

		internal static Exception ProviderNotInstalled(object p0, object p1)
		{
			return new InvalidOperationException(Strings.ProviderNotInstalled(p0, p1));
		}

		internal static Exception CouldNotIdentifyPrimaryKeyColumn(object p0, object p1)
		{
			return new InvalidOperationException(Strings.CouldNotIdentifyPrimaryKeyColumn(p0, p1));
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
