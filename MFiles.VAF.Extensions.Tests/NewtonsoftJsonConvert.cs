using MFiles.VAF.Configuration.JsonAdaptor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class NewtonsoftJsonConvertTests
	{
		[DataContract]
		public class Configuration
		{
			[DataMember]
			public SearchConditionsJA SearchConditions { get; set; }
		}

		[TestMethod]
		public void SearchConditions()
		{
			Configuration config = new Configuration();
			config.SearchConditions = new SearchConditionsJA();
			config.SearchConditions.Add(new SearchConditionJA()
			{
				ConditionType = MFilesAPI.MFConditionType.MFConditionTypeEqual,
				Expression = new ExpressionJA()
				{
					PropertyDef = 0,
					DataType = MFilesAPI.MFDataType.MFDatatypeText
				},
				TypedValue = new TypedValueJA()
				{
					DataType = MFilesAPI.MFDataType.MFDatatypeText,
					Value = "hello world"
				}
			});
			config.SearchConditions.Add(new SearchConditionJA()
			{
				ConditionType = MFilesAPI.MFConditionType.MFConditionTypeEqual,
				Expression = new ExpressionJA()
				{
					PropertyDef = 123,
					DataType = MFilesAPI.MFDataType.MFDatatypeBoolean
				},
				TypedValue = new TypedValueJA()
				{
					DataType = MFilesAPI.MFDataType.MFDatatypeBoolean,
					Value = true
				}
			});

			var serializer = new NewtonsoftJsonConvert();
			var x = serializer.Serialize(config);
			Assert.IsNotNull(x);

		}
	}
}
