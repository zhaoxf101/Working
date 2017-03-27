using System;
using System.Text;

namespace LinqToSqlShared.Generator
{
	internal class ResultFile
	{
		private string fileName;

		private string fileContent;

		private bool isSourceFile;

		private bool isPrimary;

		private Encoding encoding;

		internal string FileName
		{
			get
			{
				return this.fileName;
			}
			set
			{
				this.fileName = value;
			}
		}

		internal string FileContent
		{
			get
			{
				return this.fileContent;
			}
			set
			{
				this.fileContent = value;
			}
		}

		internal bool IsSourceFile
		{
			get
			{
				return this.isSourceFile;
			}
			set
			{
				this.isSourceFile = value;
			}
		}

		internal bool IsPrimary
		{
			get
			{
				return this.isPrimary;
			}
			set
			{
				this.isPrimary = value;
			}
		}

		internal Encoding Encoding
		{
			get
			{
				return this.encoding;
			}
			set
			{
				this.encoding = value;
			}
		}
	}
}
