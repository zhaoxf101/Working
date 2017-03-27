using LinqToSqlShared.Common;
using LinqToSqlShared.DbmlObjectModel;
using LinqToSqlShared.Generator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace SqlMetal
{
	internal static class SqlMetalMain
	{
		private static bool hasError;

		private static void DisplayMessage(Severity severity, string desc)
		{
			if (severity == Severity.Information)
			{
				Console.WriteLine(desc);
				return;
			}
			Console.WriteLine("{0} : {1}", SqlMetalMain.GetSeverityString(severity), desc);
			if (severity == Severity.Error)
			{
				SqlMetalMain.hasError = true;
			}
		}

		private static string GetSeverityString(Severity severity)
		{
			if (severity == Severity.Error)
			{
				return "Error";
			}
			if (severity == Severity.Warning)
			{
				return "Warning";
			}
			return "";
		}

		private static void DisplayValidationMessages(IEnumerable<ValidationMessage> msgs, string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				using (IEnumerator<ValidationMessage> enumerator = msgs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ValidationMessage current = enumerator.Current;
						if (current.Severity == Severity.Information)
						{
							Console.WriteLine(current.Description);
						}
						else
						{
							Console.WriteLine("{0} {1}", SqlMetalMain.GetSeverityString(current.Severity), current.Description);
							if (current.Severity == Severity.Error && !SqlMetalMain.hasError)
							{
								SqlMetalMain.hasError = true;
							}
						}
					}
					return;
				}
			}
			List<int> lineNumbers = SqlMetalMain.GetLineNumbers((List<ValidationMessage>)msgs, file);
			int num = 0;
			foreach (ValidationMessage current2 in msgs)
			{
				if (current2.Severity == Severity.Information)
				{
					Console.WriteLine(current2.Description);
				}
				else
				{
					int num2 = lineNumbers[num++];
					Console.WriteLine("{0}({1}) : {2} {3}", new object[]
					{
						file,
						num2,
						SqlMetalMain.GetSeverityString(current2.Severity),
						current2.Description
					});
					if (current2.Severity == Severity.Error && !SqlMetalMain.hasError)
					{
						SqlMetalMain.hasError = true;
					}
				}
			}
		}

		private static void DisplaySchemaErrorMessage(string msg, string file, int line)
		{
			if (string.IsNullOrEmpty(file))
			{
				Console.WriteLine("Error : {0}", msg);
				return;
			}
			Console.WriteLine("{0}({1}) : Error : {2}", file, line, msg);
		}

		private static void ValidateCommandLineOptions(CommandLineParseResults parseResults)
		{
			if (parseResults.ConnectionString != null)
			{
				if (parseResults.Server != null)
				{
					SqlMetalMain.DisplayMessage(Severity.Error, Strings.ServerAndConnectionConflict);
				}
				if (parseResults.UserId != null)
				{
					SqlMetalMain.DisplayMessage(Severity.Error, Strings.UserAndConnectionConflict);
				}
				if (parseResults.Password != null)
				{
					SqlMetalMain.DisplayMessage(Severity.Error, Strings.PasswordAndConnectionConflict);
				}
				if (parseResults.Database != null)
				{
					SqlMetalMain.DisplayMessage(Severity.Error, Strings.DatabaseAndConnectionConflict);
				}
			}
			if (parseResults.Password != null && parseResults.UserId == null && parseResults.InputFile != null && !parseResults.InputFile.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase) && !parseResults.InputFile.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
			{
				SqlMetalMain.DisplayMessage(Severity.Error, Strings.UserIsMissing);
			}
		}

		[SecuritySafeCritical]
		public static int Main(string[] args)
		{
			ExtractOptions extopts = new ExtractOptions
			{
				Types = (ExtractTypes.Tables | ExtractTypes.Relationships),
				Display = new DisplayMessage(SqlMetalMain.DisplayMessage)
			};
			SqlMetalMain.hasError = false;
			SqlMetalMain.DisplayCopyright();
			if (args == null || args.Length == 0 || (args.Length == 1 && args[0] == "/?"))
			{
				SqlMetalMain.DisplayUsage();
				return 1;
			}
			CommandLineParseResults commandLineParseResults = SqlMetalMain.ParseCommandline(args, extopts);
			SqlMetalMain.ValidateCommandLineOptions(commandLineParseResults);
			if (SqlMetalMain.hasError || commandLineParseResults == null)
			{
				return 1;
			}
			return SqlMetalMain.Process(commandLineParseResults, extopts);
		}

		private static int Process(CommandLineParseResults parseResults, ExtractOptions extopts)
		{
			Database database = null;
			try
			{
				if (parseResults.InputType == InputType.Sql)
				{
					string text;
					if (parseResults.ConnectionString != null)
					{
						text = parseResults.ConnectionString;
					}
					else
					{
						if (parseResults.Server == null || parseResults.Server.Length == 0)
						{
							if (parseResults.InputFile != null && parseResults.InputFile.Length > 0)
							{
								if (parseResults.InputFile.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
								{
									parseResults.Server = "localhost\\sqlexpress";
								}
							}
							else
							{
								parseResults.Server = "localhost";
							}
						}
						string database2 = parseResults.Database;
						if (string.IsNullOrEmpty(parseResults.Database))
						{
							if (string.IsNullOrEmpty(parseResults.InputFile))
							{
								SqlMetalMain.DisplayMessage(Severity.Error, Strings.DatabaseIsNotSpecified);
								int result = 1;
								return result;
							}
							parseResults.Database = Path.GetFileNameWithoutExtension(parseResults.InputFile);
						}
						if (parseResults.InputFile != null && parseResults.InputFile.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
						{
							text = string.Format(CultureInfo.InvariantCulture, "Data Source='{0}'", new object[]
							{
								parseResults.InputFile
							});
						}
						else
						{
							text = string.Format(CultureInfo.InvariantCulture, "server='{0}'", new object[]
							{
								parseResults.Server
							});
							if (database2 != null)
							{
								text += string.Format(CultureInfo.InvariantCulture, ";database='{0}'", new object[]
								{
									database2
								});
							}
							if (parseResults.InputFile != null && parseResults.InputFile.Length > 0)
							{
								text += string.Format(CultureInfo.InvariantCulture, ";AttachDBFileName='{0}'", new object[]
								{
									parseResults.InputFile
								});
							}
							if (parseResults.UserId == null && parseResults.Password == null)
							{
								text += ";Integrated Security=SSPI";
							}
							if (parseResults.Server == "localhost\\sqlexpress")
							{
								text += string.Format(CultureInfo.InvariantCulture, ";User Instance=True", new object[]
								{
									parseResults.InputFile
								});
							}
						}
						if (parseResults.UserId != null)
						{
							text += string.Format(CultureInfo.InvariantCulture, ";user id='{0}'", new object[]
							{
								parseResults.UserId
							});
						}
						if (parseResults.Password != null)
						{
							text += string.Format(CultureInfo.InvariantCulture, ";password='{0}'", new object[]
							{
								parseResults.Password
							});
						}
					}
					IDatabaseExtractor databaseExtractor = DbmlExtractorFactory.CreateExtractor();
					try
					{
						database = databaseExtractor.Extract(text, parseResults.Database, extopts);
					}
					catch (Exception ex)
					{
						SqlMetalMain.DisplayMessage(Severity.Error, SqlMetalMain.ExtractFullExceptionMessage(ex));
					}
					if (SqlMetalMain.hasError)
					{
						int result = 1;
						return result;
					}
				}
				else if (parseResults.InputType == InputType.Dbml)
				{
					try
					{
						database = Dbml.FromFile(parseResults.InputFile);
					}
					catch (XmlSchemaException ex2)
					{
						SqlMetalMain.DisplaySchemaErrorMessage(ex2.Message, parseResults.InputFile, ex2.LineNumber);
						SqlMetalMain.hasError = true;
						int result = 1;
						return result;
					}
					catch (XmlException ex3)
					{
						SqlMetalMain.DisplaySchemaErrorMessage(ex3.Message, parseResults.InputFile, ex3.LineNumber);
						SqlMetalMain.hasError = true;
						int result = 1;
						return result;
					}
				}
				if (database != null)
				{
					database = SqlMetalMain.ConfigureDbml(parseResults, database);
					IMappingCodeGenerator mappingCodeGenerator = (parseResults.Lang == LanguageType.CSharp) ? CodeGeneratorFactory.CreateCSharpGenerator() : CodeGeneratorFactory.CreateVisualBasicGenerator();
					IEnumerable<ValidationMessage> msgs = mappingCodeGenerator.ValidateModel(database);
					SqlMetalMain.DisplayValidationMessages(msgs, (parseResults.InputType == InputType.Dbml) ? parseResults.InputFile : null);
					if (SqlMetalMain.hasError)
					{
						int result = 1;
						return result;
					}
					TextWriter textWriter = Console.Out;
					Stream stream = null;
					try
					{
						if (!string.IsNullOrEmpty(parseResults.OutputFile))
						{
							try
							{
								stream = File.Open(parseResults.OutputFile, FileMode.Create);
							}
							catch (IOException)
							{
								SqlMetalMain.DisplayMessage(Severity.Error, Strings.OutputFileIOError(parseResults.OutputFile));
								int result = 1;
								return result;
							}
							catch (NotSupportedException)
							{
								SqlMetalMain.DisplayMessage(Severity.Error, Strings.OutputFileNameFormatError(parseResults.OutputFile));
								int result = 1;
								return result;
							}
							textWriter = new StreamWriter(stream, Encoding.UTF8);
						}
						if (parseResults.OutputType == OutputType.Dbml)
						{
							textWriter.Write(Dbml.ToText(database));
						}
						else if (parseResults.OutputType == OutputType.Code || parseResults.OutputType == OutputType.CodeAndMapping)
						{
							SqlMetalMain.ParseCode(parseResults, database, mappingCodeGenerator, ref textWriter, ref stream);
						}
					}
					finally
					{
						textWriter.Flush();
						if (stream != null)
						{
							stream.Close();
						}
						if (SqlMetalMain.hasError && !string.IsNullOrEmpty(parseResults.OutputFile) && File.Exists(parseResults.OutputFile))
						{
							File.Delete(parseResults.OutputFile);
						}
					}
				}
			}
			catch (Exception ex4)
			{
				SqlMetalMain.DisplayMessage(Severity.Error, SqlMetalMain.ExtractFullExceptionMessage(ex4));
				int result = 1;
				return result;
			}
			return 0;
		}

		private static void ParseCode(CommandLineParseResults parseResults, Database db, IMappingCodeGenerator gen, ref TextWriter tw, ref Stream outputStream)
		{
			GenerationResult generationResult = gen.Generate(db, new GenerateOptions
			{
				ResultBaseFileName = parseResults.OutputFile
			});
			IEnumerable<ResultFile> files = generationResult.Files;
			foreach (ResultFile current in files)
			{
				if (current.IsSourceFile)
				{
					if (current.Encoding == null)
					{
						current.Encoding = Encoding.UTF8;
					}
					if (!string.IsNullOrEmpty(parseResults.OutputFile))
					{
						tw = new StreamWriter(outputStream, current.Encoding);
					}
					tw.Write(current.FileContent);
				}
			}
			tw.Flush();
			if (outputStream != null)
			{
				outputStream.Close();
			}
			tw = Console.Out;
			if (!string.IsNullOrEmpty(parseResults.MapOutputFile))
			{
				outputStream = File.Open(parseResults.MapOutputFile, FileMode.Create);
				tw = new StreamWriter(outputStream);
				foreach (ResultFile current2 in files)
				{
					if (!current2.IsSourceFile)
					{
						if (current2.Encoding == null)
						{
							current2.Encoding = Encoding.UTF8;
						}
						if (parseResults.MapOutputFile.Length > 0)
						{
							tw = new StreamWriter(outputStream, current2.Encoding);
						}
						tw.Write(current2.FileContent);
					}
				}
			}
		}

		internal static Database ConfigureDbml(CommandLineParseResults parseResults, Database db)
		{
			if (parseResults.SerializationMode.HasValue)
			{
				db.Serialization = parseResults.SerializationMode;
			}
			if (parseResults.EntityBase != null)
			{
				db.EntityBase = parseResults.EntityBase;
			}
			db.Provider = parseResults.Provider;
			if (parseResults.OutputType == OutputType.CodeAndMapping)
			{
				db.ExternalMapping = new bool?(true);
			}
			if (!string.IsNullOrEmpty(parseResults.ContextClass))
			{
				db.Class = parseResults.ContextClass;
			}
			if (!string.IsNullOrEmpty(parseResults.Namespace))
			{
				db.ContextNamespace = parseResults.Namespace;
				db.EntityNamespace = parseResults.Namespace;
			}
			return Dbml.CopyWithNulledOutDefaults(db);
		}

		internal static CommandLineParseResults ParseCommandline(string[] args, ExtractOptions extopts)
		{
			CommandLineParseResults commandLineParseResults = new CommandLineParseResults();
			try
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				for (int i = 0; i < args.Length; i++)
				{
					string text = args[i];
					string text2 = text.ToLower(CultureInfo.InvariantCulture);
					if (text2.StartsWith("/server:", StringComparison.Ordinal))
					{
						commandLineParseResults.Server = text.Substring(8);
						commandLineParseResults.InputType = InputType.Sql;
					}
					else if (text2.StartsWith("/database:", StringComparison.Ordinal))
					{
						string text3 = text.Substring(10);
						if (string.IsNullOrEmpty(text3))
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.EmptyNameError("/database"));
						}
						else
						{
							commandLineParseResults.Database = text3;
						}
					}
					else if (text2.StartsWith("/user:", StringComparison.Ordinal))
					{
						string text4 = text.Substring(6);
						if (string.IsNullOrEmpty(text4))
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.EmptyNameError("/user"));
						}
						else
						{
							commandLineParseResults.UserId = text4;
						}
					}
					else if (text2.StartsWith("/password:", StringComparison.Ordinal))
					{
						string text5 = text.Substring(10);
						if (string.IsNullOrEmpty(text5))
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.EmptyNameError("/password"));
						}
						else
						{
							commandLineParseResults.Password = text5;
						}
					}
					else if (text2.StartsWith("/language:", StringComparison.Ordinal))
					{
						string text6 = text2.Substring(10);
						string key;
						switch (key = text6)
						{
						case "csharp":
						case "vcsharp":
						case "vcs":
						case "cs":
						case "c#":
							commandLineParseResults.Lang = LanguageType.CSharp;
							flag3 = true;
							goto IL_779;
						case "vb":
						case "visualbasic":
							commandLineParseResults.Lang = LanguageType.VisualBasic;
							flag3 = true;
							goto IL_779;
						}
						string desc = Strings.UnknownLanguage(text.Substring(10));
						SqlMetalMain.DisplayMessage(Severity.Error, desc);
					}
					else if (text2 == "/xml" || text2 == "/dbml")
					{
						if (flag)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrMapNotBoth);
						}
						if (flag2)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrCodeNotBoth);
						}
						flag = true;
						flag2 = true;
						commandLineParseResults.OutputType = OutputType.Dbml;
					}
					else if (text2.StartsWith("/xml:", StringComparison.Ordinal) || text2.StartsWith("/dbml:", StringComparison.Ordinal))
					{
						if (flag)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrMapNotBoth);
						}
						if (flag2)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrCodeNotBoth);
						}
						flag = true;
						flag2 = true;
						commandLineParseResults.OutputType = OutputType.Dbml;
						commandLineParseResults.OutputFile = text.Substring(text2.IndexOf(":", StringComparison.Ordinal) + 1);
					}
					else if (text2 == "/code")
					{
						if (flag2)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrCodeNotBoth);
						}
						flag2 = true;
						flag5 = true;
						if (commandLineParseResults.OutputType != OutputType.CodeAndMapping)
						{
							commandLineParseResults.OutputType = OutputType.Code;
						}
					}
					else if (text2.StartsWith("/code:", StringComparison.Ordinal))
					{
						if (flag2)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrCodeNotBoth);
						}
						flag2 = true;
						flag5 = true;
						if (commandLineParseResults.OutputType != OutputType.CodeAndMapping)
						{
							commandLineParseResults.OutputType = OutputType.Code;
						}
						commandLineParseResults.OutputFile = text.Substring(6);
						if (commandLineParseResults.OutputFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !flag3)
						{
							commandLineParseResults.Lang = LanguageType.CSharp;
						}
						else if (commandLineParseResults.OutputFile.EndsWith(".vb", StringComparison.OrdinalIgnoreCase) && !flag3)
						{
							commandLineParseResults.Lang = LanguageType.VisualBasic;
						}
					}
					else if (text2.StartsWith("/namespace:", StringComparison.Ordinal))
					{
						commandLineParseResults.Namespace = text.Substring(11);
					}
					else if (text2 == "/views" || text2 == "/v")
					{
						extopts.Types |= ExtractTypes.Views;
					}
					else if (text2 == "/functions" || text2 == "/f")
					{
						extopts.Types |= ExtractTypes.Functions;
					}
					else if (text2 == "/sprocs" || text2 == "/s")
					{
						extopts.Types |= ExtractTypes.StoredProcedures;
					}
					else if (text2 == "/pluralize")
					{
						extopts.Pluralize = true;
					}
					else if (text2.StartsWith("/serialization:", StringComparison.Ordinal))
					{
						string text7 = text2.Substring(15);
						string a;
						if ((a = text7) != null)
						{
							if (a == "none")
							{
								commandLineParseResults.SerializationMode = new SerializationMode?(SerializationMode.None);
								goto IL_779;
							}
							if (a == "unidirectional" || a == "uni")
							{
								commandLineParseResults.SerializationMode = new SerializationMode?(SerializationMode.Unidirectional);
								goto IL_779;
							}
						}
						string desc2 = Strings.UnknownSerializationMode(text.Substring(15));
						SqlMetalMain.DisplayMessage(Severity.Error, desc2);
					}
					else if (text2.StartsWith("/entitybase:", StringComparison.Ordinal))
					{
						commandLineParseResults.EntityBase = text.Substring(12);
					}
					else if (text2 == "/map")
					{
						if (flag)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrMapNotBoth);
						}
						flag = true;
						flag4 = true;
						commandLineParseResults.OutputType = OutputType.CodeAndMapping;
					}
					else if (text2.StartsWith("/map:", StringComparison.Ordinal))
					{
						if (flag)
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.DbmlOrMapNotBoth);
						}
						flag = true;
						flag4 = true;
						commandLineParseResults.OutputType = OutputType.CodeAndMapping;
						commandLineParseResults.MapOutputFile = text.Substring(5);
					}
					else
					{
						if (text2.StartsWith("/timeout:", StringComparison.Ordinal))
						{
							string text8 = text.Substring(9);
							try
							{
								int num2 = Convert.ToInt32(text8, NumberFormatInfo.InvariantInfo);
								commandLineParseResults.CommandTimeout = num2;
								extopts.CommandTimeout = num2;
								if (num2 < 0)
								{
									string desc3 = Strings.TimeoutMustNotBeNegative(text8);
									SqlMetalMain.DisplayMessage(Severity.Error, desc3);
								}
								goto IL_779;
							}
							catch (FormatException)
							{
								string desc4 = Strings.InvalidTimeoutFormat(text8);
								SqlMetalMain.DisplayMessage(Severity.Error, desc4);
								goto IL_779;
							}
						}
						if (text2.StartsWith("/provider:", StringComparison.Ordinal))
						{
							commandLineParseResults.Provider = text.Substring(10);
							string a2;
							if ((a2 = text2.Substring(10)) != null && (a2 == "sql2000" || a2 == "sql2005" || a2 == "sql2008" || a2 == "sqlcompact"))
							{
								commandLineParseResults.Provider = text.Substring(10);
							}
							else
							{
								string desc5 = Strings.UnknownProvider(text.Substring(10));
								SqlMetalMain.DisplayMessage(Severity.Error, desc5);
							}
						}
						else if (text2.StartsWith("/context:", StringComparison.Ordinal))
						{
							commandLineParseResults.ContextClass = text.Substring(9);
						}
						else if (text2.StartsWith("/conn:", StringComparison.Ordinal))
						{
							commandLineParseResults.ConnectionString = text.Substring(6);
							commandLineParseResults.InputType = InputType.Sql;
						}
						else if (text2.StartsWith("/", StringComparison.Ordinal))
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.UnknownOption(text));
						}
						else if (!File.Exists(text))
						{
							SqlMetalMain.DisplayMessage(Severity.Error, Strings.InputFileDoesNotExist(text));
						}
						else
						{
							if (!string.IsNullOrEmpty(commandLineParseResults.InputFile))
							{
								SqlMetalMain.DisplayMessage(Severity.Error, Strings.MultipleInputFiles);
							}
							else
							{
								commandLineParseResults.InputFile = text;
							}
							if (commandLineParseResults.InputFile.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase) || commandLineParseResults.InputFile.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
							{
								commandLineParseResults.InputType = InputType.Sql;
								commandLineParseResults.InputFile = Path.GetFullPath(text);
							}
							else
							{
								commandLineParseResults.InputType = InputType.Dbml;
								if (commandLineParseResults.OutputType != OutputType.CodeAndMapping)
								{
									commandLineParseResults.OutputType = OutputType.Code;
								}
							}
						}
					}
					IL_779:;
				}
				if (flag4 && !flag5)
				{
					SqlMetalMain.DisplayMessage(Severity.Error, Strings.MapWithoutCode);
				}
			}
			catch (Exception ex)
			{
				SqlMetalMain.DisplayMessage(Severity.Error, SqlMetalMain.ExtractFullExceptionMessage(ex));
			}
			return commandLineParseResults;
		}

		private static void DisplayCopyright()
		{
			System.Version version = new System.Version("4.0.0.0");
			string p = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[]
			{
				version.Major,
				version.Minor
			});
			Console.WriteLine(Strings.VersionString("4.0.30319.33440", p));
			Console.WriteLine(Strings.CopyrightString);
			Console.WriteLine();
		}

		private static void DisplayUsage()
		{
			Console.WriteLine(Strings.UsageHeader);
			Console.WriteLine();
			Console.WriteLine(Strings.ProgramDescription0);
			Console.WriteLine(Strings.ProgramDescription1);
			Console.WriteLine(Strings.ProgramDescription2);
			Console.WriteLine(Strings.ProgramDescription3);
			Console.WriteLine();
			Console.WriteLine(Strings.UsageOptions);
			string[] array = new string[]
			{
				Strings.ServerUsage,
				Strings.ServerDescription,
				Strings.DatabaseUsage,
				Strings.DatabaseDescription,
				Strings.UserUsage,
				Strings.UserDescription,
				Strings.PasswordUsage,
				Strings.PasswordDescription,
				Strings.ConnectionStringUsage,
				Strings.ConnectionStringDescription,
				Strings.TimeOutUsage,
				Strings.TimeOutDescription
			};
			string[] array2 = new string[]
			{
				Strings.ViewsUsage,
				Strings.ViewsDescription,
				Strings.FunctionsUsage,
				Strings.FunctionDescription,
				Strings.SprocsUsage,
				Strings.SprocsDescription
			};
			string[] array3 = new string[]
			{
				Strings.DbmlUsage,
				Strings.DbmlDescription,
				Strings.CodeUsage,
				Strings.CodeDescription,
				Strings.MapUsage,
				Strings.MapDescription
			};
			string[] array4 = new string[]
			{
				Strings.LanguageUsage,
				Strings.LanguageDescription,
				Strings.NamespaceUsage,
				Strings.NamespaceDescription,
				Strings.ContextUsage,
				Strings.ContextDescription,
				Strings.EntityBaseUsage,
				Strings.EntityBaseDescription,
				Strings.PluralizeUsage,
				Strings.PluralizeDescription,
				Strings.SerializationUsage,
				Strings.SerializationDescription,
				Strings.ProviderUsage,
				Strings.ProviderDescription
			};
			string[] array5 = new string[]
			{
				Strings.InputFileUsage,
				Strings.InputFileDescription
			};
			string[][] flagArrays = new string[][]
			{
				array,
				array2,
				array3,
				array4,
				array5
			};
			SqlMetalMain.DisplaySqlMetalFlags(flagArrays);
			Console.WriteLine(Strings.DirectGenerationDescription);
			Console.WriteLine("  " + Strings.DirectGenerationExample);
			Console.WriteLine();
			Console.WriteLine(Strings.ExtractDbmlDescription);
			Console.WriteLine("  " + Strings.ExtractDbmlExample);
			Console.WriteLine();
			Console.WriteLine(Strings.GenerateFromDbmlDescription);
			Console.WriteLine("  " + Strings.GenerateFromDbmlExample);
			Console.WriteLine();
			Console.WriteLine(Strings.ExtractFromSdfDescription);
			Console.WriteLine("  " + Strings.ExtractFromSdfExample);
			Console.WriteLine();
			Console.WriteLine(Strings.ExtractFromSqlExpressDescription);
			Console.WriteLine("  " + Strings.ExtractFromSqlExpressExample);
			Console.WriteLine();
			Console.WriteLine(Strings.ExtractUsingConnectionStringDescription);
			Console.WriteLine("  " + Strings.ExtractUsingConnectionStringExample);
			Console.WriteLine();
		}

		private static void DisplayFlags(string[] flags, int flagWidth)
		{
			string format = "  {0,-" + flagWidth + "} {1}";
			for (int i = 0; i < flags.Length; i += 2)
			{
				Console.WriteLine(format, flags[i], flags[i + 1]);
			}
		}

		private static int GetFlagWidth(string[] flags)
		{
			int num = 0;
			for (int i = 0; i < flags.Length; i += 2)
			{
				num = Math.Max(num, flags[i].Length + 1);
			}
			return num;
		}

		private static void DisplaySqlMetalFlags(string[][] flagArrays)
		{
			int num = 0;
			for (int i = 0; i < flagArrays.Length; i++)
			{
				string[] flags = flagArrays[i];
				num = Math.Max(num, SqlMetalMain.GetFlagWidth(flags));
			}
			for (int j = 0; j < flagArrays.Length; j++)
			{
				string[] flags2 = flagArrays[j];
				SqlMetalMain.DisplayFlags(flags2, num);
				Console.WriteLine();
			}
		}

		private static List<int> GetLineNumbers(List<ValidationMessage> msgs, string xmlFile)
		{
			int num = 0;
			int num2 = 0;
			if (msgs.Count == 0)
			{
				return new List<int>();
			}
			List<int> list = new List<int>(msgs.Count);
			using (XmlTextReader xmlTextReader = new XmlTextReader(xmlFile))
			{
				while (xmlTextReader.Read())
				{
					if (xmlTextReader.NodeType == XmlNodeType.Element)
					{
						while (msgs[num2].NodeId == num)
						{
							list.Add(xmlTextReader.LineNumber);
							if (++num2 == msgs.Count)
							{
								return list;
							}
						}
						num++;
					}
				}
			}
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[]
			{
				msgs[num2].NodeId,
				msgs[num2].Description
			}));
		}

		private static string ExtractFullExceptionMessage(Exception ex)
		{
			string text = ex.Message;
			int num = 0;
			for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				num++;
				string str = string.Empty.PadLeft(num, '\t');
				text = text + Environment.NewLine + str;
				text += innerException.Message;
			}
			return text;
		}
	}
}
