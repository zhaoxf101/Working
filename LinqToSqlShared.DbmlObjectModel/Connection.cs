using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Connection : Node
	{
		private ConnectionMode? mode;

		private string connectionString;

		private string settingsObjectName;

		private string settingsPropertyName;

		private string provider;

		internal ConnectionMode? Mode
		{
			get
			{
				return this.mode;
			}
			set
			{
				this.mode = value;
			}
		}

		internal string ConnectionString
		{
			get
			{
				return this.connectionString;
			}
			set
			{
				this.connectionString = value;
			}
		}

		internal string SettingsObjectName
		{
			get
			{
				return this.settingsObjectName;
			}
			set
			{
				this.settingsObjectName = value;
			}
		}

		internal string SettingsPropertyName
		{
			get
			{
				return this.settingsPropertyName;
			}
			set
			{
				this.settingsPropertyName = value;
			}
		}

		internal string Provider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value == null)
				{
					throw Error.SchemaRequirementViolation("Connection", "Provider");
				}
				this.provider = value;
			}
		}

		internal Connection(string provider)
		{
			if (provider == null)
			{
				throw Error.SchemaRequirementViolation("Connection", "Provider");
			}
			this.provider = provider;
		}
	}
}
