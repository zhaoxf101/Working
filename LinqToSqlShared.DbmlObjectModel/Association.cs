using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Association : Node
	{
		private string name;

		private string member;

		private string storage;

		private AccessModifier? accessModifier;

		private MemberModifier? modifier;

		private string type;

		private string thisKey;

		private string otherKey;

		private bool? isForeignKey;

		private Cardinality? cardinality;

		private string deleteRule;

		private bool? deleteOnNull;

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
					throw Error.SchemaRequirementViolation("Association", "Name");
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

		internal string Storage
		{
			get
			{
				return this.storage;
			}
			set
			{
				this.storage = value;
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

		internal string Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

		internal bool? IsForeignKey
		{
			get
			{
				return this.isForeignKey;
			}
			set
			{
				this.isForeignKey = value;
			}
		}

		internal Cardinality? Cardinality
		{
			get
			{
				return this.cardinality;
			}
			set
			{
				this.cardinality = value;
			}
		}

		internal string DeleteRule
		{
			get
			{
				return this.deleteRule;
			}
			set
			{
				this.deleteRule = value;
			}
		}

		internal bool? DeleteOnNull
		{
			get
			{
				return this.deleteOnNull;
			}
			set
			{
				this.deleteOnNull = value;
			}
		}

		internal Association(string name)
		{
			if (name == null)
			{
				throw Error.SchemaRequirementViolation("Association", "Name");
			}
			this.name = name;
		}

		internal void SetThisKey(string[] columnNames)
		{
			this.thisKey = Dbml.BuildKeyField(columnNames);
		}

		internal string[] GetThisKey()
		{
			return Dbml.ParseKeyField(this.thisKey);
		}

		internal void SetOtherKey(string[] columnNames)
		{
			this.otherKey = Dbml.BuildKeyField(columnNames);
		}

		internal string[] GetOtherKey()
		{
			return Dbml.ParseKeyField(this.otherKey);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				Node.KeyValue<string>("Name", this.Name),
				Node.KeyValue<string>("Type", this.Type),
				Node.KeyValue<string>("Member", this.Member),
				Node.KeyValue<string>("ThisKey", this.thisKey),
				Node.KeyValue<string>("OtherKey", this.otherKey),
				Node.KeyValue<bool?>("IsForeignKey", this.IsForeignKey),
				Node.KeyValue<Cardinality?>("Cardinality", this.Cardinality),
				Node.KeyValue<bool?>("DeleteOnNull", this.DeleteOnNull),
				Node.KeyValue<string>("DeleteRule", this.DeleteRule)
			});
		}
	}
}
