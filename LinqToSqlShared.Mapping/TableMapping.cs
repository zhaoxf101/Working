using System;

namespace LinqToSqlShared.Mapping
{
	internal class TableMapping
	{
		private string tableName;

		private string member;

		private TypeMapping rowType;

		internal string TableName
		{
			get
			{
				return this.tableName;
			}
			set
			{
				this.tableName = value;
			}
		}

		internal string Member
		{
			get
			{
				return this.member;
			}
			set
			{
				this.member = value;
			}
		}

		internal TypeMapping RowType
		{
			get
			{
				return this.rowType;
			}
			set
			{
				this.rowType = value;
			}
		}

		internal TableMapping()
		{
		}
	}
}
