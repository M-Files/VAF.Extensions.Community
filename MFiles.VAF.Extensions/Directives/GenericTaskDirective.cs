using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class GenericTaskDirective<T> : TaskDirectiveWithDisplayName
	{
		public GenericTaskDirective(T value, string displayName = null)
		{
			this.DisplayName = displayName;
			this.Value = value;
		}

		[DataMember]
		public T Value { get; set; }
	}

	[DataContract]
	public class GenericTaskDirective<T, Y> : GenericTaskDirective<T>
	{
		public GenericTaskDirective(T value, Y value2, string displayName = null) : base(value, displayName)
		{
			this.Value2 = value2;
		}

		[DataMember]
		public Y Value2 { get; set; }
	}

	[DataContract]
	public class GenericTaskDirective<T, Y, Z> : GenericTaskDirective<T, Y>
	{
		public GenericTaskDirective(T value, Y value2, Z value3, string displayName = null) : base(value, value2, displayName)
		{
			this.Value3 = value3;
		}

		[DataMember]
		public Z Value3 { get; set; }
	}
}
