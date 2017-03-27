using LinqToSqlShared.Common;
using LinqToSqlShared.DbmlObjectModel;
using LinqToSqlShared.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlMetal
{
    internal class Extractor
    {
        private class DbmlKeyColumn
        {
            internal string Name;
        }

        private class UniqueConstraint
        {
            private List<string> keyColumns = new List<string>();

            internal List<string> KeyColumns
            {
                get
                {
                    return this.keyColumns;
                }
            }
        }

        private class UniqueIndex
        {
            private List<string> keyColumns = new List<string>();

            internal List<string> KeyColumns
            {
                get
                {
                    return this.keyColumns;
                }
            }
        }

        private const int Max = 2147483647;

        private const uint UMax = 4294967295u;

        private ExtractOptions options;

        private Database dbml;

        private ConnectionManager connectionManager;

        private Dictionary<Table, string> tableSchemas;

        private Dictionary<Function, string> functionSchemas;

        private Dictionary<Function, string> functionTypes;

        private List<Function> functionsWithErrors;

        private List<Table> views;

        private DisplayMessage display;

        private bool hasExtractionError;

        private Dictionary<Table, List<Extractor.UniqueConstraint>> uniqueConstraints;

        private Dictionary<Table, List<Extractor.UniqueIndex>> uniqueIndexes;

        private static char[] specialCharacters = new char[]
        {
            '[',
            ']',
            '.',
            '"',
            '\'',
            '\\',
            '-',
            '@',
            ' '
        };

        private static char namepartDelimiter = '.';

        internal Extractor(string constr, ExtractOptions options)
        {
            this.options = options;
            this.connectionManager = new ConnectionManager(constr, options.CommandTimeout);
            this.display = options.Display;
        }

        private void Open()
        {
            try
            {
                this.connectionManager.Open();
            }
            catch (SqlException ex)
            {
                if (this.display != null)
                {
                    this.display(Severity.Error, ex.Message);
                }
                this.hasExtractionError = true;
                this.connectionManager.Close();
            }
        }

        private void Close()
        {
            this.connectionManager.Close();
        }

        internal Database Extract(string database)
        {
            this.dbml = new Database();
            if (database == null)
            {
                database = this.connectionManager.GetDatabase();
            }
            this.dbml.Name = database;
            this.dbml.ContextNamespace = this.options.Namespace;
            this.dbml.EntityNamespace = this.options.Namespace;
            this.GetDbml(database);
            return this.dbml;
        }

        private static string GetLegalLanguageName(string name)
        {
            return Naming.MakeLegalNameLangIndependent(new StringBuilder(Naming.CapitalizeFirstLettersOfWords(name))).ToString();
        }

        private static string GetLegalLanguageName(string schema, string fullName)
        {
            string name = fullName;
            if (string.Compare(schema, "DBO", StringComparison.OrdinalIgnoreCase) == 0)
            {
                name = fullName.Substring(4);
            }
            return Extractor.GetLegalLanguageName(name);
        }

        private void GetDbml(string database)
        {
            this.Open();
            if (this.hasExtractionError)
            {
                return;
            }
            string legalLanguageName = Extractor.GetLegalLanguageName(database);
            if (string.Compare(legalLanguageName, database, StringComparison.Ordinal) == 0)
            {
                this.dbml.Name = legalLanguageName;
            }
            else
            {
                this.dbml.Name = database;
                this.dbml.Class = legalLanguageName;
            }
            this.tableSchemas = new Dictionary<Table, string>();
            this.functionSchemas = new Dictionary<Function, string>();
            this.functionTypes = new Dictionary<Function, string>();
            this.functionsWithErrors = new List<Function>();
            this.views = new List<Table>();
            this.uniqueConstraints = new Dictionary<Table, List<Extractor.UniqueConstraint>>();
            this.uniqueIndexes = new Dictionary<Table, List<Extractor.UniqueIndex>>();
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                this.options.Types &= (ExtractTypes.Tables | ExtractTypes.Relationships);
            }
            if ((this.options.Types & ExtractTypes.Tables) != (ExtractTypes)0 || (this.options.Types & ExtractTypes.Views) != (ExtractTypes)0)
            {
                this.GetTablesAndViews();
                this.GetPrimaryKeys();
                this.GetUniqueKeys();
                this.GetIndexes();
            }
            if ((this.options.Types & ExtractTypes.Relationships) != (ExtractTypes)0)
            {
                this.GetRelationships();
            }
            if ((this.options.Types & ExtractTypes.Functions) != (ExtractTypes)0 || (this.options.Types & ExtractTypes.StoredProcedures) != (ExtractTypes)0)
            {
                this.GetSprocsAndFunctions();
            }
            this.Close();
        }

        private static void CopyStringToStringBuilder(StringBuilder sb, string str)
        {
            sb.Remove(0, sb.Length);
            sb.Append(str);
        }

        private void GetTablesAndViews()
        {
            string text = "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES ";
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                text += "WHERE TABLE_TYPE <> 'SYSTEM TABLE' ";
            }
            else
            {
                text += "WHERE ISNULL(OBJECTPROPERTY(OBJECT_ID(TABLE_NAME), 'IsMSShipped'), 0) = 0 ";
            }
            text += "ORDER BY TABLE_NAME";
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(text))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    while (dbDataReader.Read())
                    {
                        string text2 = (string)Extractor.ValueOrDefault(dbDataReader["TABLE_SCHEMA"], "");
                        string text3 = (string)Extractor.ValueOrDefault(dbDataReader["TABLE_NAME"], "");
                        string strB = ((string)Extractor.ValueOrDefault(dbDataReader["TABLE_TYPE"], "")).Trim();
                        if (string.Compare(text3, "SYSDIAGRAMS", StringComparison.OrdinalIgnoreCase) != 0 && (((string.Compare("BASE TABLE", strB, StringComparison.Ordinal) == 0 || string.Compare("TABLE", strB, StringComparison.Ordinal) == 0) && (this.options.Types & ExtractTypes.Tables) != (ExtractTypes)0) || (string.Compare("VIEW", strB, StringComparison.Ordinal) == 0 && (this.options.Types & ExtractTypes.Views) != (ExtractTypes)0)))
                        {
                            string fullName = Extractor.GetFullName(text2, text3);
                            string text4 = Extractor.GetLegalLanguageName(text2, fullName);
                            string text5 = text4;
                            if (this.options.Pluralize)
                            {
                                Extractor.CopyStringToStringBuilder(stringBuilder, text4);
                                text4 = Naming.MakePluralName(stringBuilder.ToString());
                                Extractor.CopyStringToStringBuilder(stringBuilder, text5);
                                text5 = Naming.MakeSingularName(stringBuilder.ToString());
                                if (text5 == null || text5.Length == 0)
                                {
                                    text5 = "Class";
                                }
                            }
                            if (!this.IsUniqueClassName(text5) || Shared.ReservedTypeNames.Contains(text5))
                            {
                                text5 = this.GetUniqueClassName(text5);
                            }
                            if (!this.IsUniqueContextMemberName(text4))
                            {
                                text4 = this.GetUniqueMemberName(text4);
                            }
                            string name = text5;
                            LinqToSqlShared.DbmlObjectModel.Type type = new LinqToSqlShared.DbmlObjectModel.Type(name);
                            Table table = new Table(text3, type);
                            table.Member = text4;
                            this.tableSchemas.Add(table, text2);
                            this.dbml.Tables.Add(table);
                            if (string.Compare("VIEW", strB, StringComparison.Ordinal) == 0)
                            {
                                this.views.Add(table);
                            }
                        }
                    }
                }
            }
            foreach (Table current in this.dbml.Tables)
            {
                try
                {
                    this.GetTableColumns(current);
                }
                catch (SqlException exception)
                {
                    this.LogTableOrViewError(current, exception, false);
                }
                catch (Exception exception2)
                {
                    this.LogTableOrViewError(current, exception2, true);
                }
            }
            foreach (Table current2 in this.views)
            {
                Extractor.RemovePrimaryKeyAttributeForViews(current2);
            }
        }

        private void LogTableOrViewError(Table table, Exception exception, bool showStack)
        {
            string text = exception.Message.Replace("\r\n", " ");
            text = Strings.UnableToExtractTable(table.Name, text);
            if (this.views.Contains(table))
            {
                text = Strings.UnableToExtractView(table.Name, text);
            }
            if (showStack)
            {
                text = text + "\n" + exception.StackTrace;
            }
            if (this.display != null)
            {
                this.display(Severity.Warning, text);
            }
        }

        private static void RemovePrimaryKeyAttributeForViews(Table view)
        {
            if (view != null)
            {
                foreach (Column current in view.Type.Columns)
                {
                    current.IsDbGenerated = new bool?(false);
                    current.IsPrimaryKey = new bool?(false);
                }
            }
        }

        private void GetSprocsAndFunctions()
        {
            this.GetSprocAndFunctionDefinitions();
            this.GetSprocAndFunctionResultTypes();
        }

        private void GetSprocAndFunctionDefinitions()
        {
            string sql = "SELECT r.SPECIFIC_SCHEMA, r.ROUTINE_TYPE, r.SPECIFIC_NAME, r.DATA_TYPE AS ROUTINE_DATA_TYPE, p.ORDINAL_POSITION, p.PARAMETER_MODE, p.PARAMETER_NAME, p.DATA_TYPE, p.CHARACTER_MAXIMUM_LENGTH, p.NUMERIC_PRECISION, p.NUMERIC_SCALE, p.DATETIME_PRECISION,p.IS_RESULT FROM INFORMATION_SCHEMA.ROUTINES AS r FULL OUTER JOIN INFORMATION_SCHEMA.PARAMETERS AS p on r.SPECIFIC_NAME = p.SPECIFIC_NAME AND r.SPECIFIC_SCHEMA = p.SPECIFIC_SCHEMA WHERE (r.ROUTINE_TYPE = 'PROCEDURE' OR r.ROUTINE_TYPE = 'FUNCTION') AND ISNULL(OBJECTPROPERTY(OBJECT_ID(r.SPECIFIC_NAME), 'IsMSShipped'), 0) = 0 ORDER BY r.SPECIFIC_SCHEMA, r.SPECIFIC_NAME, p.ORDINAL_POSITION";
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    Function function = null;
                    string strA = "";
                    string strA2 = "";
                    bool flag = (this.options.Types & ExtractTypes.StoredProcedures) != (ExtractTypes)0;
                    bool flag2 = (this.options.Types & ExtractTypes.Functions) != (ExtractTypes)0;
                    while (dbDataReader.Read())
                    {
                        string text = (string)dbDataReader["ROUTINE_TYPE"];
                        if ((text == "FUNCTION" && flag2) || (text == "PROCEDURE" && flag))
                        {
                            string text2 = (string)dbDataReader["SPECIFIC_NAME"];
                            string text3 = (string)dbDataReader["SPECIFIC_SCHEMA"];
                            if (string.Compare(strA, text2, StringComparison.Ordinal) != 0 || string.Compare(strA2, text3, StringComparison.Ordinal) != 0)
                            {
                                strA = text2;
                                strA2 = text3;
                                string text4 = (string)Extractor.ValueOrDefault(dbDataReader["ROUTINE_DATA_TYPE"], "");
                                function = new Function(text2);
                                this.dbml.Functions.Add(function);
                                this.functionSchemas.Add(function, text3);
                                if (text == "FUNCTION")
                                {
                                    if (text4 == "TABLE")
                                    {
                                        function.IsComposable = new bool?(true);
                                        text = "TVF";
                                    }
                                    else
                                    {
                                        text = "SVF";
                                        if (!string.IsNullOrEmpty(text4))
                                        {
                                            function.Return = new Return(text4);
                                            function.IsComposable = new bool?(true);
                                        }
                                        else
                                        {
                                            function.IsComposable = new bool?(false);
                                        }
                                    }
                                }
                                if (text == "PROCEDURE")
                                {
                                    function.Return = new Return(typeof(int).ToString());
                                    function.Return.DbType = "Int";
                                }
                                this.functionTypes.Add(function, text);
                            }
                            if (dbDataReader["ORDINAL_POSITION"] != DBNull.Value)
                            {
                                string a = (string)Extractor.ValueOrDefault(dbDataReader["IS_RESULT"], "NO");
                                string stype = (string)dbDataReader["DATA_TYPE"];
                                SqlDbType sqlDbType = DbTypeSystem.Parse(stype);
                                System.Type closestRuntimeType = DbTypeSystem.GetClosestRuntimeType(sqlDbType);
                                string scopedTypeName = Extractor.GetScopedTypeName(closestRuntimeType);
                                int size = (int)Extractor.ValueOrDefault(dbDataReader["CHARACTER_MAXIMUM_LENGTH"], 0);
                                short precision = (short)Convert.ToSByte(Extractor.ValueOrDefault(dbDataReader["NUMERIC_PRECISION"], -1), CultureInfo.InvariantCulture);
                                short scale;
                                if (Extractor.HasDateTimePrecision(sqlDbType))
                                {
                                    scale = (short)Convert.ToSByte(Extractor.ValueOrDefault(dbDataReader["DATETIME_PRECISION"], -1), CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    scale = (short)Convert.ToSByte(Extractor.ValueOrDefault(dbDataReader["NUMERIC_SCALE"], -1), CultureInfo.InvariantCulture);
                                }
                                string sqlTypeDeclaration = Extractor.GetSqlTypeDeclaration(sqlDbType, size, precision, scale, false, false);
                                if (!(a == "YES"))
                                {
                                    string name = (string)dbDataReader["PARAMETER_NAME"];
                                    Parameter parameter = new Parameter(name, scopedTypeName);
                                    parameter.DbType = sqlTypeDeclaration;
                                    parameter.Name = parameter.Name.Replace("@", "");
                                    StringBuilder stringBuilder = new StringBuilder(Extractor.GetLegalLanguageName(parameter.Name));
                                    stringBuilder[0] = char.ToLower(stringBuilder[0], CultureInfo.InvariantCulture);
                                    parameter.ParameterName = stringBuilder.ToString();
                                    if (!Naming.IsUniqueParameterName(parameter.ParameterName, function))
                                    {
                                        parameter.ParameterName = Naming.GetUniqueParameterName(function, parameter.ParameterName);
                                    }
                                    string text5 = (string)dbDataReader["PARAMETER_MODE"];
                                    string a2;
                                    if ((a2 = text5) == null)
                                    {
                                        goto IL_3FC;
                                    }
                                    LinqToSqlShared.DbmlObjectModel.ParameterDirection value;
                                    if (!(a2 == "IN"))
                                    {
                                        if (!(a2 == "INOUT"))
                                        {
                                            if (!(a2 == "OUT"))
                                            {
                                                goto IL_3FC;
                                            }
                                            value = LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out;
                                        }
                                        else
                                        {
                                            value = LinqToSqlShared.DbmlObjectModel.ParameterDirection.InOut;
                                        }
                                    }
                                    else
                                    {
                                        value = LinqToSqlShared.DbmlObjectModel.ParameterDirection.In;
                                    }
                                    IL_3FF:
                                    parameter.Direction = new LinqToSqlShared.DbmlObjectModel.ParameterDirection?(value);
                                    function.Parameters.Add(parameter);
                                    if (sqlDbType == SqlDbType.Structured)
                                    {
                                        this.display(Severity.Warning, Strings.SprocParameterTypeNotSupported(function.Name, parameter.Name, parameter.DbType));
                                        this.functionsWithErrors.Add(function);
                                        continue;
                                    }
                                    continue;
                                    IL_3FC:
                                    value = LinqToSqlShared.DbmlObjectModel.ParameterDirection.In;
                                    goto IL_3FF;
                                }
                                function.Return = new Return("");
                                function.Return.Type = scopedTypeName;
                                function.Return.DbType = sqlTypeDeclaration;
                            }
                        }
                    }
                }
            }
        }

        private bool MatchingTableMemberExists(string legalLanguageName)
        {
            return this.dbml.Tables.Exists((Table t) => Naming.IsSameName(t.Member, legalLanguageName));
        }

        private bool MatchingTypeExists(string legalLanguageName)
        {
            return Naming.IsSameName(this.dbml.Class, legalLanguageName) || (this.dbml.Class == null && Naming.IsSameName(this.dbml.Name, legalLanguageName)) || this.dbml.Tables.Exists((Table t) => Naming.IsSameName(t.Type.Name, legalLanguageName));
        }

        private bool MatchingFunctionMethodExists(string legalLanguageName)
        {
            return this.dbml.Functions.Exists((Function f) => Naming.IsSameName(f.Method, legalLanguageName));
        }

        private bool IsUniqueClassName(string legalLanguageName)
        {
            return !this.MatchingTypeExists(legalLanguageName);
        }

        private bool IsUniqueContextMemberName(string legalLanguageName)
        {
            return !Naming.IsSameName(this.dbml.Class, legalLanguageName) && (this.dbml.Class != null || !Naming.IsSameName(this.dbml.Name, legalLanguageName)) && !this.MatchingFunctionMethodExists(legalLanguageName) && !this.MatchingTableMemberExists(legalLanguageName);
        }

        private string GetUniqueClassName(string candidateLegalLanguageName)
        {
            return Naming.GetUniqueName(candidateLegalLanguageName, new Predicate<string>(this.IsUniqueClassName));
        }

        private string GetUniqueMemberName(string candidateLegalLanguageName)
        {
            return Naming.GetUniqueName(candidateLegalLanguageName, new Predicate<string>(this.IsUniqueContextMemberName));
        }

        private void GetSprocAndFunctionResultTypes()
        {
            DbCommand dbCommand = this.connectionManager.CreateCommand();
            foreach (Function current in this.dbml.Functions)
            {
                string text = this.functionTypes[current];
                string fullMethodName = Extractor.GetFullMethodName(this.functionSchemas[current], current.Name);
                string name = current.Name;
                current.Name = Extractor.GetFullRoutineName(this.functionSchemas[current], current.Name, text);
                if (!this.IsUniqueContextMemberName(fullMethodName))
                {
                    current.Method = this.GetUniqueMemberName(fullMethodName);
                }
                else
                {
                    current.Method = fullMethodName;
                }
                if (text == "PROCEDURE")
                {
                    dbCommand.CommandType = CommandType.StoredProcedure;
                    dbCommand.CommandText = Extractor.GetBrackettedName(this.functionSchemas[current], name);
                }
                else
                {
                    if (!(text == "TVF"))
                    {
                        continue;
                    }
                    dbCommand.CommandType = CommandType.Text;
                    string text2 = "";
                    foreach (Parameter current2 in current.Parameters)
                    {
                        text2 += string.Format(CultureInfo.InvariantCulture, "@{0}, ", new object[]
                        {
                            current2.Name
                        });
                    }
                    if (text2.Length > 0)
                    {
                        text2 = text2.TrimEnd(new char[]
                        {
                            ',',
                            ' '
                        });
                    }
                    dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}({1})", new object[]
                    {
                        Extractor.GetBrackettedName(this.functionSchemas[current], name),
                        text2
                    });
                }
                this.GetRoutineResultTypes(current, dbCommand);
            }
            foreach (Function current3 in this.functionsWithErrors)
            {
                this.dbml.Functions.Remove(current3);
            }
        }

        private static DbParameter CreateParameter(DbCommand command, string name, SqlDbType dbType)
        {
            DbParameter dbParameter = command.CreateParameter();
            dbParameter.ParameterName = name;
            PropertyInfo property = dbParameter.GetType().GetProperty("SqlDbType");
            if (property != null)
            {
                property.SetValue(dbParameter, dbType, null);
            }
            return dbParameter;
        }

        private void GetRoutineResultTypes(Function f, DbCommand command)
        {
            command.Parameters.Clear();
            foreach (Parameter current in f.Parameters)
            {
                SqlDbType dbType = DbTypeSystem.Parse(current.DbType);
                DbParameter value = Extractor.CreateParameter(command, current.Name, dbType);
                command.Parameters.Add(value);
            }
            try
            {
                using (DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataTable schemaTable = dbDataReader.GetSchemaTable();
                    List<LinqToSqlShared.DbmlObjectModel.Type> list = new List<LinqToSqlShared.DbmlObjectModel.Type>();
                    int num = 0;
                    while (schemaTable != null)
                    {
                        LinqToSqlShared.DbmlObjectModel.Type type = new LinqToSqlShared.DbmlObjectModel.Type("");
                        List<string> list2 = new List<string>();
                        type.Name = string.Format(CultureInfo.InvariantCulture, "{0}Result", new object[]
                        {
                            f.Method
                        });
                        int i = 0;
                        int count = schemaTable.Rows.Count;
                        while (i < count)
                        {
                            Column column = new Column("");
                            DataRow dataRow = schemaTable.Rows[i];
                            column.Name = Convert.ToString(dataRow["ColumnName"], CultureInfo.InvariantCulture);
                            if (list2.Contains(column.Name))
                            {
                                this.functionsWithErrors.Add(f);
                                if (this.display != null)
                                {
                                    if (this.display != null && column.Name.Length == 0)
                                    {
                                        this.display(Severity.Warning, Strings.SprocResultMultipleAnonymousColumns(f.Name));
                                    }
                                    else
                                    {
                                        this.display(Severity.Warning, Strings.SprocResultColumnsHaveSameName(f.Name, column.Name));
                                    }
                                }
                                return;
                            }
                            list2.Add(column.Name);
                            string text = column.Name;
                            if (string.IsNullOrEmpty(text))
                            {
                                text = string.Format(CultureInfo.InvariantCulture, "Column{0}", new object[]
                                {
                                    i + 1
                                });
                            }
                            column.Member = Extractor.GetLegalLanguageName(text);
                            string stype = Convert.ToString(dataRow["DataTypeName"], CultureInfo.InvariantCulture);
                            SqlDbType sqlDbType = DbTypeSystem.Parse(stype);
                            System.Type closestRuntimeType = DbTypeSystem.GetClosestRuntimeType(sqlDbType);
                            column.Type = Extractor.GetScopedTypeName(closestRuntimeType);
                            int size = (int)Extractor.ValueOrDefault(dataRow["ColumnSize"], 0);
                            short precision = (short)Extractor.ValueOrDefault(dataRow["NumericPrecision"], -1);
                            short scale = (short)Extractor.ValueOrDefault(dataRow["NumericScale"], -1);
                            column.DbType = Extractor.GetSqlTypeDeclaration(sqlDbType, size, precision, scale, false, false);
                            column.CanBeNull = new bool?(true);
                            type.Columns.Add(column);
                            i++;
                        }
                        num++;
                        Extractor.AddUniqueResultType(list, type);
                        dbDataReader.NextResult();
                        schemaTable = dbDataReader.GetSchemaTable();
                    }
                    if (list.Count > 1)
                    {
                        int num2 = 1;
                        foreach (LinqToSqlShared.DbmlObjectModel.Type current2 in list)
                        {
                            LinqToSqlShared.DbmlObjectModel.Type expr_2F3 = current2;
                            expr_2F3.Name += num2++.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    f.Types.AddRange(list);
                    if (num > 1)
                    {
                        f.HasMultipleResults = new bool?(true);
                    }
                    else
                    {
                        f.HasMultipleResults = new bool?(false);
                    }
                    if (this.functionTypes[f] == "PROCEDURE")
                    {
                        f.IsComposable = new bool?(false);
                        if (f.Types.Count > 0)
                        {
                            f.Return = null;
                        }
                    }
                    else
                    {
                        f.IsComposable = new bool?(true);
                        if (f.Return == null && f.Types.Count == 0)
                        {
                            f.IsComposable = new bool?(false);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                this.LogFunctionSchemaError(f, ex.Message);
            }
        }

        private void LogFunctionSchemaError(Function routine, string errorMsg)
        {
            if (!this.functionsWithErrors.Contains(routine))
            {
                this.functionsWithErrors.Add(routine);
            }
            errorMsg = errorMsg.Replace("\r\n", " ");
            string description;
            if (string.Compare("PROCEDURE", this.functionTypes[routine], StringComparison.Ordinal) == 0)
            {
                description = Strings.UnableToExtractSproc(routine.Name, errorMsg);
            }
            else
            {
                description = Strings.UnableToExtractFunction(routine.Name, errorMsg);
            }
            if (this.display != null)
            {
                this.display(Severity.Warning, description);
            }
        }

        private static void AddUniqueResultType(List<LinqToSqlShared.DbmlObjectModel.Type> types, LinqToSqlShared.DbmlObjectModel.Type type)
        {
            foreach (LinqToSqlShared.DbmlObjectModel.Type current in types)
            {
                if (type.Columns.Count == current.Columns.Count)
                {
                    for (int i = 0; i < current.Columns.Count; i++)
                    {
                        if (type.Columns[i].Name == current.Columns[i].Name && type.Columns[i].DbType == current.Columns[i].DbType)
                        {
                            return;
                        }
                    }
                }
            }
            types.Add(type);
        }

        private void GetTableColumns(Table table)
        {
            string schema = this.tableSchemas[table];
            string brackettedName = Extractor.GetBrackettedName(schema, table.Name);
            string sql = "select * from " + brackettedName + " where 1=0";
            table.Name = Extractor.GetFullName(schema, table.Name);
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                DataTable meta = null;
                DataTable dataTable = null;
                try
                {
                    CommandBehavior behavior = (this.connectionManager.ConnectionType == ConnectionType.SqlCE) ? (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo) : CommandBehavior.Default;
                    using (DbDataReader dbDataReader = dbCommand.ExecuteReader(behavior))
                    {
                        meta = dbDataReader.GetSchemaTable();
                    }
                    if (this.connectionManager.ConnectionType != ConnectionType.SqlCE)
                    {
                        dbCommand.CommandText = "SELECT sc.name AS ColumnName, sc.iscomputed AS IsComputed, text AS Definition FROM syscolumns sc LEFT OUTER JOIN syscomments scm ON sc.id = scm.id AND scm.number = sc.colid WHERE sc.id = OBJECT_ID(@p0)";
                        DbParameter dbParameter = Extractor.CreateParameter(dbCommand, "@p0", SqlDbType.NVarChar);
                        dbParameter.Value = brackettedName;
                        dbCommand.Parameters.Add(dbParameter);
                        using (DbDataReader dbDataReader2 = dbCommand.ExecuteReader())
                        {
                            dataTable = new DataTable();
                            dataTable.Locale = CultureInfo.InvariantCulture;
                            dataTable.Load(dbDataReader2);
                        }
                    }
                    this.GetTableColumns(meta, table, dataTable);
                }
                catch (SqlException exception)
                {
                    this.LogTableOrViewError(table, exception, false);
                }
                catch (Exception exception2)
                {
                    this.LogTableOrViewError(table, exception2, true);
                }
            }
        }

        private void GetTableColumns(DataTable meta, Table table, DataTable extendedColumnInfoTable)
        {
            DataColumn column = meta.Columns["AllowDBNull"];
            DataColumn column2 = meta.Columns["ColumnName"];
            DataColumn column3 = meta.Columns["ColumnSize"];
            DataColumn column4 = meta.Columns["IsAutoIncrement"];
            DataColumn column5 = meta.Columns["IsLong"];
            DataColumn column6 = meta.Columns["IsRowVersion"];
            DataColumn column7 = meta.Columns["NumericPrecision"];
            DataColumn column8 = meta.Columns["NumericScale"];
            DataColumn column9 = meta.Columns["ProviderType"];

            foreach (DataRow dataRow in meta.Rows)
            {
                string name = (string)Extractor.ValueOrDefault(dataRow[column2], null);
                if (this.connectionManager.ConnectionType != ConnectionType.SqlCE || name == null || !name.StartsWith("__sys", StringComparison.OrdinalIgnoreCase))
                {
                    string text = Extractor.GetLegalLanguageName(name);
                    string stype = Extractor.ValueOrDefault(dataRow[column9], SqlDbType.Char).ToString();
                    SqlDbType sqlDbType = DbTypeSystem.Parse(stype);
                    if (sqlDbType == SqlDbType.Udt)
                    {
                        if (this.display != null)
                        {
                            this.display(Severity.Warning, Strings.UnableToExtractColumnBecauseOfUDT(name, table.Name));
                        }
                    }
                    else
                    {
                        System.Type closestRuntimeType = DbTypeSystem.GetClosestRuntimeType(sqlDbType);
                        if (text == null || text.Length == 0)
                        {
                            text = "Column";
                        }
                        if (!Naming.IsUniqueTableClassMemberName(text, table))
                        {
                            text = Naming.GetUniqueTableMemberName(table, text);
                        }
                        Column column10 = new Column(Extractor.GetScopedTypeName(closestRuntimeType));
                        column10.Name = Extractor.QuoteIfNeeded(name);
                        column10.Member = text;
                        bool flag = (bool)Extractor.ValueOrDefault(dataRow[column4], false);
                        if (flag)
                        {
                            column10.IsDbGenerated = new bool?(true);
                            column10.IsReadOnly = new bool?(false);
                        }
                        DataRow dataRow2 = null;
                        if (extendedColumnInfoTable != null)
                        {
                            dataRow2 = extendedColumnInfoTable.Rows.OfType<DataRow>().First((DataRow r) => string.Compare((string)r["ColumnName"], name, StringComparison.OrdinalIgnoreCase) == 0);
                        }
                        if (dataRow2 != null)
                        {
                            bool flag2 = (int)dataRow2["IsComputed"] == 1;
                            if (flag2)
                            {
                                string text2 = (string)Extractor.ValueOrDefault(dataRow2["Definition"], "");
                                if (!string.IsNullOrEmpty(text2))
                                {
                                    column10.Expression = text2;
                                }
                            }
                        }
                        if ((bool)Extractor.ValueOrDefault(dataRow[column5], false))
                        {
                            column10.UpdateCheck = new UpdateCheck?(UpdateCheck.Never);
                        }
                        if (!string.IsNullOrEmpty(column10.Expression))
                        {
                            column10.UpdateCheck = new UpdateCheck?(UpdateCheck.Never);
                        }
                        bool flag3 = (bool)Extractor.ValueOrDefault(dataRow[column], false) && !(column10.IsPrimaryKey ?? false);
                        int size = (int)Extractor.ValueOrDefault(dataRow[column3], 0);
                        if (flag3 && closestRuntimeType.IsValueType)
                        {
                            column10.CanBeNull = new bool?(true);
                        }
                        else if (!flag3 && !closestRuntimeType.IsValueType)
                        {
                            column10.CanBeNull = new bool?(false);
                        }
                        if ((bool)Extractor.ValueOrDefault(dataRow[column6], false) || sqlDbType == SqlDbType.Timestamp)
                        {
                            column10.IsVersion = new bool?(true);
                            column10.IsReadOnly = new bool?(false);
                        }
                        short precision = (short)Extractor.ValueOrDefault(dataRow[column7], -1);
                        short scale = (short)Extractor.ValueOrDefault(dataRow[column8], -1);
                        column10.DbType = Extractor.GetSqlTypeDeclaration(sqlDbType, size, precision, scale, !flag3, flag);
                        column10.CanBeNull = new bool?(flag3);
                        table.Type.Columns.Add(column10);
                    }
                }
            }
        }

        private static string GetSqlTypeDeclaration(SqlDbType sqlType, int size, short precision, short scale, bool nonNull, bool isAutoIncrement)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (sqlType == SqlDbType.Timestamp)
            {
                stringBuilder.Append("rowversion");
            }
            else if (sqlType == SqlDbType.Variant)
            {
                stringBuilder.Append("sql_variant");
            }
            else
            {
                stringBuilder.Append(sqlType.ToString());
            }
            if (Extractor.HasSize(sqlType))
            {
                stringBuilder.AppendFormat("({0})", (size == 2147483647 || size == -1) ? "MAX" : size.ToString(CultureInfo.InvariantCulture));
            }
            else if (Extractor.HasPrecision(sqlType))
            {
                stringBuilder.Append("(");
                stringBuilder.Append(precision);
                if (Extractor.HasScale(sqlType))
                {
                    stringBuilder.Append(",");
                    stringBuilder.Append(scale);
                }
                stringBuilder.Append(")");
            }
            else if (Extractor.HasDateTimePrecision(sqlType))
            {
                stringBuilder.Append("(");
                stringBuilder.Append(scale);
                stringBuilder.Append(")");
            }
            if (nonNull)
            {
                stringBuilder.Append(" NOT NULL");
            }
            if (isAutoIncrement)
            {
                if (sqlType == SqlDbType.UniqueIdentifier)
                {
                    stringBuilder.Append(" ROWGUIDCOL");
                }
                else
                {
                    stringBuilder.Append(" IDENTITY");
                }
            }
            return stringBuilder.ToString();
        }

        private void GetPrimaryKeys()
        {
            string sql;
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                sql = "\r\n              select \r\n                null as CONSTRAINT_SCHEMA,\r\n                tc.CONSTRAINT_NAME,\r\n                pkcol.ORDINAL_POSITION,\r\n                null as pkSchema,\r\n                pkcol.TABLE_NAME as pkTable,\r\n                pkcol.COLUMN_NAME as pkColumn\r\n\t          from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc\r\n\t\t      inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol\r\n                 on tc.CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and\r\n                    tc.CONSTRAINT_TYPE = 'PRIMARY KEY'\r\n              inner join INFORMATION_SCHEMA.TABLES as tables\r\n                 on tables.TABLE_NAME = pkcol.TABLE_NAME and\r\n                    tables.TABLE_TYPE <> 'SYSTEM TABLE'\r\n              order by \r\n                 tc.CONSTRAINT_SCHEMA,\r\n                 tc.CONSTRAINT_NAME,\r\n                 pkcol.ORDINAL_POSITION ";
            }
            else
            {
                sql = "\r\n              select tc.CONSTRAINT_SCHEMA,\r\n                tc.CONSTRAINT_NAME,\r\n                pkcol.ORDINAL_POSITION,\r\n                pkcol.TABLE_SCHEMA as pkSchema,\r\n                pkcol.TABLE_NAME as pkTable,\r\n                pkcol.COLUMN_NAME as pkColumn\r\n\t          from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc,\r\n\t\t           INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol\r\n              where tc.CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and\r\n                    tc.CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and\r\n                    tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and\r\n                    ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0\r\n              order by \r\n                 tc.CONSTRAINT_SCHEMA,\r\n                 tc.CONSTRAINT_NAME,\r\n                 pkcol.ORDINAL_POSITION ";
            }
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        string schema = (string)Extractor.ValueOrDefault(dbDataReader["pkSchema"], "");
                        string text = (string)dbDataReader["pkTable"];
                        string text2 = (string)dbDataReader["pkColumn"];
                        if (string.Compare(text, "SYSDIAGRAMS", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            string fullName = Extractor.GetFullName(schema, text);
                            Table table = this.FindTable(fullName);
                            text2 = Extractor.QuoteIfNeeded(text2);
                            Column column = Extractor.FindColumn(table.Type.Columns, text2);
                            if (column == null)
                            {
                                throw Error.CouldNotIdentifyPrimaryKeyColumn(text2, fullName);
                            }
                            column.IsPrimaryKey = new bool?(true);
                        }
                    }
                }
            }
        }

        private void GetUniqueKeys()
        {
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                return;
            }
            string sql = "\r\n                  select tc.CONSTRAINT_SCHEMA,\r\n                    tc.CONSTRAINT_NAME,\r\n                    pkcol.ORDINAL_POSITION,\r\n                    (select count(*) \r\n                        from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc\r\n                        where tc.CONSTRAINT_SCHEMA = kc.CONSTRAINT_SCHEMA and\r\n                              tc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME and\r\n                              ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0)\r\n                              as COUNT,\r\n                    pkcol.TABLE_SCHEMA as pkSchema,\r\n                    pkcol.TABLE_NAME as pkTable,\r\n                    pkcol.COLUMN_NAME as pkColumn\r\n\t              from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc,\r\n\t\t               INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol\r\n                  where tc.CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and\r\n                        tc.CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and\r\n                        tc.CONSTRAINT_TYPE = 'UNIQUE' and\r\n                        ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0\r\n                  order by \r\n                     tc.CONSTRAINT_SCHEMA,\r\n                     tc.CONSTRAINT_NAME,\r\n                     pkcol.ORDINAL_POSITION ";
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        int num = (int)dbDataReader["COUNT"];
                        string schema = (string)dbDataReader["pkSchema"];
                        string text = (string)dbDataReader["pkTable"];
                        string item = (string)dbDataReader["pkColumn"];
                        if (string.Compare(text, "SYSDIAGRAMS", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            string fullName = Extractor.GetFullName(schema, text);
                            Table table = this.FindTable(fullName);
                            if (table != null)
                            {
                                Extractor.UniqueConstraint uniqueConstraint = new Extractor.UniqueConstraint();
                                uniqueConstraint.KeyColumns.Add(item);
                                while (num > 1 && dbDataReader.Read())
                                {
                                    item = (string)dbDataReader["pkColumn"];
                                    uniqueConstraint.KeyColumns.Add(item);
                                    num--;
                                }
                                if (this.uniqueConstraints.ContainsKey(table))
                                {
                                    this.uniqueConstraints[table].Add(uniqueConstraint);
                                }
                                else
                                {
                                    this.uniqueConstraints.Add(table, new List<Extractor.UniqueConstraint>());
                                    this.uniqueConstraints[table].Add(uniqueConstraint);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetIndexes()
        {
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                return;
            }
            string sql;
            if (this.connectionManager.ConnectionType == ConnectionType.Sql2000)
            {
                sql = "\r\n                    select \r\n\t                    user_name(t.uid) as [schema],\r\n\t                    object_name(i.id) as [table], \r\n                        i.id as [object_id],\r\n                        i.name as [index],\r\n                        case when i.indid = 1 then 'CLUSTERED' else 'NONCLUSTERED' end as [Style],\r\n                        (select count(*) from sysindexkeys where id = i.id and indid = i.indid and\r\n                         ISNULL(OBJECTPROPERTY(id, 'IsMSShipped'), 0) = 0) as [Count],\r\n                        convert(tinyint,ik.keyno) as [Ordinal],\r\n                        c.name as [Column],\r\n                        convert(bit, case when i.status & 0x2 <> 0 then 1 else 0 end) AS is_unique,\r\n                        convert(bit, case when i.status & 0x800 <> 0 then 1 else 0 end) AS is_primary_key,\r\n                        convert(bit, case when i.status & 0x1000 <> 0 then 1 else 0 end) AS is_unique_constraint\r\n                    from \r\n                        sysindexes as i\r\n                        join sysindexkeys ik on ik.id = i.id and ik.indid = i.indid\r\n                        join syscolumns c on c.id = i.id and c.colid = ik.colid\r\n                        join sysobjects t on t.id = i.id and (t.xtype='U')\r\n                    where \r\n                        i.indid>=1 and i.indid<255 AND\r\n                        ISNULL(OBJECTPROPERTY(t.id, 'IsMSShipped'), 0) = 0\r\n                    order by\r\n                        t.uid, t.id, i.indid, ik.keyno\r\n                ";
            }
            else
            {
                sql = "\r\n                    select s.name as [schema], t.name as [table], t.object_id, \r\n\t                    x.name as [index], x.type_desc as [style], \r\n                        (select count(*) \r\n                            from sys.index_columns ic2\r\n                            where ic2.object_id = ic.object_id and\r\n                                  ic2.index_id = ic.index_id and\r\n                                  ISNULL(OBJECTPROPERTY(ic2.object_id, 'IsMSShipped'), 0) = 0)\r\n                            as [count],\r\n\t                    ic.key_ordinal as [ordinal], c.name as [column], \r\n                        x.is_unique, x.is_primary_key, x.is_unique_constraint\r\n                    from sys.indexes as x,\r\n                        sys.index_columns as ic,\r\n                        sys.columns as c,\r\n                        sys.tables as t,\r\n                        sys.schemas as s\r\n                    where x.object_id = ic.object_id and\r\n                            x.index_id = ic.index_id and\r\n                            x.object_id = c.object_id and\r\n                            ic.column_id = c.column_id and\r\n                            c.object_id = t.object_id and\r\n                            t.schema_id = s.schema_id and\r\n                            ISNULL(OBJECTPROPERTY(t.object_id, 'IsMSShipped'), 0) = 0\r\n                    order by s.schema_id, t.object_id, x.index_id, ic.key_ordinal\r\n                    ";
            }
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        bool flag = (bool)dbDataReader["is_unique"];
                        if (flag)
                        {
                            string schema = (string)dbDataReader["schema"];
                            string text = (string)dbDataReader["table"];
                            int num = (int)dbDataReader["count"];
                            string item = (string)dbDataReader["column"];
                            if (string.Compare(text, "SYSDIAGRAMS", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                string fullName = Extractor.GetFullName(schema, text);
                                Table table = this.FindTable(fullName);
                                if (table != null)
                                {
                                    Extractor.UniqueIndex uniqueIndex = new Extractor.UniqueIndex();
                                    uniqueIndex.KeyColumns.Add(item);
                                    while (num > 1 && dbDataReader.Read())
                                    {
                                        item = (string)dbDataReader["column"];
                                        uniqueIndex.KeyColumns.Add(item);
                                        num--;
                                    }
                                    if (this.uniqueIndexes.ContainsKey(table))
                                    {
                                        this.uniqueIndexes[table].Add(uniqueIndex);
                                    }
                                    else
                                    {
                                        this.uniqueIndexes.Add(table, new List<Extractor.UniqueIndex>());
                                        this.uniqueIndexes[table].Add(uniqueIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetRelationships()
        {
            string sql;
            if (this.connectionManager.ConnectionType == ConnectionType.SqlCE)
            {
                sql = "\r\n                select \r\n                null as CONSTRAINT_SCHEMA,\r\n                rc.CONSTRAINT_NAME,\r\n                pkcol.ORDINAL_POSITION,\r\n                cnt.COUNT,\r\n                null as fkSchema,\r\n                fkcol.TABLE_NAME as fkTable,\r\n                fkcol.COLUMN_NAME as fkColumn,\r\n                null as pkSchema,\r\n                pkcol.TABLE_NAME as pkTable,\r\n                pkcol.COLUMN_NAME as pkColumn,\r\n                rc.UPDATE_RULE,\r\n                rc.DELETE_RULE\r\n                 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc\r\n              INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol \r\n                      ON rc.UNIQUE_CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME\r\n              INNER join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as fkcol\r\n                      ON pkcol.ORDINAL_POSITION = fkcol.ORDINAL_POSITION AND\r\n                      rc.CONSTRAINT_NAME = fkcol.CONSTRAINT_NAME\r\n              inner join INFORMATION_SCHEMA.TABLES as tables\r\n                on tables.TABLE_NAME = pkcol.TABLE_NAME and\r\n                   tables.TABLE_TYPE <> 'SYSTEM TABLE'\r\n              cross apply (\r\n                 select count(*) as COUNT \r\n                 from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc\r\n                 where rc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME\r\n                 ) as cnt \r\n                 \r\n              order by \r\n                 rc.CONSTRAINT_SCHEMA,\r\n                 rc.CONSTRAINT_NAME,\r\n                 pkcol.ORDINAL_POSITION ";
            }
            else
            {
                sql = "\r\n              select \r\n                rc.CONSTRAINT_SCHEMA,\r\n                rc.CONSTRAINT_NAME,\r\n                pkcol.ORDINAL_POSITION,\r\n                (select count(*) from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc\r\n                    where rc.CONSTRAINT_SCHEMA = kc.CONSTRAINT_SCHEMA and\r\n                          rc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME and\r\n                          ISNULL(OBJECTPROPERTY(OBJECT_ID(kc.CONSTRAINT_NAME), 'IsMSShipped'), 0) = 0)\r\n                          as COUNT,\r\n\t            fkcol.TABLE_SCHEMA as fkSchema,\r\n                fkcol.TABLE_NAME as fkTable,\r\n                fkcol.COLUMN_NAME as fkColumn,\r\n                pkcol.TABLE_SCHEMA as pkSchema,\r\n                pkcol.TABLE_NAME as pkTable,\r\n                pkcol.COLUMN_NAME as pkColumn,\r\n                rc.UPDATE_RULE,\r\n                rc.DELETE_RULE\r\n\t          from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc,\r\n                   INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol,\r\n                   INFORMATION_SCHEMA.KEY_COLUMN_USAGE as fkcol\r\n              where rc.CONSTRAINT_SCHEMA = fkcol.CONSTRAINT_SCHEMA and\r\n                 rc.CONSTRAINT_NAME = fkcol.CONSTRAINT_NAME and\r\n                 rc.UNIQUE_CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and\r\n                 rc.UNIQUE_CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and\r\n                 pkcol.ORDINAL_POSITION = fkcol.ORDINAL_POSITION and\r\n                 ISNULL(OBJECTPROPERTY(OBJECT_ID(rc.CONSTRAINT_NAME), 'IsMSShipped'), 0) = 0\r\n              order by \r\n                 rc.CONSTRAINT_SCHEMA,\r\n                 rc.CONSTRAINT_NAME,\r\n                 pkcol.ORDINAL_POSITION ";
            }
            using (DbCommand dbCommand = this.connectionManager.CreateCommand(sql))
            {
                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        string name = (string)dbDataReader["CONSTRAINT_NAME"];
                        int num = (int)dbDataReader["COUNT"];
                        string schema = (string)Extractor.ValueOrDefault(dbDataReader["fkSchema"], "");
                        string name2 = (string)dbDataReader["fkTable"];
                        string s = (string)dbDataReader["fkColumn"];
                        string schema2 = (string)Extractor.ValueOrDefault(dbDataReader["pkSchema"], "");
                        string name3 = (string)dbDataReader["pkTable"];
                        string s2 = (string)dbDataReader["pkColumn"];
                        string deleteRule = (string)dbDataReader["DELETE_RULE"];
                        Table table = this.FindTable(Extractor.GetFullName(schema, name2));
                        string name4 = Extractor.QuoteIfNeeded(s);
                        Column column = Extractor.FindColumn(table.Type.Columns, name4);
                        Table table2 = this.FindTable(Extractor.GetFullName(schema2, name3));
                        name4 = Extractor.QuoteIfNeeded(s2);
                        Column column2 = Extractor.FindColumn(table2.Type.Columns, name4);
                        string legalLanguageName = Extractor.GetLegalLanguageName(table2.Type.Name);
                        Association association = new Association(legalLanguageName);
                        association.Name = name;
                        association.Cardinality = new Cardinality?(Cardinality.One);
                        association.IsForeignKey = new bool?(true);
                        association.Type = table2.Type.Name;
                        table.Type.Associations.Add(association);
                        legalLanguageName = Extractor.GetLegalLanguageName(table.Type.Name);
                        Association association2 = new Association(legalLanguageName);
                        association2.Name = name;
                        association2.IsForeignKey = new bool?(false);
                        association2.Cardinality = new Cardinality?(Cardinality.Many);
                        association2.DeleteRule = deleteRule;
                        association2.Type = table.Type.Name;
                        table2.Type.Associations.Add(association2);
                        bool flag = true;
                        List<string> list = new List<string>();
                        List<string> list2 = new List<string>();
                        List<string> list3 = new List<string>();
                        List<string> list4 = new List<string>();
                        List<string> list5 = new List<string>();
                        list.Add(column.Member);
                        list5.Add(column.Name);
                        if (Extractor.FindColumn(table.Type.Columns, column.Name).CanBeNull == false)
                        {
                            flag = false;
                        }
                        list4.Add(column.Member);
                        list2.Add(column2.Member);
                        list3.Add(column2.Member);
                        while (num > 1 && dbDataReader.Read())
                        {
                            s = (string)dbDataReader["fkColumn"];
                            s2 = (string)dbDataReader["pkColumn"];
                            name4 = Extractor.QuoteIfNeeded(s);
                            column = Extractor.FindColumn(table.Type.Columns, name4);
                            if (column.CanBeNull == false)
                            {
                                flag = false;
                            }
                            name4 = Extractor.QuoteIfNeeded(s2);
                            column2 = Extractor.FindColumn(table2.Type.Columns, name4);
                            list.Add(column.Member);
                            list4.Add(column.Member);
                            list2.Add(column2.Member);
                            list3.Add(column2.Member);
                            list5.Add(column.Name);
                            num--;
                        }
                        association.SetThisKey(list.ToArray());
                        association2.SetOtherKey(list4.ToArray());
                        association2.SetThisKey(list2.ToArray());
                        association.SetOtherKey(list3.ToArray());
                        if (this.IsOneToOne(table, list5.ToArray()))
                        {
                            association2.Cardinality = new Cardinality?(Cardinality.One);
                        }
                        if (association2.DeleteRule.Contains("CASCADE") && !flag)
                        {
                            association.DeleteOnNull = new bool?(true);
                        }
                    }
                }
            }
            foreach (Table current in this.dbml.Tables)
            {
                foreach (Association current2 in current.Type.Associations)
                {
                    this.InferAssociationPropertyName(current, current2);
                }
            }
        }

        private static bool SqlRequiresQuoting(string s)
        {
            return s.IndexOfAny(Extractor.specialCharacters) >= 0;
        }

        private static bool RuntimeWillQuoteCorrectly(string s)
        {
            if (s == null)
            {
                throw Error.ArgumentNull("s");
            }
            return s.Length < 2 || (!s.StartsWith("@", StringComparison.Ordinal) && s.IndexOf(Extractor.namepartDelimiter) < 0 && Extractor.IsUnquoted(s));
        }

        private static bool IsUnquoted(string s)
        {
            return !s.StartsWith("[", StringComparison.Ordinal) || !s.EndsWith("]", StringComparison.Ordinal);
        }

        private static string QuoteIfNeeded(string s)
        {
            if (Extractor.NeedsQuoting(s))
            {
                SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder();
                return sqlCommandBuilder.QuoteIdentifier(s);
            }
            return s;
        }

        private static bool NeedsQuoting(string s)
        {
            return Extractor.SqlRequiresQuoting(s) && !Extractor.RuntimeWillQuoteCorrectly(s);
        }

        private static string QuoteIfNeeded(string schema, string id)
        {
            SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder();
            bool flag = Extractor.NeedsQuoting(schema);
            bool flag2 = Extractor.NeedsQuoting(id);
            if (flag && id.EndsWith("]", StringComparison.Ordinal))
            {
                flag2 = true;
            }
            if (flag2 && schema.StartsWith("[", StringComparison.Ordinal))
            {
                flag = true;
            }
            if ((schema.StartsWith("[", StringComparison.Ordinal) && id.EndsWith("]", StringComparison.Ordinal)) || schema.Contains(".") || id.Contains("."))
            {
                flag2 = true;
                flag = true;
            }
            if (flag)
            {
                schema = sqlCommandBuilder.QuoteIdentifier(schema);
            }
            if (flag2)
            {
                id = sqlCommandBuilder.QuoteIdentifier(id);
            }
            return schema + "." + id;
        }

        internal static string GetFullRoutineName(string schema, string routine, string fType)
        {
            if (fType == "SVF")
            {
                if (string.IsNullOrEmpty(schema))
                {
                    return Extractor.QuoteIfNeeded(routine);
                }
                return Extractor.QuoteIfNeeded(schema, routine);
            }
            else
            {
                if (string.IsNullOrEmpty(schema))
                {
                    return Extractor.QuoteIfNeeded(routine);
                }
                return Extractor.QuoteIfNeeded(schema, routine);
            }
        }

        internal static string GetFullName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return Extractor.QuoteIfNeeded(name);
            }
            return Extractor.QuoteIfNeeded(schema, name);
        }

        private static string GetFullMethodName(string schema, string name)
        {
            return Extractor.GetLegalLanguageName(schema, Extractor.GetFullName(schema, name));
        }

        private void InferAssociationPropertyName(Table table, Association assoc)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string text = Extractor.GetLegalLanguageName(assoc.Type);
            if (assoc.Cardinality == Cardinality.Many)
            {
                if (this.options.Pluralize)
                {
                    Extractor.CopyStringToStringBuilder(stringBuilder, text);
                    text = Naming.MakePluralName(stringBuilder.ToString());
                }
            }
            else if (this.options.Pluralize)
            {
                text = Naming.MakeSingularName(text);
            }
            if (assoc.Member != text && Naming.IsUniqueTableClassMemberName(text, table) && (assoc.Type != table.Type.Name || assoc.Cardinality == Cardinality.One))
            {
                assoc.Member = text;
                return;
            }
            string[] thisKey = assoc.GetThisKey();
            if (thisKey.Length == 1)
            {
                text = Extractor.GetLegalLanguageName(thisKey[0]);
                bool flag = text.EndsWith("id", StringComparison.CurrentCultureIgnoreCase);
                if (text.Length > 4 && flag)
                {
                    text = text.Substring(0, text.Length - 2);
                    if (this.options.Pluralize)
                    {
                        if (assoc.Cardinality == Cardinality.One)
                        {
                            text = Naming.MakeSingularName(text);
                        }
                        else
                        {
                            Extractor.CopyStringToStringBuilder(stringBuilder, text);
                            text = Naming.MakePluralName(stringBuilder.ToString());
                        }
                    }
                    if (assoc.Member != text && Naming.IsUniqueTableClassMemberName(text, table))
                    {
                        assoc.Member = text;
                        return;
                    }
                }
                else if (!flag)
                {
                    string text2 = Extractor.GetLegalLanguageName(assoc.Type);
                    if (assoc.Cardinality == Cardinality.One)
                    {
                        if (this.options.Pluralize)
                        {
                            text2 = Naming.MakeSingularName(text2);
                        }
                    }
                    else if (this.options.Pluralize)
                    {
                        Extractor.CopyStringToStringBuilder(stringBuilder, text2);
                        text2 = Naming.MakePluralName(stringBuilder.ToString());
                    }
                    text += text2;
                    if (assoc.Member != text && Naming.IsUniqueTableClassMemberName(text, table))
                    {
                        assoc.Member = text;
                        return;
                    }
                }
            }
            if (assoc.Name != null)
            {
                text = assoc.Name;
                if (string.Compare(text, 0, "fk_", 0, 3, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    text = text.Substring(3);
                }
                else if (string.Compare(text, 0, "fk", 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    text = text.Substring(2);
                }
                text = Extractor.GetLegalLanguageName(text);
                if (this.options.Pluralize)
                {
                    if (assoc.Cardinality == Cardinality.One)
                    {
                        text = Naming.MakeSingularName(text);
                    }
                    else
                    {
                        Extractor.CopyStringToStringBuilder(stringBuilder, text);
                        text = Naming.MakePluralName(stringBuilder.ToString());
                    }
                }
                if (assoc.Member != text && Naming.IsUniqueTableClassMemberName(text, table))
                {
                    assoc.Member = text;
                    return;
                }
            }
            assoc.Member = Naming.GetUniqueTableMemberName(table, text);
        }

        private bool IsOneToOne(Table table, string[] keyColumns)
        {
            List<Extractor.DbmlKeyColumn> list = new List<Extractor.DbmlKeyColumn>();
            foreach (Column current in table.Type.Columns)
            {
                if (current.IsPrimaryKey == true)
                {
                    list.Add(new Extractor.DbmlKeyColumn
                    {
                        Name = current.Name
                    });
                }
            }
            if (list.Count != 0 && Extractor.ColumnsMatch(list, keyColumns))
            {
                return true;
            }
            if (this.uniqueConstraints.ContainsKey(table))
            {
                foreach (Extractor.UniqueConstraint current2 in this.uniqueConstraints[table])
                {
                    if (Extractor.ColumnsMatch(current2.KeyColumns, keyColumns))
                    {
                        bool result = true;
                        return result;
                    }
                }
            }
            if (this.uniqueIndexes.ContainsKey(table))
            {
                foreach (Extractor.UniqueIndex current3 in this.uniqueIndexes[table])
                {
                    if (Extractor.ColumnsMatch(current3.KeyColumns, keyColumns))
                    {
                        bool result = true;
                        return result;
                    }
                }
            }
            return false;
        }

        private static List<string> GetNameList(string[] a)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i].Trim(new char[]
                {
                    ' '
                }));
            }
            return list;
        }

        private static bool ColumnsMatch(List<Extractor.DbmlKeyColumn> a, string[] b)
        {
            int count = a.Count;
            if (count != b.Length)
            {
                return false;
            }
            List<string> nameList = Extractor.GetNameList(b);
            return Extractor.IsSameSet(a, nameList);
        }

        private static bool ColumnsMatch(List<string> a, string[] b)
        {
            int count = a.Count;
            if (count != b.Length)
            {
                return false;
            }
            List<string> nameList = Extractor.GetNameList(b);
            return Extractor.IsSameSet(a, nameList);
        }

        private static bool IsSameSet(List<string> list1, List<string> list2)
        {
            return list1.Except(list2).Count<string>() == 0;
        }

        private static bool IsSameSet(List<Extractor.DbmlKeyColumn> list1, List<string> list2)
        {
            return (from key in list1
                    select key.Name).Except(list2).Count<string>() == 0;
        }

        private Table FindTable(string name)
        {
            foreach (Table current in this.dbml.Tables)
            {
                if (string.Compare(current.Name, name, StringComparison.Ordinal) == 0)
                {
                    return current;
                }
            }
            return null;
        }

        private static Column FindColumn(IEnumerable<Column> cols, string name)
        {
            if (cols == null)
            {
                return null;
            }
            foreach (Column current in cols)
            {
                if (string.Compare(current.Name, name, StringComparison.Ordinal) == 0)
                {
                    return current;
                }
            }
            return null;
        }

        private static bool HasSize(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.Binary:
                case SqlDbType.Char:
                    break;
                case SqlDbType.Bit:
                    return false;
                default:
                    switch (type)
                    {
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                            break;
                        case SqlDbType.NText:
                            return false;
                        default:
                            switch (type)
                            {
                                case SqlDbType.VarBinary:
                                case SqlDbType.VarChar:
                                    break;
                                default:
                                    return false;
                            }
                            break;
                    }
                    break;
            }
            return true;
        }

        private static bool HasPrecision(SqlDbType type)
        {
            return type == SqlDbType.Decimal;
        }

        private static bool HasDateTimePrecision(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasScale(SqlDbType type)
        {
            return type == SqlDbType.Decimal;
        }

        private static string GetScopedTypeName(System.Type type)
        {
            if (type.Namespace != null && type.Namespace.Length > 0)
            {
                return type.Namespace + "." + type.Name;
            }
            return type.Name;
        }

        private static object ValueOrDefault(object value, object defvalue)
        {
            if (value == DBNull.Value || value == null)
            {
                return defvalue;
            }
            return value;
        }

        private static string GetBrackettedName(string schema, string name)
        {
            SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder();
            if (string.IsNullOrEmpty(schema))
            {
                return sqlCommandBuilder.QuoteIdentifier(name);
            }
            return sqlCommandBuilder.QuoteIdentifier(schema) + "." + sqlCommandBuilder.QuoteIdentifier(name);
        }
    }
}
