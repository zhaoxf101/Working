using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace LinqToSqlShared.Generator
{
	internal sealed class SR
	{
		internal const string OwningTeam = "OwningTeam";

		internal const string CouldNotMakeUniqueTableName = "CouldNotMakeUniqueTableName";

		internal const string CouldNotMakeUniqueVariableName = "CouldNotMakeUniqueVariableName";

		internal const string CouldNotMakePropertyNameForAssociation = "CouldNotMakePropertyNameForAssociation";

		internal const string MismatchedThisKeyOtherKey = "MismatchedThisKeyOtherKey";

		internal const string UnableToExtractTable = "UnableToExtractTable";

		internal const string UnableToExtractView = "UnableToExtractView";

		internal const string TwoOrMoreUnnamedColumnsInResultSet = "TwoOrMoreUnnamedColumnsInResultSet";

		internal const string UnableToExtractSproc = "UnableToExtractSproc";

		internal const string UnableToExtractFunction = "UnableToExtractFunction";

		internal const string InvalidContextNameSpace = "InvalidContextNameSpace";

		internal const string InvalidEntityNameSpace = "InvalidEntityNameSpace";

		internal const string NoKeysSpecifiedInAssociation = "NoKeysSpecifiedInAssociation";

		internal const string AbstractTypeHasInheritanceCode = "AbstractTypeHasInheritanceCode";

		internal const string NoInheritanceCodeWhenIsInheritanceDefault = "NoInheritanceCodeWhenIsInheritanceDefault";

		internal const string TypeMappingNotSupported = "TypeMappingNotSupported";

		internal const string UnknownDbType = "UnknownDbType";

		internal const string DataLossToDatabase = "DataLossToDatabase";

		internal const string DataLossFromDatabase = "DataLossFromDatabase";

		internal const string DataLossBoth = "DataLossBoth";

		internal const string ThisKeyOtherKey = "ThisKeyOtherKey";

		internal const string TypeHasNoPrimaryKey = "TypeHasNoPrimaryKey";

		internal const string ColumnMemberDoesNotExist = "ColumnMemberDoesNotExist";

		internal const string AttributeIsNotPresent = "AttributeIsNotPresent";

		internal const string TypeNotForDiscriminator = "TypeNotForDiscriminator";

		internal const string MultipleInheritanceDefaultTypes = "MultipleInheritanceDefaultTypes";

		internal const string MultipleDiscriminators = "MultipleDiscriminators";

		internal const string NoInheritanceDefaultTypes = "NoInheritanceDefaultTypes";

		internal const string NoDiscriminator = "NoDiscriminator";

		internal const string InheritanceCodeMissingWhenHaveDiscriminator = "InheritanceCodeMissingWhenHaveDiscriminator";

		internal const string NotTableType = "NotTableType";

		internal const string MemberCannotBeTheSameAsTypeName = "MemberCannotBeTheSameAsTypeName";

		internal const string TableMemberAndMethodConflict = "TableMemberAndMethodConflict";

		internal const string ReturnTypeMappingNotSupported = "ReturnTypeMappingNotSupported";

		internal const string ReturnUnknownDbType = "ReturnUnknownDbType";

		internal const string ReturnDataLossToDatabase = "ReturnDataLossToDatabase";

		internal const string ReturnDataLossFromDatabase = "ReturnDataLossFromDatabase";

		internal const string ReturnDataLossBoth = "ReturnDataLossBoth";

		internal const string UnknownCLRType = "UnknownCLRType";

		internal const string ReturnUnknownCLRType = "ReturnUnknownCLRType";

		internal const string InvalidDatabaseClassIdentifier = "InvalidDatabaseClassIdentifier";

		internal const string InvalidTableMemberIdentifier = "InvalidTableMemberIdentifier";

		internal const string InvalidITypeNamedentifier = "InvalidITypeNamedentifier";

		internal const string InvalidColumnMemberIdentifier = "InvalidColumnMemberIdentifier";

		internal const string InvalidAssociationMemberIdentifier = "InvalidAssociationMemberIdentifier";

		internal const string InvalidFunctionMethodIdentifier = "InvalidFunctionMethodIdentifier";

		internal const string InvalidParameterParameterIdentifier = "InvalidParameterParameterIdentifier";

		internal const string SameTableNameIdentifier = "SameTableNameIdentifier";

		internal const string SameTableMemberIdentifier = "SameTableMemberIdentifier";

		internal const string SameTypeNameIdentifier = "SameTypeNameIdentifier";

		internal const string SameTypeInheritanceCodeIdentifier = "SameTypeInheritanceCodeIdentifier";

		internal const string SameColumnNameIdentifier = "SameColumnNameIdentifier";

		internal const string SameColumnMemberIdentifier = "SameColumnMemberIdentifier";

		internal const string SameAssociationMemberIdentifier = "SameAssociationMemberIdentifier";

		internal const string SameFunctionNameIdentifier = "SameFunctionNameIdentifier";

		internal const string SameFunctionMethodIdentifier = "SameFunctionMethodIdentifier";

		internal const string SameParameterNameIdentifier = "SameParameterNameIdentifier";

		internal const string SameParameterParameterIdentifier = "SameParameterParameterIdentifier";

		internal const string IsComposableAndNumberOfTypeDoNotMatch = "IsComposableAndNumberOfTypeDoNotMatch";

		internal const string IsComposableAndHasMultipleResultsDoNotMatch = "IsComposableAndHasMultipleResultsDoNotMatch";

		internal const string InvalidDefaultDatabaseClassIdentifier = "InvalidDefaultDatabaseClassIdentifier";

		internal const string InvalidDefaultTableMemberIdentifier = "InvalidDefaultTableMemberIdentifier";

		internal const string InvalidDefaultColumnMemberIdentifier = "InvalidDefaultColumnMemberIdentifier";

		internal const string InvalidDefaultFunctionMethodIdentifier = "InvalidDefaultFunctionMethodIdentifier";

		internal const string InvalidDefaultParameterParameterIdentifier = "InvalidDefaultParameterParameterIdentifier";

		internal const string InvalidDeleteOnNullSpecification = "InvalidDeleteOnNullSpecification";

		internal const string ParameterDoesNotExistInFunction = "ParameterDoesNotExistInFunction";

		internal const string SameColumnStorageIdentifier = "SameColumnStorageIdentifier";

		internal const string StorageCannotBeTheSameAsTypeName = "StorageCannotBeTheSameAsTypeName";

		internal const string AssociationManysideIsForeignKey = "AssociationManysideIsForeignKey";

		internal const string PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsDbGenerated = "PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsDbGenerated";

		internal const string PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsVersion = "PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsVersion";

		internal const string OtherSideHasNoPrimaryKey = "OtherSideHasNoPrimaryKey";

		internal const string InvalidIdentityMememberType = "InvalidIdentityMememberType";

		internal const string KeylessTablesCannotHaveCUDOverrides = "KeylessTablesCannotHaveCUDOverrides";

		internal const string UserDefinedDbType = "UserDefinedDbType";

		internal const string SubTypesCannotContainPK = "SubTypesCannotContainPK";

		internal const string TypeNameIsReserved = "TypeNameIsReserved";

		internal const string ExtensibilityMethodDefinitions = "ExtensibilityMethodDefinitions";

		internal const string ConnectionStringTransferredToCode = "ConnectionStringTransferredToCode";

		private static SR loader;

		private ResourceManager resources;

		private static CultureInfo Culture
		{
			get
			{
				return null;
			}
		}

		public static ResourceManager Resources
		{
			get
			{
				return SR.GetLoader().resources;
			}
		}

		internal SR()
		{
			this.resources = new ResourceManager("LinqToSqlShared.Generator", base.GetType().Assembly);
		}

		private static SR GetLoader()
		{
			if (SR.loader == null)
			{
				SR value = new SR();
				Interlocked.CompareExchange<SR>(ref SR.loader, value, null);
			}
			return SR.loader;
		}

		public static string GetString(string name, params object[] args)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			string @string = sR.resources.GetString(name, SR.Culture);
			if (args != null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					string text = args[i] as string;
					if (text != null && text.Length > 1024)
					{
						args[i] = text.Substring(0, 1021) + "...";
					}
				}
				return string.Format(CultureInfo.CurrentCulture, @string, args);
			}
			return @string;
		}

		public static string GetString(string name)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			return sR.resources.GetString(name, SR.Culture);
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return SR.GetString(name);
		}

		public static object GetObject(string name)
		{
			SR sR = SR.GetLoader();
			if (sR == null)
			{
				return null;
			}
			return sR.resources.GetObject(name, SR.Culture);
		}
	}
}
