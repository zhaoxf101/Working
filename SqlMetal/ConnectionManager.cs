using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace SqlMetal
{
	internal class ConnectionManager
	{
		private const string SqlCeProviderInvariantName = "System.Data.SqlServerCe.3.5";

		private DbConnection connection;

		private ConnectionType type;

		private int timeout;

		internal ConnectionType ConnectionType
		{
			get
			{
				if (this.type == ConnectionType.Unknown)
				{
					if (this.connection.GetType().Name == "System.Data.SqlServerCe.SqlCeConnection")
					{
						this.type = ConnectionType.SqlCE;
					}
					else if (this.IsServer2KOrEarlier)
					{
						this.type = ConnectionType.Sql2000;
					}
					else if (this.IsServer2005)
					{
						this.type = ConnectionType.Sql2005;
					}
					else
					{
						this.type = ConnectionType.Sql2008;
					}
				}
				return this.type;
			}
		}

		private bool IsServer2KOrEarlier
		{
			get
			{
				this.Open();
				string serverVersion = this.connection.ServerVersion;
				return serverVersion.StartsWith("08.00.", StringComparison.Ordinal) || serverVersion.StartsWith("07.00.", StringComparison.Ordinal) || serverVersion.StartsWith("06.50.", StringComparison.Ordinal) || serverVersion.StartsWith("06.00.", StringComparison.Ordinal);
			}
		}

		private bool IsServer2005
		{
			get
			{
				this.Open();
				return this.connection.ServerVersion.StartsWith("09.00", StringComparison.Ordinal);
			}
		}

		internal ConnectionManager(string connectionString, int timeout)
		{
			this.timeout = timeout;
			DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
			dbConnectionStringBuilder.ConnectionString = connectionString;
			if (!dbConnectionStringBuilder.ContainsKey("Data Source") || !((string)dbConnectionStringBuilder["Data Source"]).EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
			{
				this.connection = new SqlConnection(connectionString);
				return;
			}
			DbProviderFactory provider = ConnectionManager.GetProvider("System.Data.SqlServerCe.3.5");
			if (provider == null)
			{
				throw Error.ProviderNotInstalled(dbConnectionStringBuilder["Data Source"], "System.Data.SqlServerCe.3.5");
			}
			this.connection = provider.CreateConnection();
			this.connection.ConnectionString = connectionString;
			this.type = ConnectionType.SqlCE;
		}

		private static DbProviderFactory GetProvider(string providerName)
		{
			bool flag = (from r in DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>()
			select (string)r["InvariantName"]).Contains(providerName, StringComparer.OrdinalIgnoreCase);
			if (flag)
			{
				return DbProviderFactories.GetFactory(providerName);
			}
			return null;
		}

		internal DbCommand CreateCommand()
		{
			return this.CreateCommand("");
		}

		internal DbCommand CreateCommand(string sql)
		{
			DbCommand dbCommand = this.connection.CreateCommand();
			dbCommand.CommandText = sql;
			if (this.ConnectionType != ConnectionType.SqlCE)
			{
				dbCommand.CommandTimeout = this.timeout;
			}
			return dbCommand;
		}

		internal void Open()
		{
			if (this.connection.State == ConnectionState.Closed)
			{
				this.connection.Open();
			}
		}

		internal void Close()
		{
			if (this.connection.State != ConnectionState.Closed)
			{
				this.connection.Close();
			}
		}

		internal string GetDatabase()
		{
			this.Open();
			return this.connection.Database;
		}
	}
}
