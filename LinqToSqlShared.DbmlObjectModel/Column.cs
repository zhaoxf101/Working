using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Column : Node
	{
		private string name;

		private string member;

		private string storage;

		private AccessModifier? accessModifier;

		private MemberModifier? modifier;

		private string type;

		private string dbType;

		private bool? isReadOnly;

		private bool? isPrimaryKey;

		private bool? isDBGenerated;

		private bool? canBeNull;

		private UpdateCheck? updateCheck;

		private bool? isDiscriminator;

		private string expression;

		private bool? isVersion;

		private bool? isDelayLoaded;

		private AutoSync? autoSync;

		internal string Name
		{
			get
			{
				return this.name;
			}
			set
			{
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
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Column", "Type");
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

		internal bool? IsReadOnly
		{
			get
			{
				return this.isReadOnly;
			}
			set
			{
				this.isReadOnly = value;
			}
		}

		internal bool? IsPrimaryKey
		{
			get
			{
				return this.isPrimaryKey;
			}
			set
			{
				this.isPrimaryKey = value;
			}
		}

		internal bool? IsDbGenerated
		{
			get
			{
				return this.isDBGenerated;
			}
			set
			{
				this.isDBGenerated = value;
			}
		}

		internal bool? CanBeNull
		{
			get
			{
				return this.canBeNull;
			}
			set
			{
				this.canBeNull = value;
			}
		}

		internal UpdateCheck? UpdateCheck
		{
			get
			{
				return this.updateCheck;
			}
			set
			{
				this.updateCheck = value;
			}
		}

		internal bool? IsDiscriminator
		{
			get
			{
				return this.isDiscriminator;
			}
			set
			{
				this.isDiscriminator = value;
			}
		}

		internal string Expression
		{
			get
			{
				return this.expression;
			}
			set
			{
				this.expression = value;
			}
		}

		internal bool? IsVersion
		{
			get
			{
				return this.isVersion;
			}
			set
			{
				this.isVersion = value;
			}
		}

		internal bool? IsDelayLoaded
		{
			get
			{
				return this.isDelayLoaded;
			}
			set
			{
				this.isDelayLoaded = value;
			}
		}

		internal AutoSync? AutoSync
		{
			get
			{
				return this.autoSync;
			}
			set
			{
				this.autoSync = value;
			}
		}

		internal Column(string clrType)
		{
			if (clrType == null)
			{
				throw Error.SchemaRequirementViolation("Column", "Type");
			}
			this.type = clrType;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				Node.SingleValue<AccessModifier?>(this.AccessModifier),
				Node.SingleValue<MemberModifier?>(this.Modifier),
				Node.SingleValue<string>(this.Type),
				Node.SingleValue<string>(this.Member),
				Node.KeyValue<string>("Name", this.Name),
				Node.KeyValue<string>("DbType", this.DbType),
				Node.KeyValue<string>("Storage", this.Storage),
				Node.KeyValue<AutoSync?>("AutoSync", this.AutoSync),
				Node.KeyValue<bool?>("CanBeNull", this.CanBeNull),
				Node.KeyValue<string>("Expression", this.Expression),
				Node.KeyValue<bool?>("IsDbGenerated", this.IsDbGenerated),
				Node.KeyValue<bool?>("IsDelayLoaded", this.IsDelayLoaded),
				Node.KeyValue<bool?>("IsDiscriminator", this.IsDiscriminator),
				Node.KeyValue<bool?>("IsPrimaryKey", this.IsPrimaryKey),
				Node.KeyValue<bool?>("IsReadOnly", this.IsReadOnly),
				Node.KeyValue<bool?>("IsVersion", this.IsVersion),
				Node.KeyValue<UpdateCheck?>("UpdateCheck", this.UpdateCheck)
			});
		}
	}
}
