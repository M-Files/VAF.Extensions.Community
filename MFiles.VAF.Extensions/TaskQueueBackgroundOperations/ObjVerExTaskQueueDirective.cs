using MFiles.VAF.Common;
using MFiles.VAF;
using MFilesAPI;
using System;
using MFiles.VAF.MultiserverMode;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A <see cref="TaskQueueDirective"/> that represents a single <see cref="ObjVerExTaskQueueDirective.ObjVerEx"/>.
	/// </summary>
	public class ObjVerExTaskQueueDirective
		: TaskQueueDirective
	{
		/// <summary>
		/// Parse-able ObjVerEx string.
		/// </summary>
		public string ObjVerEx { get; set; }

		/// <summary>
		/// Creates a <see cref="ObjVerExTaskQueueDirective"/>
		/// representing the provided <paramref name="objVer"/>.
		/// </summary>
		/// <param name="objVer">The object version to represent.</param>
		/// <returns>The task queue directive for the supplied object version.</returns>
		public static ObjVerExTaskQueueDirective FromObjVer(ObjVer objVer)
		{
			// Sanity.
			if (null == objVer)
				throw new ArgumentNullException(nameof(objVer));

			return new ObjVerExTaskQueueDirective()
			{
				ObjVerEx = objVer.ToString(parsable: true)
			};
		}
		
		/// <summary>
		/// Creates a <see cref="ObjVerExTaskQueueDirective"/>
		/// representing the provided <paramref name="objVerEx"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to represent.</param>
		/// <returns>The task queue directive for the supplied object version.</returns>
		public static ObjVerExTaskQueueDirective FromObjVerEx(ObjVerEx objVerEx)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));

			// Use the other method.
			return ObjVerExTaskQueueDirective.FromObjVer(objVerEx.ObjVer);
		}
	}
}
