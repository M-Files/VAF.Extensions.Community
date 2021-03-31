using System.Reflection;
using MFiles.VAF.Extensions.MultiServerMode;

namespace MFiles.VAF.Extensions.Dashboard
{
	public class DashboardBackgroundOperationConfiguration
	{
		public ShowRunCommandOnDashboardAttribute Attribute { get; set; }
		public MemberInfo MemberInfo { get; set; }
		public object ParentObject { get; set; }

		public string CommandId => $"{MemberInfo.DeclaringType?.FullName ?? string.Empty}.{MemberInfo.Name}";

		public TaskQueueBackgroundOperation GetValue()
		{
			object propertyValue = null;
			switch( MemberInfo )
			{
				case PropertyInfo propertyInfo:
					propertyValue = propertyInfo.GetValue( ParentObject );
					break;
				case FieldInfo fieldInfo:
					propertyValue = fieldInfo.GetValue( ParentObject );
					break;
			}

			if( propertyValue is TaskQueueBackgroundOperation backgroundOperation )
			{
				return backgroundOperation;
			}

			return null;
		}
	}
}
