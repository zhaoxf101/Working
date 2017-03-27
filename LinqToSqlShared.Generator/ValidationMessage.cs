using LinqToSqlShared.Common;
using LinqToSqlShared.DbmlObjectModel;
using System;

namespace LinqToSqlShared.Generator
{
	internal class ValidationMessage
	{
		private string description;

		private Severity severity;

		private Node node;

		private int nodeId;

		internal string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		internal Severity Severity
		{
			get
			{
				return this.severity;
			}
			set
			{
				this.severity = value;
			}
		}

		internal int NodeId
		{
			get
			{
				return this.nodeId;
			}
		}

		internal Node Node
		{
			get
			{
				return this.node;
			}
		}

		internal ValidationMessage(string description, Severity severity, Node node, int nodeId)
		{
			this.description = description;
			this.severity = severity;
			this.node = node;
			this.nodeId = nodeId;
		}
	}
}
