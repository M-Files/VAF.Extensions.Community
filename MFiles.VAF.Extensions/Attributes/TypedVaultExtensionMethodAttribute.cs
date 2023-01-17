using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Logging;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MFiles.VAF.Extensions
{
    /// <summary>
    /// Defines that the method is a vault extension method that sends/receives data in a custom format.
    /// The method should declare one (single vault) or two (vault plus format of the data in the VEM input) parameters.
    /// The method can optionally have a return type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class TypedVaultExtensionMethodAttribute : Attribute
    {
        /// <summary>
        /// The name of the vault extension method.  Must be unique within a vault.
        /// </summary>
        public string VaultExtensionMethodName { get; }

        /// <summary>
        /// If true, exception details can be included in the response.
        /// </summary>
        public bool IncludeExceptionDetailsInResponse { get; set; }

        /// <summary>
        /// The required level of vault acccess to call this method.  
        /// Defaults to <see cref="MFVaultAccess.MFVaultAccessNone"/>.
        /// </summary>
        public MFVaultAccess RequiredVaultAccess { get; set; }

        public TypedVaultExtensionMethodAttribute
        (
            string vaultExtensionMethodName
        )
        {
            this.VaultExtensionMethodName = vaultExtensionMethodName;
            this.RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone;
        }

        /// <summary>
        /// Deserializes <paramref name="input"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to return.</typeparam>
        /// <param name="input">The VEM input.</param>
        /// <returns>The deserialized object.</returns>
        protected virtual T Deserialize<T>(string input)
            => (T)this.Deserialize(input, typeof(T));

        /// <summary>
        /// Serializes <paramref name="input"/>.
        /// </summary>
        /// <typeparam name="T">The type of the input.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>A serialized version of <paramref name="input"/>.</returns>
        protected virtual string Serialize<T>(T input)
            => this.Serialize((object)input);

        /// <summary>
        /// Deserializes <paramref name="input"/> to <paramref name="t"/>.
        /// </summary>
        /// <param name="input">The VEM input.</param>
        /// <param name="t">The type to deserialize to.</param>
        /// <returns>The deserialized object.</returns>
        protected abstract object Deserialize(string input, Type t);

        /// <summary>
        /// Serializes <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A serialized version of <paramref name="input"/>.</returns>
        protected abstract string Serialize(object input);

        /// <summary>
        /// Retrieves the data that should be returned to the caller if the call did not except.
        /// Only used if the method itself has a return type of void, otherwise the method return value is used.
        /// </summary>
        /// <returns>The data to return to the caller.</returns>
        public abstract string GetSuccessfulOutput();

        /// <summary>
        /// Retrieves the data that should be returned to the caller if the call excepts.
        /// </summary>
        /// <param name="e">Any exception thrown.</param>
        /// <returns>The data to return to the caller.</returns>
        public abstract string GetFailedOutput(Exception e);

        /// <summary>
        /// Converts this attribute to a <see cref="VaultExtensionMethodAttribute"/>
        /// which is used internally within the VAF.
        /// </summary>
        /// <returns>The <see cref="VaultExtensionMethodAttribute"/> instance.</returns>
        protected virtual VaultExtensionMethodAttribute AsVaultExtensionMethodAttribute()
        {
            return new VaultExtensionMethodAttribute(VaultExtensionMethodName)
            {
                RequiredVaultAccess = this.RequiredVaultAccess
            };
        }

        /// <summary>
        /// Converts this attribute to a <see cref="IVaultExtensionMethodInfo"/>
        /// which is used internally within the VAF.
        /// </summary>
        /// <param name="method">The method that this attribute decorates.</param>
        /// <param name="source">The instance of the object which declares <paramref name="method"/>.</param>
        /// <returns>The vault extension method info.</returns>
        public virtual IVaultExtensionMethodInfo AsVaultExtensionMethodInfo(MethodInfo method, object source)
        {
            // Create our proxy, which will be used to actually call the data.
            var proxy = this.AsProxy(method, source);

            // Get this as a vault extension method attribute so that we have the data we need.
            var attribute = this.AsVaultExtensionMethodAttribute();

            // Create the instance of VaultExtensionMethodInfo using the proxy and attribute data.
            return new VaultExtensionMethodInfo(
                    proxy.GetExecuteMethodInfo(),
                    proxy,
                    attribute.RequiredVaultAccess,
                    attribute.HasSeparateEventHandlerProxy,
                    attribute.VaultUserIdentity,
                    MFEventHandlerType.MFEventHandlerVaultExtensionMethod);
        }

        /// <summary>
        /// Gets a <see cref="Proxy"/> instance to use when routing the request.
        /// </summary>
        /// <param name="method">The method that this attribute decorates.</param>
        /// <param name="source">The instance of the object which declares <paramref name="method"/>.</param>
        /// <returns>The proxy.</returns>
        internal Proxy AsProxy(MethodInfo method, object source)
            => new Proxy(this, method, source);

        /// <summary>
        /// Proxies calls from the VAF (which expect VEM-style method declarations) to the style used by these
        /// attributes.  Also deals with ensuring that the VEM input is deserialized to the type needed by the method,
        /// and that any return data from the method is serialized to the VEM output.
        /// </summary>
        internal class Proxy
        {
            /// <summary>
            /// The logger to use for this proxy.
            /// </summary>
            private ILogger Logger { get; } = LogManager.GetLogger(typeof(Proxy));

            /// <summary>
            /// The type of the second parameter on <see cref="Method"/>.
            /// If no second parameter, will be typeof(void).
            /// </summary>
            public Type InputType { get; private set; } = typeof(void);

            /// <summary>
            /// The parameters declared by <see cref="Method"/>.
            /// </summary>
            public ParameterInfo[] ParameterInfo { get; private set; } = new ParameterInfo[0];

            /// <summary>
            /// The method that <see cref="Attribute"/> decorates.
            /// </summary>
            public MethodInfo Method { get; }

            /// <summary>
            /// The instance which declares <see cref="Method"/>.
            /// </summary>
            public object Source { get; }

            /// <summary>
            /// The attribute which made this all get kicked off.
            /// </summary>
            public TypedVaultExtensionMethodAttribute Attribute { get; }

            public Proxy(TypedVaultExtensionMethodAttribute attribute, MethodInfo method, object source)
            {
                // Sanity check and persist data.
                this.Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
                this.Method = method ?? throw new ArgumentNullException(nameof(method));
                this.Source = source;

                // Validate the method parameters.
                this.ParameterInfo = method.GetParameters();
                switch (this.ParameterInfo.Length)
                {
                    case 1:
                        // Just the vault.  Okay.
                        if (this.ParameterInfo[0].ParameterType != typeof(Vault))
                        {
                            this.Logger.Fatal($"Method {method.Name} expected first parameter of vault, actually {this.ParameterInfo[0].ParameterType.FullName}.");
                            return;
                        }
                        break;
                    case 2:
                        // Vault and something else.
                        if (this.ParameterInfo[0].ParameterType != typeof(Vault))
                        {
                            this.Logger.Fatal($"Method {method.Name} expected first parameter of vault, actually {this.ParameterInfo[0].ParameterType.FullName}.");
                            return;
                        }
                            this.InputType = this.ParameterInfo[1].ParameterType;
                        break;
                    default:
                        this.Logger.Fatal($"Method {method.Name} does not have a valid signature (must have one or two parameters, but declares {this.ParameterInfo.Length}).");
                        return;
                }

            }

            /// <summary>
            /// Gets the <see cref="MethodInfo"/> associated
            /// with the <see cref="Execute(EventHandlerEnvironment)"/> method.
            /// </summary>
            /// <returns></returns>
            public MethodInfo GetExecuteMethodInfo()
                => GetType().GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.Public);

            /// <summary>
            /// The method which will be called by the VAF when the VEM is called.
            /// Proxies the call to <see cref="Method"/>.
            /// </summary>
            /// <param name="env">The vault extension method environment.</param>
            /// <returns>The output for the VEM.</returns>
            public string Execute(EventHandlerEnvironment env)
            {
                // Declare the data we'll pass to the method.
                var methodInputs = new List<object>() { env.Vault };

                // If it's a string then pass it straight in.
                if (InputType == typeof(string))
                    methodInputs.Add(env.Input);
                else
                {
                    // Attempt to deserialize the input.
                    if (this.InputType != typeof(void) && !string.IsNullOrWhiteSpace(env.Input))
                    {
                        try
                        {
                            methodInputs.Add(this.Attribute.Deserialize(env.Input, this.InputType));
                        }
                        catch (Exception e)
                        {
                            var wrappedException = new Exception($"Could not deserialize input to {this.InputType.FullName}.", e);
                            this.Logger.Fatal(wrappedException, $"Could not deserialize input to {this.InputType.FullName}.");
                            return this.Attribute.GetFailedOutput(wrappedException);
                        }
                    }
                }

                // If the method returns something then we'll try to serialize it.
                if (typeof(void) != this.Method.ReturnType)
                {
                    try
                    {
                        return this.Attribute.Serialize(this.Method.Invoke(this.Source, methodInputs.ToArray()));
                    }
                    catch (Exception e)
                    {
                        this.Logger.Fatal(e, $"Exception executing vault extension method.");
                        return this.Attribute.GetFailedOutput(e);
                    }
                }
                else
                {
                    // It returns nothing.
                    try
                    {
                        this.Method.Invoke(this.Source, methodInputs.ToArray());
                        return this.Attribute.GetSuccessfulOutput();
                    }
                    catch (Exception e)
                    {
                        this.Logger.Fatal(e, $"Exception executing vault extension method");
                        return this.Attribute.GetFailedOutput(e);
                    }

                }
            }
        }

    }
}