using System;
using System.Collections.Generic;

namespace LinqToSqlShared.Generator
{
	internal class GenerationResult
	{
		private IEnumerable<ResultFile> files;

		private IEnumerable<string> assemblies;

		internal IEnumerable<ResultFile> Files
		{
			get
			{
				return this.files;
			}
			set
			{
				this.files = value;
			}
		}

		internal IEnumerable<string> Assemblies
		{
			get
			{
				return this.assemblies;
			}
			set
			{
				this.assemblies = value;
			}
		}
	}
}
