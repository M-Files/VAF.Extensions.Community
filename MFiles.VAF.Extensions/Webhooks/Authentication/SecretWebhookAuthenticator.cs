using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace MFiles.VAF.Extensions.Webhooks.Authentication
{
    public class SecretWebhookAuthenticator
        : WebhookAuthenticatorBase
    {

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

        protected virtual bool IsValid(NamedValues httpHeaders)
        {
            if (null == httpHeaders)
                return false;
            if (string.IsNullOrWhiteSpace(this.Key))
                return false;
            if (string.IsNullOrWhiteSpace(this.SecretValue))
                return false;
            return httpHeaders.GetValueOrEmpty(this.Key) == this.SecretValue;
        }

        protected virtual bool IsValid(NameValueCollection querystringValues)
        {
            if (null == querystringValues)
                return false;
            if (string.IsNullOrWhiteSpace(this.Key))
                return false;
            if (string.IsNullOrWhiteSpace(this.SecretValue))
                return false;
            var values = querystringValues.GetValues(this.Key);
            if (values.Length != 1)
                return false;
            return values[0] == this?.SecretValue;
        }

        protected override bool AreCredentialsValid(EventHandlerEnvironment env)
        {
            // Sanity.
            if (null == env)
                throw new ArgumentNullException(nameof(env));

            switch(this.Location)
            {
                case SecretLocation.None:
                    return false;
                case SecretLocation.HttpHeader:
                    return this.IsValid(env.InputHttpHeaders);
                case SecretLocation.Querystring:
                    return this.IsValid(HttpUtility.ParseQueryString(env.InputQueryString));
                default:
                    throw new NotImplementedException();
            }
        }

        protected override bool ContainsCredentials(EventHandlerEnvironment env)
        {
            // Sanity.
            if (null == env)
                throw new ArgumentNullException(nameof(env));

            switch (this.Location)
            {
                case SecretLocation.None:
                    return false;
                case SecretLocation.HttpHeader:
                    return this.ContainsAuthenticationDetails(env.InputHttpHeaders);
                case SecretLocation.Querystring:
                    return this.ContainsAuthenticationDetails(HttpUtility.ParseQueryString(env.InputQueryString));
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual bool ContainsAuthenticationDetails(NamedValues httpHeaders)
        {
            if (null == httpHeaders)
                return false;
            if (string.IsNullOrWhiteSpace(this.Key))
                return false;
            return !string.IsNullOrWhiteSpace(httpHeaders?.GetValueOrEmpty(this.Key));
        }

        public virtual bool ContainsAuthenticationDetails(NameValueCollection querystringValues)
        {
            if (null == querystringValues)
                return false;
            if (string.IsNullOrWhiteSpace(this.Key))
                return false;
            if (string.IsNullOrWhiteSpace(this?.SecretValue))
                return false;
            var values = querystringValues.GetValues(this.Key);
            return values != null && values.Length > 0
                && !string.IsNullOrWhiteSpace(values[0]);
        }
    }
}
