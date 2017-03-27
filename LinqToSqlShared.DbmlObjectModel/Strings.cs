using System;

namespace LinqToSqlShared.DbmlObjectModel
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

		internal static string SchemaRequirementViolation(object p0, object p1)
		{
			return SR.GetString("SchemaRequirementViolation", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SchemaDuplicateIdViolation(object p0, object p1)
		{
			return SR.GetString("SchemaDuplicateIdViolation", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SchemaInvalidIdRefToNonexistentId(object p0, object p1, object p2)
		{
			return SR.GetString("SchemaInvalidIdRefToNonexistentId", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string SchemaOrRequirementViolation(object p0, object p1, object p2)
		{
			return SR.GetString("SchemaOrRequirementViolation", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string SchemaUnexpectedElementViolation(object p0, object p1)
		{
			return SR.GetString("SchemaUnexpectedElementViolation", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SchemaUnexpectedAdditionalAttributeViolation(object p0, object p1)
		{
			return SR.GetString("SchemaUnexpectedAdditionalAttributeViolation", new object[]
			{
				p0,
				p1
			});
		}

		internal static string InvalidBooleanAttributeValueViolation(object p0)
		{
			return SR.GetString("InvalidBooleanAttributeValueViolation", new object[]
			{
				p0
			});
		}

		internal static string InvalidEnumAttributeValueViolation(object p0)
		{
			return SR.GetString("InvalidEnumAttributeValueViolation", new object[]
			{
				p0
			});
		}

		internal static string RequiredElementMissingViolation(object p0)
		{
			return SR.GetString("RequiredElementMissingViolation", new object[]
			{
				p0
			});
		}

		internal static string ElementMoreThanOnceViolation(object p0)
		{
			return SR.GetString("ElementMoreThanOnceViolation", new object[]
			{
				p0
			});
		}

		internal static string RequiredAttributeMissingViolation(object p0)
		{
			return SR.GetString("RequiredAttributeMissingViolation", new object[]
			{
				p0
			});
		}

		internal static string SchemaRecursiveTypeReference(object p0, object p1)
		{
			return SR.GetString("SchemaRecursiveTypeReference", new object[]
			{
				p0,
				p1
			});
		}

		internal static string SchemaUnrecognizedAttribute(object p0)
		{
			return SR.GetString("SchemaUnrecognizedAttribute", new object[]
			{
				p0
			});
		}

		internal static string SchemaExpectedEmptyElement(object p0, object p1, object p2)
		{
			return SR.GetString("SchemaExpectedEmptyElement", new object[]
			{
				p0,
				p1,
				p2
			});
		}

		internal static string DatabaseNodeNotFound(object p0)
		{
			return SR.GetString("DatabaseNodeNotFound", new object[]
			{
				p0
			});
		}

		internal static string TypeNameNotUnique(object p0)
		{
			return SR.GetString("TypeNameNotUnique", new object[]
			{
				p0
			});
		}

		internal static string Bug(object p0)
		{
			return SR.GetString("Bug", new object[]
			{
				p0
			});
		}
	}
}
