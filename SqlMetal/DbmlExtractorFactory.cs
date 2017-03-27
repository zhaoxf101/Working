using System;

namespace SqlMetal
{
	internal static class DbmlExtractorFactory
	{
		internal static IDatabaseExtractor CreateExtractor()
		{
			return DbmlExtractor.Create();
		}
	}
}
