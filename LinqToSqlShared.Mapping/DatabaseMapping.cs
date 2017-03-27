using System;
using System.Collections.Generic;

namespace LinqToSqlShared.Mapping
{
	internal class DatabaseMapping
	{
		private string databaseName;

		private string provider;

		private List<TableMapping> tables;

		private List<FunctionMapping> functions;

		internal string DatabaseName
		{
			get
			{
				return this.databaseName;
			}
			set
			{
				this.databaseName = value;
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
				this.provider = value;
			}
		}

		internal List<TableMapping> Tables
		{
			get
			{
				return this.tables;
			}
		}

		internal List<FunctionMapping> Functions
		{
			get
			{
				return this.functions;
			}
		}

		internal DatabaseMapping()
		{
			this.tables = new List<TableMapping>();
			this.functions = new List<FunctionMapping>();
		}

		internal TableMapping GetTable(string tableName)
		{
			foreach (TableMapping current in this.tables)
			{
				if (string.Compare(current.TableName, tableName, StringComparison.Ordinal) == 0)
				{
					return current;
				}
			}
			return null;
		}

		internal TableMapping GetTable(Type rowType)
		{
			foreach (TableMapping current in this.tables)
			{
				if (this.IsType(current.RowType, rowType))
				{
					return current;
				}
			}
			return null;
		}

		private bool IsType(TypeMapping map, Type type)
		{
			if (string.Compare(map.Name, type.Name, StringComparison.Ordinal) == 0 || string.Compare(map.Name, type.FullName, StringComparison.Ordinal) == 0 || string.Compare(map.Name, type.AssemblyQualifiedName, StringComparison.Ordinal) == 0)
			{
				return true;
			}
			foreach (TypeMapping current in map.DerivedTypes)
			{
				if (this.IsType(current, type))
				{
					return true;
				}
			}
			return false;
		}

		internal FunctionMapping GetFunction(string functionName)
		{
			foreach (FunctionMapping current in this.functions)
			{
				if (string.Compare(current.Name, functionName, StringComparison.Ordinal) == 0)
				{
					return current;
				}
			}
			return null;
		}
	}
}
