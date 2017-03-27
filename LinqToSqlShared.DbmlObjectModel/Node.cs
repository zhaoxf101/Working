using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal abstract class Node
	{
		protected static string KeyValue<T>(string key, T value)
		{
			if (value != null)
			{
				return key + "=" + value.ToString() + " ";
			}
			return string.Empty;
		}

		protected static string SingleValue<T>(T value)
		{
			if (value != null)
			{
				return value.ToString() + " ";
			}
			return string.Empty;
		}
	}
}
