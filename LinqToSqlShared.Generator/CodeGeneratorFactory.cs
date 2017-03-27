using LinqToSqlShared.Common;
using LinqToSqlShared.DbmlObjectModel;
using LinqToSqlShared.Mapping;
using LinqToSqlShared.Utility;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LinqToSqlShared.Generator
{
    internal class CodeGeneratorFactory
    {
        private enum CUDMethodType
        {
            Insert,
            Update,
            Delete
        }

        private class DbmlGenerator : IMappingCodeGenerator
        {
            private class NameLegalizer : DbmlVisitor
            {
                private CodeDomProvider provider;

                internal NameLegalizer(CodeDomProvider provider)
                {
                    this.provider = provider;
                }

                internal override Database VisitDatabase(Database db)
                {
                    if (db == null)
                    {
                        return null;
                    }
                    db.Class = Naming.LegalizeKeyword(db.Class, this.provider);
                    db.EntityNamespace = CodeGeneratorFactory.DbmlGenerator.NameLegalizer.GetLegalLanguageNamespace(db.EntityNamespace, this.provider);
                    db.ContextNamespace = CodeGeneratorFactory.DbmlGenerator.NameLegalizer.GetLegalLanguageNamespace(db.ContextNamespace, this.provider);
                    return base.VisitDatabase(db);
                }

                internal override Table VisitTable(Table table)
                {
                    if (table == null)
                    {
                        return null;
                    }
                    table.Member = Naming.LegalizeKeyword(table.Member, this.provider);
                    return base.VisitTable(table);
                }

                internal override Function VisitFunction(Function f)
                {
                    if (f == null)
                    {
                        return null;
                    }
                    f.Method = Naming.LegalizeKeyword(f.Method, this.provider);
                    return base.VisitFunction(f);
                }

                internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (type == null)
                    {
                        return null;
                    }
                    type.Name = Naming.LegalizeKeyword(type.Name, this.provider);
                    return base.VisitType(type);
                }

                internal override Column VisitColumn(Column column)
                {
                    if (column == null)
                    {
                        return null;
                    }
                    column.Member = Naming.LegalizeKeyword(column.Member, this.provider);
                    return base.VisitColumn(column);
                }

                internal override Association VisitAssociation(Association association)
                {
                    if (association == null)
                    {
                        return null;
                    }
                    association.Type = Naming.LegalizeKeyword(association.Type, this.provider);
                    association.Member = Naming.LegalizeKeyword(association.Member, this.provider);
                    return base.VisitAssociation(association);
                }

                internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
                {
                    parameter.ParameterName = Naming.LegalizeKeyword(parameter.ParameterName, this.provider);
                    parameter.Member = Naming.LegalizeKeyword(parameter.Member, this.provider);
                    return parameter;
                }

                internal override Parameter VisitParameter(Parameter parameter)
                {
                    parameter.ParameterName = Naming.LegalizeKeyword(parameter.ParameterName, this.provider);
                    return parameter;
                }

                internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn tfr)
                {
                    if (tfr == null)
                    {
                        return null;
                    }
                    tfr.Member = Naming.LegalizeKeyword(tfr.Member, this.provider);
                    return tfr;
                }

                private static string GetLegalLanguageNamespace(string ns, CodeDomProvider provider)
                {
                    if (ns == null)
                    {
                        return null;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    string[] array = ns.Split(new char[]
                    {
                        '.'
                    });
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        string text = array2[i];
                        if (text.Length != 0)
                        {
                            if (stringBuilder.Length != 0)
                            {
                                stringBuilder.Append(".");
                            }
                            stringBuilder.Append(Naming.LegalizeKeyword(text, provider));
                        }
                    }
                    return stringBuilder.ToString();
                }
            }

            internal static class MappingXmlFormatter
            {
                internal static void Build(XmlWriter writer, DatabaseMapping mapping)
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Database", "http://schemas.microsoft.com/linqtosql/mapping/2007");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", mapping.DatabaseName);
                    if (mapping.Provider != null)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Provider", mapping.Provider);
                    }
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildTables(writer, mapping.Tables);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildFunctions(writer, mapping.Functions);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                private static void WriteAttribute(XmlWriter writer, string attribute, string value)
                {
                    if (value != null)
                    {
                        writer.WriteAttributeString(attribute, value);
                    }
                }

                private static void BuildTables(XmlWriter writer, IEnumerable<TableMapping> tables)
                {
                    foreach (TableMapping current in tables)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildTable(writer, current);
                    }
                }

                private static void BuildTable(XmlWriter writer, TableMapping tm)
                {
                    writer.WriteStartElement("Table");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", tm.TableName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Member", tm.Member);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildType(writer, tm.RowType);
                    writer.WriteEndElement();
                }

                private static void BuildFunctions(XmlWriter writer, IEnumerable<FunctionMapping> functions)
                {
                    foreach (FunctionMapping current in functions)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildFunction(writer, current);
                    }
                }

                private static void BuildFunction(XmlWriter writer, FunctionMapping mf)
                {
                    writer.WriteStartElement("Function");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", mf.Name);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Method", mf.MethodName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsComposable", mf.XmlIsComposable);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildParameters(writer, mf.Parameters);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildElementTypes(writer, mf.Types);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildReturn(writer, mf.FunReturn);
                    writer.WriteEndElement();
                }

                private static void BuildParameters(XmlWriter writer, IEnumerable<ParameterMapping> pms)
                {
                    foreach (ParameterMapping current in pms)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildParameter(writer, current);
                    }
                }

                private static void BuildElementTypes(XmlWriter writer, IEnumerable<TypeMapping> tms)
                {
                    foreach (TypeMapping current in tms)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildElementType(writer, current);
                    }
                }

                private static void BuildReturn(XmlWriter writer, ReturnMapping rm)
                {
                    if (rm != null)
                    {
                        writer.WriteStartElement("Return");
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "DbType", rm.DbType);
                        writer.WriteEndElement();
                    }
                }

                private static void BuildParameter(XmlWriter writer, ParameterMapping pm)
                {
                    writer.WriteStartElement("Parameter");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", pm.Name);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Parameter", pm.ParameterName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "DbType", pm.DbType);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Direction", pm.XmlDirection);
                    writer.WriteEndElement();
                }

                private static void BuildElementType(XmlWriter writer, TypeMapping tm)
                {
                    writer.WriteStartElement("ElementType");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", tm.Name);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "InheritanceCode", tm.InheritanceCode);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsInheritanceDefault", tm.XmlIsInheritanceDefault);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildMembers(writer, tm.Members);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildTypes(writer, tm.DerivedTypes);
                    writer.WriteEndElement();
                }

                private static void BuildMembers(XmlWriter writer, IEnumerable<MemberMapping> mms)
                {
                    foreach (MemberMapping current in mms)
                    {
                        ColumnMapping columnMapping = current as ColumnMapping;
                        if (columnMapping != null)
                        {
                            CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildColumnMapping(writer, columnMapping);
                        }
                        else
                        {
                            CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildAssociationMapping(writer, (AssociationMapping)current);
                        }
                    }
                }

                private static void BuildTypes(XmlWriter writer, IEnumerable<TypeMapping> tms)
                {
                    foreach (TypeMapping current in tms)
                    {
                        CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildType(writer, current);
                    }
                }

                private static void BuildColumnMapping(XmlWriter writer, ColumnMapping cm)
                {
                    writer.WriteStartElement("Column");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", cm.DbName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Member", cm.MemberName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Storage", cm.StorageMemberName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "DbType", cm.DbType);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "CanBeNull", cm.XmlCanBeNull);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Expression", cm.Expression);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsPrimaryKey", cm.XmlIsPrimaryKey);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsDbGenerated", cm.XmlIsDbGenerated);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsVersion", cm.XmlIsVersion);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsDiscriminator", cm.XmlIsDiscriminator);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "UpdateCheck", cm.XmlUpdateCheck);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "AutoSync", cm.XmlAutoSync);
                    writer.WriteEndElement();
                }

                private static void BuildAssociationMapping(XmlWriter writer, AssociationMapping am)
                {
                    writer.WriteStartElement("Association");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", am.DbName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Member", am.MemberName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Storage", am.StorageMemberName);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "ThisKey", am.ThisKey);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "OtherKey", am.OtherKey);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "DeleteRule", am.DeleteRule);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "DeleteOnNull", am.XmlDeleteOnNull);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsForeignKey", am.XmlIsForeignKey);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsUnique", am.XmlIsUnique);
                    writer.WriteEndElement();
                }

                private static void BuildType(XmlWriter writer, TypeMapping tm)
                {
                    writer.WriteStartElement("Type");
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "Name", tm.Name);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "InheritanceCode", tm.InheritanceCode);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.WriteAttribute(writer, "IsInheritanceDefault", tm.XmlIsInheritanceDefault);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildMembers(writer, tm.Members);
                    CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.BuildTypes(writer, tm.DerivedTypes);
                    writer.WriteEndElement();
                }
            }

            private class Validator : DbmlVisitor
            {
                private class CreateDbmlTypeColumns : DbmlVisitor
                {
                    private Dictionary<string, List<Column>> typeColumns;

                    private List<Column> accessibleColumns;

                    internal Dictionary<string, List<Column>> GetTypeColumns(Database db)
                    {
                        this.typeColumns = new Dictionary<string, List<Column>>();
                        base.VisitDatabase(db);
                        return this.typeColumns;
                    }

                    internal override Table VisitTable(Table table)
                    {
                        this.accessibleColumns = new List<Column>();
                        return base.VisitTable(table);
                    }

                    internal override Function VisitFunction(Function f)
                    {
                        this.accessibleColumns = new List<Column>();
                        return base.VisitFunction(f);
                    }

                    internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                    {
                        if (this.typeColumns.ContainsKey(type.Name))
                        {
                            return type;
                        }
                        List<Column> list = new List<Column>();
                        list.AddRange(this.accessibleColumns);
                        this.accessibleColumns.AddRange(type.Columns);
                        this.typeColumns.Add(type.Name, new List<Column>(this.accessibleColumns));
                        base.VisitType(type);
                        this.accessibleColumns = list;
                        return type;
                    }
                }

                private enum Compatibility
                {
                    Compatible,
                    NotSupported,
                    DataLossToDatabase,
                    DataLossFromDatabase,
                    DataLossBoth,
                    DependOnDataLength,
                    InvalidDbType,
                    UserDefinedDbType,
                    UnknownCLRType
                }

                private static class TypeCompatibility
                {
                    private static int maxScale = -1;

                    private static Dictionary<SqlDbType, Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>> compatibilityMatrix;

                    private static void InitializeMatrix()
                    {
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix = new Dictionary<SqlDbType, Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>>();
                        Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility> dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Bit, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.TinyInt, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Enum), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.SmallInt, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Enum), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Int, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Enum), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.BigInt, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Enum), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(TimeSpan), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.SmallMoney, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Money, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Decimal, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Real, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Float, dictionary);
                        dictionary.Add(typeof(bool), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(byte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(short), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(int), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(long), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(sbyte), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ushort), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(uint), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(ulong), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(decimal), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(float), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(double), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Char, dictionary);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.NChar, dictionary);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.VarChar, dictionary);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.NVarChar, dictionary);
                        dictionary.Add(typeof(char), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Text, dictionary);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.NText, dictionary);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Xml, dictionary);
                        dictionary.Add(typeof(string), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(char[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XDocument), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(XElement), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.SmallDateTime, dictionary);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.DateTime, dictionary);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Date, dictionary);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Time, dictionary);
                        dictionary.Add(typeof(TimeSpan), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.DateTime2, dictionary);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.DateTimeOffset, dictionary);
                        dictionary.Add(typeof(DateTime), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(DateTimeOffset), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Binary, dictionary);
                        dictionary.Add(typeof(byte[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(ISerializable), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth);
                        dictionary.Add(typeof(Binary), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.VarBinary, dictionary);
                        dictionary.Add(typeof(byte[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary.Add(typeof(ISerializable), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength);
                        dictionary.Add(typeof(Binary), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Image, dictionary);
                        dictionary.Add(typeof(byte[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(ISerializable), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase);
                        dictionary.Add(typeof(Binary), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Timestamp, dictionary);
                        dictionary.Add(typeof(byte[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Binary), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.UniqueIdentifier, dictionary);
                        dictionary.Add(typeof(byte[]), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                        dictionary.Add(typeof(Guid), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible);
                        dictionary = new Dictionary<System.Type, CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility>();
                        CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.Add(SqlDbType.Variant, dictionary);
                        dictionary.Add(typeof(object), CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase);
                    }

                    internal static CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility GetCompatibility(string dbType, string clrType)
                    {
                        System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(clrType);
                        if (type == null)
                        {
                            string strA = clrType.Replace(" ", "");
                            if (string.Compare(strA, typeof(XDocument).ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                type = typeof(XDocument);
                            }
                            else
                            {
                                if (string.Compare(strA, typeof(XElement).ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    return CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UnknownCLRType;
                                }
                                type = typeof(XElement);
                            }
                        }
                        if (dbType == null)
                        {
                            return CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                        }
                        SqlDbType sqlDbType;
                        try
                        {
                            sqlDbType = DbTypeSystem.Parse(dbType);
                            if (sqlDbType == SqlDbType.Udt)
                            {
                                CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility result = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UserDefinedDbType;
                                return result;
                            }
                        }
                        catch (ArgumentException)
                        {
                            CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility result = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType;
                            return result;
                        }
                        int scale = 0;
                        try
                        {
                            scale = CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetDataScale(sqlDbType, dbType);
                            int num;
                            if (sqlDbType == SqlDbType.Decimal && CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetDecimalPrecision(dbType, out num) && num > 29)
                            {
                                CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility result = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase;
                                return result;
                            }
                        }
                        catch (FormatException)
                        {
                            CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility result = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType;
                            return result;
                        }
                        catch (OverflowException)
                        {
                            CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility result = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType;
                            return result;
                        }
                        return CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetCompatibility(sqlDbType, scale, type);
                    }

                    private static int GetDataScale(SqlDbType sqlType, string dbType)
                    {
                        string text = dbType.Replace(" ", "");
                        text = text.Split(new char[]
                        {
                            ','
                        })[0];
                        int num = text.IndexOf(')');
                        if (num == -1)
                        {
                            return 1;
                        }
                        if (sqlType == SqlDbType.Float && !text.Contains("("))
                        {
                            return 53;
                        }
                        if (sqlType == SqlDbType.Decimal && !text.Contains("("))
                        {
                            return 18;
                        }
                        if (!text.Contains("("))
                        {
                            return 1;
                        }
                        int length = sqlType.ToString().Length;
                        string text2 = text.Substring(length + 1, num - length - 1);
                        if (string.Compare(text2, "MAX", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.maxScale;
                        }
                        return int.Parse(text2, NumberFormatInfo.InvariantInfo);
                    }

                    private static bool GetDecimalPrecision(string dbType, out int precision)
                    {
                        precision = -1;
                        string text = dbType.TrimStart(null);
                        string text2 = text.Replace(" ", "");
                        text2 = text2.Split(new char[]
                        {
                            ','
                        })[0];
                        int num = text2.IndexOf('(');
                        if (num == -1)
                        {
                            return false;
                        }
                        int num2 = text2.IndexOf(')');
                        string s = (num2 == -1) ? text2.Substring(num + 1) : text2.Substring(num + 1, num2 - num - 1);
                        precision = int.Parse(s, NumberFormatInfo.InvariantInfo);
                        return true;
                    }

                    private static CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility GetCompatibility(SqlDbType sqlType, int scale, System.Type clrType)
                    {
                        if (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix == null)
                        {
                            CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.InitializeMatrix();
                        }
                        if (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.ImplementsISerializable(clrType) && clrType != typeof(DateTime) && clrType != typeof(DateTimeOffset))
                        {
                            clrType = typeof(ISerializable);
                        }
                        if (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.IsNullableType(clrType))
                        {
                            System.Type[] genericArguments = clrType.GetGenericArguments();
                            if (genericArguments.Length != 1)
                            {
                                return CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported;
                            }
                            clrType = genericArguments[0];
                        }
                        if (!CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix.ContainsKey(sqlType))
                        {
                            return CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported;
                        }
                        if (!CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix[sqlType].ContainsKey(clrType))
                        {
                            return CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported;
                        }
                        CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.compatibilityMatrix[sqlType][clrType];
                        if (compatibility == CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DependOnDataLength)
                        {
                            if (sqlType <= SqlDbType.NVarChar)
                            {
                                switch (sqlType)
                                {
                                    case SqlDbType.Char:
                                        break;
                                    case SqlDbType.DateTime:
                                        return compatibility;
                                    case SqlDbType.Decimal:
                                        if (scale < 29)
                                        {
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase;
                                            return compatibility;
                                        }
                                        if (scale == 29)
                                        {
                                            if (clrType == typeof(decimal))
                                            {
                                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase;
                                                return compatibility;
                                            }
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase;
                                            return compatibility;
                                        }
                                        else
                                        {
                                            if (clrType == typeof(double))
                                            {
                                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase;
                                                return compatibility;
                                            }
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase;
                                            return compatibility;
                                        }
                                        break;
                                    case SqlDbType.Float:
                                        if (scale > 24)
                                        {
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                                            return compatibility;
                                        }
                                        if (clrType == typeof(float))
                                        {
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                                            return compatibility;
                                        }
                                        compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase;
                                        return compatibility;
                                    default:
                                        switch (sqlType)
                                        {
                                            case SqlDbType.NChar:
                                                break;
                                            case SqlDbType.NText:
                                                return compatibility;
                                            case SqlDbType.NVarChar:
                                                goto IL_174;
                                            default:
                                                return compatibility;
                                        }
                                        break;
                                }
                                if (scale == 1)
                                {
                                    compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                                    return compatibility;
                                }
                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported;
                                return compatibility;
                            }
                            else
                            {
                                switch (sqlType)
                                {
                                    case SqlDbType.VarBinary:
                                        if (scale == CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.maxScale)
                                        {
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase;
                                            return compatibility;
                                        }
                                        compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth;
                                        return compatibility;
                                    case SqlDbType.VarChar:
                                        break;
                                    default:
                                        if (sqlType != SqlDbType.DateTime2)
                                        {
                                            return compatibility;
                                        }
                                        if (scale <= 2)
                                        {
                                            compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                                            return compatibility;
                                        }
                                        compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase;
                                        return compatibility;
                                }
                            }
                            IL_174:
                            if (scale == CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.maxScale && clrType == typeof(string))
                            {
                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                            }
                            else if (scale == 1 && clrType == typeof(char))
                            {
                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible;
                            }
                            else
                            {
                                compatibility = CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase;
                            }
                        }
                        return compatibility;
                    }

                    private static bool ImplementsISerializable(System.Type clrType)
                    {
                        List<System.Type> list = new List<System.Type>(clrType.GetInterfaces());
                        return list.Contains(typeof(ISerializable));
                    }

                    private static bool IsNullableType(System.Type clrType)
                    {
                        return clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
                    }
                }

                private CodeDomProvider provider;

                private List<ValidationMessage> messages;

                private List<string> tableNames;

                private List<string> tableMembers;

                private List<string> functionMethods;

                private Dictionary<string, List<LinqToSqlShared.DbmlObjectModel.Type>> types;

                private List<string> columnNames;

                private List<string> columnMembers;

                private List<string> columnStorages;

                private List<string> associationMembers;

                private List<string> parameterNames;

                private List<string> parameterMembers;

                private List<string> tfpNames;

                private List<string> inheritanceCodes;

                private Table currentTable;

                private LinqToSqlShared.DbmlObjectModel.Type currentType;

                private Function currentFunction;

                private int nodeIndex;

                private bool hasInheritanceDefault;

                private bool hasDiscriminator;

                private Dictionary<string, List<Column>> typeColumns;

                private bool hasInheritance;

                private TableFunction currentTableFunction;

                private bool isSubType;

                internal Validator(CodeDomProvider provider)
                {
                    this.provider = provider;
                    this.messages = new List<ValidationMessage>();
                }

                internal List<ValidationMessage> Validate(Database db)
                {
                    CodeGeneratorFactory.DbmlGenerator.Validator.CreateDbmlTypeColumns createDbmlTypeColumns = new CodeGeneratorFactory.DbmlGenerator.Validator.CreateDbmlTypeColumns();
                    this.typeColumns = createDbmlTypeColumns.GetTypeColumns(db);
                    this.VisitDatabase(db);
                    return this.messages;
                }

                internal override Database VisitDatabase(Database db)
                {
                    if (db == null)
                    {
                        return null;
                    }
                    this.tableNames = new List<string>();
                    this.tableMembers = new List<string>();
                    this.functionMethods = new List<string>();
                    this.types = new Dictionary<string, List<LinqToSqlShared.DbmlObjectModel.Type>>();
                    this.nodeIndex = 0;
                    if (db.Class != null)
                    {
                        if (!Naming.IsValidIdentifierLangIndependent(db.Class))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidDatabaseClassIdentifier(db.Class), Severity.Warning, db, this.nodeIndex));
                        }
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(db.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidDefaultDatabaseClassIdentifier(db.Name), Severity.Error, db, this.nodeIndex));
                    }
                    string[] array = new string[0];
                    if (db.ContextNamespace != null)
                    {
                        array = db.ContextNamespace.Split(new char[]
                        {
                            '.'
                        });
                    }
                    string p = (db.Name == null) ? db.Class : db.Name;
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        string name = array2[i];
                        if (!Naming.IsValidIdentifierLangIndependent(name))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidContextNameSpace(db.ContextNamespace, p), Severity.Warning, db, this.nodeIndex));
                            break;
                        }
                    }
                    string[] array3 = new string[0];
                    if (db.EntityNamespace != null)
                    {
                        array3 = db.EntityNamespace.Split(new char[]
                        {
                            '.'
                        });
                    }
                    string[] array4 = array3;
                    for (int j = 0; j < array4.Length; j++)
                    {
                        string name2 = array4[j];
                        if (!Naming.IsValidIdentifierLangIndependent(name2))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidEntityNameSpace(db.EntityNamespace, p), Severity.Warning, db, this.nodeIndex));
                            break;
                        }
                    }
                    this.nodeIndex++;
                    return base.VisitDatabase(db);
                }

                internal override Connection VisitConnection(Connection connection)
                {
                    if (connection == null)
                    {
                        return null;
                    }
                    this.nodeIndex++;
                    return connection;
                }

                internal override Table VisitTable(Table table)
                {
                    if (table == null)
                    {
                        return null;
                    }
                    this.currentTable = table;
                    this.inheritanceCodes = new List<string>();
                    if (table.Type.SubTypes.Count > 0 || table.Type.InheritanceCode != null)
                    {
                        this.hasInheritance = true;
                    }
                    else
                    {
                        this.hasInheritance = false;
                    }
                    if (!string.IsNullOrEmpty(table.Name) && this.tableNames.Contains(table.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameTableNameIdentifier(table.Name), Severity.Warning, table, this.nodeIndex));
                    }
                    else
                    {
                        this.tableNames.Add(table.Name);
                    }
                    if (this.tableMembers.Contains(table.Member) && table.Member != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameTableMemberIdentifier(table.Member), Severity.Error, table, this.nodeIndex));
                    }
                    else
                    {
                        this.tableMembers.Add(table.Member);
                    }
                    if (table.Member != null)
                    {
                        if (!Naming.IsValidIdentifierLangIndependent(table.Member))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidTableMemberIdentifier(table.Member), Severity.Warning, table, this.nodeIndex));
                        }
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(table.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidDefaultTableMemberIdentifier(table.Name), Severity.Error, table, this.nodeIndex));
                    }
                    if (!this.HasPrimaryKey(table.Type.Name) && (table.InsertFunction != null || table.UpdateFunction != null || table.DeleteFunction != null))
                    {
                        this.messages.Add(new ValidationMessage(Strings.KeylessTablesCannotHaveCUDOverrides(table.Name), Severity.Warning, table, this.nodeIndex));
                    }
                    this.hasDiscriminator = false;
                    this.hasInheritanceDefault = false;
                    if (table.Type.SubTypes.Count > 0 || table.Type.InheritanceCode != null)
                    {
                        if (!CodeGeneratorFactory.DbmlGenerator.Validator.HasInheritanceDefault(table.Type))
                        {
                            this.messages.Add(new ValidationMessage(Strings.NoInheritanceDefaultTypes(table.Name), Severity.Warning, table, this.nodeIndex));
                        }
                        if (!CodeGeneratorFactory.DbmlGenerator.Validator.HasDiscriminator(table.Type))
                        {
                            this.messages.Add(new ValidationMessage(Strings.NoDiscriminator(table.Name), Severity.Warning, table, this.nodeIndex));
                        }
                    }
                    this.nodeIndex++;
                    this.isSubType = false;
                    base.VisitTable(table);
                    return table;
                }

                private static bool HasInheritanceDefault(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (type.IsInheritanceDefault == true)
                    {
                        return true;
                    }
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current in type.SubTypes)
                    {
                        if (CodeGeneratorFactory.DbmlGenerator.Validator.HasInheritanceDefault(current))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                private static bool HasDiscriminator(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    using (List<Column>.Enumerator enumerator = type.Columns.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.IsDiscriminator == true)
                            {
                                bool result = true;
                                return result;
                            }
                        }
                    }
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current in type.SubTypes)
                    {
                        if (CodeGeneratorFactory.DbmlGenerator.Validator.HasDiscriminator(current))
                        {
                            bool result = true;
                            return result;
                        }
                    }
                    return false;
                }

                internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (type == null || string.IsNullOrEmpty(type.Name))
                    {
                        return null;
                    }
                    this.currentType = type;
                    bool flag = false;
                    this.columnNames = new List<string>();
                    this.columnMembers = new List<string>();
                    this.columnStorages = new List<string>();
                    this.associationMembers = new List<string>();
                    if (Shared.ReservedTypeNames.Contains(type.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.TypeNameIsReserved(type.Name), Severity.Error, type, this.nodeIndex));
                    }
                    if (this.types.ContainsKey(type.Name) && type.Name != null)
                    {
                        if (this.types[type.Name].Contains(type))
                        {
                            flag = true;
                        }
                        else
                        {
                            this.types[type.Name].Add(type);
                            this.messages.Add(new ValidationMessage(Strings.SameTypeNameIdentifier(type.Name), Severity.Error, type, this.nodeIndex));
                        }
                    }
                    else
                    {
                        List<LinqToSqlShared.DbmlObjectModel.Type> list = new List<LinqToSqlShared.DbmlObjectModel.Type>();
                        list.Add(type);
                        this.types.Add(type.Name, list);
                    }
                    this.nodeIndex++;
                    if (!flag)
                    {
                        if (type.Name != null && !Naming.IsValidIdentifierLangIndependent(type.Name))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidITypeNamedentifier(type.Name), Severity.Warning, type, this.nodeIndex - 1));
                        }
                        if (this.hasInheritance)
                        {
                            if (type.InheritanceCode != null)
                            {
                                if (this.inheritanceCodes.Contains(type.InheritanceCode))
                                {
                                    this.messages.Add(new ValidationMessage(Strings.SameTypeInheritanceCodeIdentifier(type.InheritanceCode, this.currentType.Name), Severity.Warning, type, this.nodeIndex - 1));
                                }
                                else
                                {
                                    this.inheritanceCodes.Add(type.InheritanceCode);
                                }
                            }
                            if (type.IsInheritanceDefault == true && this.hasInheritanceDefault)
                            {
                                this.messages.Add(new ValidationMessage(Strings.MultipleInheritanceDefaultTypes(type.Name), Severity.Warning, type, this.nodeIndex - 1));
                            }
                            else if (type.IsInheritanceDefault == true)
                            {
                                this.hasInheritanceDefault = true;
                            }
                        }
                        if (type.InheritanceCode == null)
                        {
                            if (type.IsInheritanceDefault == true)
                            {
                                this.messages.Add(new ValidationMessage(Strings.NoInheritanceCodeWhenIsInheritanceDefault(type.Name), Severity.Error, type, this.nodeIndex - 1));
                            }
                        }
                        else if (type.Modifier == ClassModifier.Abstract)
                        {
                            this.messages.Add(new ValidationMessage(Strings.AbstractTypeHasInheritanceCode(type.Name), Severity.Warning, type, this.nodeIndex - 1));
                        }
                        foreach (Column current in type.Columns)
                        {
                            this.VisitColumn(current);
                        }
                        foreach (Association current2 in type.Associations)
                        {
                            this.VisitAssociation(current2);
                        }
                        this.isSubType = true;
                        foreach (LinqToSqlShared.DbmlObjectModel.Type current3 in type.SubTypes)
                        {
                            this.VisitType(current3);
                        }
                    }
                    return type;
                }

                private static bool IsCSharpCodeProvider(CodeDomProvider codeProvider)
                {
                    return codeProvider.FileExtension.ToUpper(CultureInfo.InvariantCulture).Contains("CS");
                }

                internal override Column VisitColumn(Column column)
                {
                    if (column == null)
                    {
                        return null;
                    }
                    string text = (column.Name == null) ? column.Member : column.Name;
                    this.ValidateColumnUniqueNameAndMemberAndStorage(column);
                    this.ValidateColumnMemberIsValidIdentifier(column);
                    this.ValidateColumnIsDiscriminator(column, text);
                    this.ValidateColumnCompatibleColumnDbTypeAndClrType(column, text);
                    this.ValidateColumnDiscriminatorType(column, text);
                    this.ValidateColumnMemberAndStorageDifferentFromEnclosingType(column, text);
                    this.ValidateColumnAutoSync(column, text);
                    this.ValidateColumnIdentity(column, text);
                    if (column.IsPrimaryKey == true && this.isSubType)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SubTypesCannotContainPK(this.currentType.Name, text), Severity.Error, column, this.nodeIndex));
                    }
                    this.nodeIndex++;
                    base.VisitColumn(column);
                    return column;
                }

                private void ValidateColumnAutoSync(Column column, string colId)
                {
                    if (column.AutoSync.HasValue)
                    {
                        if (column.IsDbGenerated == true && column.IsPrimaryKey == true && column.AutoSync != LinqToSqlShared.DbmlObjectModel.AutoSync.OnInsert)
                        {
                            this.messages.Add(new ValidationMessage(Strings.PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsDbGenerated(colId, this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                        }
                        if (column.IsVersion == true && !column.IsDbGenerated.HasValue && column.IsPrimaryKey == true && column.AutoSync != LinqToSqlShared.DbmlObjectModel.AutoSync.OnInsert)
                        {
                            this.messages.Add(new ValidationMessage(Strings.PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsVersion(colId, this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                        }
                    }
                }

                private void ValidateColumnMemberAndStorageDifferentFromEnclosingType(Column column, string colId)
                {
                    if ((column.Member == this.currentType.Name && CodeGeneratorFactory.DbmlGenerator.Validator.IsCSharpCodeProvider(this.provider)) || (column.Name == this.currentType.Name && column.Member == null && CodeGeneratorFactory.DbmlGenerator.Validator.IsCSharpCodeProvider(this.provider)))
                    {
                        this.messages.Add(new ValidationMessage(Strings.MemberCannotBeTheSameAsTypeName("Column", colId, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                    string member = (column.Member == null) ? column.Name : column.Member;
                    string a;
                    if (column.Storage == null)
                    {
                        a = CodeGeneratorFactory.DbmlGenerator.Validator.GetDefaultStorage(member);
                    }
                    else
                    {
                        a = column.Storage;
                    }
                    if (a == this.currentType.Name)
                    {
                        this.messages.Add(new ValidationMessage(Strings.StorageCannotBeTheSameAsTypeName("Column", colId, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                }

                private void ValidateColumnIdentity(Column column, string colId)
                {
                    if (!(column.IsPrimaryKey ?? false))
                    {
                        return;
                    }
                    if (column.DbType != null && !MappingSystem.IsSupportedIdentityType(DbTypeSystem.Parse(column.DbType)))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidIdentityMememberType(colId, this.currentType.Name, column.DbType, "DbType"), Severity.Warning, column, this.nodeIndex));
                    }
                    System.Type type = null;
                    if (column.Type != null)
                    {
                        type = System.Type.GetType(column.Type);
                    }
                    if (type != null && !MappingSystem.IsSupportedIdentityType(type))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidIdentityMememberType(colId, this.currentType.Name, type, "Type"), Severity.Warning, column, this.nodeIndex));
                    }
                }

                private void ValidateColumnDiscriminatorType(Column column, string colId)
                {
                    if (column.IsDiscriminator == true)
                    {
                        if (column.DbType != null && !MappingSystem.IsSupportedDiscriminatorType(DbTypeSystem.Parse(column.DbType)))
                        {
                            this.messages.Add(new ValidationMessage(Strings.TypeNotForDiscriminator(colId, this.currentType.Name, "DbType", column.DbType), Severity.Warning, column, this.nodeIndex));
                        }
                        if (column.Type != null && System.Type.GetType(column.Type) != null && !MappingSystem.IsSupportedDiscriminatorType(System.Type.GetType(column.Type)))
                        {
                            this.messages.Add(new ValidationMessage(Strings.TypeNotForDiscriminator(colId, this.currentType.Name, "Type", column.Type), Severity.Warning, column, this.nodeIndex));
                        }
                    }
                }

                private void ValidateColumnIsDiscriminator(Column column, string colId)
                {
                    if (column.IsDiscriminator == true && this.hasDiscriminator)
                    {
                        this.messages.Add(new ValidationMessage(Strings.MultipleDiscriminators(colId, this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                        return;
                    }
                    if (column.IsDiscriminator == true)
                    {
                        this.hasDiscriminator = true;
                    }
                }

                private void ValidateColumnMemberIsValidIdentifier(Column column)
                {
                    if (column.Member != null)
                    {
                        if (!Naming.IsValidIdentifierLangIndependent(column.Member))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidColumnMemberIdentifier(column.Member, this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                            return;
                        }
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(column.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidDefaultColumnMemberIdentifier(column.Name, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                }

                private void ValidateColumnUniqueNameAndMemberAndStorage(Column column)
                {
                    if (this.columnNames.Contains(column.Name) && column.Name != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameColumnNameIdentifier(column.Name, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                    else
                    {
                        this.columnNames.Add(column.Name);
                    }
                    if (this.columnMembers.Contains(column.Member) && column.Member != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameColumnMemberIdentifier(column.Member, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                    else if (column.Member == null && this.columnMembers.Contains(column.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameColumnNameIdentifier(column.Name, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                    }
                    else if (column.Member == null)
                    {
                        this.columnMembers.Add(column.Name);
                    }
                    else
                    {
                        this.columnMembers.Add(column.Member);
                    }
                    string member = (column.Member == null) ? column.Name : column.Member;
                    string text;
                    if (column.Storage == null)
                    {
                        text = CodeGeneratorFactory.DbmlGenerator.Validator.GetDefaultStorage(member);
                    }
                    else
                    {
                        text = column.Storage;
                    }
                    if (this.columnStorages.Contains(text))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameColumnStorageIdentifier(text, this.currentType.Name), Severity.Error, column, this.nodeIndex));
                        return;
                    }
                    this.columnStorages.Add(text);
                }

                private static string GetDefaultStorage(string member)
                {
                    return "_" + member;
                }

                private void ValidateColumnCompatibleColumnDbTypeAndClrType(Column column, string colId)
                {
                    switch (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetCompatibility(column.DbType, column.Type))
                    {
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UnknownCLRType:
                            return;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported:
                            this.messages.Add(new ValidationMessage(Strings.TypeMappingNotSupported(column.DbType, column.Type, "Column", colId, "Type", this.currentType.Name), Severity.Error, column, this.nodeIndex));
                            return;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase:
                            this.messages.Add(new ValidationMessage(Strings.DataLossFromDatabase(column.DbType, column.Type, "Column", colId, "Type", this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                            return;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth:
                            this.messages.Add(new ValidationMessage(Strings.DataLossBoth(column.DbType, column.Type, "Column", colId, "Type", this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                            return;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType:
                            this.messages.Add(new ValidationMessage(Strings.UnknownDbType(column.DbType, "Column", colId, "Type", this.currentType.Name), Severity.Warning, column, this.nodeIndex));
                            return;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UserDefinedDbType:
                            this.messages.Add(new ValidationMessage(Strings.UserDefinedDbType(column.DbType, "Column", colId, "Type", this.currentType.Name, column.Type), Severity.Warning, column, this.nodeIndex));
                            return;
                    }
                    throw new InvalidOperationException();
                }

                internal override Association VisitAssociation(Association association)
                {
                    if (association == null)
                    {
                        return null;
                    }
                    if (this.associationMembers.Contains(association.Member) && association.Member != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameAssociationMemberIdentifier(association.Member, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    else
                    {
                        this.associationMembers.Add(association.Member);
                    }
                    if (association.Member == null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.AttributeIsNotPresent("Member", "Association", association.Name, "Type", this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(association.Member))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidAssociationMemberIdentifier(association.Member, this.currentType.Name), Severity.Warning, association, this.nodeIndex));
                    }
                    string[] array = association.GetThisKey();
                    string[] array2 = association.GetOtherKey();
                    if (array.Length == 0 && association.IsForeignKey != true)
                    {
                        array = (from c in this.typeColumns[this.currentType.Name]
                                 where c.IsPrimaryKey == true
                                 select c.Member).ToArray<string>();
                    }
                    if (array2.Length == 0 && association.IsForeignKey == true && this.typeColumns.ContainsKey(association.Type))
                    {
                        array2 = (from c in this.typeColumns[association.Type]
                                  where c.IsPrimaryKey == true
                                  select c.Member).ToArray<string>();
                    }
                    if (array.Length != 0 && array2.Length != 0 && array.Length != array2.Length)
                    {
                        this.messages.Add(new ValidationMessage(Strings.ThisKeyOtherKey(association.Name, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    if (array.Length == 0 && association.IsForeignKey == true)
                    {
                        this.messages.Add(new ValidationMessage(Strings.AttributeIsNotPresent("ThisKey", "Association", association.Name, "Type", this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    if (array2.Length == 0 && association.IsForeignKey != true)
                    {
                        this.messages.Add(new ValidationMessage(Strings.AttributeIsNotPresent("OtherKey", "Association", association.Name, "Type", this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    if (array.Length != 0 && array2.Length != 0)
                    {
                        if (this.IsKnownType(association.Type))
                        {
                            string[] array3 = array2;
                            for (int i = 0; i < array3.Length; i++)
                            {
                                string text = array3[i];
                                if (!this.ColumnMemberExistsInType(text, association.Type))
                                {
                                    this.messages.Add(new ValidationMessage(Strings.ColumnMemberDoesNotExist(text, association.Type), Severity.Error, association, this.nodeIndex));
                                }
                            }
                        }
                        string[] array4 = array;
                        for (int j = 0; j < array4.Length; j++)
                        {
                            string text2 = array4[j];
                            if (!this.ColumnMemberExistsInType(text2, this.currentType.Name))
                            {
                                this.messages.Add(new ValidationMessage(Strings.ColumnMemberDoesNotExist(text2, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                            }
                        }
                        if (association.Member == this.currentType.Name && CodeGeneratorFactory.DbmlGenerator.Validator.IsCSharpCodeProvider(this.provider))
                        {
                            this.messages.Add(new ValidationMessage(Strings.MemberCannotBeTheSameAsTypeName("Association", association.Name, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                        }
                    }
                    if (association.DeleteOnNull == true)
                    {
                        bool flag = !association.Cardinality.HasValue || association.Cardinality == Cardinality.One;
                        if (!flag || !(association.IsForeignKey == true) || CodeGeneratorFactory.DbmlGenerator.Validator.IsNullableAssociation(this.currentType, association))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidDeleteOnNullSpecification(association.Name, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                        }
                    }
                    if (!this.HasPrimaryKey(this.currentType.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.TypeHasNoPrimaryKey(this.currentType.Name, association.Name), Severity.Warning, association, this.nodeIndex));
                    }
                    else if (!this.HasPrimaryKey(association.Type))
                    {
                        this.messages.Add(new ValidationMessage(Strings.OtherSideHasNoPrimaryKey(association.Type, association.Name, this.currentType.Name), Severity.Warning, association, this.nodeIndex));
                    }
                    if (association.Cardinality == Cardinality.Many && association.IsForeignKey == true)
                    {
                        this.messages.Add(new ValidationMessage(Strings.AssociationManysideIsForeignKey(association.Name, this.currentType.Name), Severity.Error, association, this.nodeIndex));
                    }
                    this.nodeIndex++;
                    base.VisitAssociation(association);
                    return association;
                }

                private static bool IsNullableAssociation(LinqToSqlShared.DbmlObjectModel.Type t, Association assoc)
                {
                    List<string> list = new List<string>(assoc.GetThisKey());
                    foreach (Column current in t.Columns)
                    {
                        bool flag = (current.Member != null) ? list.Contains(current.Member) : list.Contains(current.Name);
                        if (flag && current.CanBeNull == false)
                        {
                            return false;
                        }
                    }
                    return true;
                }

                private bool HasPrimaryKey(string typeName)
                {
                    if (!this.IsKnownType(typeName))
                    {
                        return true;
                    }
                    return this.typeColumns[typeName].Exists((Column c) => c.IsPrimaryKey == true);
                }

                private bool IsKnownType(string typeName)
                {
                    return this.typeColumns.ContainsKey(typeName);
                }

                private bool ColumnMemberExistsInType(string columnMember, string typeName)
                {
                    return this.IsKnownType(typeName) && this.typeColumns[typeName].Exists((Column c) => columnMember == c.Member || (c.Member == null && c.Name == columnMember));
                }

                internal override Function VisitFunction(Function f)
                {
                    if (f == null)
                    {
                        return null;
                    }
                    this.currentFunction = f;
                    this.parameterNames = new List<string>();
                    this.parameterMembers = new List<string>();
                    this.inheritanceCodes = new List<string>();
                    if (this.functionMethods.Contains(f.Method) && f.Method != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameFunctionMethodIdentifier(f.Method), Severity.Error, f, this.nodeIndex));
                    }
                    else
                    {
                        this.functionMethods.Add(f.Method);
                    }
                    if ((f.Method != null && this.tableMembers.Contains(f.Method)) || (this.tableMembers.Contains(f.Name) && f.Method == null))
                    {
                        this.messages.Add(new ValidationMessage(Strings.TableMemberAndMethodConflict(f.Method), Severity.Error, f, this.nodeIndex));
                    }
                    if (f.Method != null)
                    {
                        if (!Naming.IsValidIdentifierLangIndependent(f.Method))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidFunctionMethodIdentifier(f.Method), Severity.Warning, f, this.nodeIndex));
                        }
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(f.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidDefaultFunctionMethodIdentifier(f.Name), Severity.Error, f, this.nodeIndex));
                    }
                    if (f.IsComposable == true)
                    {
                        if (f.Return == null && f.Types.Count == 0)
                        {
                            this.messages.Add(new ValidationMessage(Strings.IsComposableAndNumberOfTypeDoNotMatch(f.Name), Severity.Warning, f, this.nodeIndex));
                        }
                        else if (f.HasMultipleResults == true)
                        {
                            this.messages.Add(new ValidationMessage(Strings.IsComposableAndHasMultipleResultsDoNotMatch(f.Name), Severity.Warning, f, this.nodeIndex));
                        }
                        else if (f.Types.Count > 1)
                        {
                            this.messages.Add(new ValidationMessage(Strings.IsComposableAndNumberOfTypeDoNotMatch(f.Name), Severity.Warning, f, this.nodeIndex));
                        }
                    }
                    this.nodeIndex++;
                    foreach (Parameter current in f.Parameters)
                    {
                        this.VisitParameter(current);
                    }
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current2 in f.Types)
                    {
                        if (current2.SubTypes.Count > 0)
                        {
                            this.hasInheritance = true;
                        }
                        else
                        {
                            this.hasInheritance = false;
                        }
                        this.VisitType(current2);
                    }
                    this.VisitReturn(f.Return);
                    return f;
                }

                internal override TableFunction VisitTableFunction(TableFunction f)
                {
                    if (f == null)
                    {
                        return null;
                    }
                    this.currentTableFunction = f;
                    this.tfpNames = new List<string>();
                    this.nodeIndex++;
                    base.VisitTableFunction(f);
                    return f;
                }

                internal override Parameter VisitParameter(Parameter parameter)
                {
                    if (parameter == null)
                    {
                        return null;
                    }
                    if (this.parameterNames.Contains(parameter.Name) && !string.IsNullOrEmpty(parameter.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameParameterNameIdentifier(parameter.Name), Severity.Error, parameter, this.nodeIndex));
                    }
                    else
                    {
                        this.parameterNames.Add(parameter.Name);
                    }
                    if (this.parameterMembers.Contains(parameter.ParameterName) && parameter.ParameterName != null)
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameParameterParameterIdentifier(parameter.ParameterName), Severity.Error, parameter, this.nodeIndex));
                    }
                    else
                    {
                        this.parameterMembers.Add(parameter.ParameterName);
                    }
                    if (parameter.ParameterName != null)
                    {
                        if (!Naming.IsValidIdentifierLangIndependent(parameter.ParameterName))
                        {
                            this.messages.Add(new ValidationMessage(Strings.InvalidParameterParameterIdentifier(parameter.ParameterName), Severity.Warning, parameter, this.nodeIndex));
                        }
                    }
                    else if (!Naming.IsValidIdentifierLangIndependent(parameter.Name))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidDefaultParameterParameterIdentifier(parameter.Name), Severity.Warning, parameter, this.nodeIndex));
                    }
                    switch (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetCompatibility(parameter.DbType, parameter.Type))
                    {
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UnknownCLRType:
                            goto IL_277;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported:
                            this.messages.Add(new ValidationMessage(Strings.TypeMappingNotSupported(parameter.DbType, parameter.Type, "Parameter", parameter.Name, "Function", this.currentFunction.Name), Severity.Error, parameter, this.nodeIndex));
                            goto IL_277;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase:
                            this.messages.Add(new ValidationMessage(Strings.DataLossFromDatabase(parameter.DbType, parameter.Type, "Parameter", parameter.Name, "Function", this.currentFunction.Name), Severity.Warning, parameter, this.nodeIndex));
                            goto IL_277;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth:
                            this.messages.Add(new ValidationMessage(Strings.DataLossBoth(parameter.DbType, parameter.Type, "Parameter", parameter.Name, "Function", this.currentFunction.Name), Severity.Warning, parameter, this.nodeIndex));
                            goto IL_277;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType:
                            this.messages.Add(new ValidationMessage(Strings.UnknownDbType(parameter.DbType, "Parameter", parameter.Name, "Function", this.currentFunction.Name), Severity.Warning, parameter, this.nodeIndex));
                            goto IL_277;
                    }
                    throw new InvalidOperationException();
                    IL_277:
                    this.nodeIndex++;
                    base.VisitParameter(parameter);
                    return parameter;
                }

                internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
                {
                    if (parameter == null)
                    {
                        return null;
                    }
                    if (parameter.ParameterName != null && this.tfpNames.Contains(parameter.ParameterName))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameParameterNameIdentifier(parameter.ParameterName), Severity.Error, parameter, this.nodeIndex));
                    }
                    else
                    {
                        this.tfpNames.Add(parameter.ParameterName);
                    }
                    if (!Naming.IsValidIdentifierLangIndependent(parameter.ParameterName))
                    {
                        this.messages.Add(new ValidationMessage(Strings.SameParameterParameterIdentifier(parameter.ParameterName), Severity.Error, parameter, this.nodeIndex));
                    }
                    if (!this.currentTableFunction.MappedFunction.Parameters.Exists((Parameter p) => p.ParameterName == parameter.ParameterName || (p.ParameterName == null && p.Name == parameter.ParameterName)))
                    {
                        this.messages.Add(new ValidationMessage(Strings.ParameterDoesNotExistInFunction(parameter.ParameterName, this.currentTableFunction.MappedFunction.Name, parameter.ParameterName), Severity.Error, parameter, this.nodeIndex));
                    }
                    if (parameter.ParameterName != null && !Naming.IsValidIdentifierLangIndependent(parameter.ParameterName))
                    {
                        this.messages.Add(new ValidationMessage(Strings.InvalidParameterParameterIdentifier(parameter.ParameterName), Severity.Warning, parameter, this.nodeIndex));
                    }
                    if (parameter.Member != null && !CodeGeneratorFactory.DbmlGenerator.Validator.IsExistingColumn(parameter.Member, this.currentTable))
                    {
                        this.messages.Add(new ValidationMessage(Strings.ColumnMemberDoesNotExist(parameter.Member, this.currentTable.Type.Name), Severity.Error, parameter, this.nodeIndex));
                    }
                    this.nodeIndex++;
                    base.VisitTableFunctionParameter(parameter);
                    return parameter;
                }

                private static bool IsExistingColumn(string columnMember, Table table)
                {
                    foreach (Column current in table.Type.Columns)
                    {
                        if (current.Member == columnMember || (current.Member == null && current.Name == columnMember))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn r)
                {
                    if (r == null)
                    {
                        return null;
                    }
                    if (!CodeGeneratorFactory.DbmlGenerator.Validator.IsExistingColumn(r.Member, this.currentTable))
                    {
                        this.messages.Add(new ValidationMessage(Strings.ColumnMemberDoesNotExist(r.Member, this.currentTable.Type.Name), Severity.Error, r, this.nodeIndex));
                    }
                    this.nodeIndex++;
                    base.VisitTableFunctionReturn(r);
                    return r;
                }

                internal override Return VisitReturn(Return r)
                {
                    if (r == null)
                    {
                        return null;
                    }
                    switch (CodeGeneratorFactory.DbmlGenerator.Validator.TypeCompatibility.GetCompatibility(r.DbType, r.Type))
                    {
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.Compatible:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossToDatabase:
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.UnknownCLRType:
                            goto IL_137;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.NotSupported:
                            this.messages.Add(new ValidationMessage(Strings.ReturnTypeMappingNotSupported(r.DbType, r.Type, "Function", this.currentFunction.Name), Severity.Error, r, this.nodeIndex));
                            goto IL_137;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossFromDatabase:
                            this.messages.Add(new ValidationMessage(Strings.ReturnDataLossFromDatabase(r.DbType, r.Type, "Function", this.currentFunction.Name), Severity.Warning, r, this.nodeIndex));
                            goto IL_137;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.DataLossBoth:
                            this.messages.Add(new ValidationMessage(Strings.ReturnDataLossBoth(r.DbType, r.Type, "Function", this.currentFunction.Name), Severity.Warning, r, this.nodeIndex));
                            goto IL_137;
                        case CodeGeneratorFactory.DbmlGenerator.Validator.Compatibility.InvalidDbType:
                            this.messages.Add(new ValidationMessage(Strings.ReturnUnknownDbType(r.DbType, "Function", this.currentFunction.Name), Severity.Warning, r, this.nodeIndex));
                            goto IL_137;
                    }
                    throw new InvalidOperationException();
                    IL_137:
                    this.nodeIndex++;
                    base.VisitReturn(r);
                    return r;
                }
            }

            private class GenerateMapping : DbmlVisitor
            {
                private DatabaseMapping dbMapping;

                private TableMapping curTableMapping;

                private TypeMapping curTypeMapping;

                private FunctionMapping curFunctionMapping;

                private bool visitingFunction;

                private string nsName;

                private CodeGeneratorFactory.ModelInfo model;

                internal DatabaseMapping CreateDbMapping(Database db, CodeGeneratorFactory.ModelInfo modelInfo)
                {
                    this.nsName = db.EntityNamespace;
                    this.model = modelInfo;
                    this.VisitDatabase(db);
                    return this.dbMapping;
                }

                internal override Database VisitDatabase(Database db)
                {
                    if (db == null)
                    {
                        return null;
                    }
                    this.dbMapping = new DatabaseMapping();
                    if (db.Name != null)
                    {
                        this.dbMapping.DatabaseName = db.Name;
                    }
                    else
                    {
                        this.dbMapping.DatabaseName = db.Class;
                    }
                    this.dbMapping.Provider = db.Provider;
                    return base.VisitDatabase(db);
                }

                internal override Table VisitTable(Table table)
                {
                    if (table == null)
                    {
                        return null;
                    }
                    this.visitingFunction = false;
                    this.curTableMapping = new TableMapping();
                    this.curTableMapping.TableName = table.Name;
                    this.curTableMapping.Member = table.Member;
                    this.dbMapping.Tables.Add(this.curTableMapping);
                    return base.VisitTable(table);
                }

                internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (type == null)
                    {
                        return null;
                    }
                    TypeMapping typeMapping = this.curTypeMapping;
                    this.curTypeMapping = new TypeMapping();
                    if (!string.IsNullOrEmpty(this.nsName))
                    {
                        this.curTypeMapping.Name = this.nsName + "." + type.Name;
                    }
                    else
                    {
                        this.curTypeMapping.Name = type.Name;
                    }
                    if (type.InheritanceCode != null)
                    {
                        this.curTypeMapping.InheritanceCode = type.InheritanceCode;
                    }
                    if (type.IsInheritanceDefault == true)
                    {
                        this.curTypeMapping.IsInheritanceDefault = true;
                    }
                    if (typeMapping != null)
                    {
                        this.curTypeMapping.BaseType = typeMapping;
                        typeMapping.DerivedTypes.Add(this.curTypeMapping);
                    }
                    base.VisitType(type);
                    if (this.visitingFunction)
                    {
                        this.curFunctionMapping.Types.Add(this.curTypeMapping);
                    }
                    else
                    {
                        this.curTableMapping.RowType = this.curTypeMapping;
                    }
                    this.curTypeMapping = typeMapping;
                    return type;
                }

                internal override Column VisitColumn(Column column)
                {
                    if (column == null)
                    {
                        return null;
                    }
                    ColumnMapping columnMapping = new ColumnMapping();
                    columnMapping.CanBeNull = CodeGeneratorFactory.DbmlGenerator.ChooseCanBeNull(column.Type, column.CanBeNull);
                    columnMapping.DbName = column.Name;
                    columnMapping.DbType = column.DbType;
                    columnMapping.Expression = column.Expression;
                    if (column.IsDbGenerated == true)
                    {
                        columnMapping.IsDbGenerated = true;
                    }
                    if (column.IsDiscriminator == true)
                    {
                        columnMapping.IsDiscriminator = true;
                    }
                    if (column.IsPrimaryKey == true)
                    {
                        columnMapping.IsPrimaryKey = true;
                    }
                    if (column.IsVersion == true)
                    {
                        columnMapping.IsVersion = true;
                    }
                    columnMapping.MemberName = column.Member;
                    columnMapping.StorageMemberName = column.Storage;
                    if (column.UpdateCheck.HasValue && column.UpdateCheck != LinqToSqlShared.DbmlObjectModel.UpdateCheck.Always)
                    {
                        columnMapping.UpdateCheck = CodeGeneratorFactory.DbmlGenerator.GenerateMapping.ConvertUpdateCheck(column.UpdateCheck.Value);
                    }
                    if (column.AutoSync.HasValue && column.AutoSync != LinqToSqlShared.DbmlObjectModel.AutoSync.Never)
                    {
                        columnMapping.AutoSync = CodeGeneratorFactory.DbmlGenerator.GenerateMapping.ConvertAutoSync(column.AutoSync.Value);
                    }
                    this.curTypeMapping.Members.Add(columnMapping);
                    return base.VisitColumn(column);
                }

                private static System.Data.Linq.Mapping.AutoSync ConvertAutoSync(LinqToSqlShared.DbmlObjectModel.AutoSync autoSync)
                {
                    switch (autoSync)
                    {
                        case LinqToSqlShared.DbmlObjectModel.AutoSync.Default:
                            return System.Data.Linq.Mapping.AutoSync.Default;
                        case LinqToSqlShared.DbmlObjectModel.AutoSync.Always:
                            return System.Data.Linq.Mapping.AutoSync.Always;
                        case LinqToSqlShared.DbmlObjectModel.AutoSync.Never:
                            return System.Data.Linq.Mapping.AutoSync.Never;
                        case LinqToSqlShared.DbmlObjectModel.AutoSync.OnInsert:
                            return System.Data.Linq.Mapping.AutoSync.OnInsert;
                        case LinqToSqlShared.DbmlObjectModel.AutoSync.OnUpdate:
                            return System.Data.Linq.Mapping.AutoSync.OnUpdate;
                        default:
                            throw Error.ArgumentOutOfRange("autoSync");
                    }
                }

                private static System.Data.Linq.Mapping.UpdateCheck ConvertUpdateCheck(LinqToSqlShared.DbmlObjectModel.UpdateCheck updateCheck)
                {
                    switch (updateCheck)
                    {
                        case LinqToSqlShared.DbmlObjectModel.UpdateCheck.Always:
                            return System.Data.Linq.Mapping.UpdateCheck.Always;
                        case LinqToSqlShared.DbmlObjectModel.UpdateCheck.Never:
                            return System.Data.Linq.Mapping.UpdateCheck.Never;
                        case LinqToSqlShared.DbmlObjectModel.UpdateCheck.WhenChanged:
                            return System.Data.Linq.Mapping.UpdateCheck.WhenChanged;
                        default:
                            throw Error.ArgumentOutOfRange("updateCheck");
                    }
                }

                internal override Association VisitAssociation(Association association)
                {
                    if (association == null)
                    {
                        return null;
                    }
                    Table table = this.model.TableFromTypeName(association.Type);
                    if (table != null && !Dbml.HasPrimaryKey(table.Type))
                    {
                        return association;
                    }
                    Association otherAssociation = this.model.GetOtherAssociation(association);
                    if (otherAssociation != null)
                    {
                        Table table2 = this.model.TableFromTypeName(otherAssociation.Type);
                        if (table2 != null && !Dbml.HasPrimaryKey(table2.Type))
                        {
                            return association;
                        }
                    }
                    AssociationMapping associationMapping = new AssociationMapping();
                    associationMapping.DbName = association.Name;
                    associationMapping.DeleteRule = association.DeleteRule;
                    associationMapping.DeleteOnNull = association.DeleteOnNull.GetValueOrDefault(false);
                    associationMapping.IsForeignKey = association.IsForeignKey.GetValueOrDefault(false);
                    associationMapping.MemberName = association.Member;
                    string text = Dbml.BuildKeyField(association.GetOtherKey());
                    if (!string.IsNullOrEmpty(text))
                    {
                        associationMapping.OtherKey = text;
                    }
                    associationMapping.StorageMemberName = association.Storage;
                    string text2 = Dbml.BuildKeyField(association.GetThisKey());
                    if (!string.IsNullOrEmpty(text2))
                    {
                        associationMapping.ThisKey = text2;
                    }
                    associationMapping.IsUnique = (association.Cardinality == Cardinality.One && association.IsForeignKey != true);
                    this.curTypeMapping.Members.Add(associationMapping);
                    return association;
                }

                internal override Function VisitFunction(Function f)
                {
                    if (f == null)
                    {
                        return null;
                    }
                    this.visitingFunction = true;
                    this.curFunctionMapping = new FunctionMapping();
                    this.curFunctionMapping.MethodName = f.Method;
                    this.curFunctionMapping.Name = f.Name;
                    if (f.IsComposable == true)
                    {
                        this.curFunctionMapping.IsComposable = f.IsComposable.GetValueOrDefault(false);
                    }
                    this.dbMapping.Functions.Add(this.curFunctionMapping);
                    return base.VisitFunction(f);
                }

                internal override Parameter VisitParameter(Parameter parameter)
                {
                    if (parameter == null)
                    {
                        return null;
                    }
                    ParameterMapping parameterMapping = new ParameterMapping();
                    parameterMapping.DbType = parameter.DbType;
                    parameterMapping.Name = parameter.Name;
                    parameterMapping.ParameterName = parameter.ParameterName;
                    this.curFunctionMapping.Parameters.Add(parameterMapping);
                    return parameter;
                }

                internal override Return VisitReturn(Return r)
                {
                    if (r == null)
                    {
                        return null;
                    }
                    ReturnMapping returnMapping = new ReturnMapping();
                    returnMapping.DbType = r.DbType;
                    this.curFunctionMapping.FunReturn = returnMapping;
                    return base.VisitReturn(r);
                }
            }

            private class GenerateContext : DbmlVisitor
            {
                private CodeNamespace ns;

                private CodeTypeDeclaration contextType;

                private bool createExternalMapping;

                private CodeDomProvider provider;

                private Database database;

                private Dictionary<string, Column> columns;

                private Table currentTable;

                internal GenerateContext(CodeNamespace contextNspace, bool externalMapping, CodeDomProvider provider)
                {
                    this.ns = contextNspace;
                    this.createExternalMapping = externalMapping;
                    this.provider = provider;
                }

                internal override Database VisitDatabase(Database db)
                {
                    this.database = db;
                    if (db.Class != null || db.Name != null)
                    {
                        this.contextType = new CodeTypeDeclaration(db.Class);
                        this.contextType.TypeAttributes = CodeGeneratorFactory.DbmlGenerator.ToTypeAttributes(db.AccessModifier, db.Modifier);
                        this.contextType.IsPartial = true;
                        string typeName = (db.BaseType == null) ? "System.Data.Linq.DataContext" : db.BaseType;
                        this.contextType.BaseTypes.Add(new CodeTypeReference(typeName));
                        this.ns.Types.Add(this.contextType);
                        if (!this.createExternalMapping)
                        {
                            if (db.Name != null && db.Name != db.Class)
                            {
                                CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(DatabaseAttribute)));
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(db.Name)));
                                this.contextType.CustomAttributes.Add(codeAttributeDeclaration);
                            }
                            if (db.Provider != null)
                            {
                                CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ProviderAttribute)));
                                string a;
                                CodeTypeReference codeTypeReference;
                                if ((a = db.Provider.ToLower()) != null)
                                {
                                    if (a == "sql2000")
                                    {
                                        codeTypeReference = new CodeTypeReference(typeof(Sql2000Provider).FullName);
                                        goto IL_1C7;
                                    }
                                    if (a == "sql2005")
                                    {
                                        codeTypeReference = new CodeTypeReference(typeof(Sql2005Provider).FullName);
                                        goto IL_1C7;
                                    }
                                    if (a == "sql2008")
                                    {
                                        codeTypeReference = new CodeTypeReference(typeof(Sql2008Provider).FullName);
                                        goto IL_1C7;
                                    }
                                    if (a == "sqlcompact")
                                    {
                                        codeTypeReference = null;
                                        goto IL_1C7;
                                    }
                                }
                                codeTypeReference = new CodeTypeReference(db.Provider);
                                IL_1C7:
                                if (codeTypeReference != null)
                                {
                                    codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(codeTypeReference)));
                                    this.contextType.CustomAttributes.Add(codeAttributeDeclaration);
                                }
                            }
                        }
                        this.contextType.Members.Add(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodRegionStartTag(db.ContextNamespace != null, this.provider));
                        string methodName = "OnCreated";
                        CodeExpressionStatement value = new CodeExpressionStatement(new CodeMethodInvokeExpression(null, methodName, new CodeExpression[0]));
                        CodeConstructor codeConstructor = this.GetConnectingConstructor(db.Connection);
                        if (codeConstructor != null)
                        {
                            codeConstructor.Statements.Add(value);
                            this.contextType.Members.Add(codeConstructor);
                        }
                        if (!this.createExternalMapping)
                        {
                            CodeMemberField codeMemberField = new CodeMemberField();
                            codeMemberField.Name = "mappingSource";
                            codeMemberField.Type = new CodeTypeReference(typeof(MappingSource).FullName);
                            codeMemberField.Attributes = (MemberAttributes)20483;
                            codeMemberField.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(AttributeMappingSource).Name), new CodeExpression[0]);
                            this.contextType.Members.Add(codeMemberField);
                            codeConstructor = new CodeConstructor();
                            codeConstructor.Attributes = MemberAttributes.Public;
                            codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "connection"));
                            codeConstructor.Statements.Add(value);
                            codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connection"));
                            codeConstructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression(codeMemberField.Name));
                            this.contextType.Members.Add(codeConstructor);
                            codeConstructor = new CodeConstructor();
                            codeConstructor.Attributes = MemberAttributes.Public;
                            codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDbConnection).FullName, "connection"));
                            codeConstructor.Statements.Add(value);
                            codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connection"));
                            codeConstructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression(codeMemberField.Name));
                            this.contextType.Members.Add(codeConstructor);
                        }
                        codeConstructor = new CodeConstructor();
                        codeConstructor.Attributes = MemberAttributes.Public;
                        codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "connection"));
                        codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connection"));
                        codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(MappingSource).FullName, "mappingSource"));
                        codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("mappingSource"));
                        codeConstructor.Statements.Add(value);
                        this.contextType.Members.Add(codeConstructor);
                        codeConstructor = new CodeConstructor();
                        codeConstructor.Attributes = MemberAttributes.Public;
                        codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDbConnection).FullName, "connection"));
                        codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("connection"));
                        codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(MappingSource).FullName, "mappingSource"));
                        codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("mappingSource"));
                        codeConstructor.Statements.Add(value);
                        this.contextType.Members.Add(codeConstructor);
                        string partialOnCreated = this.GetPartialOnCreated();
                        this.contextType.Members.Add(new CodeSnippetTypeMember(partialOnCreated));
                        foreach (Table current in db.Tables)
                        {
                            this.VisitTable(current);
                        }
                        foreach (Function current2 in db.Functions)
                        {
                            this.VisitFunction(current2);
                        }
                    }
                    this.contextType.Members.Add(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodRegionEndTag(this.database.ContextNamespace != null, this.provider));
                    return db;
                }

                private CodeConstructor GetConnectingConstructor(Connection connection)
                {
                    CodeParameterDeclarationExpression codeParameterDeclarationExpression = null;
                    if (this.createExternalMapping)
                    {
                        codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(typeof(MappingSource).FullName, "mappingSource");
                    }
                    CodeConstructor codeConstructor = null;
                    if (connection != null)
                    {
                        if (connection.Mode == ConnectionMode.AppSettings)
                        {
                            if (!string.IsNullOrEmpty(connection.SettingsObjectName) && !string.IsNullOrEmpty(connection.SettingsPropertyName))
                            {
                                codeConstructor = new CodeConstructor();
                                codeConstructor.Attributes = MemberAttributes.Public;
                                codeConstructor.BaseConstructorArgs.Add(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(connection.SettingsObjectName, CodeTypeReferenceOptions.GlobalReference)), "Default"), connection.SettingsPropertyName));
                                codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("mappingSource"));
                            }
                        }
                        else if (connection.Mode == ConnectionMode.WebSettings)
                        {
                            if (!string.IsNullOrEmpty(connection.SettingsObjectName) && !string.IsNullOrEmpty(connection.SettingsPropertyName))
                            {
                                codeConstructor = new CodeConstructor();
                                codeConstructor.Attributes = MemberAttributes.Public;
                                codeConstructor.BaseConstructorArgs.Add(new CodePropertyReferenceExpression(new CodeArrayIndexerExpression(new CodeTypeReferenceExpression(new CodeTypeReference(connection.SettingsObjectName, CodeTypeReferenceOptions.GlobalReference)), new CodeExpression[]
                                {
                                    new CodePrimitiveExpression(connection.SettingsPropertyName)
                                }), "ConnectionString"));
                                codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("mappingSource"));
                            }
                        }
                        else if (!string.IsNullOrEmpty(connection.ConnectionString))
                        {
                            codeConstructor = new CodeConstructor();
                            codeConstructor.Attributes = MemberAttributes.Public;
                            codeConstructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(connection.ConnectionString));
                            codeConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("mappingSource"));
                            Console.WriteLine("Warning {0}", Strings.ConnectionStringTransferredToCode);
                        }
                    }
                    if (codeParameterDeclarationExpression != null && codeConstructor != null)
                    {
                        codeConstructor.Parameters.Add(codeParameterDeclarationExpression);
                    }
                    return codeConstructor;
                }

                private string GetPartialOnCreated()
                {
                    string text = "OnCreated";
                    string result;
                    if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                    {
                        result = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.ContextNamespace != null) + "partial void " + text + "();";
                    }
                    else
                    {
                        result = string.Concat(new string[]
                        {
                            CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.ContextNamespace != null),
                            "Partial Private Sub ",
                            text,
                            "()\r\n",
                            CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.ContextNamespace != null),
                            "End Sub"
                        });
                    }
                    return result;
                }

                private string GetQualifiedEntityTypeNameIfNeeded(string typeName)
                {
                    if (string.Compare(this.database.ContextNamespace, this.database.EntityNamespace, StringComparison.Ordinal) == 0)
                    {
                        return typeName;
                    }
                    if (!string.IsNullOrEmpty(this.database.EntityNamespace))
                    {
                        if (!CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                        {
                            typeName = CodeGeneratorFactory.DbmlGenerator.GetUnquotedNameIfNeeded(typeName, this.provider);
                        }
                        return this.database.EntityNamespace + "." + typeName;
                    }
                    return typeName;
                }

                internal override Table VisitTable(Table table)
                {
                    if (string.IsNullOrEmpty(table.Member))
                    {
                        return table;
                    }
                    this.currentTable = table;
                    this.columns = new Dictionary<string, Column>();
                    foreach (Column current in table.Type.Columns)
                    {
                        this.columns.Add(current.Member, current);
                    }
                    CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
                    codeMemberProperty.Name = table.Member;
                    string qualifiedEntityTypeNameIfNeeded = this.GetQualifiedEntityTypeNameIfNeeded(table.Type.Name);
                    codeMemberProperty.Type = new CodeTypeReference(typeof(Table<>).FullName, new CodeTypeReference[]
                    {
                        new CodeTypeReference(qualifiedEntityTypeNameIfNeeded)
                    });
                    codeMemberProperty.Attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(table.AccessModifier, table.Modifier);
                    codeMemberProperty.HasGet = true;
                    codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetTable", new CodeTypeReference[]
                    {
                        new CodeTypeReference(qualifiedEntityTypeNameIfNeeded)
                    }), new CodeExpression[0])));
                    codeMemberProperty.HasSet = false;
                    this.contextType.Members.Add(codeMemberProperty);
                    string str = "";
                    string str2 = "";
                    string str3 = "";
                    string unquotedNameIfNeeded = CodeGeneratorFactory.DbmlGenerator.GetUnquotedNameIfNeeded(table.Type.Name, this.provider);
                    bool flag = Dbml.HasPrimaryKey(table.Type);
                    if (flag)
                    {
                        if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                        {
                            str = string.Format(CultureInfo.InvariantCulture, "partial void Insert{0}({1} instance);", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            });
                            str2 = string.Format(CultureInfo.InvariantCulture, "partial void Update{0}({1} instance);", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            });
                            str3 = string.Format(CultureInfo.InvariantCulture, "partial void Delete{0}({1} instance);", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            });
                        }
                        else
                        {
                            str = string.Format(CultureInfo.InvariantCulture, "Partial Private Sub Insert{0}(instance As {1})\r\n", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            }) + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                            str2 = string.Format(CultureInfo.InvariantCulture, "Partial Private Sub Update{0}(instance As {1})\r\n", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            }) + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                            str3 = string.Format(CultureInfo.InvariantCulture, "Partial Private Sub Delete{0}(instance As {1})\r\n", new object[]
                            {
                                unquotedNameIfNeeded,
                                qualifiedEntityTypeNameIfNeeded
                            }) + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                        }
                    }
                    if (table.InsertFunction != null)
                    {
                        this.contextType.Members.Add(this.GenerateCUDMethod(table.InsertFunction, CodeGeneratorFactory.CUDMethodType.Insert, table));
                    }
                    else if (flag)
                    {
                        this.contextType.Members.Add(new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.EntityNamespace != null) + str));
                    }
                    if (table.UpdateFunction != null)
                    {
                        this.contextType.Members.Add(this.GenerateCUDMethod(table.UpdateFunction, CodeGeneratorFactory.CUDMethodType.Update, table));
                    }
                    else if (flag)
                    {
                        this.contextType.Members.Add(new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.EntityNamespace != null) + str2));
                    }
                    if (table.DeleteFunction != null)
                    {
                        this.contextType.Members.Add(this.GenerateCUDMethod(table.DeleteFunction, CodeGeneratorFactory.CUDMethodType.Delete, table));
                    }
                    else if (flag)
                    {
                        this.contextType.Members.Add(new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.database.EntityNamespace != null) + str3));
                    }
                    return table;
                }

                private Column FindColumnFromMember(string columnMember)
                {
                    if (string.IsNullOrEmpty(columnMember))
                    {
                        return null;
                    }
                    Column result;
                    if (this.columns.TryGetValue(columnMember, out result))
                    {
                        return result;
                    }
                    return null;
                }

                private static CodeTypeReference GetParameterType(Parameter methodParam)
                {
                    System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(methodParam.Type);
                    CodeTypeReference result;
                    if (type == null)
                    {
                        result = new CodeTypeReference(methodParam.Type);
                    }
                    else
                    {
                        if (type.IsValueType)
                        {
                            type = typeof(Nullable<>).MakeGenericType(new System.Type[]
                            {
                                type
                            });
                        }
                        result = new CodeTypeReference(type);
                    }
                    return result;
                }

                internal CodeMemberMethod GenerateCUDMethod(TableFunction tf, CodeGeneratorFactory.CUDMethodType cudMethodType, Table table)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    string text;
                    switch (cudMethodType)
                    {
                        case CodeGeneratorFactory.CUDMethodType.Insert:
                            text = "Insert";
                            break;
                        case CodeGeneratorFactory.CUDMethodType.Update:
                            text = "Update";
                            break;
                        case CodeGeneratorFactory.CUDMethodType.Delete:
                            text = "Delete";
                            break;
                        default:
                            throw Error.ArgumentOutOfRange("methodAttr");
                    }
                    text += CodeGeneratorFactory.DbmlGenerator.GetUnquotedNameIfNeeded(table.Type.Name, this.provider);
                    codeMemberMethod.Name = text;
                    codeMemberMethod.Attributes = CodeGeneratorFactory.DbmlGenerator.AccessModifierToMemberAttributes(tf.AccessModifier);
                    System.Type typeFromHandle = typeof(void);
                    codeMemberMethod.ReturnType = new CodeTypeReference(typeFromHandle);
                    string qualifiedEntityTypeNameIfNeeded = this.GetQualifiedEntityTypeNameIfNeeded(table.Type.Name);
                    string text2 = "obj";
                    CodeParameterDeclarationExpression value = new CodeParameterDeclarationExpression(new CodeTypeReference(qualifiedEntityTypeNameIfNeeded), text2);
                    codeMemberMethod.Parameters.Add(value);
                    List<CodeExpression> list = new List<CodeExpression>();
                    int num = 1;
                    List<CodeVariableDeclarationStatement> list2 = new List<CodeVariableDeclarationStatement>();
                    List<CodeAssignStatement> list3 = new List<CodeAssignStatement>();
                    bool flag = false;
                    string text3 = "original";

                    foreach (var methodParameter in tf.MappedFunction.Parameters)
                    {
                        TableFunctionParameter param = tf.Arguments.Find((TableFunctionParameter p) => p.ParameterName == methodParameter.ParameterName);
                        if (param == null)
                        {
                            CodeExpression item = new CodeDefaultValueExpression(CodeGeneratorFactory.DbmlGenerator.GenerateContext.GetParameterType(methodParameter));
                            list.Add(item);
                        }
                        else
                        {
                            Column column = this.FindColumnFromMember(param.Member);
                            if (column == null)
                            {
                                CodeExpression item = new CodeDefaultValueExpression(CodeGeneratorFactory.DbmlGenerator.GenerateContext.GetParameterType(methodParameter));
                                list.Add(item);
                            }
                            else
                            {
                                System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(column.Type);
                                bool flag2 = false;
                                CodeTypeReference codeTypeReference;
                                if (type == null)
                                {
                                    codeTypeReference = new CodeTypeReference(column.Type);
                                }
                                else
                                {
                                    if (type.IsValueType)
                                    {
                                        type = typeof(Nullable<>).MakeGenericType(new System.Type[]
                                        {
                                                type
                                        });
                                        flag2 = true;
                                    }
                                    codeTypeReference = new CodeTypeReference(type);
                                }
                                string variableName;
                                if (param.Version == LinqToSqlShared.DbmlObjectModel.Version.Original)
                                {
                                    flag = true;
                                    variableName = text3;
                                }
                                else
                                {
                                    variableName = text2;
                                }
                                CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(variableName), column.Member);
                                Parameter parameter = tf.MappedFunction.Parameters.Find((Parameter p) => p.ParameterName == param.ParameterName);
                                CodeExpression item;
                                if (parameter.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out || parameter.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.InOut)
                                {
                                    string text4 = "p" + num.ToString(CultureInfo.InvariantCulture);
                                    num++;
                                    list2.Add(new CodeVariableDeclarationStatement(codeTypeReference, text4, codePropertyReferenceExpression));
                                    CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression(text4);
                                    if (flag2)
                                    {
                                        list3.Add(new CodeAssignStatement(codePropertyReferenceExpression, new CodeMethodInvokeExpression(codeVariableReferenceExpression, "GetValueOrDefault", new CodeExpression[0])));
                                    }
                                    else
                                    {
                                        list3.Add(new CodeAssignStatement(codePropertyReferenceExpression, codeVariableReferenceExpression));
                                    }
                                    if (parameter.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out)
                                    {
                                        item = new CodeDirectionExpression(FieldDirection.Out, codeVariableReferenceExpression);
                                    }
                                    else
                                    {
                                        item = new CodeDirectionExpression(FieldDirection.Ref, codeVariableReferenceExpression);
                                    }
                                }
                                else if (flag2)
                                {
                                    item = new CodeCastExpression(codeTypeReference, codePropertyReferenceExpression);
                                }
                                else
                                {
                                    item = codePropertyReferenceExpression;
                                }
                                list.Add(item);
                            }
                        }
                    }

                    CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), tf.MappedFunction.Method, list.ToArray());
                    CodeStatement value2 = new CodeExpressionStatement(codeMethodInvokeExpression);
                    if (tf.Return != null)
                    {
                        Column column2 = this.FindColumnFromMember(tf.Return.Member);
                        CodePropertyReferenceExpression left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(text2), column2.Member);
                        value2 = new CodeAssignStatement(left, codeMethodInvokeExpression);
                    }
                    else
                    {
                        value2 = new CodeExpressionStatement(codeMethodInvokeExpression);
                    }
                    if (flag)
                    {
                        CodeVariableDeclarationStatement value3 = new CodeVariableDeclarationStatement(this.GetQualifiedEntityTypeNameIfNeeded(this.currentTable.Type.Name), text3, new CodeCastExpression(this.GetQualifiedEntityTypeNameIfNeeded(this.currentTable.Type.Name), new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(this.currentTable.Member), "GetOriginalEntityState", new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression(text2)
                        })));
                        codeMemberMethod.Statements.Add(value3);
                    }
                    codeMemberMethod.Statements.AddRange(list2.ToArray());
                    codeMemberMethod.Statements.Add(value2);
                    codeMemberMethod.Statements.AddRange(list3.ToArray());
                    return codeMemberMethod;
                }

                private static bool IsStoredProcedure(Function f)
                {
                    return !(f.IsComposable == true) && (f.Return != null || f.Types.Count != 0);
                }

                internal override Function VisitFunction(Function f)
                {
                    CodeMemberMethod value;
                    if (CodeGeneratorFactory.DbmlGenerator.GenerateContext.IsStoredProcedure(f))
                    {
                        value = this.GenerateSprocMethod(f);
                    }
                    else
                    {
                        value = this.GenerateFunctionMethod(f);
                    }
                    this.contextType.Members.Add(value);
                    return f;
                }

                private bool IsSameName(string name1, string name2)
                {
                    if (name1 == null || name2 == null)
                    {
                        return false;
                    }
                    if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                    {
                        return string.Compare(name1, name2, StringComparison.CurrentCulture) == 0;
                    }
                    return string.Compare(name1.ToLower(CultureInfo.CurrentCulture), name2.ToLower(CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase) == 0 || string.Compare(name1.ToUpper(CultureInfo.CurrentCulture), name2.ToUpper(CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase) == 0;
                }

                private bool IsUniqueVarName(string name, Function f)
                {
                    return !f.Parameters.Exists((Parameter p) => this.IsSameName(p.ParameterName, name));
                }

                private string GetUniqueVarName(string candidateName, Function f)
                {
                    string arg = candidateName;
                    for (int i = 1; i < 1000; i++)
                    {
                        if (this.IsUniqueVarName(candidateName, f))
                        {
                            return candidateName;
                        }
                        candidateName = arg + i;
                    }
                    throw Error.CouldNotMakeUniqueVariableName(candidateName);
                }

                private CodeMemberMethod GenerateFunctionMethod(Function function)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = function.Method;
                    codeMemberMethod.Attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(function.AccessModifier, function.Modifier);
                    codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
                    List<CodeAssignStatement> list = this.GenerateFunctionMethodParameters(function, codeMemberMethod);
                    if (!this.createExternalMapping)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(FunctionAttribute)));
                        if (function.Name != function.Method)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(function.Name)));
                        }
                        if (function.IsComposable == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsComposable", new CodePrimitiveExpression(function.IsComposable)));
                        }
                        codeMemberMethod.CustomAttributes.Add(codeAttributeDeclaration);
                        if (function.Return != null && function.Return.DbType != null && function.Return.DbType.Length != 0)
                        {
                            CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ParameterAttribute)));
                            codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("DbType", new CodePrimitiveExpression(function.Return.DbType)));
                            codeMemberMethod.ReturnTypeCustomAttributes.Add(codeAttributeDeclaration2);
                        }
                    }
                    List<CodeExpression> list2 = new List<CodeExpression>(function.Parameters.Count);
                    CodeExpression codeExpression = new CodeThisReferenceExpression();
                    list2.Add(codeExpression);
                    CodeExpression item = new CodeCastExpression(typeof(MethodInfo).Name, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(MethodInfo).Name), "GetCurrentMethod"), new CodeExpression[0]));
                    list2.Add(item);
                    foreach (Parameter current in function.Parameters)
                    {
                        list2.Add(new CodeVariableReferenceExpression(current.ParameterName));
                    }
                    bool flag = true;
                    CodeMethodInvokeExpression codeMethodInvokeExpression;
                    if (function.Types.Count == 0)
                    {
                        System.Type type;
                        if (function.Return == null)
                        {
                            type = typeof(void);
                            flag = false;
                        }
                        else
                        {
                            type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(function.Return.Type);
                        }
                        if (type == null)
                        {
                            codeMemberMethod.ReturnType = new CodeTypeReference(function.Return.Type);
                        }
                        else
                        {
                            if (type.IsValueType && type != typeof(void))
                            {
                                type = typeof(Nullable<>).MakeGenericType(new System.Type[]
                                {
                                    type
                                });
                                flag = true;
                            }
                            codeMemberMethod.ReturnType = new CodeTypeReference(type);
                        }
                        codeMethodInvokeExpression = new CodeMethodInvokeExpression(codeExpression, "ExecuteMethodCall", list2.ToArray());
                    }
                    else
                    {
                        string qualifiedEntityTypeNameIfNeeded = this.GetQualifiedEntityTypeNameIfNeeded(function.Types[0].Name);
                        CodeTypeReference codeTypeReference = new CodeTypeReference(qualifiedEntityTypeNameIfNeeded);
                        CodeTypeReference returnType = new CodeTypeReference(typeof(IQueryable<>).Name, new CodeTypeReference[]
                        {
                            codeTypeReference
                        });
                        codeMemberMethod.ReturnType = returnType;
                        codeMethodInvokeExpression = new CodeMethodInvokeExpression(codeExpression, "CreateMethodCallQuery", list2.ToArray());
                        codeMethodInvokeExpression.Method.TypeArguments.Add(codeTypeReference);
                    }
                    if (list.Count > 0)
                    {
                        string uniqueVarName = this.GetUniqueVarName("result", function);
                        CodeVariableDeclarationStatement value = new CodeVariableDeclarationStatement(typeof(IExecuteResult).Name, uniqueVarName, codeMethodInvokeExpression);
                        codeMemberMethod.Statements.Add(value);
                        codeMemberMethod.Statements.AddRange(list.ToArray());
                        if (codeMemberMethod.ReturnType.BaseType != typeof(void).ToString())
                        {
                            CodeMethodReturnStatement value2;
                            if (flag)
                            {
                                value2 = new CodeMethodReturnStatement(new CodeCastExpression(codeMemberMethod.ReturnType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(uniqueVarName), "ReturnValue")));
                            }
                            else
                            {
                                value2 = new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(uniqueVarName), "ReturnValue"));
                            }
                            codeMemberMethod.Statements.Add(value2);
                        }
                    }
                    else if (codeMemberMethod.ReturnType.BaseType == "IQueryable`1")
                    {
                        codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(codeMethodInvokeExpression));
                    }
                    else if (codeMemberMethod.ReturnType.BaseType != typeof(void).ToString())
                    {
                        codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(codeMemberMethod.ReturnType, new CodePropertyReferenceExpression(codeMethodInvokeExpression, "ReturnValue"))));
                    }
                    else
                    {
                        codeMemberMethod.Statements.Add(new CodeExpressionStatement(codeMethodInvokeExpression));
                    }
                    return codeMemberMethod;
                }

                private List<CodeAssignStatement> GenerateFunctionMethodParameters(Function function, CodeMemberMethod functionDecl)
                {
                    string uniqueVarName = this.GetUniqueVarName("result", function);
                    CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression(uniqueVarName);
                    List<CodeArgumentReferenceExpression> list = new List<CodeArgumentReferenceExpression>();
                    List<CodeAssignStatement> list2 = new List<CodeAssignStatement>();
                    int num = 0;
                    foreach (Parameter current in function.Parameters)
                    {
                        string parameterName = current.ParameterName;
                        System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(current.Type);
                        CodeTypeReference codeTypeReference;
                        if (type == null)
                        {
                            codeTypeReference = new CodeTypeReference(current.Type);
                        }
                        else
                        {
                            if (type.IsValueType)
                            {
                                type = typeof(Nullable<>).MakeGenericType(new System.Type[]
                                {
                                    type
                                });
                            }
                            codeTypeReference = new CodeTypeReference(type);
                        }
                        CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(codeTypeReference, parameterName);
                        CodeArgumentReferenceExpression codeArgumentReferenceExpression = new CodeArgumentReferenceExpression(codeParameterDeclarationExpression.Name);
                        list.Add(codeArgumentReferenceExpression);
                        if (current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.InOut || current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out)
                        {
                            CodeExpression right = new CodeCastExpression(codeTypeReference, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetObject, "GetParameterValue"), new CodeExpression[]
                            {
                                new CodePrimitiveExpression(num)
                            }));
                            CodeAssignStatement item = new CodeAssignStatement(codeArgumentReferenceExpression, right);
                            list2.Add(item);
                            if (current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out)
                            {
                                functionDecl.Statements.Add(new CodeAssignStatement(codeArgumentReferenceExpression, new CodeDefaultValueExpression(codeTypeReference)));
                                codeParameterDeclarationExpression.Direction = FieldDirection.Out;
                            }
                            else
                            {
                                codeParameterDeclarationExpression.Direction = FieldDirection.Ref;
                            }
                        }
                        if (!this.createExternalMapping)
                        {
                            CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ParameterAttribute)));
                            if (current.Name != current.ParameterName)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(current.Name)));
                            }
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DbType", new CodePrimitiveExpression(current.DbType)));
                            codeParameterDeclarationExpression.CustomAttributes.Add(codeAttributeDeclaration);
                        }
                        functionDecl.Parameters.Add(codeParameterDeclarationExpression);
                        num++;
                    }
                    return list2;
                }

                private CodeMemberMethod GenerateSprocMethod(Function f)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = f.Method;
                    MemberAttributes attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(f.AccessModifier, f.Modifier);
                    codeMemberMethod.Attributes = attributes;
                    System.Type type = typeof(void);
                    if (f.Return != null)
                    {
                        type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(f.Return.Type);
                    }
                    if (type == null)
                    {
                        codeMemberMethod.ReturnType = new CodeTypeReference(f.Return.Type);
                    }
                    else
                    {
                        codeMemberMethod.ReturnType = new CodeTypeReference(type);
                    }
                    string uniqueVarName = this.GetUniqueVarName("result", f);
                    CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression(uniqueVarName);
                    List<CodeArgumentReferenceExpression> list = new List<CodeArgumentReferenceExpression>();
                    List<CodeAssignStatement> list2 = new List<CodeAssignStatement>();
                    int num = 0;
                    foreach (Parameter current in f.Parameters)
                    {
                        string parameterName = current.ParameterName;
                        System.Type type2 = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(current.Type);
                        CodeTypeReference codeTypeReference;
                        if (type2 == null)
                        {
                            codeTypeReference = new CodeTypeReference(current.Type);
                        }
                        else
                        {
                            if (type2.IsValueType)
                            {
                                type2 = typeof(Nullable<>).MakeGenericType(new System.Type[]
                                {
                                    type2
                                });
                            }
                            codeTypeReference = new CodeTypeReference(type2);
                        }
                        CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(codeTypeReference, parameterName);
                        CodeArgumentReferenceExpression codeArgumentReferenceExpression = new CodeArgumentReferenceExpression(codeParameterDeclarationExpression.Name);
                        list.Add(codeArgumentReferenceExpression);
                        if (current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.InOut || current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out)
                        {
                            if (current.Direction == LinqToSqlShared.DbmlObjectModel.ParameterDirection.Out)
                            {
                                codeMemberMethod.Statements.Add(new CodeAssignStatement(codeArgumentReferenceExpression, new CodeDefaultValueExpression(codeTypeReference)));
                                codeParameterDeclarationExpression.Direction = FieldDirection.Out;
                            }
                            else
                            {
                                codeParameterDeclarationExpression.Direction = FieldDirection.Ref;
                            }
                            CodeExpression right = new CodeCastExpression(codeTypeReference, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(targetObject, "GetParameterValue"), new CodeExpression[]
                            {
                                new CodePrimitiveExpression(num)
                            }));
                            CodeAssignStatement item = new CodeAssignStatement(codeArgumentReferenceExpression, right);
                            list2.Add(item);
                        }
                        if (!this.createExternalMapping)
                        {
                            CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ParameterAttribute)));
                            if (current.Name != current.ParameterName)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(current.Name)));
                            }
                            if (current.DbType != null)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DbType", new CodePrimitiveExpression(current.DbType)));
                            }
                            codeParameterDeclarationExpression.CustomAttributes.Add(codeAttributeDeclaration);
                        }
                        codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
                        num++;
                    }
                    if (!this.createExternalMapping)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(FunctionAttribute)));
                        if (f.Name != f.Method)
                        {
                            codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(f.Name)));
                        }
                        codeMemberMethod.CustomAttributes.Add(codeAttributeDeclaration2);
                        if (f.Return != null && f.Return.DbType != null && f.Return.DbType.Length != 0)
                        {
                            CodeAttributeDeclaration codeAttributeDeclaration3 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ParameterAttribute)));
                            codeAttributeDeclaration3.Arguments.Add(new CodeAttributeArgument("DbType", new CodePrimitiveExpression(f.Return.DbType)));
                            codeMemberMethod.ReturnTypeCustomAttributes.Add(codeAttributeDeclaration3);
                        }
                    }
                    CodeExpression targetObject2 = new CodeThisReferenceExpression();
                    CodeExpression item2 = new CodeCastExpression(typeof(MethodInfo).Name, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(MethodInfo).Name), "GetCurrentMethod"), new CodeExpression[0]));
                    List<CodeExpression> list3 = new List<CodeExpression>();
                    list3.Add(new CodeThisReferenceExpression());
                    list3.Add(item2);
                    list3.AddRange(list.ToArray());
                    CodeMethodInvokeExpression initExpression = new CodeMethodInvokeExpression(targetObject2, "ExecuteMethodCall", list3.ToArray());
                    CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement(typeof(IExecuteResult).Name, uniqueVarName, initExpression);
                    codeMemberMethod.Statements.Add(codeVariableDeclarationStatement);
                    if (list2.Count > 0)
                    {
                        codeMemberMethod.Statements.AddRange(list2.ToArray());
                    }
                    if (f.HasMultipleResults == false && f.IsComposable == false && f.Types.Count == 1)
                    {
                        string qualifiedEntityTypeNameIfNeeded = this.GetQualifiedEntityTypeNameIfNeeded(f.Types[0].Name);
                        CodeTypeReference codeTypeReference2 = new CodeTypeReference(qualifiedEntityTypeNameIfNeeded);
                        codeMemberMethod.ReturnType = new CodeTypeReference(typeof(ISingleResult<>).Name, new CodeTypeReference[]
                        {
                            codeTypeReference2
                        });
                    }
                    else if (f.HasMultipleResults == true && f.IsComposable == false && f.Return == null && f.Types.Count > 0)
                    {
                        if (!this.createExternalMapping)
                        {
                            foreach (LinqToSqlShared.DbmlObjectModel.Type current2 in f.Types)
                            {
                                CodeAttributeDeclaration codeAttributeDeclaration4 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ResultTypeAttribute)));
                                string qualifiedEntityTypeNameIfNeeded2 = this.GetQualifiedEntityTypeNameIfNeeded(current2.Name);
                                codeAttributeDeclaration4.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(new CodeTypeReference(qualifiedEntityTypeNameIfNeeded2))));
                                codeMemberMethod.CustomAttributes.Add(codeAttributeDeclaration4);
                            }
                        }
                        codeMemberMethod.ReturnType = new CodeTypeReference(typeof(IMultipleResults).Name);
                    }
                    CodeMethodReturnStatement value = new CodeMethodReturnStatement(new CodeCastExpression(codeMemberMethod.ReturnType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(codeVariableDeclarationStatement.Name), "ReturnValue")));
                    codeMemberMethod.Statements.Add(value);
                    return codeMemberMethod;
                }
            }

            private class CreateTypeColumns : DbmlVisitor
            {
                private Dictionary<string, List<Column>> typeColumns;

                private List<Column> accessibleColumns;

                internal Dictionary<string, List<Column>> GetTypeColumns(Database db)
                {
                    this.typeColumns = new Dictionary<string, List<Column>>();
                    base.VisitDatabase(db);
                    return this.typeColumns;
                }

                internal override Table VisitTable(Table table)
                {
                    this.accessibleColumns = new List<Column>();
                    return base.VisitTable(table);
                }

                internal override Function VisitFunction(Function f)
                {
                    this.accessibleColumns = new List<Column>();
                    return base.VisitFunction(f);
                }

                internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (this.typeColumns.ContainsKey(type.Name))
                    {
                        return type;
                    }
                    List<Column> list = new List<Column>();
                    list.AddRange(this.accessibleColumns);
                    this.accessibleColumns.AddRange(type.Columns);
                    this.typeColumns.Add(type.Name, new List<Column>(this.accessibleColumns));
                    base.VisitType(type);
                    this.accessibleColumns = list;
                    return type;
                }
            }

            private class GenerateEntities : DbmlVisitor
            {
                [Flags]
                private enum GeneratedMethods
                {
                    None = 0,
                    OnSerialized = 1,
                    OnSerializing = 2,
                    OnDeserializing = 4
                }

                private CodeNamespace ns;

                private CodeAttributeDeclaration tableAttribute;

                private Table thisTable;

                private CodeTypeDeclaration tableType;

                private int serializationOrdinal;

                private CodeMemberMethod constructor;

                private bool createExternalMapping;

                private SerializationMode serializationMode;

                private CodeDomProvider provider;

                private Dictionary<string, List<Association>> foreignKeys;

                private bool isSubType;

                private bool isFunctionResultType;

                private List<string> processedTypes;

                private Dictionary<string, List<Column>> typeColumns;

                private LinqToSqlShared.DbmlObjectModel.Type currentType;

                private CodeGeneratorFactory.ModelInfo model;

                private bool typeIsSerializable;

                private CodeTypeReference entityBaseType;

                private bool hasPrimaryKey;

                private string serializationFlagMemberName;

                private string emptyChangingEventArgsMemberName;

                private CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods generatedMethods;

                internal void Generate(Database db, CodeGeneratorFactory.ModelInfo modelInfo, CodeNamespace @namespace, bool externalMapping, CodeDomProvider codeDomProvider, SerializationMode serialization)
                {
                    this.model = modelInfo;
                    this.ns = @namespace;
                    this.createExternalMapping = externalMapping;
                    this.serializationMode = serialization;
                    this.provider = codeDomProvider;
                    this.entityBaseType = ((db.EntityBase == null) ? null : new CodeTypeReference(db.EntityBase));
                    this.VisitDatabase(db);
                }

                internal override Database VisitDatabase(Database db)
                {
                    this.processedTypes = new List<string>();
                    CodeGeneratorFactory.DbmlGenerator.CreateTypeColumns createTypeColumns = new CodeGeneratorFactory.DbmlGenerator.CreateTypeColumns();
                    this.typeColumns = createTypeColumns.GetTypeColumns(db);
                    return base.VisitDatabase(db);
                }

                internal override Table VisitTable(Table table)
                {
                    this.thisTable = table;
                    if (!this.createExternalMapping)
                    {
                        this.tableAttribute = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(TableAttribute)));
                        if (table.Name != null && table.Name != table.Type.Name)
                        {
                            CodeAttributeArgument value = new CodeAttributeArgument("Name", new CodePrimitiveExpression(table.Name));
                            this.tableAttribute.Arguments.Add(value);
                        }
                    }
                    return base.VisitTable(table);
                }

                internal override Function VisitFunction(Function f)
                {
                    this.isFunctionResultType = true;
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current in f.Types)
                    {
                        this.VisitType(current);
                    }
                    this.isFunctionResultType = false;
                    return f;
                }

                private void GeneratePartialMethodsInTableType()
                {
                    string text = "";
                    string text2 = "";
                    string text3 = "";
                    if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                    {
                        if (this.hasPrimaryKey)
                        {
                            text = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "partial void OnCreated();";
                            text3 = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "partial void OnValidate(System.Data.Linq.ChangeAction action);";
                            text2 = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "partial void OnLoaded();";
                        }
                    }
                    else if (this.hasPrimaryKey)
                    {
                        text = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "Partial Private Sub OnCreated()\r\n" + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                        text3 = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "Partial Private Sub OnValidate(action As System.Data.Linq.ChangeAction)\r\n" + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                        text2 = CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "Partial Private Sub OnLoaded()\r\n" + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                    }
                    if (this.hasPrimaryKey)
                    {
                        this.tableType.Members.Add(new CodeSnippetTypeMember(text2));
                        this.tableType.Members.Add(new CodeSnippetTypeMember(text3));
                        this.tableType.Members.Add(new CodeSnippetTypeMember(text));
                    }
                }

                internal override LinqToSqlShared.DbmlObjectModel.Type VisitType(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    if (this.processedTypes.Contains(type.Name))
                    {
                        return type;
                    }
                    this.currentType = type;
                    this.processedTypes.Add(type.Name);
                    CodeTypeDeclaration codeTypeDeclaration = this.tableType;
                    CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods generatedMethods = this.generatedMethods;
                    string text = this.serializationFlagMemberName;
                    string text2 = this.emptyChangingEventArgsMemberName;
                    this.foreignKeys = CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GetForeignKeys(type);
                    this.tableType = new CodeTypeDeclaration(type.Name);
                    this.tableType.TypeAttributes = CodeGeneratorFactory.DbmlGenerator.ToTypeAttributes(type.AccessModifier, type.Modifier);
                    this.tableType.IsPartial = true;
                    this.hasPrimaryKey = this.HasPrimaryKey(type.Name);
                    if (this.hasPrimaryKey)
                    {
                        this.tableType.Members.Add(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodRegionStartTag(this.ns != null, this.provider));
                    }
                    this.serializationOrdinal = 1;
                    if (this.isSubType)
                    {
                        this.tableType.BaseTypes.Add(new CodeTypeReference(codeTypeDeclaration.Name));
                    }
                    else if (this.entityBaseType != null && !this.isFunctionResultType)
                    {
                        this.tableType.BaseTypes.Add(this.entityBaseType);
                    }
                    if (!this.createExternalMapping && !this.isFunctionResultType && !this.isSubType && this.tableAttribute != null)
                    {
                        this.tableType.CustomAttributes.Add(this.tableAttribute);
                        this.tableAttribute = null;
                    }
                    if (this.serializationMode == SerializationMode.Unidirectional && type.AccessModifier == AccessModifier.Public)
                    {
                        CodeAttributeDeclaration value = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(DataContractAttribute)));
                        this.tableType.CustomAttributes.Add(value);
                        this.typeIsSerializable = true;
                    }
                    else
                    {
                        this.typeIsSerializable = false;
                    }
                    if (!this.isSubType && !this.createExternalMapping)
                    {
                        if (type.InheritanceCode != null)
                        {
                            CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(InheritanceMappingAttribute)));
                            this.tableType.CustomAttributes.Add(codeAttributeDeclaration);
                            if (type.InheritanceCode != null)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Code", new CodePrimitiveExpression(type.InheritanceCode)));
                            }
                            if (type.Name != null)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Type", new CodeTypeOfExpression(type.Name)));
                            }
                            if (type.IsInheritanceDefault == true)
                            {
                                codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsDefault", new CodePrimitiveExpression(type.IsInheritanceDefault)));
                            }
                        }
                        foreach (LinqToSqlShared.DbmlObjectModel.Type current in type.SubTypes)
                        {
                            this.tableType.CustomAttributes.AddRange(this.GetSubTypeInheritanceAttributes(current));
                        }
                    }
                    if (!this.createExternalMapping)
                    {
                        foreach (LinqToSqlShared.DbmlObjectModel.Type current2 in type.SubTypes)
                        {
                            this.tableType.CustomAttributes.AddRange(this.GetKnownTypeAttributes(current2));
                        }
                    }
                    if (!this.isSubType && this.hasPrimaryKey)
                    {
                        CodeMemberField codeMemberField = new CodeMemberField();
                        this.emptyChangingEventArgsMemberName = CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GetUniqueMemberName(this.thisTable, "emptyChangingEventArgs");
                        codeMemberField.Name = this.emptyChangingEventArgsMemberName;
                        codeMemberField.Type = new CodeTypeReference(typeof(PropertyChangingEventArgs).Name);
                        codeMemberField.Attributes = (MemberAttributes)20483;
                        codeMemberField.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PropertyChangingEventArgs).Name), new CodeExpression[]
                        {
                            new CodeArgumentReferenceExpression("String.Empty")
                        });
                        this.tableType.Members.Add(codeMemberField);
                        this.GeneratePropertyChangingNotifier(this.tableType);
                        this.GeneratePropertyChangedNotifier(this.tableType);
                    }
                    this.GeneratePartialMethodsInTableType();
                    CodeExpressionStatement codeExpressionStatement = null;
                    if (this.hasPrimaryKey)
                    {
                        string methodName = "OnCreated";
                        codeExpressionStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(null, methodName, new CodeExpression[0]));
                    }
                    CodeConstructor codeConstructor = new CodeConstructor();
                    codeConstructor.Attributes = MemberAttributes.Public;
                    this.tableType.Members.Add(codeConstructor);
                    this.constructor = codeConstructor;
                    this.ns.Types.Add(this.tableType);
                    foreach (Column current3 in type.Columns)
                    {
                        this.VisitColumn(current3);
                    }
                    bool flag = this.typeIsSerializable;
                    bool flag2 = false;
                    foreach (Association current4 in type.Associations)
                    {
                        Table table = this.model.TableFromTypeName(current4.Type);
                        if (Dbml.HasPrimaryKey(this.thisTable.Type) && (table == null || Dbml.HasPrimaryKey(table.Type)))
                        {
                            if (flag && this.IsSerializableAssociation(current4))
                            {
                                flag2 = true;
                                this.serializationFlagMemberName = CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GetUniqueMemberName(this.thisTable, "serializing");
                            }
                            this.VisitAssociation(current4);
                        }
                    }
                    if (codeExpressionStatement != null)
                    {
                        codeConstructor.Statements.Add(codeExpressionStatement);
                    }
                    if (flag && codeConstructor.Statements.Count > 0)
                    {
                        if (flag2)
                        {
                            CodeMemberField codeMemberField2 = new CodeMemberField(typeof(bool), this.serializationFlagMemberName);
                            codeMemberField2.Attributes = MemberAttributes.Private;
                            this.tableType.Members.Add(codeMemberField2);
                        }
                        this.GenerateSerializationCallbacks(this.tableType, codeConstructor, flag2);
                    }
                    if (this.hasPrimaryKey)
                    {
                        this.tableType.Members.Add(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodRegionEndTag(this.ns != null, this.provider));
                    }
                    bool flag3 = this.isSubType;
                    this.isSubType = true;
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current5 in type.SubTypes)
                    {
                        this.VisitType(current5);
                    }
                    this.isSubType = flag3;
                    this.tableType = codeTypeDeclaration;
                    this.generatedMethods = generatedMethods;
                    this.serializationFlagMemberName = text;
                    this.emptyChangingEventArgsMemberName = text2;
                    return type;
                }

                private static string GetUniqueMemberName(Table tableType, string name)
                {
                    Predicate<string> predicate = (string s) => Naming.IsUniqueTableClassMemberName(s, tableType) && CodeGeneratorFactory.DbmlGenerator.GenerateEntities.IsUniqueStorageMemberName(s, tableType);
                    if (predicate(name))
                    {
                        return name;
                    }
                    return Naming.GetUniqueName(name, predicate);
                }

                private static bool IsUniqueStorageMemberName(string name, Table tableType)
                {
                    foreach (Column current in tableType.Type.Columns)
                    {
                        if (Naming.IsSameName(current.Storage, name))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                private void GenerateSerializationCallbacks(CodeTypeDeclaration tblType, CodeConstructor cons, bool hasSerializableEntitySets)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = "Initialize";
                    codeMemberMethod.Attributes = MemberAttributes.Private;
                    codeMemberMethod.Statements.AddRange(cons.Statements);
                    cons.Statements.Clear();
                    cons.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Initialize", new CodeExpression[0]));
                    tblType.Members.Add(codeMemberMethod);
                    CodeMemberMethod codeMemberMethod2 = new CodeMemberMethod();
                    codeMemberMethod2.Name = "OnDeserializing";
                    codeMemberMethod2.Attributes = (MemberAttributes)24578;
                    if ((this.generatedMethods & CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnDeserializing) != CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.None)
                    {
                        codeMemberMethod2.Attributes |= MemberAttributes.New;
                    }
                    this.generatedMethods |= CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnDeserializing;
                    codeMemberMethod2.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Initialize", new CodeExpression[0]));
                    codeMemberMethod2.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(OnDeserializingAttribute))));
                    CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(EditorBrowsableAttribute)));
                    codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("EditorBrowsableState"), "Never")));
                    codeMemberMethod2.CustomAttributes.Add(codeAttributeDeclaration);
                    tblType.Members.Add(codeMemberMethod2);
                    CodeParameterDeclarationExpression value = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(StreamingContext).Name), "context");
                    codeMemberMethod2.Parameters.Add(value);
                    if (hasSerializableEntitySets)
                    {
                        CodeMemberMethod codeMemberMethod3 = new CodeMemberMethod();
                        codeMemberMethod3.Name = "OnSerializing";
                        codeMemberMethod3.Attributes = (MemberAttributes)24578;
                        if ((this.generatedMethods & CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnSerializing) != CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.None)
                        {
                            codeMemberMethod3.Attributes |= MemberAttributes.New;
                        }
                        this.generatedMethods |= CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnSerializing;
                        codeMemberMethod3.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.serializationFlagMemberName), new CodePrimitiveExpression(true)));
                        codeMemberMethod3.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(OnSerializingAttribute))));
                        codeMemberMethod3.CustomAttributes.Add(codeAttributeDeclaration);
                        tblType.Members.Add(codeMemberMethod3);
                        value = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(StreamingContext).Name), "context");
                        codeMemberMethod3.Parameters.Add(value);
                        CodeMemberMethod codeMemberMethod4 = new CodeMemberMethod();
                        codeMemberMethod4.Name = "OnSerialized";
                        codeMemberMethod4.Attributes = (MemberAttributes)24578;
                        if ((this.generatedMethods & CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnSerialized) != CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.None)
                        {
                            codeMemberMethod4.Attributes |= MemberAttributes.New;
                        }
                        this.generatedMethods |= CodeGeneratorFactory.DbmlGenerator.GenerateEntities.GeneratedMethods.OnSerialized;
                        codeMemberMethod4.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.serializationFlagMemberName), new CodePrimitiveExpression(false)));
                        codeMemberMethod4.CustomAttributes.Add(new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(OnSerializedAttribute))));
                        codeMemberMethod4.CustomAttributes.Add(codeAttributeDeclaration);
                        tblType.Members.Add(codeMemberMethod4);
                        value = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(StreamingContext).Name), "context");
                        codeMemberMethod4.Parameters.Add(value);
                    }
                }

                private CodeAttributeDeclaration[] GetSubTypeInheritanceAttributes(LinqToSqlShared.DbmlObjectModel.Type subT)
                {
                    List<CodeAttributeDeclaration> list = new List<CodeAttributeDeclaration>();
                    if (subT.InheritanceCode != null)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(InheritanceMappingAttribute)));
                        list.Add(codeAttributeDeclaration);
                        if (subT.InheritanceCode != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Code", new CodePrimitiveExpression(subT.InheritanceCode)));
                        }
                        if (subT.Name != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Type", new CodeTypeOfExpression(subT.Name)));
                        }
                        if (subT.IsInheritanceDefault == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsDefault", new CodePrimitiveExpression(subT.IsInheritanceDefault)));
                        }
                    }
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current in subT.SubTypes)
                    {
                        list.AddRange(this.GetSubTypeInheritanceAttributes(current));
                    }
                    return list.ToArray();
                }

                private CodeAttributeDeclaration[] GetKnownTypeAttributes(LinqToSqlShared.DbmlObjectModel.Type subT)
                {
                    List<CodeAttributeDeclaration> list = new List<CodeAttributeDeclaration>();
                    bool flag = this.serializationMode == SerializationMode.Unidirectional && subT.AccessModifier == AccessModifier.Public;
                    if (flag && subT.Name != null)
                    {
                        list.Add(new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(KnownTypeAttribute)))
                        {
                            Arguments =
                            {
                                new CodeAttributeArgument("", new CodeTypeOfExpression(subT.Name))
                            }
                        });
                    }
                    return list.ToArray();
                }

                private static bool AnySubTypeThisKeyReferencesMember(LinqToSqlShared.DbmlObjectModel.Type type, string columnName)
                {
                    foreach (LinqToSqlShared.DbmlObjectModel.Type current in type.SubTypes)
                    {
                        foreach (Association current2 in current.Associations)
                        {
                            string[] thisKey = current2.GetThisKey();
                            for (int i = 0; i < thisKey.Length; i++)
                            {
                                string a = thisKey[i];
                                if (a == columnName)
                                {
                                    bool result = true;
                                    return result;
                                }
                            }
                        }
                        if (CodeGeneratorFactory.DbmlGenerator.GenerateEntities.AnySubTypeThisKeyReferencesMember(current, columnName))
                        {
                            bool result = true;
                            return result;
                        }
                    }
                    return false;
                }

                internal override Column VisitColumn(Column column)
                {
                    System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(column.Type);
                    CodeTypeReference codeTypeReference;
                    if (type == null)
                    {
                        codeTypeReference = new CodeTypeReference(column.Type);
                    }
                    else
                    {
                        type = CodeGeneratorFactory.DbmlGenerator.GetNullableType(type, column.CanBeNull);
                        codeTypeReference = new CodeTypeReference(type);
                    }
                    CodeMemberField codeMemberField = new CodeMemberField();
                    codeMemberField.Name = column.Storage;
                    if (column.IsDelayLoaded == true)
                    {
                        codeMemberField.Type = new CodeTypeReference(typeof(Link<>).FullName, new CodeTypeReference[]
                        {
                            codeTypeReference
                        });
                    }
                    else
                    {
                        codeMemberField.Type = codeTypeReference;
                    }
                    if (column.IsReadOnly == true)
                    {
                        codeMemberField.InitExpression = new CodeDefaultValueExpression(codeMemberField.Type);
                    }
                    if (CodeGeneratorFactory.DbmlGenerator.GenerateEntities.AnySubTypeThisKeyReferencesMember(this.currentType, column.Member))
                    {
                        codeMemberField.Attributes = MemberAttributes.Family;
                    }
                    this.tableType.Members.Add(codeMemberField);
                    CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
                    codeMemberProperty.Name = column.Member;
                    codeMemberProperty.Attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(column.AccessModifier, column.Modifier);
                    codeMemberProperty.Type = codeTypeReference;
                    if (!this.createExternalMapping)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(ColumnAttribute)));
                        if (column.Name != column.Member && column.Name != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(column.Name)));
                        }
                        if (column.Storage != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Storage", new CodePrimitiveExpression(column.Storage)));
                        }
                        if (column.AutoSync.HasValue && column.AutoSync != LinqToSqlShared.DbmlObjectModel.AutoSync.Never)
                        {
                            CodeFieldReferenceExpression value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("AutoSync"), column.AutoSync.ToString());
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("AutoSync", value));
                        }
                        if (column.DbType != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DbType", new CodePrimitiveExpression(column.DbType)));
                        }
                        bool? flag = CodeGeneratorFactory.DbmlGenerator.ChooseCanBeNull(column.Type, column.CanBeNull);
                        if (flag.HasValue)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("CanBeNull", new CodePrimitiveExpression(flag.Value)));
                        }
                        if (column.IsPrimaryKey == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsPrimaryKey", new CodePrimitiveExpression(true)));
                        }
                        if (column.IsDbGenerated == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsDbGenerated", new CodePrimitiveExpression(true)));
                        }
                        if (column.IsVersion == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsVersion", new CodePrimitiveExpression(true)));
                        }
                        if (column.UpdateCheck != LinqToSqlShared.DbmlObjectModel.UpdateCheck.Always)
                        {
                            CodeFieldReferenceExpression value2 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("UpdateCheck"), column.UpdateCheck.ToString());
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("UpdateCheck", value2));
                        }
                        if (column.IsDiscriminator == true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsDiscriminator", new CodePrimitiveExpression(true)));
                        }
                        if (column.Expression != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Expression", new CodePrimitiveExpression(column.Expression)));
                        }
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    bool flag2 = false;
                    if (this.typeIsSerializable && column.AccessModifier == AccessModifier.Public)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(DataMemberAttribute)));
                        codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(this.serializationOrdinal++)));
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration2);
                        flag2 = true;
                    }
                    CodeExpression codeExpression;
                    if (column.IsDelayLoaded == true)
                    {
                        codeExpression = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name), "Value");
                    }
                    else
                    {
                        codeExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
                    }
                    codeMemberProperty.HasGet = true;
                    codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(codeExpression));
                    if (!flag2 && column.IsReadOnly == true)
                    {
                        codeMemberProperty.HasSet = false;
                    }
                    else
                    {
                        codeMemberProperty.HasSet = true;
                        List<CodeStatement> list = new List<CodeStatement>();
                        if (this.foreignKeys.ContainsKey(column.Name) && this.HasPrimaryKey(this.currentType.Name))
                        {
                            CodeStatement loadedCheckStatement = this.GetLoadedCheckStatement(column);
                            if (loadedCheckStatement != null)
                            {
                                list.Add(loadedCheckStatement);
                            }
                        }
                        if (this.hasPrimaryKey)
                        {
                            string unquotedNameIfNeeded = CodeGeneratorFactory.DbmlGenerator.GetUnquotedNameIfNeeded(codeMemberProperty.Name, this.provider);
                            string text = "On" + unquotedNameIfNeeded + "Changing";
                            string text2 = "On" + unquotedNameIfNeeded + "Changed";
                            string typeOutput = this.provider.GetTypeOutput(codeMemberProperty.Type);
                            string str;
                            string str2;
                            if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                            {
                                str = string.Format(CultureInfo.InvariantCulture, "partial void {0}({1} value);", new object[]
                                {
                                    text,
                                    typeOutput
                                });
                                str2 = string.Format(CultureInfo.InvariantCulture, "partial void {0}();", new object[]
                                {
                                    text2
                                });
                            }
                            else
                            {
                                str = string.Format(CultureInfo.InvariantCulture, "Partial Private Sub {0}(value As {1})\r\n", new object[]
                                {
                                    text,
                                    typeOutput
                                }) + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                                str2 = string.Format(CultureInfo.InvariantCulture, "Partial Private Sub {0}()\r\n", new object[]
                                {
                                    text2
                                }) + CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + "End Sub";
                            }
                            this.tableType.Members.Add(new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + str));
                            this.tableType.Members.Add(new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(this.ns != null) + str2));
                            list.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), text, new CodeExpression[]
                            {
                                new CodePropertySetValueReferenceExpression()
                            })));
                            list.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanging", new CodeExpression[0])));
                            list.Add(new CodeAssignStatement(codeExpression, new CodePropertySetValueReferenceExpression()));
                            list.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanged", new CodeExpression[]
                            {
                                new CodePrimitiveExpression(codeMemberProperty.Name)
                            })));
                            list.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), text2, new CodeExpression[0])));
                        }
                        else
                        {
                            list.Add(new CodeAssignStatement(codeExpression, new CodePropertySetValueReferenceExpression()));
                        }
                        string clrTypeName = (type != null) ? type.Name : column.Type;
                        codeMemberProperty.SetStatements.Add(new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEquals(type, clrTypeName, codeExpression, new CodePropertySetValueReferenceExpression(), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), list.ToArray()));
                    }
                    this.tableType.Members.Add(codeMemberProperty);
                    return column;
                }

                private bool HasPrimaryKey(string typeName)
                {
                    if (this.typeColumns.ContainsKey(typeName))
                    {
                        return this.typeColumns[typeName].Exists((Column c) => c.IsPrimaryKey == true);
                    }
                    return true;
                }

                private CodeConditionStatement GetLoadedCheckStatement(Column column)
                {
                    List<Association> list = this.foreignKeys[column.Name];
                    if (list == null)
                    {
                        return null;
                    }
                    List<CodePropertyReferenceExpression> list2 = (from a in list
                                                                   where !string.IsNullOrEmpty(a.Storage) && this.HasPrimaryKey(a.Type)
                                                                   select new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), a.Storage), "HasLoadedOrAssignedValue")).ToList<CodePropertyReferenceExpression>();
                    if (list2.Count == 0)
                    {
                        return null;
                    }
                    CodeExpression test = null;
                    list2.ForEach(delegate (CodePropertyReferenceExpression property)
                    {
                        if (test == null)
                        {
                            test = property;
                        }
                        else
                        {
                            test = new CodeBinaryOperatorExpression(test, CodeBinaryOperatorType.BooleanOr, property);
                        }
                    });
                    return new CodeConditionStatement(test, new CodeStatement[]
                    {
                        new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference("System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException"), new CodeExpression[0]))
                    });
                }

                private static Dictionary<string, List<Association>> GetForeignKeys(LinqToSqlShared.DbmlObjectModel.Type type)
                {
                    Dictionary<string, List<Association>> dictionary = new Dictionary<string, List<Association>>();
                    foreach (Association current in type.Associations)
                    {
                        if (current.IsForeignKey == true)
                        {
                            string[] thisKey = current.GetThisKey();
                            string[] array = thisKey;
                            for (int i = 0; i < array.Length; i++)
                            {
                                string key = array[i];
                                List<Association> list;
                                if (!dictionary.ContainsKey(key))
                                {
                                    list = new List<Association>();
                                    dictionary.Add(key, list);
                                }
                                else
                                {
                                    list = dictionary[key];
                                }
                                list.Add(current);
                            }
                        }
                    }
                    return dictionary;
                }

                private void GeneratePropertyChangingNotifier(CodeTypeDeclaration decl)
                {
                    this.GeneratePropertyNotifier(decl, typeof(INotifyPropertyChanging), typeof(PropertyChangingEventHandler), typeof(PropertyChangingEventArgs), "PropertyChanging", "SendPropertyChanging", false);
                }

                private void GeneratePropertyChangedNotifier(CodeTypeDeclaration decl)
                {
                    this.GeneratePropertyNotifier(decl, typeof(INotifyPropertyChanged), typeof(PropertyChangedEventHandler), typeof(PropertyChangedEventArgs), "PropertyChanged", "SendPropertyChanged", true);
                }

                private void GeneratePropertyNotifier(CodeTypeDeclaration decl, System.Type interfaceType, System.Type handlerType, System.Type argsType, string eventName, string methodName, bool sendName)
                {
                    CodeTypeReference value;
                    if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider))
                    {
                        value = new CodeTypeReference(interfaceType.Name);
                    }
                    else
                    {
                        value = new CodeTypeReference(interfaceType);
                    }
                    decl.BaseTypes.Add(value);
                    CodeMemberEvent codeMemberEvent = new CodeMemberEvent();
                    codeMemberEvent.Name = eventName;
                    codeMemberEvent.Attributes = MemberAttributes.Public;
                    codeMemberEvent.Type = new CodeTypeReference(handlerType.Name);
                    decl.Members.Add(codeMemberEvent);
                    codeMemberEvent.ImplementationTypes.Add(value);
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = methodName;
                    if (this.currentType.Modifier == ClassModifier.Sealed)
                    {
                        codeMemberMethod.Attributes |= (MemberAttributes)20482;
                    }
                    else
                    {
                        codeMemberMethod.Attributes = MemberAttributes.Family;
                    }
                    CodeExpression codeExpression = new CodeArgumentReferenceExpression(this.emptyChangingEventArgsMemberName);
                    if (sendName)
                    {
                        codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string).Name), "propertyName"));
                        codeExpression = new CodeObjectCreateExpression(new CodeTypeReference(argsType.Name), new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("propertyName")
                        });
                    }
                    codeMemberMethod.Statements.Add(new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEqualsToNull(new CodeEventReferenceExpression(new CodeThisReferenceExpression(), codeMemberEvent.Name), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), new CodeStatement[]
                    {
                        new CodeExpressionStatement(new CodeDelegateInvokeExpression(new CodeEventReferenceExpression(new CodeThisReferenceExpression(), codeMemberEvent.Name), new CodeExpression[]
                        {
                            new CodeThisReferenceExpression(),
                            codeExpression
                        }))
                    }));
                    decl.Members.Add(codeMemberMethod);
                }

                internal override Association VisitAssociation(Association association)
                {
                    this.GenerateAssociationProperty(association);
                    return base.VisitAssociation(association);
                }

                private void GenerateAssociationProperty(Association rel)
                {
                    if (rel.Cardinality == Cardinality.One)
                    {
                        this.GenerateSingleSideAssociationProperty(rel);
                        return;
                    }
                    this.GenerateCollectionSideAssociationProperty(rel);
                }

                private void GenerateCollectionSideAssociationProperty(Association rel)
                {
                    CodeTypeReference codeTypeReference = new CodeTypeReference("EntitySet", new CodeTypeReference[]
                    {
                        new CodeTypeReference(rel.Type)
                    });
                    CodeTypeReference type = codeTypeReference;
                    CodeMemberField codeMemberField = new CodeMemberField();
                    codeMemberField.Attributes = MemberAttributes.Private;
                    codeMemberField.Name = rel.Storage;
                    codeMemberField.Type = type;
                    this.tableType.Members.Add(codeMemberField);
                    if (!this.model.IsOneWay(rel))
                    {
                        if (rel.Cardinality == Cardinality.Many)
                        {
                            CodeExpression codeExpression = this.GenerateAttach(this.tableType, rel, rel.Storage);
                            CodeExpression codeExpression2 = this.GenerateDetach(this.tableType, rel, rel.Storage);
                            this.constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name), new CodeObjectCreateExpression(new CodeTypeReference("EntitySet", new CodeTypeReference[]
                            {
                                new CodeTypeReference(rel.Type)
                            }), new CodeExpression[]
                            {
                                codeExpression,
                                codeExpression2
                            })));
                        }
                    }
                    else
                    {
                        this.constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name), new CodeObjectCreateExpression(new CodeTypeReference("EntitySet", new CodeTypeReference[]
                        {
                            new CodeTypeReference(rel.Type)
                        }), new CodeExpression[0])));
                    }
                    CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
                    codeMemberProperty.Name = rel.Member;
                    codeMemberProperty.Attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(rel.AccessModifier, rel.Modifier);
                    codeMemberProperty.Type = codeTypeReference;
                    codeMemberProperty.HasGet = true;
                    bool flag = this.typeIsSerializable && this.IsSerializableAssociation(rel);
                    if (flag)
                    {
                        CodeExpression codeExpression3 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
                        codeExpression3 = new CodePropertyReferenceExpression(codeExpression3, "HasLoadedOrAssignedValues");
                        codeExpression3 = CodeGeneratorFactory.DbmlGenerator.MakeEquals(typeof(bool), "bool", codeExpression3, new CodePrimitiveExpression(false), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider));
                        CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.serializationFlagMemberName), CodeBinaryOperatorType.BooleanAnd, codeExpression3);
                        codeMemberProperty.GetStatements.Add(new CodeConditionStatement(condition, new CodeStatement[]
                        {
                            new CodeMethodReturnStatement(new CodePrimitiveExpression(null))
                        }));
                    }
                    codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name)));
                    codeMemberProperty.HasSet = true;
                    codeMemberProperty.SetStatements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name), "Assign", new CodeExpression[]
                    {
                        new CodePropertySetValueReferenceExpression()
                    })));
                    if (!this.createExternalMapping)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(AssociationAttribute)));
                        if (rel.Name != codeMemberProperty.Name)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(rel.Name)));
                        }
                        if (rel.Storage != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Storage", new CodePrimitiveExpression(rel.Storage)));
                        }
                        string[] thisKey = rel.GetThisKey();
                        if (thisKey.Length != 0)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ThisKey", new CodePrimitiveExpression(Dbml.BuildKeyField(rel.GetThisKey()))));
                        }
                        if (rel.GetOtherKey().Length != 0)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("OtherKey", new CodePrimitiveExpression(Dbml.BuildKeyField(rel.GetOtherKey()))));
                        }
                        if (rel.DeleteRule != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DeleteRule", new CodePrimitiveExpression(rel.DeleteRule)));
                        }
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    if (flag)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(DataMemberAttribute)));
                        codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(this.serializationOrdinal++)));
                        codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(false)));
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration2);
                    }
                    this.tableType.Members.Add(codeMemberProperty);
                }

                private static CodeTypeReference MakeNullableTypeIfNeeded(string clrTypeName, bool? canBeNull)
                {
                    CodeTypeReference codeTypeReference = new CodeTypeReference(clrTypeName);
                    if (canBeNull == true)
                    {
                        System.Type clrTypeFromTypeName = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(clrTypeName);
                        if (clrTypeFromTypeName != null && !TypeSystem.IsNullableType(clrTypeFromTypeName) && clrTypeFromTypeName.IsValueType)
                        {
                            codeTypeReference = new CodeTypeReference("Nullable", new CodeTypeReference[]
                            {
                                codeTypeReference
                            });
                        }
                    }
                    return codeTypeReference;
                }

                private void GenerateSingleSideAssociationProperty(Association rel)
                {
                    CodeTypeReference codeTypeReference = new CodeTypeReference(rel.Type);
                    CodeTypeReference codeTypeReference2 = codeTypeReference;
                    codeTypeReference2 = new CodeTypeReference("EntityRef", new CodeTypeReference[]
                    {
                        codeTypeReference2
                    });
                    CodeMemberField codeMemberField = new CodeMemberField();
                    codeMemberField.Attributes = MemberAttributes.Private;
                    codeMemberField.Name = rel.Storage;
                    codeMemberField.Type = codeTypeReference2;
                    this.tableType.Members.Add(codeMemberField);
                    this.constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name), new CodeDefaultValueExpression(codeMemberField.Type)));
                    CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
                    codeMemberProperty.Name = rel.Member;
                    codeMemberProperty.Attributes = CodeGeneratorFactory.DbmlGenerator.ToMemberAttributes(rel.AccessModifier, rel.Modifier);
                    codeMemberProperty.Type = codeTypeReference;
                    codeMemberProperty.HasGet = true;
                    codeMemberProperty.HasSet = true;
                    CodeExpression codeExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
                    codeExpression = new CodePropertyReferenceExpression(codeExpression, "Entity");
                    bool flag = this.typeIsSerializable && this.IsSerializableAssociation(rel);
                    if (flag)
                    {
                        CodeExpression codeExpression2 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
                        codeExpression2 = new CodePropertyReferenceExpression(codeExpression2, "HasLoadedOrAssignedValue");
                        codeExpression2 = CodeGeneratorFactory.DbmlGenerator.MakeEquals(typeof(bool), "bool", codeExpression2, new CodePrimitiveExpression(false), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider));
                        CodeBinaryOperatorExpression condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.serializationFlagMemberName), CodeBinaryOperatorType.BooleanAnd, codeExpression2);
                        codeMemberProperty.GetStatements.Add(new CodeConditionStatement(condition, new CodeStatement[]
                        {
                            new CodeMethodReturnStatement(new CodePrimitiveExpression(null))
                        }));
                    }
                    codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(codeExpression));
                    Association otherAssociation = this.model.GetOtherAssociation(rel);
                    if (otherAssociation == null)
                    {
                        codeMemberProperty.SetStatements.Add(new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEquals(null, rel.Type, codeExpression, new CodePropertySetValueReferenceExpression(), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), new CodeStatement[]
                        {
                            new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanging", new CodeExpression[0])),
                            new CodeAssignStatement(codeExpression, new CodePropertySetValueReferenceExpression()),
                            new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanged", new CodeExpression[]
                            {
                                new CodePrimitiveExpression(codeMemberProperty.Name)
                            }))
                        }));
                    }
                    else
                    {
                        string propertyName = (otherAssociation.Member != null) ? otherAssociation.Member : otherAssociation.Name;
                        CodeStatement codeStatement;
                        CodeStatement item;
                        if (otherAssociation.Cardinality == Cardinality.Many)
                        {
                            codeStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("previousValue"), propertyName), "Remove", new CodeExpression[]
                            {
                                new CodeThisReferenceExpression()
                            }));
                            item = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), propertyName), "Add", new CodeExpression[]
                            {
                                new CodeThisReferenceExpression()
                            }));
                        }
                        else
                        {
                            codeStatement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("previousValue"), propertyName), new CodePrimitiveExpression(null));
                            item = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), propertyName), new CodeThisReferenceExpression());
                        }
                        List<CodeStatement> list = null;
                        List<CodeStatement> list2 = new List<CodeStatement>();
                        list2.Add(item);
                        if (rel.IsForeignKey.GetValueOrDefault(false))
                        {
                            string[] thisKey = rel.GetThisKey();
                            List<Column> columns = this.GetColumns(thisKey, this.currentType.Name);
                            string[] otherKey = rel.GetOtherKey();
                            List<Column> columns2 = this.GetColumns(otherKey, rel.Type);
                            if (columns.Count != columns2.Count)
                            {
                                throw Error.MismatchedThisKeyOtherKey(Dbml.BuildKeyField(rel.GetThisKey()), (this.thisTable == null) ? "<Unknown>" : this.thisTable.Name, Dbml.BuildKeyField(otherKey), rel.Type);
                            }
                            for (int i = 0; i < columns.Count; i++)
                            {
                                CodePropertyReferenceExpression codePropertyReferenceExpression = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), columns[i].Storage);
                                if (columns[i].IsDelayLoaded == true)
                                {
                                    codePropertyReferenceExpression = new CodePropertyReferenceExpression(codePropertyReferenceExpression, "Value");
                                }
                                list2.Add(new CodeAssignStatement(codePropertyReferenceExpression, new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), columns2[i].Member)));
                            }
                            list = new List<CodeStatement>();
                            for (int j = 0; j < columns.Count; j++)
                            {
                                CodeTypeReference type = CodeGeneratorFactory.DbmlGenerator.GenerateEntities.MakeNullableTypeIfNeeded(columns[j].Type, columns[j].CanBeNull);
                                CodePropertyReferenceExpression codePropertyReferenceExpression2 = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), columns[j].Storage);
                                if (columns[j].IsDelayLoaded == true)
                                {
                                    codePropertyReferenceExpression2 = new CodePropertyReferenceExpression(codePropertyReferenceExpression2, "Value");
                                }
                                list.Add(new CodeAssignStatement(codePropertyReferenceExpression2, new CodeDefaultValueExpression(type)));
                            }
                        }
                        CodeExpression codeExpression3 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
                        codeExpression3 = new CodePropertyReferenceExpression(codeExpression3, "HasLoadedOrAssignedValue");
                        codeExpression3 = CodeGeneratorFactory.DbmlGenerator.MakeEquals(typeof(bool), "bool", codeExpression3, new CodePrimitiveExpression(false), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider));
                        CodeExpression left = CodeGeneratorFactory.DbmlGenerator.MakeNotEquals(null, rel.Type, new CodeVariableReferenceExpression("previousValue"), new CodePropertySetValueReferenceExpression(), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider));
                        CodeBinaryOperatorExpression condition2 = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanOr, codeExpression3);
                        codeMemberProperty.SetStatements.Add(new CodeVariableDeclarationStatement(codeMemberProperty.Type, "previousValue", codeExpression));
                        List<CodeStatement> list3 = new List<CodeStatement>();
                        list3.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanging", new CodeExpression[0])));
                        list3.Add(new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEqualsToNull(new CodeVariableReferenceExpression("previousValue"), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), new CodeStatement[]
                        {
                            new CodeAssignStatement(codeExpression, new CodePrimitiveExpression(null)),
                            codeStatement
                        }));
                        list3.Add(new CodeAssignStatement(codeExpression, new CodePropertySetValueReferenceExpression()));
                        CodeConditionStatement item2;
                        if (rel.IsForeignKey.GetValueOrDefault(false))
                        {
                            item2 = new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEqualsToNull(new CodePropertySetValueReferenceExpression(), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), list2.ToArray(), list.ToArray());
                        }
                        else
                        {
                            item2 = new CodeConditionStatement(CodeGeneratorFactory.DbmlGenerator.MakeNotEquals(null, rel.Type, new CodePropertySetValueReferenceExpression(), new CodePrimitiveExpression(null), CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(this.provider)), list2.ToArray());
                        }
                        list3.Add(item2);
                        list3.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanged", new CodeExpression[]
                        {
                            new CodePrimitiveExpression(codeMemberProperty.Name)
                        })));
                        codeMemberProperty.SetStatements.Add(new CodeConditionStatement(condition2, list3.ToArray()));
                    }
                    if (!this.createExternalMapping)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(AssociationAttribute)));
                        if (rel.Name != null && rel.Name != codeMemberProperty.Name)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(rel.Name)));
                        }
                        if (rel.Storage != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("Storage", new CodePrimitiveExpression(rel.Storage)));
                        }
                        if (rel.GetThisKey().Length != 0)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("ThisKey", new CodePrimitiveExpression(Dbml.BuildKeyField(rel.GetThisKey()))));
                        }
                        if (rel.GetOtherKey().Length != 0)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("OtherKey", new CodePrimitiveExpression(Dbml.BuildKeyField(rel.GetOtherKey()))));
                        }
                        if (rel.Cardinality == Cardinality.One && rel.IsForeignKey != true)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsUnique", new CodePrimitiveExpression(true)));
                        }
                        if (rel.IsForeignKey.HasValue)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("IsForeignKey", new CodePrimitiveExpression(rel.IsForeignKey)));
                        }
                        if (rel.DeleteOnNull.HasValue && rel.DeleteOnNull.Value)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DeleteOnNull", new CodePrimitiveExpression(rel.DeleteOnNull)));
                        }
                        if (rel.DeleteRule != null)
                        {
                            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument("DeleteRule", new CodePrimitiveExpression(rel.DeleteRule)));
                        }
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration);
                    }
                    if (flag)
                    {
                        CodeAttributeDeclaration codeAttributeDeclaration2 = new CodeAttributeDeclaration(CodeGeneratorFactory.DbmlGenerator.AttributeType(typeof(DataMemberAttribute)));
                        codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(this.serializationOrdinal++)));
                        codeAttributeDeclaration2.Arguments.Add(new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(false)));
                        codeMemberProperty.CustomAttributes.Add(codeAttributeDeclaration2);
                    }
                    this.tableType.Members.Add(codeMemberProperty);
                }

                private bool IsSerializableAssociation(Association rel)
                {
                    if (rel.AccessModifier != AccessModifier.Public)
                    {
                        return false;
                    }
                    bool flag = false;
                    if (rel.Cardinality == Cardinality.One)
                    {
                        flag = (this.model.IsOneWay(rel) || !rel.IsForeignKey.GetValueOrDefault(false));
                    }
                    bool flag2 = rel.Cardinality == Cardinality.Many;
                    return flag || flag2;
                }

                private List<Column> GetColumns(string[] columnMemberNames, string typeName)
                {
                    this.typeColumns.ContainsKey(typeName);
                    List<Column> list = new List<Column>();
                    for (int i = 0; i < columnMemberNames.Length; i++)
                    {
                        string cMember = columnMemberNames[i];
                        Column item = this.typeColumns[typeName].Find((Column col) => cMember == CodeGeneratorFactory.DbmlGenerator.GetUnquotedNameIfNeeded(col.Member, this.provider));
                        list.Add(item);
                    }
                    return list;
                }

                private CodeExpression GenerateAttach(CodeTypeDeclaration rowTypeDecl, Association rel, string name)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = "attach" + name;
                    codeMemberMethod.Attributes = MemberAttributes.Private;
                    codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
                    CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(new CodeTypeReference(rel.Type), "entity");
                    codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
                    CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression(codeParameterDeclarationExpression.Name);
                    codeMemberMethod.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanging", new CodeExpression[0])));
                    Association otherAssociation = this.model.GetOtherAssociation(rel);
                    string propertyName = (otherAssociation.Member != null) ? otherAssociation.Member : otherAssociation.Name;
                    if (rel.Cardinality == Cardinality.One)
                    {
                        codeMemberMethod.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(targetObject, propertyName), "Add", new CodeExpression[]
                        {
                            new CodeThisReferenceExpression()
                        })));
                    }
                    else
                    {
                        codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(targetObject, propertyName), new CodeThisReferenceExpression()));
                    }
                    rowTypeDecl.Members.Add(codeMemberMethod);
                    return new CodeDelegateCreateExpression(new CodeTypeReference("Action", new CodeTypeReference[]
                    {
                        codeParameterDeclarationExpression.Type
                    }), new CodeThisReferenceExpression(), codeMemberMethod.Name);
                }

                private CodeExpression GenerateDetach(CodeTypeDeclaration rowTypeDecl, Association rel, string name)
                {
                    CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
                    codeMemberMethod.Name = "detach" + name;
                    codeMemberMethod.Attributes = MemberAttributes.Private;
                    codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
                    CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(new CodeTypeReference(rel.Type), "entity");
                    codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
                    CodeVariableReferenceExpression targetObject = new CodeVariableReferenceExpression(codeParameterDeclarationExpression.Name);
                    codeMemberMethod.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SendPropertyChanging", new CodeExpression[0])));
                    Association otherAssociation = this.model.GetOtherAssociation(rel);
                    string propertyName = (otherAssociation.Member != null) ? otherAssociation.Member : otherAssociation.Name;
                    if (rel.Cardinality == Cardinality.One)
                    {
                        codeMemberMethod.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(targetObject, propertyName), "Remove", new CodeExpression[]
                        {
                            new CodeThisReferenceExpression()
                        })));
                    }
                    else
                    {
                        codeMemberMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(targetObject, propertyName), new CodePrimitiveExpression(null)));
                    }
                    rowTypeDecl.Members.Add(codeMemberMethod);
                    return new CodeDelegateCreateExpression(new CodeTypeReference("Action", new CodeTypeReference[]
                    {
                        codeParameterDeclarationExpression.Type
                    }), new CodeThisReferenceExpression(), codeMemberMethod.Name);
                }
            }

            private CodeDomProvider provider;

            private CodeGeneratorFactory.DbmlGenerator.Validator validator;

            private static Dictionary<string, System.Type> typeMappings;

            internal static CodeGeneratorFactory.DbmlGenerator Create(CodeDomProvider provider)
            {
                CodeGeneratorFactory.DbmlGenerator.InitializeTypeMappings();
                return new CodeGeneratorFactory.DbmlGenerator(provider);
            }

            private DbmlGenerator(CodeDomProvider provider)
            {
                this.provider = provider;
                this.validator = new CodeGeneratorFactory.DbmlGenerator.Validator(this.provider);
            }

            public IEnumerable<ValidationMessage> ValidateModel(Database db)
            {
                return this.validator.Validate(db);
            }

            public GenerationResult Generate(Database db, GenerateOptions options)
            {
                db = Dbml.CopyWithFilledInDefaults(db);
                return CodeGeneratorFactory.DbmlGenerator.CreateFromDbml(db, this.provider, options);
            }

            internal static bool IsCSharpCodeProvider(CodeDomProvider provider)
            {
                return string.Compare(provider.FileExtension, "CS", StringComparison.OrdinalIgnoreCase) == 0;
            }

            internal static string GetUnquotedNameIfNeeded(string name, CodeDomProvider provider)
            {
                int length = name.Length;
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    if (name.StartsWith("@", StringComparison.Ordinal))
                    {
                        return name.Substring(1);
                    }
                }
                else if (name.StartsWith("[", StringComparison.Ordinal) && name.EndsWith("]", StringComparison.Ordinal))
                {
                    return name.Substring(1, length - 2);
                }
                return name;
            }

            private static GenerationResult CreateFromDbml(Database dbml, CodeDomProvider provider, GenerateOptions options)
            {
                CodeGeneratorFactory.ModelInfo modelInfo = new CodeGeneratorFactory.ModelInfo(dbml);
                List<ResultFile> list = new List<ResultFile>();
                string text = options.ResultBaseDirectory;
                ResultFile resultFile = new ResultFile();
                if (dbml.ExternalMapping == true)
                {
                    CodeGeneratorFactory.DbmlGenerator.GenerateMapping generateMapping = new CodeGeneratorFactory.DbmlGenerator.GenerateMapping();
                    DatabaseMapping databaseMapping = generateMapping.CreateDbMapping(dbml, modelInfo);
                    if (databaseMapping != null)
                    {
                        resultFile.FileContent = CodeGeneratorFactory.DbmlGenerator.SerializeDbMapping(databaseMapping);
                        resultFile.FileName = text + options.ResultBaseFileName + ".map";
                        resultFile.IsPrimary = false;
                        resultFile.IsSourceFile = false;
                        resultFile.Encoding = Encoding.UTF8;
                        list.Add(resultFile);
                    }
                }
                CodeGeneratorFactory.DbmlGenerator.NameLegalizer nameLegalizer = new CodeGeneratorFactory.DbmlGenerator.NameLegalizer(provider);
                dbml = nameLegalizer.VisitDatabase(dbml);
                CodeGeneratorOptions options2 = CodeGeneratorFactory.DbmlGenerator.CreateGenerateOptions();
                CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
                modelInfo = new CodeGeneratorFactory.ModelInfo(dbml);
                CodeGeneratorFactory.DbmlGenerator.BuildCode(codeCompileUnit, dbml, modelInfo, provider);
                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    provider.GenerateCodeFromCompileUnit(new CodeSnippetCompileUnit("#pragma warning disable 1591"), stringWriter, options2);
                }
                provider.GenerateCodeFromCompileUnit(codeCompileUnit, stringWriter, options2);
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    provider.GenerateCodeFromCompileUnit(new CodeSnippetCompileUnit("#pragma warning restore 1591"), stringWriter, options2);
                }
                ResultFile resultFile2 = new ResultFile();
                resultFile2.FileContent = stringWriter.ToString();
                if (text == null || text.TrimEnd(new char[]
                {
                    ' ',
                    '\n',
                    '\t',
                    '\r'
                }).Length == 0)
                {
                    text = "";
                }
                else if (!text.EndsWith("\\", StringComparison.Ordinal))
                {
                    text += "\\";
                }
                if (text.Length > 0)
                {
                    Directory.CreateDirectory(text);
                }
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    resultFile2.FileName = text + options.ResultBaseFileName + ".cs";
                }
                else
                {
                    resultFile2.FileName = text + options.ResultBaseFileName + ".vb";
                }
                resultFile2.IsPrimary = true;
                resultFile2.IsSourceFile = true;
                resultFile2.Encoding = Encoding.UTF8;
                list.Add(resultFile2);
                return new GenerationResult
                {
                    Files = list,
                    Assemblies = CodeGeneratorFactory.DbmlGenerator.GetAssemblies(dbml)
                };
            }

            private static List<string> GetAssemblies(Database dbml)
            {
                List<string> list = new List<string>();
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(DataContext));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(DatabaseAttribute));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(IDbConnection));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(IEnumerable<>));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(MemberInfo));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(IQueryable<>));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(MethodCallExpression));
                SerializationMode valueOrDefault = dbml.Serialization.GetValueOrDefault(SerializationMode.None);
                if (valueOrDefault != SerializationMode.None)
                {
                    CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(DataContractAttribute));
                }
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(INotifyPropertyChanged));
                CodeGeneratorFactory.DbmlGenerator.AddAssemblyFor(list, typeof(Action<>));
                return list;
            }

            private static void AddAssemblyFor(List<string> assemblies, System.Type type)
            {
                string name = type.Assembly.GetName().Name;
                if (!assemblies.Contains(name))
                {
                    assemblies.Add(name);
                }
            }

            internal static CodeSnippetTypeMember GetPartialMethodRegionStartTag(bool hasNamespace, CodeDomProvider provider)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    stringBuilder.AppendFormat("#region {0}", Strings.ExtensibilityMethodDefinitions);
                    return new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(hasNamespace) + stringBuilder.ToString());
                }
                stringBuilder.AppendFormat("#Region \"{0}\"", Strings.ExtensibilityMethodDefinitions);
                return new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(hasNamespace) + stringBuilder.ToString());
            }

            internal static CodeSnippetTypeMember GetPartialMethodRegionEndTag(bool hasNamespace, CodeDomProvider provider)
            {
                if (CodeGeneratorFactory.DbmlGenerator.IsCSharpCodeProvider(provider))
                {
                    return new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(hasNamespace) + "#endregion");
                }
                return new CodeSnippetTypeMember(CodeGeneratorFactory.DbmlGenerator.GetPartialMethodIndentation(hasNamespace) + "#End Region");
            }

            private static string SerializeDbMapping(DatabaseMapping dbMapping)
            {
                string result;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
                        {
                            Indent = true
                        }))
                        {
                            CodeGeneratorFactory.DbmlGenerator.MappingXmlFormatter.Build(xmlWriter, dbMapping);
                        }
                        using (StreamReader streamReader = new StreamReader(memoryStream))
                        {
                            memoryStream.Position = 0L;
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                return result;
            }

            private static CodeGeneratorOptions CreateGenerateOptions()
            {
                return new CodeGeneratorOptions
                {
                    IndentString = "\t",
                    VerbatimOrder = false,
                    BlankLinesBetweenMembers = true,
                    BracingStyle = "C"
                };
            }

            private static void BuildCode(CodeCompileUnit ccu, Database dbml, CodeGeneratorFactory.ModelInfo model, CodeDomProvider provider)
            {
                ccu.UserData.Add("AllowLateBound", false);
                CodeNamespace codeNamespace = new CodeNamespace(dbml.ContextNamespace);
                ccu.Namespaces.Add(codeNamespace);
                CodeNamespace codeNamespace2;
                if (string.Compare(dbml.ContextNamespace, dbml.EntityNamespace, StringComparison.Ordinal) == 0)
                {
                    codeNamespace2 = codeNamespace;
                }
                else
                {
                    codeNamespace2 = new CodeNamespace(dbml.EntityNamespace);
                    ccu.Namespaces.Add(codeNamespace2);
                }
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataContext).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(MappingSource).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(IDbConnection).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(IEnumerable<>).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(MemberInfo).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(IQueryable<>).Namespace));
                codeNamespace.Imports.Add(new CodeNamespaceImport(typeof(MethodCallExpression).Namespace));
                SerializationMode valueOrDefault = dbml.Serialization.GetValueOrDefault(SerializationMode.None);
                if (valueOrDefault != SerializationMode.None)
                {
                    codeNamespace2.Imports.Add(new CodeNamespaceImport(typeof(DataContractAttribute).Namespace));
                }
                codeNamespace2.Imports.Add(new CodeNamespaceImport(typeof(EntitySet<>).Namespace));
                codeNamespace2.Imports.Add(new CodeNamespaceImport(typeof(ColumnAttribute).Namespace));
                codeNamespace2.Imports.Add(new CodeNamespaceImport(typeof(INotifyPropertyChanged).Namespace));
                codeNamespace2.Imports.Add(new CodeNamespaceImport(typeof(Action<string>).Namespace));
                new CodeGeneratorFactory.DbmlGenerator.GenerateContext(codeNamespace, dbml.ExternalMapping.GetValueOrDefault(false), provider).VisitDatabase(dbml);
                new CodeGeneratorFactory.DbmlGenerator.GenerateEntities().Generate(dbml, model, codeNamespace2, dbml.ExternalMapping.GetValueOrDefault(false), provider, valueOrDefault);
            }

            private static MemberAttributes ToMemberAttributes(AccessModifier? modifier, MemberModifier? memberModifier)
            {
                MemberAttributes? memberAttributes = CodeGeneratorFactory.DbmlGenerator.MemberModifierToMemberAttributes(memberModifier);
                if (!memberAttributes.HasValue)
                {
                    return CodeGeneratorFactory.DbmlGenerator.AccessModifierToMemberAttributes(modifier);
                }
                return memberAttributes.Value | CodeGeneratorFactory.DbmlGenerator.AccessModifierToMemberAttributes(modifier);
            }

            private static MemberAttributes AccessModifierToMemberAttributes(AccessModifier? modifier)
            {
                AccessModifier valueOrDefault = modifier.GetValueOrDefault();
                if (!modifier.HasValue)
                {
                    return (MemberAttributes)24578;
                }
                switch (valueOrDefault)
                {
                    case AccessModifier.Public:
                        return MemberAttributes.Public;
                    case AccessModifier.Internal:
                        return MemberAttributes.Assembly;
                    case AccessModifier.Protected:
                        return MemberAttributes.Family;
                    case AccessModifier.ProtectedInternal:
                        return MemberAttributes.FamilyOrAssembly;
                    case AccessModifier.Private:
                        return MemberAttributes.Private;
                    default:
                        throw Error.ArgumentOutOfRange("modifier");
                }
            }

            private static MemberAttributes? MemberModifierToMemberAttributes(MemberModifier? modifier)
            {
                MemberModifier valueOrDefault = modifier.GetValueOrDefault();
                if (!modifier.HasValue)
                {
                    return new MemberAttributes?(MemberAttributes.Final);
                }
                switch (valueOrDefault)
                {
                    case MemberModifier.Virtual:
                        return null;
                    case MemberModifier.Override:
                        return new MemberAttributes?(MemberAttributes.Override);
                    case MemberModifier.New:
                        return new MemberAttributes?((MemberAttributes)18);
                    case MemberModifier.NewVirtual:
                        return new MemberAttributes?(MemberAttributes.New);
                    default:
                        throw Error.ArgumentOutOfRange("modifier");
                }
            }

            private static TypeAttributes ToTypeAttributes(AccessModifier? modifier, ClassModifier? classModifier)
            {
                TypeAttributes? typeAttributes = CodeGeneratorFactory.DbmlGenerator.ClassModifierToTypeAttributes(classModifier);
                if (!typeAttributes.HasValue)
                {
                    return CodeGeneratorFactory.DbmlGenerator.AccessModifierToTypeAttributes(modifier);
                }
                return typeAttributes.Value | CodeGeneratorFactory.DbmlGenerator.AccessModifierToTypeAttributes(modifier);
            }

            private static TypeAttributes AccessModifierToTypeAttributes(AccessModifier? modifier)
            {
                AccessModifier valueOrDefault = modifier.GetValueOrDefault();
                if (!modifier.HasValue)
                {
                    return TypeAttributes.Public;
                }
                switch (valueOrDefault)
                {
                    case AccessModifier.Public:
                        return TypeAttributes.Public;
                    case AccessModifier.Internal:
                        return TypeAttributes.NestedAssembly;
                    case AccessModifier.Protected:
                        return TypeAttributes.NestedFamily;
                    case AccessModifier.ProtectedInternal:
                        return TypeAttributes.VisibilityMask;
                    case AccessModifier.Private:
                        return TypeAttributes.NestedPrivate;
                    default:
                        throw Error.ArgumentOutOfRange("modifier");
                }
            }

            private static TypeAttributes? ClassModifierToTypeAttributes(ClassModifier? modifier)
            {
                ClassModifier valueOrDefault = modifier.GetValueOrDefault();
                if (!modifier.HasValue)
                {
                    return null;
                }
                switch (valueOrDefault)
                {
                    case ClassModifier.Sealed:
                        return new TypeAttributes?(TypeAttributes.Sealed);
                    case ClassModifier.Abstract:
                        return new TypeAttributes?(TypeAttributes.Abstract);
                    default:
                        throw Error.ArgumentOutOfRange("modifier");
                }
            }

            private static CodeTypeReference AttributeType(System.Type attributeType)
            {
                return new CodeTypeReference(attributeType, CodeTypeReferenceOptions.GlobalReference);
            }

            private static bool IsNullableType(System.Type type)
            {
                return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            private static System.Type GetNullableType(System.Type type, bool? isNullable)
            {
                if (isNullable.GetValueOrDefault() && type.IsValueType && !CodeGeneratorFactory.DbmlGenerator.IsNullableType(type))
                {
                    return typeof(Nullable<>).MakeGenericType(new System.Type[]
                    {
                        type
                    });
                }
                return type;
            }

            private static CodeExpression MakeEquals(System.Type clrType, string clrTypeName, CodeExpression left, CodeExpression right, bool isCSharp)
            {
                if (isCSharp)
                {
                    return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityEquality, right);
                }
                CodeExpression result;
                if (clrType != null && clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    result = new CodeMethodInvokeExpression(left, "Equals", new CodeExpression[]
                    {
                        right
                    });
                }
                else if (clrType != null && clrType.IsValueType)
                {
                    result = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, right);
                }
                else if (clrType == typeof(string))
                {
                    result = new CodeMethodInvokeExpression(null, "String.Equals", new CodeExpression[]
                    {
                        left,
                        right
                    });
                }
                else
                {
                    result = new CodeMethodInvokeExpression(null, typeof(object).Name + ".Equals", new CodeExpression[]
                    {
                        left,
                        right
                    });
                }
                return result;
            }

            private static CodeExpression MakeNotEquals(System.Type clrType, string clrTypeName, CodeExpression left, CodeExpression right, bool isCSharp)
            {
                if (isCSharp)
                {
                    return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, right);
                }
                CodeExpression left2 = CodeGeneratorFactory.DbmlGenerator.MakeEquals(clrType, clrTypeName, left, right, isCSharp);
                return new CodeBinaryOperatorExpression(left2, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            }

            private static CodeExpression MakeNotEqualsToNull(CodeExpression value, bool isCSharp)
            {
                if (isCSharp)
                {
                    return new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                }
                CodeExpression left = new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                return new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false));
            }

            private static System.Type GetClrTypeFromTypeName(string typeName)
            {
                System.Type type = System.Type.GetType(typeName);
                if (type == null && !CodeGeneratorFactory.DbmlGenerator.typeMappings.TryGetValue(typeName, out type))
                {
                    type = null;
                }
                return type;
            }

            private static System.Type GetType(System.Type type, bool isNullable)
            {
                if (isNullable && type.IsValueType && !TypeSystem.IsNullableType(type))
                {
                    return typeof(Nullable<>).MakeGenericType(new System.Type[]
                    {
                        type
                    });
                }
                return type;
            }

            private static bool? ChooseCanBeNull(string dbmlType, bool? dbmlCanBeNull)
            {
                if (!dbmlCanBeNull.HasValue)
                {
                    return null;
                }
                System.Type type = CodeGeneratorFactory.DbmlGenerator.GetClrTypeFromTypeName(dbmlType);
                if (type == null)
                {
                    return dbmlCanBeNull;
                }
                type = CodeGeneratorFactory.DbmlGenerator.GetType(type, dbmlCanBeNull == true);
                bool flag = TypeSystem.IsNullableType(type) || !type.IsValueType;
                if (dbmlCanBeNull == false && flag)
                {
                    return new bool?(false);
                }
                if (dbmlCanBeNull == true && !flag)
                {
                    return new bool?(true);
                }
                return null;
            }

            private static void InitializeTypeMappings()
            {
                if (CodeGeneratorFactory.DbmlGenerator.typeMappings != null)
                {
                    return;
                }
                CodeGeneratorFactory.DbmlGenerator.typeMappings = new Dictionary<string, System.Type>();
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("byte", typeof(byte));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("sbyte", typeof(sbyte));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("short", typeof(short));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("int", typeof(int));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("long", typeof(long));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("float", typeof(float));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("double", typeof(double));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("bool", typeof(bool));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("char", typeof(char));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("decimal", typeof(decimal));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("object", typeof(object));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("string", typeof(string));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Byte", typeof(byte));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("SByte", typeof(sbyte));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Short", typeof(short));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Integer", typeof(int));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Long", typeof(long));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Single", typeof(float));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Double", typeof(double));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Boolean", typeof(bool));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Char", typeof(char));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Decimal", typeof(decimal));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("Object", typeof(object));
                CodeGeneratorFactory.DbmlGenerator.typeMappings.Add("String", typeof(string));
            }

            internal static string GetPartialMethodIndentation(bool inNamespace)
            {
                if (inNamespace)
                {
                    return "    ";
                }
                return "  ";
            }
        }

        internal class ModelInfo
        {
            private Dictionary<Association, Association> associationPartners;

            private Dictionary<string, Table> typeToTable;

            internal ModelInfo(Database db)
            {
                this.associationPartners = Dbml.GetAssociationPairs(db);
                this.typeToTable = Dbml.GetTablesByTypeName(db);
            }

            internal Association GetOtherAssociation(Association association)
            {
                return this.associationPartners[association];
            }

            internal bool IsOneWay(Association association)
            {
                return this.associationPartners[association] == null;
            }

            internal Table TableFromTypeName(string typeName)
            {
                Table result = null;
                this.typeToTable.TryGetValue(typeName, out result);
                return result;
            }

            internal bool IsPrimaryKeyOfType(string typeName, string[] columns)
            {
                Table table = this.TableFromTypeName(typeName);
                return table != null && table.Type != null && Dbml.IsPrimaryKeyOfType(table.Type, columns);
            }
        }

        internal static IMappingCodeGenerator CreateCSharpGenerator()
        {
            return CodeGeneratorFactory.DbmlGenerator.Create(new CSharpCodeProvider());
        }

        internal static IMappingCodeGenerator CreateVisualBasicGenerator()
        {
            return CodeGeneratorFactory.DbmlGenerator.Create(new VBCodeProvider());
        }
    }
}
