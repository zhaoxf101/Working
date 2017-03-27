using System;
using System.Collections.Generic;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Type : Node
	{
		private List<Column> columns;

		private List<Association> associations;

		private List<Type> subTypes;

		private string name;

		private string inheritanceCode;

		private bool? isInheritanceDefault;

		private AccessModifier? accessModifier;

		private ClassModifier? modifier;

		internal List<Column> Columns
		{
			get
			{
				return this.columns;
			}
		}

		internal List<Association> Associations
		{
			get
			{
				return this.associations;
			}
		}

		internal List<Type> SubTypes
		{
			get
			{
				return this.subTypes;
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
					throw Error.SchemaRequirementViolation("Type", "Name");
				}
				this.name = value;
			}
		}

		internal string InheritanceCode
		{
			get
			{
				return this.inheritanceCode;
			}
			set
			{
				this.inheritanceCode = value;
			}
		}

		internal bool? IsInheritanceDefault
		{
			get
			{
				return this.isInheritanceDefault;
			}
			set
			{
				this.isInheritanceDefault = value;
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

		internal ClassModifier? Modifier
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

		internal Type(string name)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Type", "Name");
			}
			this.name = name;
			this.columns = new List<Column>();
			this.associations = new List<Association>();
			this.subTypes = new List<Type>();
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				Node.KeyValue<string>("Name", this.Name),
				Node.KeyValue<AccessModifier?>("AccessModifier", this.AccessModifier),
				Node.KeyValue<ClassModifier?>("Modifier", this.Modifier),
				Node.KeyValue<int>("Columns", this.Columns.Count),
				Node.KeyValue<int>("Associations", this.Associations.Count),
				Node.KeyValue<int>("SubTypes", this.SubTypes.Count),
				Node.KeyValue<bool?>("IsInheritanceDefault", this.IsInheritanceDefault),
				Node.KeyValue<string>("InheritanceCode", this.InheritanceCode)
			});
		}
	}
}
