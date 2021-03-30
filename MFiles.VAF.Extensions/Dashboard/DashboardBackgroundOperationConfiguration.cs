using System.Reflection;

namespace MFiles.VAF.Extensions.Dashboard
{
	public class DashboardBackgroundOperationConfiguration
	{
		public BackgroundOperationTriggerableByDashboardAttribute Attribute { get; set; }
		public MemberInfo MemberInfo { get; set; }

		public string CommandId => $"{MemberInfo.ReflectedType?.FullName ?? string.Empty}.{MemberInfo.Name}";
	}
}
