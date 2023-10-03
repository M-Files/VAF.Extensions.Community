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

        protected IWebhook Details { get; }
        public string WebhookName => this.Details?.Name;
        
        protected MethodInfo MethodInfo { get; }
		public IEnumerable<T> GetCustomMethodAttributes<T>() 
			where T : Attribute
			=> this.MethodInfo?.GetCustomAttributes<T>();

        protected object Instance { get; }

		protected ConfigurableVaultApplicationBase<TConfiguration> VaultApplication { get; }

		/// <inheritdoc />
        public bool HasSeparateEventHandlerProxy => (this.Details as WebhookAttribute)?.HasSeparateEventHandlerProxy ?? true;

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
			ConfigurableVaultApplicationBase<TConfiguration> vaultApplication,
			IWebhook details,
            MethodInfo methodInfo, 
            object instance
        )
        {
            this.VaultApplication = vaultApplication
				?? throw new ArgumentNullException(nameof(vaultApplication));
            this.Details = details ?? throw new ArgumentNullException(nameof(details));

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
				var authenticator = this.VaultApplication.WebhookAuthenticationConfigurationManager.GetAuthenticator(this.WebhookName);

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
				if (this.Details is IAsynchronousWebhook)
					result = this.QueueAsynchronousWebhook(environment);
				else
					result = this.Execute(environment);

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

		protected virtual AnonymousExtensionMethodResult QueueAsynchronousWebhook(EventHandlerEnvironment env)
		{
			// Sanity.
			if (!(this.Details is IAsynchronousWebhook a))
				throw new InvalidOperationException($"Webhook {this.Details?.Name} is marked as asynchronous but does not implement IAsynchronousWebhook");
			var queueId = a.TaskQueueId ?? this.VaultApplication.GetAsynchronousWebhookTaskQueueConfiguration()?.QueueID;
			var taskType = a.TaskQueueTaskType ?? this.VaultApplication.GetAsynchronousWebhookTaskQueueConfiguration()?.TaskType;
			if (string.IsNullOrWhiteSpace(queueId) || string.IsNullOrWhiteSpace(taskType))
				throw new InvalidOperationException($"Webhook {this.Details?.Name} is marked as asynchronous, but does not have an allocated task queue and type.");

			// Add the task to the queue.
			this.VaultApplication.TaskManager.AddTask
			(
				env.Vault,
				queueId,
				taskType,
				new AsynchronousWebhookTaskDirective(env)
			);

			// Generate the output.
			var output = new WebhookOutput();
			var responseText = a.GetResponseText();
			var contentType = a.GetContentType();
			if (!string.IsNullOrWhiteSpace(responseText))
				output.ResponseBody = Encoding.UTF8.GetBytes(responseText);
			if (!string.IsNullOrWhiteSpace(contentType))
			{
				output.ResponseHeaders = output.ResponseHeaders ?? new NamedValues();
				output.ResponseHeaders["Content-Type"] = contentType;
			}
			return output.AsAnonymousExtensionMethodResult();
		}

		protected virtual ISerializer CreateSerializerFromType(Type serializerType)
		{
			if (null == serializerType)
				throw new ArgumentNullException(nameof(serializerType));
			if (!typeof(ISerializer).IsAssignableFrom(serializerType))
			{
				var e = new InvalidOperationException("Webhook serializer declaration invalid.");
				this.Logger.Fatal(e, $"Serializer type {serializerType.FullName} does not implement ISerializer.");
				throw e;
			}
			return Activator.CreateInstance(serializerType) as ISerializer;
		}

		/// <summary>
		/// Ensures that the method signature is one of the expected ones,
		/// then calls <see cref="MethodInfo"/>.
		/// </summary>
		/// <param name="env">The environment to pass to the method.</param>
		/// <returns>The result of the call to the method, or an empty <see cref="AnonymousExtensionMethodResult"/>.</returns>
		public virtual AnonymousExtensionMethodResult Execute(EventHandlerEnvironment env)
		{
			// Work out what we need to provide to the method.
			var returnType = MethodInfo.ReturnType;
			var parameters = MethodInfo.GetParameters();

			// What is the output?
			object output = null;

			// Validate the serializers.
			ISerializer requestSerializer = this.CreateSerializerFromType
			(
				this.Details.IncomingSerializerType
					?? this.Details.DefaultSerializerType
					?? typeof(NewtonsoftJsonSerializer)
			);
			ISerializer responseSerializer = this.CreateSerializerFromType
			(
				this.Details.OutgoingSerializerType
					?? this.Details.DefaultSerializerType
					?? typeof(NewtonsoftJsonSerializer)
			);

			// Is it the type that takes an input and output?
			if ((parameters.Length == 1 || parameters.Length == 2))
			{
				switch (parameters.Length)
				{
					case 1:
						// public void XXXX(EventHandlerEnvironment env)
						// public WebhookOutput XXXX(EventHandlerEnvironment env);
						if (parameters[0].ParameterType != typeof(EventHandlerEnvironment))
						{
							{
								var e = new InvalidOperationException("Method signature not valid");
								this.Logger.Fatal(e, $"Parameter 1 is not assignable to EventHandlerEnvironment.");
								throw e;
							}
						}
						output = this.MethodInfo.Invoke(this.Instance, new[] { env });
						break;
					case 2:
						if (parameters[0].ParameterType != typeof(EventHandlerEnvironment))
						{
							{
								var e = new InvalidOperationException("Method signature not valid");
								this.Logger.Fatal(e, $"Parameter 1 is not assignable to EventHandlerEnvironment.");
								throw e;
							}
						}
						// public void XXXX(EventHandlerEnvironment env, MyType input);
						// public WebhookOutput XXXX(EventHandlerEnvironment env, MyType input);
						// public WebhookOutput<MyType> XXXX(EventHandlerEnvironment env, MyType input);
						if (!requestSerializer.CanDeserialize(parameters[1].ParameterType))
						{
							{
								var e = new InvalidOperationException($"Deserializer {requestSerializer.GetType().FullName} cannot deserialize to {parameters[1].ParameterType}.");
								this.Logger.Fatal(e, $"Deserializer {requestSerializer.GetType().FullName} cannot deserialize to {parameters[1].ParameterType}.");
								throw e;
							}
						}
						var input = requestSerializer.Deserialize(env.InputBytes, parameters[1].ParameterType);
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
			if(!responseSerializer.CanSerialize(output.GetType()))
			{
				{
					var e = new InvalidOperationException($"Serializer {responseSerializer.GetType().FullName} cannot serialize to {output.GetType()}.");
					this.Logger.Fatal(e, $"Serializer {responseSerializer.GetType().FullName} cannot serialize to {output.GetType()}.");
					throw e;
				}
			}
			return new AnonymousExtensionMethodResult()
			{
				OutputBytesValue = responseSerializer.Serialize(output)
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

			// Is it enabled?
			if (!this.Details.Enabled)
			{
				this.Logger.Warn($"Web hook {this.WebhookName} is not enabled.  Request is being denied.");
				return false;
			}

			// Validate that this type is okay for the declaration.
			if ((authenticator == null || authenticator is BlockAllRequestsWebhookAuthenticator)
                    && !this.Details.SupportsNoAuthentication)
            {
                this.Logger.Warn($"Web hook {this.WebhookName} requires authentication, but no authentication is configured.  Request is being denied.");
                throw new UnauthorizedAccessException();
            }
			if (authenticator == null)
			{
				// No authenticator, but the attribute supports no authentication, so...  Awesome.
				return true;
			}
            //if (!this.Details.SupportsAuthenticator(authenticator.GetType()))
            //{
            //    this.Logger.Warn($"Web hook {this.WebhookName} does not support authenticator of type {authenticator.GetType().FullName}.  Request is being denied.");
            //    throw new UnauthorizedAccessException();
            //}

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
