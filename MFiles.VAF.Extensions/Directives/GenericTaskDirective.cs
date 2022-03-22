using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class GenericTaskDirective<TA> : TaskDirectiveWithDisplayName
	{
		public GenericTaskDirective()
		{

		}
		public GenericTaskDirective(TA item1, string displayName = null)
			: this()
		{
			this.DisplayName = displayName;
			this.Item1 = item1;
		}

		[DataMember]
		public TA Item1 { get; set; }
	}

	[DataContract]
	public class GenericTaskDirective<TA, TB> : GenericTaskDirective<TA>
	{
		public GenericTaskDirective()
			: base()
		{

		}
		public GenericTaskDirective(TA item1, TB item2, string displayName = null)
			: base(item1, displayName)
		{
			this.Item2 = item2;
		}

		[DataMember]
		public TB Item2 { get; set; }
	}

	[DataContract]
	public class GenericTaskDirective<TA, TB, TC> : GenericTaskDirective<TA, TB>
	{
		public GenericTaskDirective()
			: base()
		{

		}
		public GenericTaskDirective(TA item1, TB item2, TC item3, string displayName = null)
			: base(item1, item2, displayName)
		{
			this.Item3 = item3;
		}

		[DataMember]
		public TC Item3 { get; set; }
	}
}
