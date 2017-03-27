using System;

namespace SqlMetal
{
	internal class ExtractOptions
	{
		private ExtractTypes types;

		private bool pluralize;

		private int commandTimeout = 30;

		private string @namespace;

		private DisplayMessage display;

		internal ExtractTypes Types
		{
			get
			{
				return this.types;
			}
			set
			{
				this.types = value;
			}
		}

		internal bool Pluralize
		{
			get
			{
				return this.pluralize;
			}
			set
			{
				this.pluralize = value;
			}
		}

		internal int CommandTimeout
		{
			get
			{
				return this.commandTimeout;
			}
			set
			{
				this.commandTimeout = value;
			}
		}

		internal string Namespace
		{
			get
			{
				return this.@namespace;
			}
			set
			{
				this.@namespace = value;
			}
		}

		internal DisplayMessage Display
		{
			get
			{
				return this.display;
			}
			set
			{
				this.display = value;
			}
		}
	}
}
