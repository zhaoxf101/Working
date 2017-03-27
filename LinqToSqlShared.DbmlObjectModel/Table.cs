using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Table : Node
	{
		private Type type;

		private string name;

		private string member;

		private AccessModifier? accessModifier;

		private MemberModifier? modifier;

		private TableFunction insertFunction;

		private TableFunction updateFunction;

		private TableFunction deleteFunction;

		internal Type Type
		{
			get
			{
				return this.type;
			}
			set
			{
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Table", "Type");
				}
				this.type = value;
			}
		}

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
					throw Error.SchemaRequirementViolation("Table", "Name");
				}
				this.name = value;
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

		internal AccessModifier? AccessModifier
		{
			get
			{
				return this.accessModifier;
			}
			set
			{
				this.accessModifier = value;
			}
		}

		internal MemberModifier? Modifier
		{
			get
			{
				return this.modifier;
			}
			set
			{
				this.modifier = value;
			}
		}

		internal TableFunction InsertFunction
		{
			get
			{
				return this.insertFunction;
			}
			set
			{
				this.insertFunction = value;
			}
		}

		internal TableFunction UpdateFunction
		{
			get
			{
				return this.updateFunction;
			}
			set
			{
				this.updateFunction = value;
			}
		}

		internal TableFunction DeleteFunction
		{
			get
			{
				return this.deleteFunction;
			}
			set
			{
				this.deleteFunction = value;
			}
		}

		internal Table(string name, Type type)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Table", "Name");
			}
			this.name = name;
			if (type == null)
			{
				throw Error.SchemaRequirementViolation("Table", "Type");
			}
			this.type = type;
		}
	}
}
