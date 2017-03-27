using System;

namespace LinqToSqlShared.Generator
{
	internal static class Strings
	{
		internal static string OwningTeam
		{
			get
			{
				return SR.GetString("OwningTeam");
			}
		}

		internal static string TwoOrMoreUnnamedColumnsInResultSet
		{
			get
			{
				return SR.GetString("TwoOrMoreUnnamedColumnsInResultSet");
			}
		}

		internal static string ExtensibilityMethodDefinitions
		{
			get
			{
				return SR.GetString("ExtensibilityMethodDefinitions");
			}
		}

		internal static string ConnectionStringTransferredToCode
		{
			get
			{
				return SR.GetString("ConnectionStringTransferredToCode");
			}
		}

		internal static string CouldNotMakeUniqueTableName(object p0)
		{
			return SR.GetString("CouldNotMakeUniqueTableName", new object[]
			{
				p0
			});
		}

		internal static string CouldNotMakeUniqueVariableName(object p0)
		{
			return SR.GetString("CouldNotMakeUniqueVariableName", new object[]
			{
				p0
			});
		}

		internal static string CouldNotMakePropertyNameForAssociation(object p0)
		{
			return SR.GetString("CouldNotMakePropertyNameForAssociation", new object[]
			{
				p0
			});
		}

		internal static string MismatchedThisKeyOtherKey(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("MismatchedThisKeyOtherKey", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string UnableToExtractTable(object p0, object p1)
		{
			return SR.GetString("UnableToExtractTable", new object[]
			{
				p0,
				p1
			});
		}

		internal static string UnableToExtractView(object p0, object p1)
		{
			return SR.GetString("UnableToExtractView", new object[]
			{
				p0,
				p1
			});
		}

		internal static string UnableToExtractSproc(object p0, object p1)
		{
			return SR.GetString("UnableToExtractSproc", new object[]
			{
				p0,
				p1
			});
		}

		internal static string UnableToExtractFunction(object p0, object p1)
		{
			return SR.GetString("UnableToExtractFunction", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidContextNameSpace(object p0, object p1)
		{
			return SR.GetString("InvalidContextNameSpace", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidEntityNameSpace(object p0, object p1)
		{
			return SR.GetString("InvalidEntityNameSpace", new object[]
			{
				p0,
				p1
			});
		}

		internal static string NoKeysSpecifiedInAssociation(object p0, object p1)
		{
			return SR.GetString("NoKeysSpecifiedInAssociation", new object[]
			{
				p0,
				p1
			});
		}

		internal static string AbstractTypeHasInheritanceCode(object p0)
		{
			return SR.GetString("AbstractTypeHasInheritanceCode", new object[]
			{
				p0
			});
		}

		internal static string NoInheritanceCodeWhenIsInheritanceDefault(object p0)
		{
			return SR.GetString("NoInheritanceCodeWhenIsInheritanceDefault", new object[]
			{
				p0
			});
		}

		internal static string TypeMappingNotSupported(object p0, object p1, object p2, object p3, object p4, object p5)
		{
			return SR.GetString("TypeMappingNotSupported", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4,
				p5
			});
		}

		internal static string UnknownDbType(object p0, object p1, object p2, object p3, object p4)
		{
			return SR.GetString("UnknownDbType", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4
			});
		}

		internal static string DataLossToDatabase(object p0, object p1, object p2, object p3, object p4, object p5)
		{
			return SR.GetString("DataLossToDatabase", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4,
				p5
			});
		}

		internal static string DataLossFromDatabase(object p0, object p1, object p2, object p3, object p4, object p5)
		{
			return SR.GetString("DataLossFromDatabase", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4,
				p5
			});
		}

		internal static string DataLossBoth(object p0, object p1, object p2, object p3, object p4, object p5)
		{
			return SR.GetString("DataLossBoth", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4,
				p5
			});
		}

		internal static string ThisKeyOtherKey(object p0, object p1)
		{
			return SR.GetString("ThisKeyOtherKey", new object[]
			{
				p0,
				p1
			});
		}

		internal static string TypeHasNoPrimaryKey(object p0, object p1)
		{
			return SR.GetString("TypeHasNoPrimaryKey", new object[]
			{
				p0,
				p1
			});
		}

		internal static string ColumnMemberDoesNotExist(object p0, object p1)
		{
			return SR.GetString("ColumnMemberDoesNotExist", new object[]
			{
				p0,
				p1
			});
		}

		internal static string AttributeIsNotPresent(object p0, object p1, object p2, object p3, object p4)
		{
			return SR.GetString("AttributeIsNotPresent", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4
			});
		}

		internal static string TypeNotForDiscriminator(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("TypeNotForDiscriminator", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string MultipleInheritanceDefaultTypes(object p0)
		{
			return SR.GetString("MultipleInheritanceDefaultTypes", new object[]
			{
				p0
			});
		}

		internal static string MultipleDiscriminators(object p0, object p1)
		{
			return SR.GetString("MultipleDiscriminators", new object[]
			{
				p0,
				p1
			});
		}

		internal static string NoInheritanceDefaultTypes(object p0)
		{
			return SR.GetString("NoInheritanceDefaultTypes", new object[]
			{
				p0
			});
		}

		internal static string NoDiscriminator(object p0)
		{
			return SR.GetString("NoDiscriminator", new object[]
			{
				p0
			});
		}

		internal static string InheritanceCodeMissingWhenHaveDiscriminator(object p0, object p1)
		{
			return SR.GetString("InheritanceCodeMissingWhenHaveDiscriminator", new object[]
			{
				p0,
				p1
			});
		}

		internal static string NotTableType(object p0)
		{
			return SR.GetString("NotTableType", new object[]
			{
				p0
			});
		}

		internal static string MemberCannotBeTheSameAsTypeName(object p0, object p1, object p2)
		{
			return SR.GetString("MemberCannotBeTheSameAsTypeName", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string TableMemberAndMethodConflict(object p0)
		{
			return SR.GetString("TableMemberAndMethodConflict", new object[]
			{
				p0
			});
		}

		internal static string ReturnTypeMappingNotSupported(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("ReturnTypeMappingNotSupported", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string ReturnUnknownDbType(object p0, object p1, object p2)
		{
			return SR.GetString("ReturnUnknownDbType", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string ReturnDataLossToDatabase(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("ReturnDataLossToDatabase", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string ReturnDataLossFromDatabase(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("ReturnDataLossFromDatabase", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string ReturnDataLossBoth(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("ReturnDataLossBoth", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string UnknownCLRType(object p0, object p1, object p2, object p3, object p4)
		{
			return SR.GetString("UnknownCLRType", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4
			});
		}

		internal static string ReturnUnknownCLRType(object p0, object p1, object p2)
		{
			return SR.GetString("ReturnUnknownCLRType", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string InvalidDatabaseClassIdentifier(object p0)
		{
			return SR.GetString("InvalidDatabaseClassIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidTableMemberIdentifier(object p0)
		{
			return SR.GetString("InvalidTableMemberIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidITypeNamedentifier(object p0)
		{
			return SR.GetString("InvalidITypeNamedentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidColumnMemberIdentifier(object p0, object p1)
		{
			return SR.GetString("InvalidColumnMemberIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidAssociationMemberIdentifier(object p0, object p1)
		{
			return SR.GetString("InvalidAssociationMemberIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidFunctionMethodIdentifier(object p0)
		{
			return SR.GetString("InvalidFunctionMethodIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidParameterParameterIdentifier(object p0)
		{
			return SR.GetString("InvalidParameterParameterIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameTableNameIdentifier(object p0)
		{
			return SR.GetString("SameTableNameIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameTableMemberIdentifier(object p0)
		{
			return SR.GetString("SameTableMemberIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameTypeNameIdentifier(object p0)
		{
			return SR.GetString("SameTypeNameIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameTypeInheritanceCodeIdentifier(object p0, object p1)
		{
			return SR.GetString("SameTypeInheritanceCodeIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SameColumnNameIdentifier(object p0, object p1)
		{
			return SR.GetString("SameColumnNameIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SameColumnMemberIdentifier(object p0, object p1)
		{
			return SR.GetString("SameColumnMemberIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SameAssociationMemberIdentifier(object p0, object p1)
		{
			return SR.GetString("SameAssociationMemberIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SameFunctionNameIdentifier(object p0)
		{
			return SR.GetString("SameFunctionNameIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameFunctionMethodIdentifier(object p0)
		{
			return SR.GetString("SameFunctionMethodIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameParameterNameIdentifier(object p0)
		{
			return SR.GetString("SameParameterNameIdentifier", new object[]
			{
				p0
			});
		}

		internal static string SameParameterParameterIdentifier(object p0)
		{
			return SR.GetString("SameParameterParameterIdentifier", new object[]
			{
				p0
			});
		}

		internal static string IsComposableAndNumberOfTypeDoNotMatch(object p0)
		{
			return SR.GetString("IsComposableAndNumberOfTypeDoNotMatch", new object[]
			{
				p0
			});
		}

		internal static string IsComposableAndHasMultipleResultsDoNotMatch(object p0)
		{
			return SR.GetString("IsComposableAndHasMultipleResultsDoNotMatch", new object[]
			{
				p0
			});
		}

		internal static string InvalidDefaultDatabaseClassIdentifier(object p0)
		{
			return SR.GetString("InvalidDefaultDatabaseClassIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidDefaultTableMemberIdentifier(object p0)
		{
			return SR.GetString("InvalidDefaultTableMemberIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidDefaultColumnMemberIdentifier(object p0, object p1)
		{
			return SR.GetString("InvalidDefaultColumnMemberIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidDefaultFunctionMethodIdentifier(object p0)
		{
			return SR.GetString("InvalidDefaultFunctionMethodIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidDefaultParameterParameterIdentifier(object p0)
		{
			return SR.GetString("InvalidDefaultParameterParameterIdentifier", new object[]
			{
				p0
			});
		}

		internal static string InvalidDeleteOnNullSpecification(object p0, object p1)
		{
			return SR.GetString("InvalidDeleteOnNullSpecification", new object[]
			{
				p0,
				p1
			});
		}

		internal static string ParameterDoesNotExistInFunction(object p0, object p1, object p2)
		{
			return SR.GetString("ParameterDoesNotExistInFunction", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string SameColumnStorageIdentifier(object p0, object p1)
		{
			return SR.GetString("SameColumnStorageIdentifier", new object[]
			{
				p0,
				p1
			});
		}

		internal static string StorageCannotBeTheSameAsTypeName(object p0, object p1, object p2)
		{
			return SR.GetString("StorageCannotBeTheSameAsTypeName", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string AssociationManysideIsForeignKey(object p0, object p1)
		{
			return SR.GetString("AssociationManysideIsForeignKey", new object[]
			{
				p0,
				p1
			});
		}

		internal static string PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsDbGenerated(object p0, object p1)
		{
			return SR.GetString("PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsDbGenerated", new object[]
			{
				p0,
				p1
			});
		}

		internal static string PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsVersion(object p0, object p1)
		{
			return SR.GetString("PrimaryKeyAutoSyncNeedsToBeOnInsertWhenIsVersion", new object[]
			{
				p0,
				p1
			});
		}

		internal static string OtherSideHasNoPrimaryKey(object p0, object p1, object p2)
		{
			return SR.GetString("OtherSideHasNoPrimaryKey", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string InvalidIdentityMememberType(object p0, object p1, object p2, object p3)
		{
			return SR.GetString("InvalidIdentityMememberType", new object[]
			{
				p0,
				p1,
				p2,
				p3
			});
		}

		internal static string KeylessTablesCannotHaveCUDOverrides(object p0)
		{
			return SR.GetString("KeylessTablesCannotHaveCUDOverrides", new object[]
			{
				p0
			});
		}

		internal static string UserDefinedDbType(object p0, object p1, object p2, object p3, object p4, object p5)
		{
			return SR.GetString("UserDefinedDbType", new object[]
			{
				p0,
				p1,
				p2,
				p3,
				p4,
				p5
			});
		}

		internal static string SubTypesCannotContainPK(object p0, object p1)
		{
			return SR.GetString("SubTypesCannotContainPK", new object[]
			{
				p0,
				p1
			});
		}

		internal static string TypeNameIsReserved(object p0)
		{
			return SR.GetString("TypeNameIsReserved", new object[]
			{
				p0
			});
		}
	}
}
