using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    public class SecretWebhookAuthenticator
        : WebhookAuthenticatorBase
	{
		[DataContract]
		[JsonConfEditor(NameMember = nameof(Key))]
		public class Secret
			: VAF.Extensions.Configuration.ConfigurationCollectionItemBase
		{
			/// <inheritdoc />
			[DataMember]
			public bool Enabled { get; set; } = false;

			[DataMember]
			[JsonConfEditor(DefaultValue = SecretLocation.HttpHeader)]
			public SecretLocation Location { get; set; } = SecretLocation.HttpHeader;

			[DataMember]
			[JsonConfEditor
			(
				HelpText = "The name of the HTTP header or the querystring item name."
			)]
			public string Key { get; set; }

			[DataMember]
			[Security(IsPassword = true)]
			[JsonConfEditor
			(
				HelpText = "The value to compare."
			)]
			public string SecretValue { get; set; }

		}
		private ILogger Logger { get; }
			= LogManager.GetLogger<SecretWebhookAuthenticator>();

		[DataMember]
		[JsonConfEditor(ChildName = "Secret")]
		public List<Secret> Secrets { get; set; }
			= new List<Secret>();

        public enum SecretLocation
        {
            None = 0,
            HttpHeader = 1,
            Querystring = 2
        }

        public SecretWebhookAuthenticator()
            : base(WebhookAuthenticationType.Secret)
        {
		}

		/// <inheritdoc />
		public override IEnumerable<ValidationFinding> CustomValidation(Vault vault, string webhookName)
		{
			if (!(this.Secrets?.Any() ?? false))
			{
				yield return new ValidationFinding
				(
					ValidationFindingType.Warning,
					$"WebhookConfiguration.{webhookName}",
					$"No credentials are stored for webhook {webhookName}.  This webhook may not be able to be called."
				);
			}

			// Check each item.
			foreach (var c in this.Secrets)
			{
				if (c != null)
					continue;

				switch(c.Location)
				{
					case SecretLocation.HttpHeader:
					case SecretLocation.Querystring:
						break; // Fine.
					default:
						yield return new ValidationFinding
						(
							ValidationFindingType.Info,
							$"WebhookConfiguration.{webhookName}",
							$"A secret location type of {c.Location} was used, but unexpected.  This secret key-value pair will be ignored."
						);
						break;
				}

				if (string.IsNullOrWhiteSpace(c.Key) || string.IsNullOrWhiteSpace(c.SecretValue))
				{
					yield return new ValidationFinding
					(
						ValidationFindingType.Warning,
						$"WebhookConfiguration.{webhookName}",
						$"An empty key or secret is stored.  This is a security concern."
					);
				}
			}
		}

		protected virtual bool IsValid(NamedValues httpHeaders)
        {
            if (null == httpHeaders)
                return false;
			foreach(var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				if (string.IsNullOrWhiteSpace(c.Key))
					continue;
				if (string.IsNullOrWhiteSpace(c.SecretValue))
					continue;
				if (httpHeaders.GetValueOrEmpty(c.Key) == c.SecretValue)
					return true;
			}
			return false;
        }

        protected virtual bool IsValid(NameValueCollection querystringValues)
        {
            if (null == querystringValues)
                return false;
			foreach (var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				if (string.IsNullOrWhiteSpace(c.Key))
					continue;
				if (string.IsNullOrWhiteSpace(c.SecretValue))
					continue;
				var values = querystringValues.GetValues(c.Key);
				if (values.Length != 1)
					return false;
				if(values[0] == c?.SecretValue)
					return true;
			}
			return false;
		}

        protected override bool AreCredentialsValid(EventHandlerEnvironment env)
        {
            // Sanity.
            if (null == env)
                throw new ArgumentNullException(nameof(env));

			foreach (var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				switch (c.Location)
				{
					case SecretLocation.None:
						continue; // Check another.
					case SecretLocation.HttpHeader:
						if (this.IsValid(env.InputHttpHeaders))
							return true;
						break;
					case SecretLocation.Querystring:
						if (this.IsValid(HttpUtility.ParseQueryString(env.InputQueryString)))
							return true;
						break;
					default:
						this.Logger?.Info($"Unhandled credentials location: {c.Location}");
						continue;
				}
			}
			return false;
        }

        protected override bool ContainsCredentials(EventHandlerEnvironment env)
        {
            // Sanity.
            if (null == env)
                throw new ArgumentNullException(nameof(env));
			foreach (var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				switch (c.Location)
				{
					case SecretLocation.None:
						continue;
					case SecretLocation.HttpHeader:
						if (this.ContainsAuthenticationDetails(env.InputHttpHeaders))
							return true;
						break;
					case SecretLocation.Querystring:
						if (this.ContainsAuthenticationDetails(HttpUtility.ParseQueryString(env.InputQueryString)))
							return true;
						break;
					default:
						this.Logger?.Info($"Unhandled credentials location: {c.Location}");
						continue;
				}
			}
			return false;
        }

        protected virtual bool ContainsAuthenticationDetails(NamedValues httpHeaders)
        {
            if (null == httpHeaders)
                return false;
			foreach (var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				if (string.IsNullOrWhiteSpace(c.Key))
					continue;
				if (!string.IsNullOrWhiteSpace(httpHeaders?.GetValueOrEmpty(c.Key)))
					return true;
			}
			return false;
        }

        public virtual bool ContainsAuthenticationDetails(NameValueCollection querystringValues)
        {
            if (null == querystringValues)
                return false;
			foreach (var c in this.Secrets.Where(c => c?.Enabled ?? false))
			{
				if (string.IsNullOrWhiteSpace(c.Key))
					continue;
				if (string.IsNullOrWhiteSpace(c?.SecretValue))
					continue;
				var values = querystringValues.GetValues(c.Key);
				if (values != null && values.Length > 0
					&& !string.IsNullOrWhiteSpace(values[0]))
					return true;
			}
			return false;
        }
    }
}
