using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFiles.VaultApplications.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.VAF.Extensions.Dashboards.Commands
{
	public class ShowLatestLogEntriesDashboardCommand
		: DefaultLogDashboardCommandBase
	{
		/// <summary>
		/// The logger for this command.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(ShowLatestLogEntriesDashboardCommand));

		/// <summary>
		/// The ID for the command to show the dashboard that shows the latest log entries.
		/// </summary>
		public static string CommandId = "__ShowLatestLogEntriesDashboard";

		/// <summary>
		/// Location of the dashboard file as an embedded resource.
		/// </summary>
		private const string DashboardResourceId =
				"MFiles.VAF.Extensions.Resources.DisplayLatestLogEntries.html";

#if DEBUG
		/// <summary>
		/// Location of the development dashboard file, for easy development in debug mode.
		/// </summary>
		private const string DebugResourcePath =
				@"C:\Users\craig.hawker\source\repos\MFiles.VAF.Extensions\MFiles.VAF.Extensions\Resources\DisplayLatestLogEntries.html";
#endif

		private ShowLatestLogEntriesDashboardCommand() { }

		public static ShowLatestLogEntriesDashboardCommand Create()
		{
			var command = new ShowLatestLogEntriesDashboardCommand
			{
				ID = CommandId,
				DisplayName = Resources.Dashboard.Logging_Table_ShowLatestLogEntries,
				Locations = new List<ICommandLocation> { }
			};
			command.Execute = command.ShowLogEntriesDashboard;
			return command;
		}

		/// <summary>
		/// Launches the status dashboard in an MFAdmin modal dialog.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="clientOps">Client operations.</param>
		private void ShowLogEntriesDashboard(
			IConfigurationRequestContext context,
			ClientOperations clientOps
		)
		{
			// Check if there are any logs to download.
			List<LogFileInfo> logFiles = ResolveLogFiles(context.Vault);
			if (logFiles.Count == 0)
			{
				// Show a message that there are no files to download and be done.
				clientOps.ShowMessage("There are no log files available for download.");
				return;
			}

			// Resolve the assembly that contains the dashboard template.
			string template = null;

			// Populate the template with the source file if available while in debug.
			// This allows for "live" editing of the file during development.
#if DEBUG
			if (File.Exists(DebugResourcePath))
				template = File.ReadAllText(DebugResourcePath);
#endif

			// Check if we've loaded the template already.
			if (template == null)
			{
				// Load the template from embedded resources.
				Assembly assembly = Assembly.GetExecutingAssembly();
				using (Stream s = assembly.GetManifestResourceStream(DashboardResourceId))
				using (var reader = new StreamReader(s))
					template = reader.ReadToEnd();
			}

			// Inject the log file list into the dashboard template.
			var downloadMethod = clientOps.Manager.CreateCommandMethodSource(
					clientOps.DefaultNodeLocation, RetrieveLatestLogEntriesCommand.CommandId);
			string downloadMethodJson = JsonConvert.SerializeObject(downloadMethod);
			string dashboard = template
				.Replace("%RETRIEVELOGENTRIES_METHOD%", downloadMethodJson)
				.Replace("%IMG_WARNING_DATAURI%", DashboardHelpersEx.ImageFileToDataUri("Resources/Images/warning.png"));

			// Show the dashboard.
			clientOps.Directives.Add(new VAF.Configuration.Domain.ClientDirective.ShowModalDashboard { Content = dashboard });
		}
	}

	public class RetrieveLatestLogEntriesCommand
		: DefaultLogDashboardCommandBase
	{
		/// <summary>
		/// The logger for this command.
		/// </summary>
		private ILogger Logger { get; } = LogManager.GetLogger(typeof(RetrieveLatestLogEntriesCommand));

		/// <summary>
		/// The ID for the command to retrieve the latest log entries.
		/// </summary>
		public static string CommandId = "__RetrieveLatestLogEntries";

		/// <summary>
		/// The maximum number of lines to scan to try to retrieve data.
		/// Setting this to a larger number may mean that it takes longer to retrieve data,
		/// possibly timing out.
		/// </summary>
		public int MaximumLinesToScan { get; set; } = 10000;

		private RetrieveLatestLogEntriesCommand() { }

		public static RetrieveLatestLogEntriesCommand Create()
		{
			var command = new RetrieveLatestLogEntriesCommand
			{
				ID = CommandId,
				DisplayName = "Retrieve latest log entries",
				// This will be invoked via the dashboard from ShowLatestLogEntriesDashboardCommand.
				Locations = new List<ICommandLocation> {  }
			};
			command.Execute = command.ShowLogEntriesDashboard;
			return command;
		}

		/// <summary>
		/// Launches the status dashboard in an MFAdmin modal dialog.
		/// </summary>
		/// <param name="context">The request context.</param>
		/// <param name="clientOps">Client operations.</param>
		private void ShowLogEntriesDashboard(
			IConfigurationRequestContext context,
			ClientOperations clientOps
		)
		{
			// Resolve the files to download from the input params.
			var envContext = (EventHandlerRequestContext)context;
			int methodIndex = envContext.Environment.InputParams.FindIndex(a => a == CommandId);
			var requestParameters = JsonConvert.DeserializeObject<RequestParameters>
				(
					envContext.Environment.InputParams.Skip(methodIndex + 1).FirstOrDefault() ?? "{}"
				);

			// Create the data to send.
			var logEntries = new LogEntries();

			// If we have no filters then we should die now.
			if(requestParameters.LogLevels.Count == 0)
			{
				// Nothing will ever match; let's not dig through the files.
				clientOps.Directives.Add(new VAF.Configuration.Domain.ClientDirective.UpdateDashboardContent()
				{
					Content = JsonConvert.SerializeObject(logEntries)
				});
				return;
			}

			// Find the latest log file.
			var logFiles = this.ResolveLogFiles(context.Vault)?.OrderByDescending(f => f.LastWrite)?.ToList() ?? new List<LogFileInfo>();
			if (logFiles.Any())
			{
				// We have data, so let's extract what we need.
				try
				{
					string rootPath = ResolveRootLogPath(context.Vault);

					// Do we have a default layout?  If so then we can parse the content.
					{
						logEntries.StructuredEntries = true; // Default to assuming we CAN parse the data.
						var defaultLoggingConfiguration = LogManager.Current.Configuration.GetAllLogTargetConfigurations()
							.FirstOrDefault(c => c is VaultApplications.Logging.NLog.Targets.DefaultTargetConfiguration)
							as VaultApplications.Logging.NLog.Targets.DefaultTargetConfiguration;

						// If it is not using the default layout then we can't parse it.
						if (false == string.IsNullOrEmpty(defaultLoggingConfiguration?.Advanced?.Layout)
							&& defaultLoggingConfiguration?.Advanced?.Layout != "${longdate}\t(v${application-version})\t${logger}\t${log-context}\t${level}:\t${message}${onexception:${newline}${exception:format=ToString:innerformat=ToString:separator=\r\n}}")
							logEntries.StructuredEntries = false;
					}
					var linesRead = 0;

					foreach(var logFile in logFiles)
					{
						// Only read a certain number of entries.
						if (
							logEntries.Entries.Count >= requestParameters.MaximumNumberOfLogEntries
							|| linesRead > this.MaximumLinesToScan)
						{
							logEntries.ReachedMaximumLinesToScan = linesRead > this.MaximumLinesToScan;
							break;
						}

						// Sanity and safety check.
						// All paths must be inside the log folder.
						var fullPath = Path.GetFullPath(Path.Combine(rootPath, logFile.RelativePath));
						if (!fullPath.StartsWith(rootPath) ||
							Path.GetExtension(fullPath).ToLower() != ".log" ||
							!File.Exists(fullPath))
							throw new FileNotFoundException();

						// Read the file.
						{
							var reader = new ReverseLineReader(fullPath);
							var logEntry = new LogEntry();
							foreach (var line in reader)
							{
								linesRead++;
								if (linesRead > this.MaximumLinesToScan)
									break;

								// Add the line to the log entry.
								logEntry.PopulateFromFullLine(line + Environment.NewLine + logEntry.FullLine);

								// If we are dealing with structured entries then we can analyse them further.
								if (logEntries.StructuredEntries)
								{

									// If it is not yet a full line then keep going.
									if (false == LogEntry.IsFullLine(logEntry.FullLine))
									{
										continue; // Keep going until we get the log line.
									}

									// Does the log entry meet our search criteria?
									if (false == requestParameters.ShouldBeShown(logEntry))
									{
										logEntry = new LogEntry();
										continue;
									}
								}

								// Add the log entry.
								logEntries.Entries.Add(logEntry);

								// Only read a certain number of entries.
								if (logEntries.Entries.Count >= requestParameters.MaximumNumberOfLogEntries)
									break;

								// Start creating a new one.
								logEntry = new LogEntry();
							}

						}
					}
				}
				catch(Exception e)
				{
					logEntries.Columns.Clear();
					logEntries.Entries.Clear();
					logEntries.Exception = e.Message;
				}
			}

			// Create the directive.
			clientOps.Directives.Add(new VAF.Configuration.Domain.ClientDirective.UpdateDashboardContent()
			{
				Content = JsonConvert.SerializeObject(logEntries)
			});

		}

		#region Reverse stream reader

		/// <summary>
		/// Takes an encoding (defaulting to UTF-8) and a function which produces a seekable stream
		/// (or a filename for convenience) and yields lines from the end of the stream backwards.
		/// Only single byte encodings, and UTF-8 and Unicode, are supported. The stream
		/// returned by the function must be seekable.
		/// </summary>
		// https://stackoverflow.com/questions/452902/how-to-read-a-text-file-reversely-with-iterator-in-c-sharp
		internal sealed class ReverseLineReader : IEnumerable<string>
		{
			/// <summary>
			/// Buffer size to use by default. Classes with internal access can specify
			/// a different buffer size - this is useful for testing.
			/// </summary>
			private const int DefaultBufferSize = 4096;

			/// <summary>
			/// The stream to read from
			/// </summary>
			private readonly Func<Stream> streamSource;

			/// <summary>
			/// Encoding to use when converting bytes to text
			/// </summary>
			private readonly Encoding encoding;

			/// <summary>
			/// Size of buffer (in bytes) to read each time we read from the
			/// stream. This must be at least as big as the maximum number of
			/// bytes for a single character.
			/// </summary>
			private readonly int bufferSize;

			/// <summary>
			/// Function which, when given a position within a file and a byte, states whether
			/// or not the byte represents the start of a character.
			/// </summary>
			private Func<long, byte, bool> characterStartDetector;

			/// <summary>
			/// Creates a LineReader from a stream source. The delegate is only
			/// called when the enumerator is fetched. UTF-8 is used to decode
			/// the stream into text.
			/// </summary>
			/// <param name="streamSource">Data source</param>
			public ReverseLineReader(Func<Stream> streamSource)
				: this(streamSource, Encoding.UTF8)
			{
			}

			/// <summary>
			/// Creates a LineReader from a filename. The file is only opened
			/// (or even checked for existence) when the enumerator is fetched.
			/// UTF8 is used to decode the file into text.
			/// </summary>
			/// <param name="filename">File to read from</param>
			public ReverseLineReader(string filename)
				: this(filename, Encoding.UTF8)
			{
			}

			/// <summary>
			/// Creates a LineReader from a filename. The file is only opened
			/// (or even checked for existence) when the enumerator is fetched.
			/// </summary>
			/// <param name="filename">File to read from</param>
			/// <param name="encoding">Encoding to use to decode the file into text</param>
			public ReverseLineReader(string filename, Encoding encoding)
				: this(() => File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), encoding)
			{
			}

			/// <summary>
			/// Creates a LineReader from a stream source. The delegate is only
			/// called when the enumerator is fetched.
			/// </summary>
			/// <param name="streamSource">Data source</param>
			/// <param name="encoding">Encoding to use to decode the stream into text</param>
			public ReverseLineReader(Func<Stream> streamSource, Encoding encoding)
				: this(streamSource, encoding, DefaultBufferSize)
			{
			}

			internal ReverseLineReader(Func<Stream> streamSource, Encoding encoding, int bufferSize)
			{
				this.streamSource = streamSource;
				this.encoding = encoding;
				this.bufferSize = bufferSize;
				if (encoding.IsSingleByte)
				{
					// For a single byte encoding, every byte is the start (and end) of a character
					characterStartDetector = (pos, data) => true;
				}
				else if (encoding is UnicodeEncoding)
				{
					// For UTF-16, even-numbered positions are the start of a character.
					// TODO: This assumes no surrogate pairs. More work required
					// to handle that.
					characterStartDetector = (pos, data) => (pos & 1) == 0;
				}
				else if (encoding is UTF8Encoding)
				{
					// For UTF-8, bytes with the top bit clear or the second bit set are the start of a character
					// See http://www.cl.cam.ac.uk/~mgk25/unicode.html
					characterStartDetector = (pos, data) => (data & 0x80) == 0 || (data & 0x40) != 0;
				}
				else
				{
					throw new ArgumentException("Only single byte, UTF-8 and Unicode encodings are permitted");
				}
			}

			/// <summary>
			/// Returns the enumerator reading strings backwards. If this method discovers that
			/// the returned stream is either unreadable or unseekable, a NotSupportedException is thrown.
			/// </summary>
			public IEnumerator<string> GetEnumerator()
			{
				Stream stream = streamSource();
				if (!stream.CanSeek)
				{
					stream.Dispose();
					throw new NotSupportedException("Unable to seek within stream");
				}
				if (!stream.CanRead)
				{
					stream.Dispose();
					throw new NotSupportedException("Unable to read within stream");
				}
				return GetEnumeratorImpl(stream);
			}

			private IEnumerator<string> GetEnumeratorImpl(Stream stream)
			{
				try
				{
					long position = stream.Length;

					if (encoding is UnicodeEncoding && (position & 1) != 0)
					{
						throw new InvalidDataException("UTF-16 encoding provided, but stream has odd length.");
					}

					// Allow up to two bytes for data from the start of the previous
					// read which didn't quite make it as full characters
					byte[] buffer = new byte[bufferSize + 2];
					char[] charBuffer = new char[encoding.GetMaxCharCount(buffer.Length)];
					int leftOverData = 0;
					String previousEnd = null;
					// TextReader doesn't return an empty string if there's line break at the end
					// of the data. Therefore we don't return an empty string if it's our *first*
					// return.
					bool firstYield = true;

					// A line-feed at the start of the previous buffer means we need to swallow
					// the carriage-return at the end of this buffer - hence this needs declaring
					// way up here!
					bool swallowCarriageReturn = false;

					while (position > 0)
					{
						int bytesToRead = Math.Min(position > int.MaxValue ? bufferSize : (int)position, bufferSize);

						position -= bytesToRead;
						stream.Position = position;
						ReadExactly(stream, buffer, bytesToRead);
						// If we haven't read a full buffer, but we had bytes left
						// over from before, copy them to the end of the buffer
						if (leftOverData > 0 && bytesToRead != bufferSize)
						{
							// Buffer.BlockCopy doesn't document its behaviour with respect
							// to overlapping data: we *might* just have read 7 bytes instead of
							// 8, and have two bytes to copy...
							Array.Copy(buffer, bufferSize, buffer, bytesToRead, leftOverData);
						}
						// We've now *effectively* read this much data.
						bytesToRead += leftOverData;

						int firstCharPosition = 0;
						while (!characterStartDetector(position + firstCharPosition, buffer[firstCharPosition]))
						{
							firstCharPosition++;
							// Bad UTF-8 sequences could trigger this. For UTF-8 we should always
							// see a valid character start in every 3 bytes, and if this is the start of the file
							// so we've done a short read, we should have the character start
							// somewhere in the usable buffer.
							if (firstCharPosition == 3 || firstCharPosition == bytesToRead)
							{
								throw new InvalidDataException("Invalid UTF-8 data");
							}
						}
						leftOverData = firstCharPosition;

						int charsRead = encoding.GetChars(buffer, firstCharPosition, bytesToRead - firstCharPosition, charBuffer, 0);
						int endExclusive = charsRead;

						for (int i = charsRead - 1; i >= 0; i--)
						{
							char lookingAt = charBuffer[i];
							if (swallowCarriageReturn)
							{
								swallowCarriageReturn = false;
								if (lookingAt == '\r')
								{
									endExclusive--;
									continue;
								}
							}
							// Anything non-line-breaking, just keep looking backwards
							if (lookingAt != '\n' && lookingAt != '\r')
							{
								continue;
							}
							// End of CRLF? Swallow the preceding CR
							if (lookingAt == '\n')
							{
								swallowCarriageReturn = true;
							}
							int start = i + 1;
							string bufferContents = new string(charBuffer, start, endExclusive - start);
							endExclusive = i;
							string stringToYield = previousEnd == null ? bufferContents : bufferContents + previousEnd;
							if (!firstYield || stringToYield.Length != 0)
							{
								yield return stringToYield;
							}
							firstYield = false;
							previousEnd = null;
						}

						previousEnd = endExclusive == 0 ? null : (new string(charBuffer, 0, endExclusive) + previousEnd);

						// If we didn't decode the start of the array, put it at the end for next time
						if (leftOverData != 0)
						{
							Buffer.BlockCopy(buffer, 0, buffer, bufferSize, leftOverData);
						}
					}
					if (leftOverData != 0)
					{
						// At the start of the final buffer, we had the end of another character.
						throw new InvalidDataException("Invalid UTF-8 data at start of stream");
					}
					if (firstYield && string.IsNullOrEmpty(previousEnd))
					{
						yield break;
					}
					yield return previousEnd ?? "";
				}
				finally
				{
					stream.Dispose();
				}
			}

			public static void ReadExactly(Stream input, byte[] buffer, int bytesToRead)
			{
				int index = 0;
				while (index < bytesToRead)
				{
					int read = input.Read(buffer, index, bytesToRead - index);
					if (read == 0)
					{
						throw new EndOfStreamException
							(String.Format("End of stream reached with {0} byte{1} left to read.",
										   bytesToRead - index,
										   bytesToRead - index == 1 ? "s" : ""));
					}
					index += read;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

		}
		#endregion

		[DataContract]
		internal class RequestParameters
		{
			private int maximumNumberOfLogEntries = 20;

			[DataMember(Name = "maximumNumberOfLogEntries")]
			public int MaximumNumberOfLogEntries
			{
				get => this.maximumNumberOfLogEntries;
				set
				{
					this.maximumNumberOfLogEntries = value;
					if (this.maximumNumberOfLogEntries <= 0)
						this.maximumNumberOfLogEntries = 1;
					if (this.maximumNumberOfLogEntries > 200)
						this.maximumNumberOfLogEntries = 200;
				}
			}
			[DataMember(Name = "logLevels")]
			public List<LogLevel> LogLevels { get; set; } = new List<LogLevel>();

			public RequestParameters()
			{
			}

			public bool ShouldBeShown(LogEntry logEntry)
			{
				if (null == logEntry)
					return false;
				if (logEntry.LogLevel.HasValue && false == this.LogLevels.Contains(logEntry.LogLevel.Value))
					return false;

				return true;
			}

		}

		[DataContract]
		internal class LogEntry : Dictionary<string, string>
		{
			/// <summary>
			/// The regular expression to parse the line.
			/// </summary>
			internal static Regex ParseLineRegularExpression = new Regex
			(
				"^(?<DateTime>[0-9\\-\\:\\.\\sz]{1,})\\t\\(v(?<ApplicationVersion>[\\.0-9]{1,})\\)\\t" +
				"(?<Logger>.*?)\\t(?<LogContext>.*?)\\t(?<LogLevel>.*?)\\:\\t" +
				"(?<Message>.*)",
				RegexOptions.IgnoreCase
					| RegexOptions.Multiline
					| RegexOptions.Singleline
					| RegexOptions.CultureInvariant
					| RegexOptions.Compiled
			);

			public DateTime? DateTime
			{
				get => this.ContainsKey("DateTime")
					? System.DateTime.Parse(this["DateTime"])
					: (DateTime?)null;
			}

			public LogLevel? LogLevel
			{
				get => this.ContainsKey("LogLevel") && Enum.TryParse<LogLevel>(this["LogLevel"], out LogLevel logLevel)
					? logLevel
					: (LogLevel?)null;
			}

			/// <summary>
			/// Returns true if the <paramref name="input"/> is a full line according
			/// to <see cref="ParseLineRegularExpression"/>.
			/// </summary>
			/// <param name="input"></param>
			/// <returns></returns>
			public static bool IsFullLine(string input) => ParseLineRegularExpression.IsMatch(input);

			private static string fullLineKey = "FullLine";
			public string FullLine
			{
				get
				{
					if (false == this.ContainsKey(fullLineKey))
						this.Add(fullLineKey, null);
					return this[fullLineKey];
				}
			}

			public virtual bool PopulateFromFullLine(string fullLine)
			{
				this[fullLineKey] = fullLine;
				
				foreach (var key in this.Keys)
					if (key != fullLineKey)
						this.Remove(key);
				if (string.IsNullOrWhiteSpace(this.FullLine))
					return false;
				var match = ParseLineRegularExpression.Match(this.FullLine);
				if (!match.Success)
					return false;

				// Add each in turn.
				foreach (var name in ParseLineRegularExpression.GetGroupNames())
				{
					this.Add(name, match.Groups[name].Value);
				}
				return true;
			}

			public override string ToString()
				=> this.FullLine;
		}
		[DataContract]
		internal class LogEntries
		{
			[DataMember(Name = "columns")]
			public List<string> Columns { get; } = new List<string>(LogEntry.ParseLineRegularExpression.GetGroupNames());

			[DataMember(Name = "entries")]
			public List<LogEntry> Entries { get; } = new List<LogEntry>();

			[DataMember(Name = "exception")]
			public string Exception { get; set; }

			[DataMember(Name = "structuredEntries")]
			public bool StructuredEntries { get; set; }

			[DataMember(Name = "reachedMaximumLinesToScan")]
			public bool ReachedMaximumLinesToScan { get; set; }
		}
	}
}
