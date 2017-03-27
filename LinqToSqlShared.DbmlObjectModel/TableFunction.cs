using System;
using System.Collections.Generic;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal class TableFunction : Node
	{
		private List<TableFunctionParameter> arguments;

		private TableFunctionReturn tfunReturn;

		private AccessModifier? accessModifier;

		private Function mappedFunction;

		internal Function MappedFunction
		{
			get
			{
				return this.mappedFunction;
			}
			set
			{
				this.mappedFunction = value;
			}
		}

		internal List<TableFunctionParameter> Arguments
		{
			get
			{
				return this.arguments;
			}
		}

		internal TableFunctionReturn Return
		{
			get
			{
				return this.tfunReturn;
			}
			set
			{
				this.tfunReturn = value;
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

		internal TableFunction()
		{
			this.arguments = new List<TableFunctionParameter>();
		}
	}
}
