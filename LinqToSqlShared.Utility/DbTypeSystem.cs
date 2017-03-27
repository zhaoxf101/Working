using System;
using System.Data;
using System.Data.Linq;
using System.Globalization;
using System.Xml.Linq;

namespace LinqToSqlShared.Utility
{
	internal static class DbTypeSystem
	{
		internal static SqlDbType Parse(string stype)
		{
			stype = stype.ToUpper(CultureInfo.InvariantCulture).Replace("NOT NULL", "");
			stype = stype.ToUpper(CultureInfo.InvariantCulture).Replace("NULL", "");
			int num = stype.IndexOf('(');
			int num2 = stype.IndexOf(' ');
			int num3 = (num != -1 && num2 != -1) ? Math.Min(num2, num) : ((num != -1) ? num : ((num2 != -1) ? num2 : -1));
			string text;
			if (num3 == -1)
			{
				text = stype;
				num3 = stype.Length;
			}
			else
			{
				text = stype.Substring(0, num3);
			}
			int num4 = num3;
			if (num4 < stype.Length && stype[num4] == '(')
			{
				num4++;
				num3 = stype.IndexOf(',', num4);
				if (num3 > 0)
				{
					num4 = num3 + 1;
					num3 = stype.IndexOf(')', num4);
				}
				else
				{
					num3 = stype.IndexOf(')', num4);
				}
				int expr_D1 = num3++;
			}
			if (string.Compare(text, "rowversion", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Timestamp";
			}
			if (string.Compare(text, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Decimal";
			}
			if (string.Compare(text, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Variant";
			}
			if (string.Compare(text, "filestream", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Binary";
			}
			if (string.Compare(text, "table", StringComparison.OrdinalIgnoreCase) == 0)
			{
				text = "Structured";
			}
			SqlDbType sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), text, true);
			SqlDbType sqlDbType2 = sqlDbType;
			switch (sqlDbType2)
			{
			case SqlDbType.Binary:
			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.NVarChar:
				break;
			case SqlDbType.Bit:
			case SqlDbType.DateTime:
			case SqlDbType.Image:
			case SqlDbType.Int:
			case SqlDbType.Money:
			case SqlDbType.NText:
				return sqlDbType;
			case SqlDbType.Decimal:
			case SqlDbType.Float:
			case SqlDbType.Real:
				return sqlDbType;
			default:
				switch (sqlDbType2)
				{
				case SqlDbType.Timestamp:
				case SqlDbType.TinyInt:
					return sqlDbType;
				case SqlDbType.VarBinary:
				case SqlDbType.VarChar:
					break;
				default:
					return sqlDbType;
				}
				break;
			}
			return sqlDbType;
		}

		internal static Type GetClosestRuntimeType(SqlDbType sqlDbType)
		{
			switch (sqlDbType)
			{
			case SqlDbType.BigInt:
				return typeof(long);
			case SqlDbType.Binary:
			case SqlDbType.Image:
			case SqlDbType.Timestamp:
			case SqlDbType.VarBinary:
				return typeof(Binary);
			case SqlDbType.Bit:
				return typeof(bool);
			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.NText:
			case SqlDbType.NVarChar:
			case SqlDbType.Text:
			case SqlDbType.VarChar:
				return typeof(string);
			case SqlDbType.DateTime:
			case SqlDbType.SmallDateTime:
			case SqlDbType.Date:
			case SqlDbType.DateTime2:
				return typeof(DateTime);
			case SqlDbType.Decimal:
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				return typeof(decimal);
			case SqlDbType.Float:
				return typeof(double);
			case SqlDbType.Int:
				return typeof(int);
			case SqlDbType.Real:
				return typeof(float);
			case SqlDbType.UniqueIdentifier:
				return typeof(Guid);
			case SqlDbType.SmallInt:
				return typeof(short);
			case SqlDbType.TinyInt:
				return typeof(byte);
			case SqlDbType.Xml:
				return typeof(XElement);
			case SqlDbType.Time:
				return typeof(TimeSpan);
			case SqlDbType.DateTimeOffset:
				return typeof(DateTimeOffset);
			}
			return typeof(object);
		}
	}
}
