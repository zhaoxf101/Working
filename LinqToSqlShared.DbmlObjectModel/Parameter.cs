using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Parameter : Node
	{
		private string name;

		private string parameterName;

		private string type;

		private string dbType;

		private ParameterDirection? direction;

		internal string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Parameter", "Name");
				}
				this.name = value;
			}
		}

		internal string ParameterName
		{
			get
			{
				return this.parameterName;
			}
			set
			{
				this.parameterName = value;
			}
		}

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
					throw Error.SchemaRequirementViolation("Parameter", "Type");
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

		internal ParameterDirection? Direction
		{
			get
			{
				return this.direction;
			}
			set
			{
				this.direction = value;
			}
		}

		internal Parameter(string name, string type)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Parameter", "Name");
			}
			this.name = name;
			if (type == null)
			{
				throw Error.SchemaRequirementViolation("Parameter", "Type");
			}
			this.type = type;
		}
	}
}
