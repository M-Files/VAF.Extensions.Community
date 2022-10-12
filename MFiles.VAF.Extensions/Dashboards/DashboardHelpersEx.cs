using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards
{
	public static class DashboardHelpersEx
	{
		/// <summary>
		/// Converts an icon image file to a valid path.
		/// Also works with resources.
		/// </summary>
		/// <param name="iconUri">The icon URI.</param>
		/// <returns>A value that can be used as an icon source.</returns>
		public static string ImageFileToDataUri(string iconUri, Assembly assembly = null)
		{
			// Sanity.
			if(string.IsNullOrWhiteSpace(iconUri))
				return string.Empty;

			// If it starts with http/https then assume remote and reference it.
			if (iconUri.StartsWith("http:") || iconUri.StartsWith("https:"))
				return iconUri;
			// If it exists on the disk then load that and convert to base64.
			else if (System.IO.File.Exists(iconUri))
				return DashboardHelper.ImageFileToDataUri(iconUri);
			else
			{

				// Default to the calling assembly.
				assembly = assembly ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
				if (null == assembly)
					return String.Empty;

				// Is it in a resource?
				foreach (var resource in assembly.GetManifestResourceNames())
				{
					// Is this good enough?
					if (resource.EndsWith(iconUri.Replace("/", ".")))
					{
						// Resolve the mime type.
						string mimeType = "image/unknown";
						string ext = iconUri.Substring(iconUri.LastIndexOf(".")).ToLower();
						Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
						if (regKey != null && regKey.GetValue("Content Type") != null)
							mimeType = regKey.GetValue("Content Type").ToString();

						// Base64 encode the image file content.
						using (var memoryStream = new System.IO.MemoryStream())
						{
							using (var stream = assembly.GetManifestResourceStream(resource))
							{
								stream.CopyTo(memoryStream);
							}
							// Create and return the data uri.
							return String.Format
							(
								"data:{0};base64,{1}",
								mimeType,
								Convert.ToBase64String(memoryStream.ToArray())
							);
						}

					}
				}

				// If we're not in the executing assembly then check that.
				if (assembly != Assembly.GetExecutingAssembly())
					return ImageFileToDataUri(iconUri, Assembly.GetExecutingAssembly());

				return String.Format("'{0}'", iconUri);
			}
		}
	}
}
