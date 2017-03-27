using System;

namespace LinqToSqlShared.Mapping
{
	internal abstract class MemberMapping
	{
		private string name;

		private string member;

		private string storageMember;

		internal string DbName
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

		internal string MemberName
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

		internal string StorageMemberName
		{
			get
			{
				return this.storageMember;
			}
			set
			{
				this.storageMember = value;
			}
		}

		internal MemberMapping()
		{
		}
	}
}
