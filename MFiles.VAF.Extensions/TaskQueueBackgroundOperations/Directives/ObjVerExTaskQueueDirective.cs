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
		: TaskQueueDirectiveWithDisplayName
	{
		/// <summary>
		/// Parse-able ObjVerEx string.
		/// </summary>
		public string ObjVerEx { get; set; }

		/// <summary>
		/// Attempts to get an <see cref="Common.ObjVerEx"/> instance from <see cref="ObjVerEx"/>.
		/// </summary>
		/// <param name="vault">The vault to use when creating the ObjVerEx.</param>
		/// <param name="objVerEx">The ObjVerEx loaded.</param>
		/// <returns><see langword="true"/>if the ObjVerEx could be extracted.</returns>
		public bool TryGetObjVerEx(Vault vault, out ObjVerEx objVerEx)
		{
			return Common.ObjVerEx.TryParse(vault, this.ObjVerEx, out objVerEx);
		}

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
