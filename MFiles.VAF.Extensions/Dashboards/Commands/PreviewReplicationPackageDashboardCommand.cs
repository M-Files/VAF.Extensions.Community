using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.ClientDirective;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using MFilesAPI.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public class PreviewReplicationPackageDashboardCommand<TConfiguration>
		: ReplicationPackageDashboardCommandBase
		where TConfiguration : class, new()
	{

		/// <summary>
		/// The logger to use for this class.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(PreviewReplicationPackageDashboardCommand<TConfiguration>));

		protected ImportReplicationPackageDashboardCommand<TConfiguration> ImportCommand { get; set; }

		protected const string ResourcePrefix = "MFiles.VAF.Extensions.Resources.ImportReplicationPackage.";

		/// <summary>
		/// The name of the resource to use for previewing.
		/// </summary>
		public string XsltResourceName { get; set; } = ResourcePrefix + "vault_impact_projection";

		/// <summary>
		/// Creates a command which, when clicked, will import a replication package.
		/// </summary>
		/// <param name="vaultApplication">The vault application for this command.</param>
		/// <param name="commandId">The ID of this command - must be unique within the application.</param>
		/// <param name="displayName">What to display for this command, in context menus etc.</param>
		/// <param name="replicationPackagePath">The path to the replication package.  Can either be to a .zip file or to the index.xml file of a ready-extracted package.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="vaultApplication"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="replicationPackagePath"/> does not exist.</exception>
		public PreviewReplicationPackageDashboardCommand
		(
			ConfigurableVaultApplicationBase<TConfiguration> vaultApplication,
			string commandId,
			string displayName,
			string replicationPackagePath,
			ImportReplicationPackageDashboardCommand<TConfiguration> importCommand = null
		)
			: base(vaultApplication, commandId, displayName, replicationPackagePath)
		{
			this.ImportCommand = importCommand;

			// When it's executed generate the preview and
			// show it in a popup dashboard.
			this.Execute = (c, o) =>
			{
				// Create the dashboard content.
				var content = this.GetPreview(c.Vault, o);

				// Show it.
				o.Directives.Add(new ShowModalDashboard()
				{
					Content = content
				});
			};
		}

		public enum ReferencedFileType
		{
			Unknown = 0,
			XhtmlFile = 1,
			StyleSheet = 2,
			Script = 3,
			Image = 4
		}

		private static Regex getStylesheetImageRegex = new Regex
			(
				"(?<url>url\\s*?\\(\\s*?(?<path>[^\\s\\)]*?)\\s*?\\)\\s*?)",
				RegexOptions.Multiline
					| RegexOptions.CultureInvariant
					| RegexOptions.Compiled
			);

		protected virtual Regex GetStylesheetImageRegex()
			=> getStylesheetImageRegex;
		public virtual string ReplaceStylesheetImages(Match input)
		{
			// Sanity.
			if (input == null || false == input.Success)
				return null;

			// Get the image.
			var path = input.Groups["path"].Value;
			return $"url(data:image/svg+xml;base64,{this.GetReferencedFileContent(ReferencedFileType.Image, path)})";
		}

		public string ReplaceReferencedFileContent(string input, ReferencedFileType type)
		{
			if (null == input)
				return null;
			switch(type)
			{
				case ReferencedFileType.XhtmlFile:
					{
						var xDoc = XDocument.Parse(input);
						// Replace stylesheets.
						foreach(var e in xDoc.XPathSelectElements("//link[@rel='stylesheet']"))
						{
							// Get the path to the stylesheet.
							var path = e.Attribute("href").Value;

							// Create the new style element and replace the matched one.
							var style = new XElement("style");
							style.SetAttributeValue("type", "text/css");
							var v =  this.GetReferencedFileContent
							(
								ReferencedFileType.StyleSheet,
								path
							);
							style.Value = v;
							e.ReplaceWith(style);
						}
						// Replace scripts.
						foreach (var e in xDoc.XPathSelectElements("//script[@src]"))
						{
							// Get the path to the script.
							var path = e.Attribute("src").Value;

							// Create the new style element and replace the matched one.
							var script = new XElement("script");
							script.SetAttributeValue("type", "text/javascript");
							script.Value = this.GetReferencedFileContent
							(
								ReferencedFileType.Script,
								path
							);
							e.ReplaceWith(script);
						}
						// Replace images.
						foreach (var e in xDoc.XPathSelectElements("//img[@src]"))
						{
							// Skip ones that are already encoded.
							var path = e.Attribute("src").Value;
							if (path.StartsWith("data:image"))
								continue;

							// Update the value.
							e.SetAttributeValue
							(
								"src",
								$"url(data:image/svg+xml;base64,{this.GetReferencedFileContent(ReferencedFileType.Image, path )})"
							);
						}

						// Return the document.
						return xDoc.ToString();
					}
				case ReferencedFileType.StyleSheet:
					// Replace images.
					return this.GetStylesheetImageRegex()?
						.Replace(input, this.ReplaceStylesheetImages);
				default:
					return input;
			}
		}

		public string GetReferencedFileContent(ReferencedFileType type, string filename)
		{
			var resourceName = ResourcePrefix + filename.Replace("/", ".");
			string output = null;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				switch(type)
				{
					case ReferencedFileType.StyleSheet:
					case ReferencedFileType.Script:
						// Read and replace data as appropriate.
						using (var reader = new StreamReader(stream))
						{
							output = ReplaceReferencedFileContent
							(
								reader.ReadToEnd(),
								type
							);
						}
						break;
					case ReferencedFileType.Image:
						// Read and base64-encode.
						using(var base64Stream = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read))
						{
							using (var reader = new StreamReader(base64Stream))
							{
								output = reader.ReadToEnd();
							}
						}
						break;
				}
			}
			return output;
		}

		protected virtual string GetPreview(FileInfo xmlFile, ClientOperations clientOps)
		{
			// Sanity.
			if (null == xmlFile || false == xmlFile.Exists)
				return null;

			// Initialize the xml transformation.
			var xmlResolver = new XmlUrlResolver();
			XmlReaderSettings readerSettings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Parse
			};
			var assembly = Assembly.GetExecutingAssembly();
			using (var xmlReader = XmlReader.Create
				(
					assembly.GetManifestResourceStream($"{XsltResourceName}.xsl"),
					readerSettings
				))
			{
				// Read in the source XML document.
				xmlReader.Read();

				System.Xml.Xsl.XsltSettings xsltSettings = new System.Xml.Xsl.XsltSettings
				{
					EnableDocumentFunction = true
				};

				System.Xml.Xsl.XsltArgumentList argList = new System.Xml.Xsl.XsltArgumentList();

				// Check if the correct translations file exists and set it as xsl parameter (if exists).
				string culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
				string cultureSpecificResource = $"{XsltResourceName}.translations.{culture}.xml";
				if (assembly.GetManifestResourceNames().Contains(cultureSpecificResource))
				{
					{
						XmlDocument doc = new XmlDocument();
						doc.Load(assembly.GetManifestResourceStream(cultureSpecificResource));
						argList.AddParam("localeUrl", "", doc);
					}
				}
				else
				{
					{
						XmlDocument doc = new XmlDocument();
						doc.Load(assembly.GetManifestResourceStream($"{XsltResourceName}.translations.xml"));
						argList.AddParam("localeUrl", "", doc);
					}
				}

				// Transform the XML to HTML.
				System.Xml.Xsl.XslCompiledTransform xslTransform = new System.Xml.Xsl.XslCompiledTransform(true);
				xslTransform.Load(xmlReader, xsltSettings, xmlResolver);
				using (var stringWriter = new StringWriter())
				{
					using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings()
					{
						OmitXmlDeclaration = true
					}))
					{
						xslTransform.Transform(xmlFile.FullName, argList, xmlWriter);
						xmlReader.Close();

						// Build up the json to call the import method.
						var importMethod = this.ImportCommand != null
							? clientOps.Manager.CreateCommandMethodSource
							(
								clientOps.DefaultNodeLocation, 
								this.ImportCommand.ID
							)
							: null;
						string importMethodJson = null != importMethod
							? Newtonsoft.Json.JsonConvert.SerializeObject(importMethod)
							: "null";

						// Return the transformed string.
						return this.ReplaceReferencedFileContent
						(
							stringWriter.ToString(),
							ReferencedFileType.XhtmlFile
						)
							.Replace("%IMPORT_METHOD%", importMethodJson)
							.Replace("&#xD;&#xA;", "\\n");
					}
				}
			}

		}

		public virtual string GetPreview(Vault vault, ClientOperations clientOps)
		{
			try
			{
				// Create the import job.
				ImportContentJob importcontentjob = this.CreateImportContentJob(out IDisposable disposable);
				using(disposable ?? new EmptyDisposable())
				{
					// Run the import job in preview mode. Create a temporary file for the output xml.
					var xmlOutput = disposable is FileDownloadLocation l
						? new FileInfo(Path.Combine(l.Directory.FullName, "preview.xml"))
						: new FileInfo(Path.GetTempFileName());
					vault.ManagementOperations.PreviewImportContent
					(
						importcontentjob, 
						xmlOutput.FullName
					);

					// Convert the XML to something we can see.
					return this.GetPreview(xmlOutput, clientOps);
				}
			}
			catch(Exception e)
			{
				this.Logger?.Warn(e, $"Unable to create preview of replication package import.");
				return "Unable to create preview of replication package import.";
			}
		}


	}
}
