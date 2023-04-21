using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class ReplicationPackageAttribute 
		: Attribute
	{
		public string PackagePath { get; }

		public bool PreviewPackageBeforeImport { get; set; } = true;

		public string ImportCommandId { get; set; }
		public string ImportLabel { get; set; } = "Import";
		public string PreviewCommandId { get; set; }
		public string PreviewLabel { get; set; } = "Preview";

		public ReplicationPackageAttribute(string packagePath)
		{
			if (!System.IO.File.Exists(packagePath))
				throw new ArgumentException($"The package could not be found.");
			this.PackagePath = packagePath;
		}
	}
}
