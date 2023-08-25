using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Logging;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Webhooks.Authentication;
using MFiles.VAF.Extensions.Webhooks.Configuration;
using MFilesAPI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Webhooks
{
    public class WebhookMethodInfo<TConfiguration>
        : IVaultExtensionMethodInfo
        where TConfiguration : class, new()
    {
        private ILogger Logger = LogManager.GetLogger(typeof(WebhookMethodInfo<TConfiguration>));

        protected WebhookAttribute Attribute { get; }
        protected string WebhookName => this.Attribute?.Name;
        
        protected MethodInfo MethodInfo { get; }
        protected object Instance { get; }

        protected WebhookAuthenticationConfigurationManager<TConfiguration> AuthenticationConfigurationManager { get; }

		/// <inheritdoc />
        public bool HasSeparateEventHandlerProxy => this.Attribute?.HasSeparateEventHandlerProxy ?? true;

		/// <inheritdoc />
		public EventHandlerVaultUserIdentity VaultUserIdentity => EventHandlerVaultUserIdentity.Server;

		/// <inheritdoc />
		public MFEventHandlerType GetEventHandlerType => MFEventHandlerType.MFEventHandlerVaultAnonymousExtensionMethod;

		/// <inheritdoc />
		public string LogString => $"{this.MethodInfo?.DeclaringType?.Name}.{this.MethodInfo.Name}";

		/// <inheritdoc />
		public int Priority => 0;

        public WebhookMethodInfo
        (
            WebhookAuthenticationConfigurationManager<TConfiguration> authenticationConfigurationManager,
            WebhookAttribute attribute,
            MethodInfo methodInfo, 
            object instance
        )
        {
            this.AuthenticationConfigurationManager = authenticationConfigurationManager
                ?? throw new ArgumentNullException(nameof(authenticationConfigurationManager));
            this.Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

            // Override the extension method.
            this.MethodInfo = methodInfo;
            this.Instance = instance;
        }

		/// <inheritdoc />
		public string Execute(EventHandlerEnvironment env, IExecutionTrace trace)
            => throw new InvalidOperationException("Only ExecuteAnonymous is supported");

		/// <inheritdoc />
		public AnonymousExtensionMethodResult ExecuteAnonymous(EventHandlerEnvironment environment, IExecutionTrace trace)
        {
			try
			{
				trace?.TraceBeforeExtensionMethod(environment.ActivityID.DisplayValue, this.LogString);
				this.Logger.Trace($"Executing webhook {this.WebhookName}");

				// Get the authenticator.
				var authenticator = this.AuthenticationConfigurationManager.GetAuthenticator(this.WebhookName);

				// If it is not a valid request then return now.
				AnonymousExtensionMethodResult result;
				if (!this.IsValidRequest(environment, authenticator, out result))
				{
					if (null == result)
						throw new UnauthorizedAccessException();
					this.Logger.Trace($"Call to webhook {this.WebhookName} was not authorised.");
					trace?.TraceAfterExtensionMethod(environment.ActivityID.DisplayValue, this.LogString, null);
					return result;
				}

				// Run it.
				result = this.ExecuteMethod(environment);

				// Awesome.
				this.Logger.Trace($"Webhook completed successfully {this.WebhookName}");
				trace?.TraceAfterExtensionMethod(environment.ActivityID.DisplayValue, this.LogString, null);
				return result;
			}
			catch(Exception e)
			{
				// Notify the execution end also in case of error.
				trace?.TraceAfterExtensionMethod(environment.ActivityID.DisplayValue, this.LogString, e.Message);
				this.Logger?.Warn(e, $"Webhook failed. {this.LogString}");
				throw;
			}
        }

		/// <summary>
		/// Ensures that the method signature is one of the expected ones,
		/// then calls <see cref="MethodInfo"/>.
		/// </summary>
		/// <param name="env">The environment to pass to the method.</param>
		/// <returns>The result of the call to the method, or an empty <see cref="AnonymousExtensionMethodResult"/>.</returns>
        protected virtual AnonymousExtensionMethodResult ExecuteMethod(EventHandlerEnvironment env)
        {

            // Work out what we need to provide to the method.
            var returnType = MethodInfo.ReturnType;
            var parameters = MethodInfo.GetParameters();
            var genericParameters = MethodInfo.GetGenericArguments();

            // What is the output?
            object output = null;

            // Validate the serializer.
            var serializerType = this.Attribute.SerializerType ?? typeof(NewtonsoftJsonSerializer);
            if (!typeof(ISerializer).IsAssignableFrom(serializerType))
            {
                var e = new InvalidOperationException("Webhook serializer declaration invalid.");
                this.Logger.Fatal(e, $"Serializer type does not implement ISerializer.");
                throw e;
            }
            var serializer = Activator.CreateInstance(serializerType) as ISerializer;

            // Is it the simple syntax?
            if (typeof(AnonymousExtensionMethodResult).IsAssignableFrom(returnType)
                && genericParameters.Length == 0
                && parameters.Length == 1
                && parameters[0].ParameterType == typeof(EventHandlerEnvironment))
            {
				// Simple syntax.
				// public AnonymousExtensionMethodResult XXXX(EventHandlerEnvironment env);
				output = this.MethodInfo.Invoke(this.Instance, new[] { env }) as AnonymousExtensionMethodResult;
            }
            else
            {

                // Is it the type that takes an input and output?
                if ((parameters.Length == 1 || parameters.Length == 2))
                {
                    if (parameters[0].ParameterType != typeof(EventHandlerEnvironment))
                    {
                        {
                            var e = new InvalidOperationException("Method signature not valid");
                            this.Logger.Fatal(e, $"Parameter 1 is not assignable to EventHandlerEnvironment.");
                            throw e;
                        }
                    }
                    switch (parameters.Length)
                    {
                        case 1:
                            // public void XXXX(EventHandlerEnvironment env)
                            // public WebhookOutput XXXX(EventHandlerEnvironment env);
                            output = this.MethodInfo.Invoke(this.Instance, new[] { env });
                            break;
                        case 2:
							// public void XXXX(EventHandlerEnvironment env, MyType input);
							// public WebhookOutput XXXX(EventHandlerEnvironment env, MyType input);
							// public WebhookOutput<MyType> XXXX(EventHandlerEnvironment env, MyType input);
                            object input = serializer.Deserialize(env.InputBytes, parameters[1].ParameterType);
                            output = this.MethodInfo.Invoke(this.Instance, new object[] { env, input });
                            break;
                    }
                }
                else
                {
                    {
                        // Method signature not valid.
                        var e = new InvalidOperationException("Method signature not valid");
                        this.Logger.Fatal(e, $"Method return type was incorrect, or incorrect number or type of parameters.");
                        throw e;
                    }
                }
            }

            // If we have no return type needed, create a blank implementation.
            if (returnType == typeof(void))
                return new AnonymousExtensionMethodResult();
            
            // Null means an empty result.
            if (output == null)
                return new AnonymousExtensionMethodResult();

            // If we already have a result then return that.
            if (output is AnonymousExtensionMethodResult r)
                return r;

            // If we can cast to the correct type then return that.
            if (output is IConvertableToAnonymousExtensionMethodResult o)
                return o.AsAnonymousExtensionMethodResult();

            // Serialize it.
            return new AnonymousExtensionMethodResult()
            {
                OutputBytesValue = serializer.Serialize(output)
            };


        }

		/// <summary>
		/// Returns true if the <paramref name="authenticator"/> is both valid for this type of
		/// webhook, and states that the request is valid.
		/// </summary>
		/// <param name="env">The environment representing the request.</param>
		/// <param name="authenticator">The authenticator to use to authorise access.</param>
		/// <param name="output">The output of the authorisation process, if one is provided.</param>
		/// <returns><see langword="true"/> if the request is authorised, <see langword="false"/> otherwise.</returns>
		/// <exception cref="UnauthorizedAccessException"></exception>
        public virtual bool IsValidRequest
        (
            EventHandlerEnvironment env, 
            IWebhookAuthenticator authenticator,
            out AnonymousExtensionMethodResult output
        )
        {
			output = null;

			// Validate that this type is okay for the declaration.
			if ((authenticator == null || authenticator is NoAuthenticationWebhookAuthenticator)
                    && !this.Attribute.SupportsNoAuthentication)
            {
                this.Logger.Warn($"Web hook {this.WebhookName} requires authentication, but no authentication is configured.  Request is being denied.");
                throw new UnauthorizedAccessException();
            }
			if (authenticator == null)
			{
				// No authenticator, but the attribute supports no authentication, so...  Awesome.
				return true;
			}
            if (!this.Attribute.SupportsAuthenticator(authenticator.GetType()))
            {
                this.Logger.Warn($"Web hook {this.WebhookName} does not support authenticator of type {authenticator.GetType().FullName}.  Request is being denied.");
                throw new UnauthorizedAccessException();
            }

			// Authenticate!
            if(!authenticator.IsRequestAuthenticated(env, out output))
            {
                this.Logger.Debug($"Authenticator of type {authenticator.GetType().FullName} stated that the request was not authenticated.  Request is being denied.");
                return false;
            }

            return true;

        }
    }
}
