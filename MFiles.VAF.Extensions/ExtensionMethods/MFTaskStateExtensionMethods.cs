using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	internal static class MFTaskStateExtensionMethods
	{
		public static string ForDisplay(this MFTaskState taskState)
		{
			switch (taskState)
			{
				case MFTaskState.MFTaskStateWaiting:
					return Resources.Dashboard.MFTaskStateWaiting;
				case MFTaskState.MFTaskStateInProgress:
					return Resources.Dashboard.MFTaskStateInProgress;
				case MFTaskState.MFTaskStateCompleted:
					return Resources.Dashboard.MFTaskStateCompleted;
				case MFTaskState.MFTaskStateFailed:
					return Resources.Dashboard.MFTaskStateFailed;
				case MFTaskState.MFTaskStateCanceled:
					return Resources.Dashboard.MFTaskStateCanceled;
				case MFTaskState.MFTaskStateNone:
					return Resources.Dashboard.MFTaskStateNone;
			}
			return "Undefined";
		}
	}
}
