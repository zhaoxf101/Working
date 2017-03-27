using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class TableFunctionParameter : Node
	{
		private string parameterName;

		private string member;

		private Version? version;

		internal string Member
		{
			get
			{
				return this.member;
			}
			set
			{
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Parameter", "Member");
				}
				this.member = value;
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
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Parameter", "Parameter");
				}
				this.parameterName = value;
			}
		}

		internal Version? Version
		{
			get
			{
				return this.version;
			}
			set
			{
				this.version = value;
			}
		}

		internal TableFunctionParameter(string name, string member)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Parameter", "Parameter");
			}
			if (member == null)
			{
				throw Error.SchemaRequirementViolation("Parameter", "Member");
			}
			this.parameterName = name;
			this.member = member;
		}
	}
}
