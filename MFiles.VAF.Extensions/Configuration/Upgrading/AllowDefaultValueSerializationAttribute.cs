using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration.Upgrading
{
	[AttributeUsage
	(
		AttributeTargets.Property | AttributeTargets.Field, 
		AllowMultiple = false, 
		Inherited = true
	)]
	public class AllowDefaultValueSerializationAttribute
		: Attribute
	{
	}
}
