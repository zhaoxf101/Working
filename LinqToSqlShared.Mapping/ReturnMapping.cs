using System;

namespace LinqToSqlShared.Mapping
{
	internal class ReturnMapping
	{
		private string dbType;

		internal string DbType
		{
			get
			{
				return this.dbType;
			}
			set
			{
				this.dbType = value;
			}
		}
	}
}
