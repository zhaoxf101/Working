using System;

namespace LinqToSqlShared.Mapping
{
	internal sealed class AssociationMapping : MemberMapping
	{
		private string thisKey;

		private string otherKey;

		private string deleteRule;

		private bool deleteOnNull;

		private bool isForeignKey;

		private bool isUnique;

		internal string ThisKey
		{
			get
			{
				return this.thisKey;
			}
			set
			{
				this.thisKey = value;
			}
		}

		internal string OtherKey
		{
			get
			{
				return this.otherKey;
			}
			set
			{
				this.otherKey = value;
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

		internal bool DeleteOnNull
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

		internal bool IsForeignKey
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

		internal string XmlIsForeignKey
		{
			get
			{
				if (!this.isForeignKey)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isForeignKey = (value != null && bool.Parse(value));
			}
		}

		internal string XmlDeleteOnNull
		{
			get
			{
				if (!this.deleteOnNull)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.deleteOnNull = (value != null && bool.Parse(value));
			}
		}

		internal bool IsUnique
		{
			get
			{
				return this.isUnique;
			}
			set
			{
				this.isUnique = value;
			}
		}

		internal string XmlIsUnique
		{
			get
			{
				if (!this.isUnique)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isUnique = (value != null && bool.Parse(value));
			}
		}

		internal AssociationMapping()
		{
		}
	}
}
