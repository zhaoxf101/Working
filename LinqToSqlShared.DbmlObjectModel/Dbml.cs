using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace LinqToSqlShared.DbmlObjectModel
{
	internal static class Dbml
	{
		private class VerifyDbml : DbmlVisitor
		{
			private string message;

			private Dictionary<string, Type> dbTypes = new Dictionary<string, Type>();

			internal VerifyDbml(string message)
			{
				this.message = message;
			}

			internal override Type VisitType(Type type)
			{
				Type type2;
				if (this.dbTypes.TryGetValue(type.Name, out type2))
				{
					if (type2 != type)
					{
						throw Error.Bug(string.Concat(new string[]
						{
							" (",
							this.message,
							"): Type with name ",
							type.Name,
							" has multiple Type instances in DBML."
						}));
					}
				}
				else
				{
					this.dbTypes.Add(type.Name, type);
				}
				return base.VisitType(type);
			}
		}

		private class DbmlSerializer : DbmlVisitor
		{
			private class IdsCreater : DbmlVisitor
			{
				private Dictionary<Type, string> typeIds = new Dictionary<Type, string>();

				private Dictionary<Function, string> functionIds = new Dictionary<Function, string>();

				private int currentTypeId = 1;

				private int currentFunctionId = 1;

				internal void GetTypeIds(Database db, ref Dictionary<Type, string> tyIds, ref Dictionary<Function, string> funIds)
				{
					this.VisitDatabase(db);
					tyIds = this.typeIds;
					funIds = this.functionIds;
				}

				internal override Type VisitType(Type type)
				{
					if (type == null)
					{
						return type;
					}
					if (this.typeIds.ContainsKey(type))
					{
						if (this.typeIds[type] == null)
						{
							this.typeIds[type] = "ID" + this.currentTypeId.ToString(CultureInfo.InvariantCulture);
							this.currentTypeId++;
						}
					}
					else
					{
						this.typeIds.Add(type, null);
					}
					return base.VisitType(type);
				}

				internal override TableFunction VisitTableFunction(TableFunction tf)
				{
					if (tf == null)
					{
						return null;
					}
					string value = "FunctionId" + this.currentFunctionId.ToString(CultureInfo.InvariantCulture);
					if (tf.MappedFunction != null && !this.functionIds.ContainsKey(tf.MappedFunction))
					{
						this.functionIds.Add(tf.MappedFunction, value);
						this.currentFunctionId++;
					}
					return tf;
				}
			}

			private XmlWriter writer;

			private Dbml.DbmlSerializer.IdsCreater creater = new Dbml.DbmlSerializer.IdsCreater();

			private Dictionary<Type, string> typeIds;

			private List<Type> pocessedTypes = new List<Type>();

			private string currentTableFun;

			private bool isFunctionElementType;

			private Dictionary<Function, string> functionIds;

			private bool isSubType;

			internal string DbmlToString(Database db, Encoding encoding)
			{
				string result;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (StreamWriter streamWriter = new StreamWriter(memoryStream, encoding))
					{
						this.writer = XmlWriter.Create(streamWriter, new XmlWriterSettings
						{
							Indent = true,
							Encoding = encoding
						});
						this.VisitDatabase(db);
						this.writer.Flush();
						this.writer.Close();
						using (StreamReader streamReader = new StreamReader(memoryStream, encoding))
						{
							memoryStream.Position = 0L;
							result = streamReader.ReadToEnd();
						}
					}
				}
				return result;
			}

			internal void DbmlToFile(Database db, string filename, Encoding encoding)
			{
				this.writer = XmlWriter.Create(filename, new XmlWriterSettings
				{
					Indent = true,
					Encoding = encoding
				});
				this.VisitDatabase(db);
				this.writer.Flush();
				this.writer.Close();
			}

			private static string ToXmlBooleanString(bool? b)
			{
				if (!b.HasValue)
				{
					return null;
				}
				return b.ToString().ToLower(CultureInfo.InvariantCulture);
			}

			internal override Database VisitDatabase(Database db)
			{
				if (db == null)
				{
					return null;
				}
				this.creater.GetTypeIds(db, ref this.typeIds, ref this.functionIds);
				this.writer.WriteStartElement("Database", "http://schemas.microsoft.com/linqtosql/dbml/2007");
				if (db.Name != null)
				{
					this.writer.WriteAttributeString("Name", db.Name);
				}
				if (db.EntityNamespace != null)
				{
					this.writer.WriteAttributeString("EntityNamespace", db.EntityNamespace);
				}
				if (db.ContextNamespace != null)
				{
					this.writer.WriteAttributeString("ContextNamespace", db.ContextNamespace);
				}
				if (db.Class != null)
				{
					this.writer.WriteAttributeString("Class", db.Class);
				}
				if (db.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", db.AccessModifier.ToString());
				}
				if (db.Modifier.HasValue)
				{
					this.writer.WriteAttributeString("Modifier", db.Modifier.ToString());
				}
				if (db.BaseType != null)
				{
					this.writer.WriteAttributeString("BaseType", db.BaseType);
				}
				if (db.Provider != null)
				{
					this.writer.WriteAttributeString("Provider", db.Provider);
				}
				if (db.ExternalMapping.HasValue)
				{
					this.writer.WriteAttributeString("ExternalMapping", Dbml.DbmlSerializer.ToXmlBooleanString(db.ExternalMapping));
				}
				if (db.Serialization.HasValue)
				{
					this.writer.WriteAttributeString("Serialization", db.Serialization.ToString());
				}
				if (db.EntityBase != null)
				{
					this.writer.WriteAttributeString("EntityBase", db.EntityBase);
				}
				base.VisitDatabase(db);
				this.writer.WriteEndElement();
				return db;
			}

			internal override Table VisitTable(Table table)
			{
				if (table == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Table");
				if (table.Name != null)
				{
					this.writer.WriteAttributeString("Name", table.Name);
				}
				if (table.Member != null)
				{
					this.writer.WriteAttributeString("Member", table.Member);
				}
				if (table.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", table.AccessModifier.ToString());
				}
				if (table.Modifier.HasValue)
				{
					this.writer.WriteAttributeString("Modifier", table.Modifier.ToString());
				}
				this.isSubType = false;
				this.VisitType(table.Type);
				this.currentTableFun = "InsertFunction";
				this.VisitTableFunction(table.InsertFunction);
				this.currentTableFun = "UpdateFunction";
				this.VisitTableFunction(table.UpdateFunction);
				this.currentTableFun = "DeleteFunction";
				this.VisitTableFunction(table.DeleteFunction);
				this.writer.WriteEndElement();
				return table;
			}

			internal override Function VisitFunction(Function f)
			{
				if (f == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Function");
				string text;
				this.functionIds.TryGetValue(f, out text);
				if (text != null)
				{
					this.writer.WriteAttributeString("Id", text);
				}
				if (f.Name != null)
				{
					this.writer.WriteAttributeString("Name", f.Name);
				}
				if (f.Method != null)
				{
					this.writer.WriteAttributeString("Method", f.Method);
				}
				if (f.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", f.AccessModifier.ToString());
				}
				if (f.Modifier.HasValue)
				{
					this.writer.WriteAttributeString("Modifier", f.Modifier.ToString());
				}
				if (f.HasMultipleResults.HasValue)
				{
					this.writer.WriteAttributeString("HasMultipleResults", Dbml.DbmlSerializer.ToXmlBooleanString(f.HasMultipleResults));
				}
				if (f.IsComposable.HasValue)
				{
					this.writer.WriteAttributeString("IsComposable", Dbml.DbmlSerializer.ToXmlBooleanString(f.IsComposable));
				}
				this.isFunctionElementType = true;
				this.isSubType = false;
				base.VisitFunction(f);
				this.isFunctionElementType = false;
				this.writer.WriteEndElement();
				return f;
			}

			internal override TableFunction VisitTableFunction(TableFunction tf)
			{
				if (tf == null)
				{
					return null;
				}
				this.writer.WriteStartElement(this.currentTableFun);
				string text;
				this.functionIds.TryGetValue(tf.MappedFunction, out text);
				if (text != null)
				{
					this.writer.WriteAttributeString("FunctionId", text);
				}
				if (tf.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", tf.AccessModifier.ToString());
				}
				base.VisitTableFunction(tf);
				this.writer.WriteEndElement();
				return tf;
			}

			internal override Type VisitType(Type type)
			{
				string localName;
				if (this.isFunctionElementType && !this.isSubType)
				{
					localName = "ElementType";
				}
				else
				{
					localName = "Type";
				}
				if (type == null)
				{
					return null;
				}
				if (this.typeIds[type] != null && this.pocessedTypes.Contains(type))
				{
					string value = this.typeIds[type];
					this.writer.WriteStartElement(localName);
					this.writer.WriteAttributeString("IdRef", value);
				}
				else
				{
					this.pocessedTypes.Add(type);
					this.writer.WriteStartElement(localName);
					if (type.Name != null)
					{
						this.writer.WriteAttributeString("Name", type.Name);
					}
					if (this.typeIds[type] != null)
					{
						this.writer.WriteAttributeString("Id", this.typeIds[type]);
					}
					if (type.InheritanceCode != null)
					{
						this.writer.WriteAttributeString("InheritanceCode", type.InheritanceCode);
					}
					if (type.IsInheritanceDefault.HasValue)
					{
						this.writer.WriteAttributeString("IsInheritanceDefault", Dbml.DbmlSerializer.ToXmlBooleanString(type.IsInheritanceDefault));
					}
					if (type.AccessModifier.HasValue)
					{
						this.writer.WriteAttributeString("AccessModifier", type.AccessModifier.ToString());
					}
					if (type.Modifier.HasValue)
					{
						this.writer.WriteAttributeString("Modifier", type.Modifier.ToString());
					}
					bool flag = this.isSubType;
					this.isSubType = true;
					base.VisitType(type);
					this.isSubType = flag;
				}
				this.writer.WriteEndElement();
				return type;
			}

			internal override Column VisitColumn(Column column)
			{
				if (column == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Column");
				if (column.Name != null)
				{
					this.writer.WriteAttributeString("Name", column.Name);
				}
				if (column.Member != null)
				{
					this.writer.WriteAttributeString("Member", column.Member);
				}
				if (column.Storage != null)
				{
					this.writer.WriteAttributeString("Storage", column.Storage);
				}
				if (column.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", column.AccessModifier.ToString());
				}
				if (column.Modifier.HasValue)
				{
					this.writer.WriteAttributeString("Modifier", column.Modifier.ToString());
				}
				if (column.AutoSync.HasValue)
				{
					this.writer.WriteAttributeString("AutoSync", column.AutoSync.ToString());
				}
				if (column.Type != null)
				{
					this.writer.WriteAttributeString("Type", column.Type);
				}
				if (column.DbType != null)
				{
					this.writer.WriteAttributeString("DbType", column.DbType);
				}
				if (column.IsReadOnly.HasValue)
				{
					this.writer.WriteAttributeString("IsReadOnly", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsReadOnly));
				}
				if (column.IsPrimaryKey.HasValue)
				{
					this.writer.WriteAttributeString("IsPrimaryKey", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsPrimaryKey));
				}
				if (column.IsDbGenerated.HasValue)
				{
					this.writer.WriteAttributeString("IsDbGenerated", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsDbGenerated));
				}
				if (column.CanBeNull.HasValue)
				{
					this.writer.WriteAttributeString("CanBeNull", Dbml.DbmlSerializer.ToXmlBooleanString(column.CanBeNull));
				}
				if (column.UpdateCheck.HasValue)
				{
					this.writer.WriteAttributeString("UpdateCheck", column.UpdateCheck.ToString());
				}
				if (column.IsDiscriminator.HasValue)
				{
					this.writer.WriteAttributeString("IsDiscriminator", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsDiscriminator));
				}
				if (column.Expression != null)
				{
					this.writer.WriteAttributeString("Expression", column.Expression);
				}
				if (column.IsVersion.HasValue)
				{
					this.writer.WriteAttributeString("IsVersion", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsVersion));
				}
				if (column.IsDelayLoaded.HasValue)
				{
					this.writer.WriteAttributeString("IsDelayLoaded", Dbml.DbmlSerializer.ToXmlBooleanString(column.IsDelayLoaded));
				}
				this.writer.WriteEndElement();
				return column;
			}

			internal override Association VisitAssociation(Association association)
			{
				if (association == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Association");
				if (association.Name != null)
				{
					this.writer.WriteAttributeString("Name", association.Name);
				}
				if (association.Member != null)
				{
					this.writer.WriteAttributeString("Member", association.Member);
				}
				if (association.Storage != null)
				{
					this.writer.WriteAttributeString("Storage", association.Storage);
				}
				if (association.AccessModifier.HasValue)
				{
					this.writer.WriteAttributeString("AccessModifier", association.AccessModifier.ToString());
				}
				if (association.Modifier.HasValue)
				{
					this.writer.WriteAttributeString("Modifier", association.Modifier.ToString());
				}
				string value = Dbml.BuildKeyField(association.GetThisKey());
				if (!string.IsNullOrEmpty(value))
				{
					this.writer.WriteAttributeString("ThisKey", value);
				}
				string value2 = Dbml.BuildKeyField(association.GetOtherKey());
				if (!string.IsNullOrEmpty(value2))
				{
					this.writer.WriteAttributeString("OtherKey", value2);
				}
				if (association.Type != null)
				{
					this.writer.WriteAttributeString("Type", association.Type);
				}
				if (association.IsForeignKey.HasValue)
				{
					this.writer.WriteAttributeString("IsForeignKey", Dbml.DbmlSerializer.ToXmlBooleanString(association.IsForeignKey));
				}
				if (association.Cardinality.HasValue)
				{
					this.writer.WriteAttributeString("Cardinality", association.Cardinality.ToString());
				}
				if (association.DeleteRule != null)
				{
					this.writer.WriteAttributeString("DeleteRule", association.DeleteRule);
				}
				if (association.DeleteOnNull.HasValue)
				{
					this.writer.WriteAttributeString("DeleteOnNull", Dbml.DbmlSerializer.ToXmlBooleanString(association.DeleteOnNull));
				}
				this.writer.WriteEndElement();
				return association;
			}

			internal override Return VisitReturn(Return r)
			{
				if (r == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Return");
				if (r.Type != null)
				{
					this.writer.WriteAttributeString("Type", r.Type);
				}
				if (r.DbType != null)
				{
					this.writer.WriteAttributeString("DbType", r.DbType);
				}
				this.writer.WriteEndElement();
				return r;
			}

			internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn r)
			{
				if (r == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Return");
				if (r.Member != null)
				{
					this.writer.WriteAttributeString("Member", r.Member);
				}
				this.writer.WriteEndElement();
				return r;
			}

			internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Argument");
				if (parameter.ParameterName != null)
				{
					this.writer.WriteAttributeString("Parameter", parameter.ParameterName);
				}
				if (parameter.Member != null)
				{
					this.writer.WriteAttributeString("Member", parameter.Member);
				}
				if (parameter.Version.HasValue)
				{
					this.writer.WriteAttributeString("Version", parameter.Version.ToString());
				}
				this.writer.WriteEndElement();
				return parameter;
			}

			internal override Parameter VisitParameter(Parameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Parameter");
				if (parameter.Name != null)
				{
					this.writer.WriteAttributeString("Name", parameter.Name);
				}
				if (parameter.ParameterName != null && parameter.ParameterName != parameter.Name)
				{
					this.writer.WriteAttributeString("Parameter", parameter.ParameterName);
				}
				if (parameter.Type != null)
				{
					this.writer.WriteAttributeString("Type", parameter.Type);
				}
				if (parameter.DbType != null)
				{
					this.writer.WriteAttributeString("DbType", parameter.DbType);
				}
				if (parameter.Direction.HasValue)
				{
					this.writer.WriteAttributeString("Direction", parameter.Direction.ToString());
				}
				this.writer.WriteEndElement();
				return parameter;
			}

			internal override Connection VisitConnection(Connection connection)
			{
				if (connection == null)
				{
					return null;
				}
				this.writer.WriteStartElement("Connection");
				if (connection.Mode.HasValue)
				{
					this.writer.WriteAttributeString("Mode", connection.Mode.ToString());
				}
				if (connection.ConnectionString != null)
				{
					this.writer.WriteAttributeString("ConnectionString", connection.ConnectionString);
				}
				if (connection.SettingsObjectName != null)
				{
					this.writer.WriteAttributeString("SettingsObjectName", connection.SettingsObjectName);
				}
				if (connection.SettingsPropertyName != null)
				{
					this.writer.WriteAttributeString("SettingsPropertyName", connection.SettingsPropertyName);
				}
				if (connection.Provider != null)
				{
					this.writer.WriteAttributeString("Provider", connection.Provider);
				}
				this.writer.WriteEndElement();
				return connection;
			}
		}

		private class DbmlReader
		{
			private Dictionary<string, Type> dbTypes = new Dictionary<string, Type>();

			private Dictionary<string, List<TableFunction>> cudFunctionIds = new Dictionary<string, List<TableFunction>>();

			private List<string> functionIds;

			private Type currentTableType;

			private bool isTableType;

			private List<string> typeNames = new List<string>();

			internal Database FileToDbml(string filename)
			{
				Database database = null;
				using (FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
				{
					this.functionIds = Dbml.DbmlReader.GetAllFunctionIds(fileStream);
					using (XmlTextReader xmlTextReader = new XmlTextReader(fileStream))
					{
						while (xmlTextReader.Read())
						{
							if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "Database" && Dbml.DbmlReader.IsInNamespace(xmlTextReader))
							{
								database = this.ReadDatabase(xmlTextReader);
							}
						}
					}
				}
				if (database == null)
				{
					throw Error.DatabaseNodeNotFound("http://schemas.microsoft.com/linqtosql/dbml/2007");
				}
				return database;
			}

			internal Database StreamToDbml(Stream stream)
			{
				this.functionIds = Dbml.DbmlReader.GetAllFunctionIds(stream);
				Database database = null;
				using (XmlTextReader xmlTextReader = new XmlTextReader(stream))
				{
					while (xmlTextReader.Read())
					{
						if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "Database" && Dbml.DbmlReader.IsInNamespace(xmlTextReader))
						{
							database = this.ReadDatabase(xmlTextReader);
						}
					}
				}
				if (database == null)
				{
					throw Error.DatabaseNodeNotFound("http://schemas.microsoft.com/linqtosql/dbml/2007");
				}
				return database;
			}

			private static List<string> GetAllFunctionIds(Stream stream)
			{
				XmlTextReader xmlTextReader = new XmlTextReader(stream);
				List<string> list = new List<string>();
				while (xmlTextReader.ReadToFollowing("Function"))
				{
					string attribute = xmlTextReader.GetAttribute("Id");
					if (attribute != null)
					{
						list.Add(attribute);
					}
				}
				stream.Position = 0L;
				return list;
			}

			internal static bool IsInNamespace(XmlReader reader)
			{
				return reader.LookupNamespace(reader.Prefix) == "http://schemas.microsoft.com/linqtosql/dbml/2007";
			}

			internal static void ValidateAttributes(XmlTextReader reader, string[] validAttributes)
			{
				if (reader.HasAttributes)
				{
					List<string> list = new List<string>(validAttributes);
					for (int i = 0; i < reader.AttributeCount; i++)
					{
						reader.MoveToAttribute(i);
						if (Dbml.DbmlReader.IsInNamespace(reader) && reader.LocalName != "xmlns" && !list.Contains(reader.LocalName))
						{
							throw Error.SchemaUnrecognizedAttribute(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[]
							{
								reader.Prefix,
								string.IsNullOrEmpty(reader.Prefix) ? "" : ":",
								reader.LocalName
							}), reader.LineNumber);
						}
					}
					reader.MoveToElement();
				}
			}

			private Database ReadDatabase(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Class",
					"EntityNamespace",
					"ContextNamespace",
					"BaseType",
					"Provider",
					"AccessModifier",
					"Modifier",
					"ExternalMapping",
					"EntityBase",
					"Serialization"
				});
				Database database = new Database();
				database.Name = reader.GetAttribute("Name");
				database.Class = reader.GetAttribute("Class");
				if (database.Name == null && database.Class == null)
				{
					throw Error.SchemaOrRequirementViolation("Database", "Name", "Class", reader.LineNumber);
				}
				database.EntityNamespace = reader.GetAttribute("EntityNamespace");
				database.ContextNamespace = reader.GetAttribute("ContextNamespace");
				database.BaseType = reader.GetAttribute("BaseType");
				database.Provider = reader.GetAttribute("Provider");
				database.EntityBase = reader.GetAttribute("EntityBase");
				string attribute = reader.GetAttribute("AccessModifier");
				try
				{
					database.AccessModifier = ((attribute == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute, true)));
					attribute = reader.GetAttribute("Modifier");
					database.Modifier = ((attribute == null) ? null : new ClassModifier?((ClassModifier)Enum.Parse(typeof(ClassModifier), attribute, true)));
					attribute = reader.GetAttribute("ExternalMapping");
					database.ExternalMapping = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					attribute = reader.GetAttribute("Serialization");
					database.Serialization = ((attribute == null) ? null : new SerializationMode?((SerializationMode)Enum.Parse(typeof(SerializationMode), attribute, true)));
				}
				catch (FormatException)
				{
					throw Error.InvalidBooleanAttributeValueViolation(attribute, reader.LineNumber);
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute, reader.LineNumber);
				}
				if (reader.IsEmptyElement)
				{
					return database;
				}
				int num = 0;
				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Whitespace && Dbml.DbmlReader.IsInNamespace(reader))
					{
						XmlNodeType nodeType = reader.NodeType;
						if (nodeType == XmlNodeType.Element)
						{
							string name;
							if ((name = reader.Name) != null)
							{
								if (name == "Table")
								{
									database.Tables.Add(this.ReadTable(reader));
									continue;
								}
								if (name == "Function")
								{
									database.Functions.Add(this.ReadFunction(reader));
									continue;
								}
								if (name == "Connection")
								{
									database.Connection = Dbml.DbmlReader.ReadConnection(reader);
									num++;
									if (num > 1)
									{
										throw Error.ElementMoreThanOnceViolation("Connection", reader.LineNumber);
									}
									continue;
								}
							}
							throw Error.SchemaUnexpectedElementViolation(reader.Name, "Database", reader.LineNumber);
						}
						if (nodeType == XmlNodeType.EndElement)
						{
							return database;
						}
					}
				}
				return database;
			}

			private Table ReadTable(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Member",
					"AccessModifier",
					"Modifier"
				});
				Table table = new Table("", new Type(""));
				string attribute = reader.GetAttribute("Name");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Name", reader.LineNumber);
				}
				table.Name = attribute;
				table.Member = reader.GetAttribute("Member");
				string attribute2 = reader.GetAttribute("AccessModifier");
				try
				{
					table.AccessModifier = ((attribute2 == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute2, true)));
					attribute2 = reader.GetAttribute("Modifier");
					table.Modifier = ((attribute2 == null) ? null : new MemberModifier?((MemberModifier)Enum.Parse(typeof(MemberModifier), attribute2, true)));
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute2, reader.LineNumber);
				}
				if (reader.IsEmptyElement)
				{
					return table;
				}
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Whitespace && Dbml.DbmlReader.IsInNamespace(reader))
					{
						XmlNodeType nodeType = reader.NodeType;
						if (nodeType == XmlNodeType.Element)
						{
							string name;
							if ((name = reader.Name) != null)
							{
								if (!(name == "Type"))
								{
									if (!(name == "InsertFunction"))
									{
										if (!(name == "UpdateFunction"))
										{
											if (name == "DeleteFunction")
											{
												table.DeleteFunction = this.ReadTableFunction(reader);
												num4++;
												if (num4 > 1)
												{
													throw Error.ElementMoreThanOnceViolation("DeleteFunction", reader.LineNumber);
												}
												continue;
											}
										}
										else
										{
											table.UpdateFunction = this.ReadTableFunction(reader);
											num3++;
											if (num3 > 1)
											{
												throw Error.ElementMoreThanOnceViolation("UpdateFunction", reader.LineNumber);
											}
											continue;
										}
									}
									else
									{
										table.InsertFunction = this.ReadTableFunction(reader);
										num2++;
										if (num2 > 1)
										{
											throw Error.ElementMoreThanOnceViolation("InsertFunction", reader.LineNumber);
										}
										continue;
									}
								}
								else
								{
									this.isTableType = true;
									table.Type = this.ReadType(reader);
									num++;
									if (num > 1)
									{
										throw Error.ElementMoreThanOnceViolation("Type", reader.LineNumber);
									}
									continue;
								}
							}
							throw Error.SchemaUnexpectedElementViolation(reader.Name, "Table", reader.LineNumber);
						}
						if (nodeType == XmlNodeType.EndElement)
						{
							if (num == 0)
							{
								throw Error.RequiredElementMissingViolation("Type", reader.LineNumber);
							}
							return table;
						}
					}
				}
				return table;
			}

			private Function ReadFunction(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Method",
					"Id",
					"AccessModifier",
					"Modifier",
					"HasMultipleResults",
					"IsComposable"
				});
				Function function = new Function("");
				this.currentTableType = null;
				string attribute = reader.GetAttribute("Name");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Name", reader.LineNumber);
				}
				function.Name = attribute;
				function.Method = reader.GetAttribute("Method");
				string attribute2 = reader.GetAttribute("Id");
				if (!string.IsNullOrEmpty(attribute2) && this.cudFunctionIds.ContainsKey(attribute2))
				{
					foreach (TableFunction current in this.cudFunctionIds[attribute2])
					{
						current.MappedFunction = function;
					}
				}
				string attribute3 = reader.GetAttribute("AccessModifier");
				try
				{
					function.AccessModifier = ((attribute3 == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute3, true)));
					attribute3 = reader.GetAttribute("Modifier");
					function.Modifier = ((attribute3 == null) ? null : new MemberModifier?((MemberModifier)Enum.Parse(typeof(MemberModifier), attribute3, true)));
					attribute3 = reader.GetAttribute("HasMultipleResults");
					function.HasMultipleResults = ((attribute3 == null) ? null : new bool?(bool.Parse(attribute3)));
					attribute3 = reader.GetAttribute("IsComposable");
					function.IsComposable = ((attribute3 == null) ? null : new bool?(bool.Parse(attribute3)));
				}
				catch (FormatException)
				{
					throw Error.InvalidBooleanAttributeValueViolation(attribute3, reader.LineNumber);
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute3, reader.LineNumber);
				}
				if (reader.IsEmptyElement)
				{
					return function;
				}
				int num = 0;
				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Whitespace && Dbml.DbmlReader.IsInNamespace(reader))
					{
						XmlNodeType nodeType = reader.NodeType;
						if (nodeType == XmlNodeType.Element)
						{
							string name;
							if ((name = reader.Name) != null)
							{
								if (name == "Parameter")
								{
									function.Parameters.Add(Dbml.DbmlReader.ReadParameter(reader));
									continue;
								}
								if (name == "ElementType")
								{
									function.Types.Add(this.ReadType(reader));
									continue;
								}
								if (name == "Return")
								{
									function.Return = Dbml.DbmlReader.ReadReturn(reader);
									num++;
									if (num > 1)
									{
										throw Error.ElementMoreThanOnceViolation("Return", reader.LineNumber);
									}
									continue;
								}
							}
							throw Error.SchemaUnexpectedElementViolation(reader.Name, "Function", reader.LineNumber);
						}
						if (nodeType == XmlNodeType.EndElement)
						{
							return function;
						}
					}
				}
				return function;
			}

			private TableFunction ReadTableFunction(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"FunctionId",
					"AccessModifier"
				});
				TableFunction tableFunction = new TableFunction();
				string attribute = reader.GetAttribute("FunctionId");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("FunctionId", reader.LineNumber);
				}
				if (!this.functionIds.Contains(attribute))
				{
					throw Error.SchemaInvalidIdRefToNonexistentId("TableFunction", "FunctionId", attribute, reader.LineNumber);
				}
				if (this.cudFunctionIds.ContainsKey(attribute))
				{
					this.cudFunctionIds[attribute].Add(tableFunction);
				}
				else
				{
					List<TableFunction> list = new List<TableFunction>();
					list.Add(tableFunction);
					this.cudFunctionIds.Add(attribute, list);
				}
				string attribute2 = reader.GetAttribute("AccessModifier");
				try
				{
					tableFunction.AccessModifier = ((attribute2 == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute2, true)));
				}
				catch (FormatException)
				{
					throw Error.InvalidBooleanAttributeValueViolation(attribute2, reader.LineNumber);
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute2, reader.LineNumber);
				}
				if (reader.IsEmptyElement)
				{
					return tableFunction;
				}
				int num = 0;
				while (reader.Read())
				{
					if (reader.NodeType != XmlNodeType.Whitespace && Dbml.DbmlReader.IsInNamespace(reader))
					{
						XmlNodeType nodeType = reader.NodeType;
						if (nodeType == XmlNodeType.Element)
						{
							string name;
							if ((name = reader.Name) != null)
							{
								if (name == "Argument")
								{
									tableFunction.Arguments.Add(Dbml.DbmlReader.ReadTableFunctionParameter(reader));
									continue;
								}
								if (name == "Return")
								{
									tableFunction.Return = Dbml.DbmlReader.ReadTableFunctionReturn(reader);
									num++;
									if (num > 1)
									{
										throw Error.ElementMoreThanOnceViolation("Return", reader.LineNumber);
									}
									continue;
								}
							}
							throw Error.SchemaUnexpectedElementViolation(reader.Name, "TableFunction", reader.LineNumber);
						}
						if (nodeType == XmlNodeType.EndElement)
						{
							return tableFunction;
						}
					}
				}
				return tableFunction;
			}

			private static bool HasOtherTypeAttributes(XmlTextReader reader)
			{
				return reader.GetAttribute("Name") != null || reader.GetAttribute("InheritanceCode") != null || reader.GetAttribute("IsInheritanceDefault") != null || reader.GetAttribute("AccessModifier") != null || reader.GetAttribute("Modifier") != null || !reader.IsEmptyElement;
			}

			private static bool HasInheritanceRelationship(Type root, Type child)
			{
				if (root == null)
				{
					return false;
				}
				if (root == child)
				{
					return true;
				}
				foreach (Type current in root.SubTypes)
				{
					if (current == child || Dbml.DbmlReader.HasInheritanceRelationship(current, child))
					{
						return true;
					}
				}
				return false;
			}

			private Type ReadType(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"IdRef",
					"Name",
					"Id",
					"InheritanceCode",
					"IsInheritanceDefault",
					"AccessModifier",
					"Modifier"
				});
				string attribute = reader.GetAttribute("IdRef");
				if (attribute != null)
				{
					if (Dbml.DbmlReader.HasOtherTypeAttributes(reader))
					{
						throw Error.SchemaUnexpectedAdditionalAttributeViolation("IdRef", "Type", reader.LineNumber);
					}
					if (!this.dbTypes.ContainsKey(attribute))
					{
						throw Error.SchemaInvalidIdRefToNonexistentId("Type", "IdRef", attribute, reader.LineNumber);
					}
					Type type = this.dbTypes[attribute];
					if (!this.isTableType && Dbml.DbmlReader.HasInheritanceRelationship(this.currentTableType, type))
					{
						throw Error.SchemaRecursiveTypeReference(attribute, type.Name, reader.LineNumber);
					}
					return type;
				}
				else
				{
					Type type2 = new Type("");
					if (this.isTableType)
					{
						this.currentTableType = type2;
						this.isTableType = false;
					}
					string attribute2 = reader.GetAttribute("Name");
					if (attribute2 == null)
					{
						throw Error.RequiredAttributeMissingViolation("Name", reader.LineNumber);
					}
					if (this.typeNames.Contains(attribute2))
					{
						throw Error.TypeNameNotUnique(attribute2, reader.LineNumber);
					}
					this.typeNames.Add(attribute2);
					type2.Name = attribute2;
					string attribute3 = reader.GetAttribute("Id");
					if (attribute3 != null && attribute3.Length != 0)
					{
						if (this.dbTypes.ContainsKey(attribute3))
						{
							throw Error.SchemaDuplicateIdViolation("IdRef", attribute3, reader.LineNumber);
						}
						this.dbTypes.Add(attribute3, type2);
					}
					type2.InheritanceCode = reader.GetAttribute("InheritanceCode");
					string attribute4 = reader.GetAttribute("IsInheritanceDefault");
					try
					{
						type2.IsInheritanceDefault = ((attribute4 == null) ? null : new bool?(bool.Parse(attribute4)));
						attribute4 = reader.GetAttribute("AccessModifier");
						type2.AccessModifier = ((attribute4 == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute4, true)));
						attribute4 = reader.GetAttribute("Modifier");
						type2.Modifier = ((attribute4 == null) ? null : new ClassModifier?((ClassModifier)Enum.Parse(typeof(ClassModifier), attribute4, true)));
					}
					catch (FormatException)
					{
						throw Error.InvalidBooleanAttributeValueViolation(attribute4, reader.LineNumber);
					}
					catch (ArgumentException)
					{
						throw Error.InvalidEnumAttributeValueViolation(attribute4, reader.LineNumber);
					}
					if (reader.IsEmptyElement)
					{
						return type2;
					}
					while (reader.Read())
					{
						if (reader.NodeType != XmlNodeType.Whitespace && Dbml.DbmlReader.IsInNamespace(reader))
						{
							XmlNodeType nodeType = reader.NodeType;
							if (nodeType == XmlNodeType.Element)
							{
								string name;
								if ((name = reader.Name) != null)
								{
									if (name == "Column")
									{
										type2.Columns.Add(Dbml.DbmlReader.ReadColumn(reader));
										continue;
									}
									if (name == "Association")
									{
										type2.Associations.Add(Dbml.DbmlReader.ReadAssociation(reader));
										continue;
									}
									if (name == "Type")
									{
										type2.SubTypes.Add(this.ReadType(reader));
										continue;
									}
								}
								throw Error.SchemaUnexpectedElementViolation(reader.Name, "Type", reader.LineNumber);
							}
							if (nodeType == XmlNodeType.EndElement)
							{
								return type2;
							}
						}
					}
					return type2;
				}
			}

			private static Column ReadColumn(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Type",
					"Member",
					"Storage",
					"AccessModifier",
					"Modifier",
					"AutoSync",
					"IsDbGenerated",
					"IsReadOnly",
					"IsPrimaryKey",
					"CanBeNull",
					"UpdateCheck",
					"Expression",
					"IsDiscriminator",
					"IsVersion",
					"IsDelayLoaded",
					"DbType"
				});
				Column column = new Column("");
				column.Name = reader.GetAttribute("Name");
				column.Member = reader.GetAttribute("Member");
				column.Storage = reader.GetAttribute("Storage");
				if (column.Name == null && column.Member == null)
				{
					throw Error.SchemaOrRequirementViolation("Column", "Name", "Member", reader.LineNumber);
				}
				string attribute = reader.GetAttribute("AccessModifier");
				try
				{
					column.AccessModifier = ((attribute == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute, true)));
					attribute = reader.GetAttribute("Modifier");
					column.Modifier = ((attribute == null) ? null : new MemberModifier?((MemberModifier)Enum.Parse(typeof(MemberModifier), attribute, true)));
					attribute = reader.GetAttribute("AutoSync");
					column.AutoSync = ((attribute == null) ? null : new AutoSync?((AutoSync)Enum.Parse(typeof(AutoSync), attribute, true)));
					attribute = reader.GetAttribute("IsDbGenerated");
					column.IsDbGenerated = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					attribute = reader.GetAttribute("IsReadOnly");
					column.IsReadOnly = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					attribute = reader.GetAttribute("IsPrimaryKey");
					column.IsPrimaryKey = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					attribute = reader.GetAttribute("CanBeNull");
					column.CanBeNull = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					column.UpdateCheck = (((attribute = reader.GetAttribute("UpdateCheck")) == null) ? null : new UpdateCheck?((UpdateCheck)Enum.Parse(typeof(UpdateCheck), attribute, true)));
					attribute = reader.GetAttribute("IsDiscriminator");
					column.IsDiscriminator = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					column.Expression = reader.GetAttribute("Expression");
					attribute = reader.GetAttribute("IsVersion");
					column.IsVersion = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
					attribute = reader.GetAttribute("IsDelayLoaded");
					column.IsDelayLoaded = ((attribute == null) ? null : new bool?(bool.Parse(attribute)));
				}
				catch (FormatException)
				{
					throw Error.InvalidBooleanAttributeValueViolation(attribute, reader.LineNumber);
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute, reader.LineNumber);
				}
				string attribute2 = reader.GetAttribute("Type");
				if (attribute2 == null)
				{
					throw Error.RequiredAttributeMissingViolation("Type", reader.LineNumber);
				}
				column.Type = attribute2;
				column.DbType = reader.GetAttribute("DbType");
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return column;
			}

			private static Association ReadAssociation(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Type",
					"Member",
					"Storage",
					"AccessModifier",
					"Modifier",
					"IsForeignKey",
					"Cardinality",
					"DeleteOnNull",
					"ThisKey",
					"OtherKey",
					"DeleteRule"
				});
				Association association = new Association("");
				string attribute = reader.GetAttribute("Name");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Name", reader.LineNumber);
				}
				if (reader.GetAttribute("Type") == null)
				{
					throw Error.RequiredAttributeMissingViolation("Type", reader.LineNumber);
				}
				association.Name = attribute;
				association.Member = reader.GetAttribute("Member");
				if (reader.GetAttribute("Member") == null)
				{
					throw Error.RequiredAttributeMissingViolation("Member", reader.LineNumber);
				}
				association.Storage = reader.GetAttribute("Storage");
				string attribute2 = reader.GetAttribute("AccessModifier");
				try
				{
					association.AccessModifier = ((attribute2 == null) ? null : new AccessModifier?((AccessModifier)Enum.Parse(typeof(AccessModifier), attribute2, true)));
					attribute2 = reader.GetAttribute("Modifier");
					association.Modifier = ((attribute2 == null) ? null : new MemberModifier?((MemberModifier)Enum.Parse(typeof(MemberModifier), attribute2, true)));
					attribute2 = reader.GetAttribute("IsForeignKey");
					association.IsForeignKey = ((attribute2 == null) ? null : new bool?(bool.Parse(attribute2)));
					attribute2 = reader.GetAttribute("Cardinality");
					association.Cardinality = ((attribute2 == null) ? null : new Cardinality?((Cardinality)Enum.Parse(typeof(Cardinality), attribute2, true)));
					attribute2 = reader.GetAttribute("DeleteOnNull");
					association.DeleteOnNull = ((attribute2 == null) ? null : new bool?(bool.Parse(attribute2)));
				}
				catch (FormatException)
				{
					throw Error.InvalidBooleanAttributeValueViolation(attribute2, reader.LineNumber);
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute2, reader.LineNumber);
				}
				association.SetThisKey(Dbml.ParseKeyField(reader.GetAttribute("ThisKey")));
				association.Type = reader.GetAttribute("Type");
				association.SetOtherKey(Dbml.ParseKeyField(reader.GetAttribute("OtherKey")));
				association.DeleteRule = reader.GetAttribute("DeleteRule");
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return association;
			}

			private static Return ReadReturn(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Type",
					"DbType"
				});
				Return @return = new Return("");
				string attribute = reader.GetAttribute("Type");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Type", reader.LineNumber);
				}
				@return.Type = attribute;
				@return.DbType = reader.GetAttribute("DbType");
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return @return;
			}

			private static TableFunctionReturn ReadTableFunctionReturn(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Member"
				});
				TableFunctionReturn tableFunctionReturn = new TableFunctionReturn();
				string attribute = reader.GetAttribute("Member");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Member", reader.LineNumber);
				}
				tableFunctionReturn.Member = attribute;
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return tableFunctionReturn;
			}

			private static TableFunctionParameter ReadTableFunctionParameter(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Parameter",
					"Member",
					"Version"
				});
				TableFunctionParameter tableFunctionParameter = new TableFunctionParameter("", "");
				string attribute = reader.GetAttribute("Parameter");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Parameter", reader.LineNumber);
				}
				tableFunctionParameter.ParameterName = attribute;
				attribute = reader.GetAttribute("Member");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Member", reader.LineNumber);
				}
				tableFunctionParameter.Member = attribute;
				attribute = reader.GetAttribute("Version");
				try
				{
					if (attribute == null)
					{
						tableFunctionParameter.Version = null;
					}
					else
					{
						tableFunctionParameter.Version = new Version?((Version)Enum.Parse(typeof(Version), attribute, true));
					}
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute, reader.LineNumber);
				}
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return tableFunctionParameter;
			}

			private static Parameter ReadParameter(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Name",
					"Parameter",
					"Type",
					"DbType",
					"Direction"
				});
				Parameter parameter = new Parameter("", "");
				string attribute = reader.GetAttribute("Name");
				if (attribute == null)
				{
					throw Error.RequiredAttributeMissingViolation("Name", reader.LineNumber);
				}
				parameter.Name = attribute;
				parameter.ParameterName = reader.GetAttribute("Parameter");
				string attribute2 = reader.GetAttribute("Type");
				if (attribute2 == null)
				{
					throw Error.RequiredAttributeMissingViolation("Type", reader.LineNumber);
				}
				parameter.Type = attribute2;
				parameter.DbType = reader.GetAttribute("DbType");
				string attribute3 = reader.GetAttribute("Direction");
				try
				{
					parameter.Direction = ((attribute3 == null) ? null : new ParameterDirection?((ParameterDirection)Enum.Parse(typeof(ParameterDirection), attribute3, true)));
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute3, reader.LineNumber);
				}
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return parameter;
			}

			private static Connection ReadConnection(XmlTextReader reader)
			{
				Dbml.DbmlReader.ValidateAttributes(reader, new string[]
				{
					"Provider",
					"Mode",
					"ConnectionString",
					"SettingsObjectName",
					"SettingsPropertyName"
				});
				string attribute = reader.GetAttribute("Provider");
				Connection connection = new Connection(attribute);
				attribute = reader.GetAttribute("Mode");
				try
				{
					connection.Mode = ((attribute == null) ? null : new ConnectionMode?((ConnectionMode)Enum.Parse(typeof(ConnectionMode), attribute)));
				}
				catch (ArgumentException)
				{
					throw Error.InvalidEnumAttributeValueViolation(attribute, reader.LineNumber);
				}
				connection.ConnectionString = reader.GetAttribute("ConnectionString");
				connection.SettingsObjectName = reader.GetAttribute("SettingsObjectName");
				connection.SettingsPropertyName = reader.GetAttribute("SettingsPropertyName");
				Dbml.DbmlReader.AssertEmptyElement(reader);
				return connection;
			}

			private static void AssertEmptyElement(XmlReader reader)
			{
				if (!reader.IsEmptyElement)
				{
					string name = reader.Name;
					reader.Read();
					if (reader.NodeType != XmlNodeType.EndElement)
					{
						throw Error.SchemaExpectedEmptyElement(name, reader.NodeType, reader.Name);
					}
				}
			}
		}

		private class DbmlDefaultValueAssigner : DbmlVisitor
		{
			private Dictionary<string, Table> typeToTable;

			private Dictionary<Association, Association> associationPartners;

			internal Database AssignDefaultValues(Database db)
			{
				this.associationPartners = Dbml.GetAssociationPairs(db);
				return this.VisitDatabase(db);
			}

			internal override Database VisitDatabase(Database db)
			{
				if (db == null)
				{
					return null;
				}
				if (db.Class == null && db.Name == null)
				{
					if (db.Connection != null)
					{
						db.Class = db.Connection.ConnectionString;
					}
					else
					{
						db.Class = "Context";
					}
				}
				else if (db.Class == null)
				{
					db.Class = db.Name;
				}
				if (!db.AccessModifier.HasValue)
				{
					db.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				if (db.BaseType == null)
				{
					db.BaseType = "System.Data.Linq.DataContext";
				}
				if (!db.ExternalMapping.HasValue)
				{
					db.ExternalMapping = new bool?(false);
				}
				if (!db.Serialization.HasValue)
				{
					db.Serialization = new SerializationMode?(SerializationMode.None);
				}
				this.typeToTable = Dbml.GetTablesByTypeName(db);
				return base.VisitDatabase(db);
			}

			private Table TableFromTypeName(string typeName)
			{
				Table result = null;
				this.typeToTable.TryGetValue(typeName, out result);
				return result;
			}

			private Type TypeFromTypeName(string typeName)
			{
				Table table = this.TableFromTypeName(typeName);
				if (table == null)
				{
					return null;
				}
				return table.Type;
			}

			internal override Table VisitTable(Table table)
			{
				if (table == null)
				{
					return null;
				}
				if (table.Name == null && table.Member != null)
				{
					table.Name = table.Member;
				}
				if (table.Member == null && table.Name != null)
				{
					table.Member = table.Name;
				}
				if (!table.AccessModifier.HasValue)
				{
					table.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				return base.VisitTable(table);
			}

			internal override Function VisitFunction(Function f)
			{
				if (f == null)
				{
					return null;
				}
				if (f.Method == null)
				{
					f.Method = f.Name;
				}
				if (!f.AccessModifier.HasValue)
				{
					f.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				if (!f.HasMultipleResults.HasValue)
				{
					f.HasMultipleResults = new bool?(f.Types.Count > 1);
				}
				if (!f.IsComposable.HasValue)
				{
					f.IsComposable = new bool?(false);
				}
				return base.VisitFunction(f);
			}

			internal override TableFunction VisitTableFunction(TableFunction tf)
			{
				if (tf == null)
				{
					return null;
				}
				if (!tf.AccessModifier.HasValue)
				{
					tf.AccessModifier = new AccessModifier?(AccessModifier.Private);
				}
				this.VisitFunction(tf.MappedFunction);
				return base.VisitTableFunction(tf);
			}

			internal override Type VisitType(Type type)
			{
				if (type == null)
				{
					return null;
				}
				if (!type.IsInheritanceDefault.HasValue)
				{
					type.IsInheritanceDefault = new bool?(false);
				}
				if (!type.AccessModifier.HasValue)
				{
					type.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				bool flag = false;
				using (List<Column>.Enumerator enumerator = type.Columns.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.IsVersion == true)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					using (List<Column>.Enumerator enumerator2 = type.Columns.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Column current = enumerator2.Current;
							if (!current.UpdateCheck.HasValue)
							{
								current.UpdateCheck = new UpdateCheck?(UpdateCheck.Never);
							}
						}
						goto IL_12F;
					}
				}
				foreach (Column current2 in type.Columns)
				{
					if (!current2.UpdateCheck.HasValue)
					{
						current2.UpdateCheck = new UpdateCheck?(UpdateCheck.Always);
					}
				}
				IL_12F:
				foreach (Column current3 in type.Columns)
				{
					this.VisitColumn(current3);
				}
				foreach (Association current4 in type.Associations)
				{
					this.VisitAssociation(current4);
				}
				foreach (Type current5 in type.SubTypes)
				{
					this.VisitType(current5);
				}
				return type;
			}

			internal override Column VisitColumn(Column column)
			{
				if (column == null)
				{
					return null;
				}
				if (column.Name == null && column.Member != null)
				{
					column.Name = column.Member;
				}
				if (column.Member == null && column.Name != null)
				{
					column.Member = column.Name;
				}
				if (column.Storage == null)
				{
					column.Storage = "_" + column.Member;
				}
				if (!column.AccessModifier.HasValue)
				{
					column.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				if (!column.IsPrimaryKey.HasValue)
				{
					column.IsPrimaryKey = new bool?(false);
				}
				if (!column.IsDiscriminator.HasValue)
				{
					column.IsDiscriminator = new bool?(false);
				}
				if (!column.IsVersion.HasValue)
				{
					column.IsVersion = new bool?(false);
				}
				if (!column.IsDelayLoaded.HasValue)
				{
					column.IsDelayLoaded = new bool?(false);
				}
				if (!column.IsDbGenerated.HasValue)
				{
					if (!string.IsNullOrEmpty(column.Expression) || column.IsVersion == true)
					{
						column.IsDbGenerated = new bool?(true);
					}
					else
					{
						column.IsDbGenerated = new bool?(false);
					}
				}
				if (!column.IsReadOnly.HasValue)
				{
					column.IsReadOnly = new bool?(false);
				}
				if (!column.AutoSync.HasValue)
				{
					if (column.IsDbGenerated == true && column.IsPrimaryKey == true)
					{
						column.AutoSync = new AutoSync?(AutoSync.OnInsert);
					}
					else if (column.IsDbGenerated == true)
					{
						column.AutoSync = new AutoSync?(AutoSync.Always);
					}
					else
					{
						column.AutoSync = new AutoSync?(AutoSync.Never);
					}
				}
				if (column.IsReadOnly == true)
				{
					column.UpdateCheck = new UpdateCheck?(UpdateCheck.Never);
				}
				return column;
			}

			internal override Association VisitAssociation(Association association)
			{
				if (association == null)
				{
					return null;
				}
				if (!association.AccessModifier.HasValue)
				{
					association.AccessModifier = new AccessModifier?(AccessModifier.Public);
				}
				if (!association.IsForeignKey.HasValue)
				{
					association.IsForeignKey = new bool?(false);
				}
				if (!association.Cardinality.HasValue)
				{
					if (association.IsForeignKey == true)
					{
						association.Cardinality = new Cardinality?(Cardinality.One);
					}
					else
					{
						association.Cardinality = new Cardinality?(Cardinality.Many);
					}
				}
				if (!association.DeleteOnNull.HasValue)
				{
					association.DeleteOnNull = new bool?(false);
				}
				if (association.Storage == null)
				{
					association.Storage = "_" + association.Member;
				}
				if (association.GetThisKey().Length == 0 && association.IsForeignKey != true)
				{
					Association association2 = this.associationPartners[association];
					if (association2 != null)
					{
						Type type = this.TypeFromTypeName(association2.Type);
						if (type != null)
						{
							association.SetThisKey(Dbml.GetPrimaryKeys(type));
						}
					}
				}
				if (association.GetOtherKey().Length == 0 && association.IsForeignKey == true)
				{
					Type type2 = this.TypeFromTypeName(association.Type);
					if (type2 != null)
					{
						association.SetOtherKey(Dbml.GetPrimaryKeys(type2));
					}
				}
				return association;
			}

			internal override Return VisitReturn(Return r)
			{
				return r;
			}

			internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn r)
			{
				return r;
			}

			internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				if (!parameter.Version.HasValue)
				{
					parameter.Version = new Version?(Version.Current);
				}
				return parameter;
			}

			internal override Parameter VisitParameter(Parameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				if (parameter.ParameterName == null)
				{
					parameter.ParameterName = parameter.Name;
				}
				if (!parameter.Direction.HasValue)
				{
					parameter.Direction = new ParameterDirection?(ParameterDirection.In);
				}
				return parameter;
			}

			internal override Connection VisitConnection(Connection connection)
			{
				if (connection == null)
				{
					return null;
				}
				if (!connection.Mode.HasValue)
				{
					connection.Mode = new ConnectionMode?(ConnectionMode.ConnectionString);
				}
				return connection;
			}
		}

		private class DbmlDefaultValueNullifier : DbmlVisitor
		{
			private Dictionary<Association, Association> associationPartners;

			private Dictionary<string, Table> typeToTable;

			internal Database NullifyDefaultValues(Database db)
			{
				this.associationPartners = Dbml.GetAssociationPairs(db);
				this.typeToTable = Dbml.GetTablesByTypeName(db);
				return this.VisitDatabase(db);
			}

			internal override Database VisitDatabase(Database db)
			{
				if (db == null)
				{
					return null;
				}
				if (db.Class == db.Name)
				{
					db.Class = null;
				}
				if (db.BaseType == "System.Data.Linq.DataContext")
				{
					db.BaseType = null;
				}
				if (db.AccessModifier == AccessModifier.Public)
				{
					db.AccessModifier = null;
				}
				if (db.ExternalMapping == false)
				{
					db.ExternalMapping = null;
				}
				if (db.Serialization == SerializationMode.None)
				{
					db.Serialization = null;
				}
				return base.VisitDatabase(db);
			}

			internal override Table VisitTable(Table table)
			{
				if (table.Name == table.Member)
				{
					table.Member = null;
				}
				if (table.AccessModifier == AccessModifier.Public)
				{
					table.AccessModifier = null;
				}
				return base.VisitTable(table);
			}

			internal override Function VisitFunction(Function f)
			{
				if (f == null)
				{
					return null;
				}
				if (f.Method == f.Name)
				{
					f.Method = null;
				}
				if (f.AccessModifier == AccessModifier.Public)
				{
					f.AccessModifier = null;
				}
				if (f.HasMultipleResults == f.Types.Count > 1)
				{
					f.HasMultipleResults = null;
				}
				if (f.IsComposable == false)
				{
					f.IsComposable = null;
				}
				return base.VisitFunction(f);
			}

			internal override TableFunction VisitTableFunction(TableFunction tf)
			{
				if (tf == null)
				{
					return null;
				}
				if (tf.AccessModifier == AccessModifier.Private)
				{
					tf.AccessModifier = null;
				}
				return base.VisitTableFunction(tf);
			}

			private static void NullifyUpdateCheckOfColumns(Type type)
			{
				bool flag = false;
				using (List<Column>.Enumerator enumerator = type.Columns.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.IsVersion == true)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					using (List<Column>.Enumerator enumerator2 = type.Columns.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Column current = enumerator2.Current;
							if (current.UpdateCheck == UpdateCheck.Never)
							{
								current.UpdateCheck = null;
							}
						}
						return;
					}
				}
				foreach (Column current2 in type.Columns)
				{
					if (current2.IsReadOnly == true)
					{
						if (current2.UpdateCheck == UpdateCheck.Never)
						{
							current2.UpdateCheck = null;
						}
					}
					else if (current2.UpdateCheck == UpdateCheck.Always)
					{
						current2.UpdateCheck = null;
					}
				}
			}

			internal override Type VisitType(Type type)
			{
				if (type == null)
				{
					return null;
				}
				if (type.IsInheritanceDefault == false)
				{
					type.IsInheritanceDefault = null;
				}
				if (type.AccessModifier == AccessModifier.Public)
				{
					type.AccessModifier = null;
				}
				Dbml.DbmlDefaultValueNullifier.NullifyUpdateCheckOfColumns(type);
				return base.VisitType(type);
			}

			internal override Column VisitColumn(Column column)
			{
				if (column == null)
				{
					return null;
				}
				if (column.Storage == "_" + column.Member)
				{
					column.Storage = null;
				}
				if (column.Name == column.Member)
				{
					column.Member = null;
				}
				if (column.AccessModifier == AccessModifier.Public)
				{
					column.AccessModifier = null;
				}
				if (column.AutoSync == AutoSync.OnInsert && column.IsDbGenerated == true && column.IsPrimaryKey == true)
				{
					column.AutoSync = null;
				}
				else if (column.AutoSync == AutoSync.Always && column.IsDbGenerated == true)
				{
					column.AutoSync = null;
				}
				else if (column.AutoSync == AutoSync.Never && column.IsVersion != true && column.IsDbGenerated != true)
				{
					column.AutoSync = null;
				}
				if (column.IsReadOnly == false)
				{
					column.IsReadOnly = null;
				}
				if (!string.IsNullOrEmpty(column.Expression) || column.IsVersion == true)
				{
					if (column.IsDbGenerated == true)
					{
						column.IsDbGenerated = null;
					}
				}
				else if (column.IsDbGenerated == false)
				{
					column.IsDbGenerated = null;
				}
				if (column.IsPrimaryKey == false)
				{
					column.IsPrimaryKey = null;
				}
				if (column.IsDiscriminator == false)
				{
					column.IsDiscriminator = null;
				}
				if (column.IsVersion == false)
				{
					column.IsVersion = null;
				}
				if (column.IsDelayLoaded == false)
				{
					column.IsDelayLoaded = null;
				}
				return column;
			}

			internal override Association VisitAssociation(Association association)
			{
				if (association == null)
				{
					return null;
				}
				if (association.AccessModifier == AccessModifier.Public)
				{
					association.AccessModifier = null;
				}
				if (association.IsForeignKey == false)
				{
					association.IsForeignKey = null;
				}
				if (association.Cardinality == Cardinality.One && association.IsForeignKey == true)
				{
					association.Cardinality = null;
				}
				if (association.DeleteOnNull == false)
				{
					association.DeleteOnNull = null;
				}
				if (association.Cardinality == Cardinality.Many && association.IsForeignKey != true)
				{
					association.Cardinality = null;
				}
				if (association.Member != null && association.Storage == "_" + association.Member)
				{
					association.Storage = null;
				}
				return association;
			}

			internal override Return VisitReturn(Return r)
			{
				return r;
			}

			internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn r)
			{
				return r;
			}

			internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				if (parameter.Version == Version.Current)
				{
					parameter.Version = null;
				}
				return parameter;
			}

			internal override Parameter VisitParameter(Parameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				if (parameter.ParameterName == parameter.Name)
				{
					parameter.ParameterName = null;
				}
				if (parameter.Direction == ParameterDirection.In)
				{
					parameter.Direction = null;
				}
				return parameter;
			}

			internal override Connection VisitConnection(Connection connection)
			{
				if (connection == null)
				{
					return null;
				}
				return connection;
			}
		}

		private class DbmlDuplicator : DbmlVisitor
		{
			private Database originalDb;

			private Dictionary<TableFunction, int> functionReferences;

			private Dictionary<string, Type> processedTypes;

			internal Database DuplicateDatabase(Database db)
			{
				this.functionReferences = new Dictionary<TableFunction, int>();
				this.processedTypes = new Dictionary<string, Type>();
				db = this.VisitDatabase(db);
				return db;
			}

			internal override Database VisitDatabase(Database db)
			{
				if (db == null)
				{
					return null;
				}
				Database database = new Database();
				this.originalDb = db;
				database.Name = db.Name;
				database.EntityNamespace = db.EntityNamespace;
				database.ContextNamespace = db.ContextNamespace;
				database.Class = db.Class;
				database.AccessModifier = db.AccessModifier;
				database.Modifier = db.Modifier;
				database.BaseType = db.BaseType;
				database.Provider = db.Provider;
				database.ExternalMapping = db.ExternalMapping;
				database.Serialization = db.Serialization;
				database.EntityBase = db.EntityBase;
				database.Connection = this.VisitConnection(db.Connection);
				foreach (Table current in db.Tables)
				{
					database.Tables.Add(this.VisitTable(current));
				}
				foreach (Function current2 in db.Functions)
				{
					database.Functions.Add(this.VisitFunction(current2));
				}
				foreach (TableFunction current3 in this.functionReferences.Keys)
				{
					current3.MappedFunction = database.Functions[this.functionReferences[current3]];
				}
				return database;
			}

			internal override Table VisitTable(Table table)
			{
				if (table == null)
				{
					return null;
				}
				return new Table(table.Name, this.VisitType(table.Type))
				{
					Member = table.Member,
					AccessModifier = table.AccessModifier,
					Modifier = table.Modifier,
					Type = this.VisitType(table.Type),
					InsertFunction = this.VisitTableFunction(table.InsertFunction),
					UpdateFunction = this.VisitTableFunction(table.UpdateFunction),
					DeleteFunction = this.VisitTableFunction(table.DeleteFunction)
				};
			}

			internal override Function VisitFunction(Function f)
			{
				if (f == null)
				{
					return null;
				}
				Function function = new Function(f.Name);
				function.Method = f.Method;
				function.AccessModifier = f.AccessModifier;
				function.Modifier = f.Modifier;
				function.HasMultipleResults = f.HasMultipleResults;
				function.IsComposable = f.IsComposable;
				foreach (Parameter current in f.Parameters)
				{
					function.Parameters.Add(this.VisitParameter(current));
				}
				foreach (Type current2 in f.Types)
				{
					function.Types.Add(this.VisitType(current2));
				}
				function.Return = this.VisitReturn(f.Return);
				return function;
			}

			private static int FindReferencedFunctionIndex(Database db, Function function)
			{
				int num = 0;
				foreach (Function current in db.Functions)
				{
					if (function == current)
					{
						return num;
					}
					num++;
				}
				throw new InvalidOperationException();
			}

			internal override TableFunction VisitTableFunction(TableFunction tf)
			{
				if (tf == null)
				{
					return null;
				}
				TableFunction tableFunction = new TableFunction();
				tableFunction.AccessModifier = tf.AccessModifier;
				int value = Dbml.DbmlDuplicator.FindReferencedFunctionIndex(this.originalDb, tf.MappedFunction);
				this.functionReferences.Add(tableFunction, value);
				foreach (TableFunctionParameter current in tf.Arguments)
				{
					tableFunction.Arguments.Add(this.VisitTableFunctionParameter(current));
				}
				tableFunction.Return = this.VisitTableFunctionReturn(tf.Return);
				return tableFunction;
			}

			internal override Type VisitType(Type type)
			{
				if (type == null)
				{
					return null;
				}
				Type type2;
				if (this.processedTypes.TryGetValue(type.Name, out type2))
				{
					return type2;
				}
				type2 = new Type(type.Name);
				this.processedTypes.Add(type.Name, type2);
				type2.InheritanceCode = type.InheritanceCode;
				type2.IsInheritanceDefault = type.IsInheritanceDefault;
				type2.AccessModifier = type.AccessModifier;
				type2.Modifier = type.Modifier;
				foreach (Column current in type.Columns)
				{
					type2.Columns.Add(this.VisitColumn(current));
				}
				foreach (Association current2 in type.Associations)
				{
					type2.Associations.Add(this.VisitAssociation(current2));
				}
				foreach (Type current3 in type.SubTypes)
				{
					type2.SubTypes.Add(this.VisitType(current3));
				}
				return type2;
			}

			internal override Column VisitColumn(Column column)
			{
				if (column == null)
				{
					return null;
				}
				return new Column(column.Type)
				{
					Name = column.Name,
					Member = column.Member,
					Storage = column.Storage,
					AccessModifier = column.AccessModifier,
					Modifier = column.Modifier,
					AutoSync = column.AutoSync,
					DbType = column.DbType,
					IsReadOnly = column.IsReadOnly,
					IsPrimaryKey = column.IsPrimaryKey,
					IsDbGenerated = column.IsDbGenerated,
					CanBeNull = column.CanBeNull,
					UpdateCheck = column.UpdateCheck,
					IsDiscriminator = column.IsDiscriminator,
					Expression = column.Expression,
					IsVersion = column.IsVersion,
					IsDelayLoaded = column.IsDelayLoaded
				};
			}

			internal override Association VisitAssociation(Association association)
			{
				if (association == null)
				{
					return null;
				}
				Association association2 = new Association(association.Name);
				association2.Member = association.Member;
				association2.Storage = association.Storage;
				association2.AccessModifier = association.AccessModifier;
				association2.Modifier = association.Modifier;
				association2.SetThisKey(association.GetThisKey());
				association2.SetOtherKey(association.GetOtherKey());
				association2.IsForeignKey = association.IsForeignKey;
				association2.Cardinality = association.Cardinality;
				association2.DeleteOnNull = association.DeleteOnNull;
				association2.DeleteRule = association.DeleteRule;
				association2.Type = association.Type;
				return association2;
			}

			internal override Return VisitReturn(Return r)
			{
				if (r == null)
				{
					return null;
				}
				return new Return(r.Type)
				{
					DbType = r.DbType
				};
			}

			internal override TableFunctionReturn VisitTableFunctionReturn(TableFunctionReturn r)
			{
				if (r == null)
				{
					return null;
				}
				return new TableFunctionReturn
				{
					Member = r.Member
				};
			}

			internal override TableFunctionParameter VisitTableFunctionParameter(TableFunctionParameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				return new TableFunctionParameter(parameter.ParameterName, parameter.Member)
				{
					Version = parameter.Version
				};
			}

			internal override Parameter VisitParameter(Parameter parameter)
			{
				if (parameter == null)
				{
					return null;
				}
				return new Parameter(parameter.Name, parameter.Type)
				{
					ParameterName = parameter.ParameterName,
					Type = parameter.Type,
					DbType = parameter.DbType,
					Direction = parameter.Direction
				};
			}

			internal override Connection VisitConnection(Connection connection)
			{
				if (connection == null)
				{
					return null;
				}
				return new Connection(connection.Provider)
				{
					Mode = connection.Mode,
					ConnectionString = connection.ConnectionString,
					SettingsObjectName = connection.SettingsObjectName,
					SettingsPropertyName = connection.SettingsPropertyName
				};
			}
		}

		private class PairAssociations : DbmlVisitor
		{
			private enum MatchLevel
			{
				NoMatch,
				ThisTypeAgrees,
				OtherTypeAgrees,
				ForeignNonForeign = 4,
				ThisOtherKeyMatchesOtherThisKey = 8,
				ThisThisKeyMatchisOtherOtherKey = 16,
				MinBar = 3
			}

			private Type currentType;

			private Dictionary<string, List<Association>> associations = new Dictionary<string, List<Association>>();

			private Dictionary<Association, Type> associationTypes = new Dictionary<Association, Type>();

			private Dictionary<Type, bool> seen = new Dictionary<Type, bool>();

			internal static Dictionary<Association, Association> Gather(Database db)
			{
				Dbml.PairAssociations pairAssociations = new Dbml.PairAssociations();
				pairAssociations.VisitDatabase(db);
				Dictionary<Association, Association> dictionary = new Dictionary<Association, Association>();
				foreach (string current in pairAssociations.associations.Keys)
				{
					Association[] array = pairAssociations.associations[current].ToArray();
					int num = array.Length;
					int num2 = 0;
					while (num2 != num)
					{
						int num3 = -1;
						int num4 = -1;
						Dbml.PairAssociations.MatchLevel matchLevel = Dbml.PairAssociations.MatchLevel.NoMatch;
						for (int i = 0; i < num - 1; i++)
						{
							for (int j = i + 1; j < num; j++)
							{
								Association association = array[i];
								Association association2 = array[j];
								if (association != null && association2 != null)
								{
									Dbml.PairAssociations.MatchLevel matchLevel2 = Dbml.PairAssociations.Compare(pairAssociations.associationTypes[association], association, pairAssociations.associationTypes[association2], association2);
									if (matchLevel2 > matchLevel)
									{
										num3 = i;
										num4 = j;
										matchLevel = matchLevel2;
									}
								}
							}
						}
						if ((matchLevel & Dbml.PairAssociations.MatchLevel.MinBar) == Dbml.PairAssociations.MatchLevel.MinBar)
						{
							Association association3 = array[num3];
							Association association4 = array[num4];
							dictionary[association3] = association4;
							dictionary[association4] = association3;
							array[num3] = null;
							array[num4] = null;
							num2 += 2;
						}
						else
						{
							Association[] array2 = array;
							for (int k = 0; k < array2.Length; k++)
							{
								Association association5 = array2[k];
								if (association5 != null)
								{
									dictionary[association5] = null;
									num2++;
								}
							}
						}
					}
				}
				return dictionary;
			}

			internal override Type VisitType(Type type)
			{
				Type result;
				try
				{
					this.currentType = type;
					if (this.seen.ContainsKey(type))
					{
						result = type;
					}
					else
					{
						this.seen.Add(type, true);
						result = base.VisitType(type);
					}
				}
				finally
				{
					this.currentType = type;
				}
				return result;
			}

			internal override Association VisitAssociation(Association association)
			{
				if (!this.associations.ContainsKey(association.Name))
				{
					this.associations[association.Name] = new List<Association>();
				}
				this.associations[association.Name].Add(association);
				this.associationTypes[association] = this.currentType;
				return base.VisitAssociation(association);
			}

			private static Dbml.PairAssociations.MatchLevel Compare(Type thisSideType, Association thisSide, Type otherSideType, Association otherSide)
			{
				Dbml.PairAssociations.MatchLevel matchLevel = Dbml.PairAssociations.MatchLevel.NoMatch;
				if ((thisSide.IsForeignKey == true && otherSide.IsForeignKey != true) || (thisSide.IsForeignKey != true && otherSide.IsForeignKey == true))
				{
					matchLevel = matchLevel;
				}
				if (thisSideType.Name == otherSide.Type)
				{
					matchLevel |= Dbml.PairAssociations.MatchLevel.ThisTypeAgrees;
				}
				if (otherSideType.Name == thisSide.Type)
				{
					matchLevel |= Dbml.PairAssociations.MatchLevel.OtherTypeAgrees;
				}
				if (Dbml.BuildKeyField(thisSide.GetOtherKey()) == Dbml.BuildKeyField(otherSide.GetThisKey()))
				{
					matchLevel |= Dbml.PairAssociations.MatchLevel.ThisOtherKeyMatchesOtherThisKey;
				}
				if (Dbml.BuildKeyField(thisSide.GetThisKey()) == Dbml.BuildKeyField(otherSide.GetOtherKey()))
				{
					matchLevel |= Dbml.PairAssociations.MatchLevel.ThisThisKeyMatchisOtherOtherKey;
				}
				return matchLevel;
			}
		}

		private class TypeTableLookup : DbmlVisitor
		{
			private Table currentTable;

			private Dictionary<string, Table> typeToTable = new Dictionary<string, Table>();

			internal static Dictionary<string, Table> CreateLookup(Database db)
			{
				Dbml.TypeTableLookup typeTableLookup = new Dbml.TypeTableLookup();
				typeTableLookup.VisitDatabase(db);
				return typeTableLookup.typeToTable;
			}

			internal override Table VisitTable(Table table)
			{
				this.currentTable = table;
				Table result;
				try
				{
					result = base.VisitTable(table);
				}
				finally
				{
					this.currentTable = null;
				}
				return result;
			}

			internal override Type VisitType(Type type)
			{
				if (this.currentTable != null)
				{
					this.typeToTable[type.Name] = this.currentTable;
				}
				return base.VisitType(type);
			}
		}

		private static string[] emptyKeys = new string[0];

		internal static Database CopyWithFilledInDefaults(Database db)
		{
			Dbml.DbmlDuplicator dbmlDuplicator = new Dbml.DbmlDuplicator();
			Database db2 = dbmlDuplicator.DuplicateDatabase(db);
			Dbml.DbmlDefaultValueAssigner dbmlDefaultValueAssigner = new Dbml.DbmlDefaultValueAssigner();
			return dbmlDefaultValueAssigner.AssignDefaultValues(db2);
		}

		internal static Database CopyWithNulledOutDefaults(Database db)
		{
			Dbml.DbmlDuplicator dbmlDuplicator = new Dbml.DbmlDuplicator();
			Database database = dbmlDuplicator.DuplicateDatabase(db);
			Dbml.DbmlDefaultValueNullifier dbmlDefaultValueNullifier = new Dbml.DbmlDefaultValueNullifier();
			return dbmlDefaultValueNullifier.NullifyDefaultValues(db);
		}

		internal static void FillInDefaults(Database db)
		{
			Dbml.DbmlDefaultValueAssigner dbmlDefaultValueAssigner = new Dbml.DbmlDefaultValueAssigner();
			dbmlDefaultValueAssigner.AssignDefaultValues(db);
		}

		internal static void NullOutDefaults(Database db)
		{
			Dbml.DbmlDefaultValueNullifier dbmlDefaultValueNullifier = new Dbml.DbmlDefaultValueNullifier();
			dbmlDefaultValueNullifier.NullifyDefaultValues(db);
		}

		internal static Database FromStream(Stream dbmlStream)
		{
			Dbml.DbmlReader dbmlReader = new Dbml.DbmlReader();
			return dbmlReader.StreamToDbml(dbmlStream);
		}

		internal static Database FromFile(string dbmlFile)
		{
			Dbml.DbmlReader dbmlReader = new Dbml.DbmlReader();
			return dbmlReader.FileToDbml(dbmlFile);
		}

		internal static string ToText(Database db)
		{
			return Dbml.ToText(db, Encoding.UTF8);
		}

		internal static string ToText(Database db, Encoding encoding)
		{
			Dbml.DbmlSerializer dbmlSerializer = new Dbml.DbmlSerializer();
			return dbmlSerializer.DbmlToString(db, encoding);
		}

		internal static void ToFile(Database db, string dbmlFile)
		{
			Dbml.ToFile(db, dbmlFile, Encoding.UTF8);
		}

		internal static void ToFile(Database db, string dbmlFile, Encoding encoding)
		{
			Dbml.DbmlSerializer dbmlSerializer = new Dbml.DbmlSerializer();
			dbmlSerializer.DbmlToFile(db, dbmlFile, encoding);
		}

		internal static Database Duplicate(Database db)
		{
			Dbml.DbmlDuplicator dbmlDuplicator = new Dbml.DbmlDuplicator();
			db = dbmlDuplicator.DuplicateDatabase(db);
			return db;
		}

		internal static string[] GetPrimaryKeys(Type type)
		{
			List<string> list = new List<string>();
			foreach (Column current in type.Columns)
			{
				if (current.IsPrimaryKey == true)
				{
					list.Add(current.Name);
				}
			}
			return list.ToArray();
		}

		internal static string[] ParseKeyField(string keyField)
		{
			if (string.IsNullOrEmpty(keyField))
			{
				return Dbml.emptyKeys;
			}
			return keyField.Split(new char[]
			{
				','
			});
		}

		internal static string BuildKeyField(string[] columnNames)
		{
			if (columnNames == null)
			{
				return string.Empty;
			}
			return string.Join(",", columnNames);
		}

		internal static bool IsPrimaryKeyOfType(Type type, string[] columns)
		{
			int num = 0;
			int num2 = 0;
			foreach (Column current in type.Columns)
			{
				if (current.IsPrimaryKey == true)
				{
					num2++;
					for (int i = 0; i < columns.Length; i++)
					{
						string b = columns[i];
						if (current.Name == b)
						{
							num++;
							break;
						}
					}
					if (num2 != num)
					{
						return false;
					}
				}
			}
			return columns.Length == num;
		}

		internal static bool HasPrimaryKey(Type type)
		{
			using (List<Column>.Enumerator enumerator = type.Columns.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsPrimaryKey == true)
					{
						return true;
					}
				}
			}
			return false;
		}

		[Conditional("DEBUG")]
		internal static void Verify(Database db, string message)
		{
			new Dbml.VerifyDbml(message).VisitDatabase(db);
		}

		internal static Dictionary<Association, Association> GetAssociationPairs(Database db)
		{
			return Dbml.PairAssociations.Gather(db);
		}

		internal static Dictionary<string, Table> GetTablesByTypeName(Database db)
		{
			return Dbml.TypeTableLookup.CreateLookup(db);
		}
	}
}
