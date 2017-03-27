using System;

namespace LinqToSqlShared.Mapping
{
	internal class ParameterMapping
	{
		private string name;

		private string parameterName;

		private string dbType;

		private MappingParameterDirection direction;

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

		internal string ParameterName
		{
			get
			{
				return this.parameterName;
			}
			set
			{
				this.parameterName = value;
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

		public string XmlDirection
		{
			get
			{
				if (this.direction != MappingParameterDirection.In)
				{
					return this.direction.ToString();
				}
				return null;
			}
			set
			{
				this.direction = ((value == null) ? MappingParameterDirection.In : ((MappingParameterDirection)Enum.Parse(typeof(MappingParameterDirection), value, true)));
			}
		}

		public MappingParameterDirection Direction
		{
			get
			{
				return this.direction;
			}
			set
			{
				this.direction = value;
			}
		}
	}
}
