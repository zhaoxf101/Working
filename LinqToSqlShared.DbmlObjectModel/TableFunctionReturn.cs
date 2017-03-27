using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class TableFunctionReturn : Node
	{
		private string member;

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
	}
}
