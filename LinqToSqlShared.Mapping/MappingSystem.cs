using System;
using System.Data;
using System.Data.Linq;

namespace LinqToSqlShared.Mapping
{
	internal static class MappingSystem
	{
		internal static bool IsSupportedDiscriminatorType(Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				type = type.GetGenericArguments()[0];
			}
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.String:
				return true;
			}
			return false;
		}

		internal static bool IsSupportedDiscriminatorType(SqlDbType type)
		{
			if (type <= SqlDbType.NVarChar)
			{
				switch (type)
				{
				case SqlDbType.BigInt:
				case SqlDbType.Bit:
				case SqlDbType.Char:
					break;
				case SqlDbType.Binary:
					return false;
				default:
					switch (type)
					{
					case SqlDbType.Int:
					case SqlDbType.NChar:
					case SqlDbType.NVarChar:
						break;
					case SqlDbType.Money:
					case SqlDbType.NText:
						return false;
					default:
						return false;
					}
					break;
				}
			}
			else if (type != SqlDbType.SmallInt)
			{
				switch (type)
				{
				case SqlDbType.TinyInt:
				case SqlDbType.VarChar:
					break;
				case SqlDbType.VarBinary:
					return false;
				default:
					return false;
				}
			}
			return true;
		}

		internal static bool IsSupportedIdentityType(Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				type = type.GetGenericArguments()[0];
			}
			if (type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Binary))
			{
				return true;
			}
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
			case TypeCode.String:
				return true;
			}
			return false;
		}

		internal static bool IsSupportedIdentityType(SqlDbType type)
		{
			if (type <= SqlDbType.NText)
			{
				if (type != SqlDbType.Image && type != SqlDbType.NText)
				{
					return true;
				}
			}
			else if (type != SqlDbType.Text)
			{
				switch (type)
				{
				case SqlDbType.Variant:
				case SqlDbType.Xml:
					break;
				case (SqlDbType)24:
					return true;
				default:
					if (type != SqlDbType.Udt)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
