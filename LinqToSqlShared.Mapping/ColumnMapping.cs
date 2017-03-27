using System;
using System.Data.Linq.Mapping;

namespace LinqToSqlShared.Mapping
{
	internal sealed class ColumnMapping : MemberMapping
	{
		private string dbType;

		private string expression;

		private bool isPrimaryKey;

		private bool isDBGenerated;

		private bool isVersion;

		private bool isDiscriminator;

		private bool? canBeNull = null;

		private UpdateCheck updateCheck;

		private AutoSync autoSync;

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

		internal string XmlCanBeNull
		{
			get
			{
				if (!this.canBeNull.HasValue)
				{
					return null;
				}
				if (!(this.canBeNull == true))
				{
					return "false";
				}
				return null;
			}
			set
			{
				this.canBeNull = new bool?(value == null || bool.Parse(value));
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

		internal bool IsPrimaryKey
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

		internal string XmlIsPrimaryKey
		{
			get
			{
				if (!this.isPrimaryKey)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isPrimaryKey = (value != null && bool.Parse(value));
			}
		}

		internal bool IsDbGenerated
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

		internal string XmlIsDbGenerated
		{
			get
			{
				if (!this.isDBGenerated)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isDBGenerated = (value != null && bool.Parse(value));
			}
		}

		internal bool IsVersion
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

		internal string XmlIsVersion
		{
			get
			{
				if (!this.isVersion)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isVersion = (value != null && bool.Parse(value));
			}
		}

		internal bool IsDiscriminator
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

		internal string XmlIsDiscriminator
		{
			get
			{
				if (!this.isDiscriminator)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isDiscriminator = (value != null && bool.Parse(value));
			}
		}

		internal UpdateCheck UpdateCheck
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

		internal string XmlUpdateCheck
		{
			get
			{
				if (this.updateCheck == UpdateCheck.Always)
				{
					return null;
				}
				return this.updateCheck.ToString();
			}
			set
			{
				this.updateCheck = ((value == null) ? UpdateCheck.Always : ((UpdateCheck)Enum.Parse(typeof(UpdateCheck), value)));
			}
		}

		internal AutoSync AutoSync
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

		internal string XmlAutoSync
		{
			get
			{
				if (this.autoSync == AutoSync.Default)
				{
					return null;
				}
				return this.autoSync.ToString();
			}
			set
			{
				this.autoSync = ((value != null) ? ((AutoSync)Enum.Parse(typeof(AutoSync), value)) : AutoSync.Default);
			}
		}

		internal ColumnMapping()
		{
		}
	}
}
