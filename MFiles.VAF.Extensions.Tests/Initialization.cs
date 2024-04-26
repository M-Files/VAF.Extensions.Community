using MFilesAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Configuration.Logging.Targets;
using MFiles.VAF.Configuration.Logging.NLog;

namespace MFiles.VAF.Extensions.Tests
{
	[TestClass]
	public class Initialization
	{
		private class MSTestContextTarget 
			: global::NLog.Targets.TargetWithLayout
		{
			protected TestContext TestContext { get; }
			public MSTestContextTarget(TestContext testContext)
			{
				this.TestContext = testContext;
			}
			protected override void Write(NLog.LogEventInfo logEvent)
			{
				this.TestContext.WriteLine(this.RenderLogEvent(Layout, logEvent));
			}
		}
		[AssemblyInitialize]
		public static void MyTestInitialize(TestContext testContext)
		{
			var layout = "${level}:\t${message}\t${logger}${onexception:${newline}${exception:format=ToString:innerformat=ToString:separator=\r\n}}";

			LogManager.Current = new MFiles.VAF.Configuration.Logging.NLog.NLogLogManager();
			LogManager.Initialize(Moq.Mock.Of<Vault>());
			global::NLog.LogManager.Configuration = new NLog.Config.LoggingConfiguration();
			// Output some stuff to the standard output.
			// This means it's associated with each test; click a test to see the standard output (debug lines!)
			global::NLog.LogManager.Configuration.AddRule
			(
				global::NLog.LogLevel.Trace,
				global::NLog.LogLevel.Fatal,
				new MSTestContextTarget(testContext)
				{
					Layout = layout
				}
			);
			global::NLog.LogManager.ReconfigExistingLoggers();

		}
	}
}
