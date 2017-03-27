using System;
using System.Collections.Generic;

namespace LinqToSqlShared.Mapping
{
	internal class FunctionMapping
	{
		private string name;

		private string methodName;

		private bool isComposable;

		private List<ParameterMapping> parameters;

		private List<TypeMapping> types;

		private ReturnMapping funReturn;

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

		internal string MethodName
		{
			get
			{
				return this.methodName;
			}
			set
			{
				this.methodName = value;
			}
		}

		internal bool IsComposable
		{
			get
			{
				return this.isComposable;
			}
			set
			{
				this.isComposable = value;
			}
		}

		internal string XmlIsComposable
		{
			get
			{
				if (!this.isComposable)
				{
					return null;
				}
				return "true";
			}
			set
			{
				this.isComposable = (value != null && bool.Parse(value));
			}
		}

		internal List<ParameterMapping> Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		internal List<TypeMapping> Types
		{
			get
			{
				return this.types;
			}
		}

		internal ReturnMapping FunReturn
		{
			get
			{
				return this.funReturn;
			}
			set
			{
				this.funReturn = value;
			}
		}

		internal FunctionMapping()
		{
			this.parameters = new List<ParameterMapping>();
			this.types = new List<TypeMapping>();
		}
	}
}
