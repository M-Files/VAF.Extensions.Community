using System.Collections.Generic;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Dashboard
{
	public static class VaultApplicationBaseExtensionMethods
	{
		public static ConfigurationDomainCommand CreateConfigurationDomainCommand
		(
			this VaultApplicationBase vaultApplicationBase,
			string commandId,
			string extMethod,
			params object[] additionalParameters
		)
		{
			var commandParams = new List<object> { extMethod };
			if( additionalParameters != null && additionalParameters.Length > 0 )
			{
				commandParams.AddRange( additionalParameters );
			}
			var extMethodCall = new VaultExtensionMethodCall
			{
				Method = $"{vaultApplicationBase.GetType().Namespace}.{nameof( MFEventHandlerType.MFEventHandlerVaultExtensionMethod )}",
				Params = commandParams.ToArray()
			};
			var extMethodSrc = new VaultExtensionMethodSource { Read = extMethodCall };
			var extMethodSrcDef = new MethodSourceDefinition( extMethodSrc );

			return new ConfigurationDomainCommand
			{
				ID = commandId,
				ExtensionMethod = extMethodSrcDef
			};
		}


	}

}
