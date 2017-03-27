using System;
using System.Collections.Generic;

namespace LinqToSqlShared.Mapping
{
	internal class TypeMapping
	{
		private string name;

		private TypeMapping baseType;

		private List<MemberMapping> members;

		private string inheritanceCode;

		private bool isInheritanceDefault;

		private List<TypeMapping> derivedTypes;

		internal TypeMapping BaseType
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

		internal List<MemberMapping> Members
		{
			get
			{
				return this.members;
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

		internal bool IsInheritanceDefault
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

		internal string XmlIsInheritanceDefault
		{
			get
			{
				if (!this.isInheritanceDefault)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isInheritanceDefault = (value != null && bool.Parse(value));
			}
		}

		internal List<TypeMapping> DerivedTypes
		{
			get
			{
				return this.derivedTypes;
			}
		}

		internal TypeMapping()
		{
			this.members = new List<MemberMapping>();
			this.derivedTypes = new List<TypeMapping>();
		}
	}
}
