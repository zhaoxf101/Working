using System;

namespace LinqToSqlShared.Generator
{
	internal class GenerateOptions
	{
		private string resultBaseFileName;

		private string resultBaseDirectory;

		internal string ResultBaseFileName
		{
			get
			{
				return this.resultBaseFileName;
			}
			set
			{
				this.resultBaseFileName = value;
			}
		}

		internal string ResultBaseDirectory
		{
			get
			{
				return this.resultBaseDirectory;
			}
			set
			{
				this.resultBaseDirectory = value;
			}
		}
	}
}
