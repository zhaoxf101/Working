using System;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal abstract class DbmlVisitor
	{
		internal virtual Database VisitDatabase(Database db)
		{
			if (db == null)
			{
				return null;
			}
			this.VisitConnection(db.Connection);
			foreach (Table current in db.Tables)
			{
				this.VisitTable(current);
			}
			foreach (Function current2 in db.Functions)
			{
				this.VisitFunction(current2);
			}
			return db;
		}

		internal virtual Table VisitTable(Table table)
		{
			if (table == null)
			{
				return null;
			}
			this.VisitType(table.Type);
			this.VisitTableFunction(table.InsertFunction);
			this.VisitTableFunction(table.UpdateFunction);
			this.VisitTableFunction(table.DeleteFunction);
			return table;
		}

		internal virtual Function VisitFunction(Function f)
		{
			if (f == null)
			{
				return null;
			}
			foreach (Parameter current in f.Parameters)
			{
				this.VisitParameter(current);
			}
			foreach (Type current2 in f.Types)
			{
				this.VisitType(current2);
			}
			this.VisitReturn(f.Return);
			return f;
		}

		internal virtual TableFunction VisitTableFunction(TableFunction tf)
		{
			if (tf == null)
			{
				return null;
			}
			foreach (TableFunctionParameter current in tf.Arguments)
			{
				this.VisitTableFunctionParameter(current);
			}
			this.VisitTableFunctionReturn(tf.Return);
			return tf;
		}

		internal virtual Type VisitType(Type type)
		{
			if (type == null)
			{
				return null;
			}
			foreach (Column current in type.Columns)
			{
				this.VisitColumn(current);
			}
			foreach (Association current2 in type.Associations)
			{
				this.VisitAssociation(current2);
			}
			foreach (Type current3 in type.SubTypes)
			{
				this.VisitType(current3);
			}
			return type;
		}

		internal virtual Column VisitColumn(Column column)
		{
			return column;
		}

		internal virtual Association VisitAssociation(Association association)
		{
			return association;
		}

		internal virtual TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
		{
			return parameter;
		}

		internal virtual Parameter VisitParameter(Parameter parameter)
		{
			return parameter;
		}

		internal virtual Return VisitReturn(Return r)
		{
			return r;
		}

		internal virtual TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn tfr)
		{
			return tfr;
		}

		internal virtual Connection VisitConnection(Connection connection)
		{
			return connection;
		}
	}
}
