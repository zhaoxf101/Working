using System;

namespace LinqToSqlShared.Utility
{
	internal static class TypeSystem
	{
		internal static bool IsNullableType(Type type)
		{
			return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		internal static Type GetNonNullableType(Type type)
		{
			if (TypeSystem.IsNullableType(type))
			{
				return type.GetGenericArguments()[0];
			}
			return type;
		}
	}
}
