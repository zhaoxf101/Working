using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Return : Node
	{
		private string type;

		private string dbType;

		internal string Type
		{
			get
			{
				return this.type;
			}
			set
			{
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Return", "Type");
				}
				this.type = value;
			}
		}

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

		internal Return(string type)
		{
			if (type == null)
			{
				throw Error.SchemaRequirementViolation("Return", "Type");
			}
			this.type = type;
		}
	}
}
