using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
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
		public static string ImageFileToDataUri(string iconUri)
		{
			// Resolve the icon uri. If the icon is a file, convert it to a data-uri,
			// otherwise assume the specified icon is a path that will resolve on the client
			// (just wrap in quotes).
			if (System.IO.File.Exists(iconUri))
				return DashboardHelper.ImageFileToDataUri(iconUri);
			else
			{
				// Is it in a resource?
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				foreach (var resource in assembly.GetManifestResourceNames())
				{
					// TODO: Is this good enough?
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

				return String.Format("'{0}'", iconUri);
			}
		}
	}
}
