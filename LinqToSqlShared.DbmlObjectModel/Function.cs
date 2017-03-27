using System;
using System.Collections.Generic;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Function : Node
	{
		private List<Parameter> parameters;

		private List<Type> types;

		private Return funReturn;

		private string name;

		private string method;

		private AccessModifier? accessModifier;

		private MemberModifier? modifier;

		private bool? hasMultipleResults;

		private bool? isComposable;

		internal List<Parameter> Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		internal List<Type> Types
		{
			get
			{
				return this.types;
			}
		}

		internal Return Return
		{
			get
			{
				return this.funReturn;
			}
			set
			{
				this.funReturn = value;
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
					throw Error.SchemaRequirementViolation("Function", "Name");
				}
				this.name = value;
			}
		}

		internal string Method
		{
			get
			{
				return this.method;
			}
			set
			{
				this.method = value;
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

		internal bool? HasMultipleResults
		{
			get
			{
				return this.hasMultipleResults;
			}
			set
			{
				this.hasMultipleResults = value;
			}
		}

		internal bool? IsComposable
		{
			get
			{
				return this.isComposable;
			}
			set
			{
				this.isComposable = value;
			}
		}

		internal Function(string name)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Function", "Name");
			}
			this.name = name;
			this.parameters = new List<Parameter>();
			this.types = new List<Type>();
		}
	}
}
