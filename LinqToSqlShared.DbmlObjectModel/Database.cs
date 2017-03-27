using System;
using System.Collections.Generic;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class Database : Node
	{
		private List<Table> tables;

		private List<Function> functions;

		private Connection connection;

		private string name;

		private string dbClass;

		private string entityNamespace;

		private string contextNamespace;

		private AccessModifier? accessModifier;

		private ClassModifier? modifier;

		private string baseType;

		private string provider;

		private bool? externalMapping;

		private SerializationMode? serialization;

		private string entityBase;

		internal List<Table> Tables
		{
			get
			{
				return this.tables;
			}
		}

		internal List<Function> Functions
		{
			get
			{
				return this.functions;
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
				this.name = value;
			}
		}

		internal string Class
		{
			get
			{
				return this.dbClass;
			}
			set
			{
				this.dbClass = value;
			}
		}

		internal string EntityNamespace
		{
			get
			{
				return this.entityNamespace;
			}
			set
			{
				this.entityNamespace = value;
			}
		}

		internal string ContextNamespace
		{
			get
			{
				return this.contextNamespace;
			}
			set
			{
				this.contextNamespace = value;
			}
		}

		internal Connection Connection
		{
			get
			{
				return this.connection;
			}
			set
			{
				this.connection = value;
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

		internal string BaseType
		{
			get
			{
				return this.baseType;
			}
			set
			{
				this.baseType = value;
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

		internal bool? ExternalMapping
		{
			get
			{
				return this.externalMapping;
			}
			set
			{
				this.externalMapping = value;
			}
		}

		internal SerializationMode? Serialization
		{
			get
			{
				return this.serialization;
			}
			set
			{
				this.serialization = value;
			}
		}

		internal string EntityBase
		{
			get
			{
				return this.entityBase;
			}
			set
			{
				this.entityBase = value;
			}
		}

		internal Database()
		{
			this.tables = new List<Table>();
			this.functions = new List<Function>();
		}
	}
}
