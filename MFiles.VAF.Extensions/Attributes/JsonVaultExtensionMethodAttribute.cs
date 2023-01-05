using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MFiles.VAF.Extensions
{
    /// <summary>
    /// Defines that the method is a vault extension method that sends/receives data in JSON format.
    /// The method should declare one (single vault) or two (vault plus format of the data in the VEM input) parameters.
    /// The method can optionally have a return type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class JsonVaultExtensionMethodAttribute
        : TypedVaultExtensionMethodAttribute
    {
        public JsonVaultExtensionMethodAttribute(string vaultExtensionMethodName)
            : base(vaultExtensionMethodName)
        {
        }

        /// <summary>
        /// Converts <paramref name="e"/> to a <see cref="JObject"/>
        /// that can be included in the failure details.
        /// </summary>
        /// <param name="e">The exception</param>
        /// <returns>null if <paramref name="e"/> is null, the <see cref="JObject"/> if not.</returns>
        protected virtual JObject GetExceptionJObject(Exception e)
        {
            // Sanity.
            if (e == null) 
                return null;
            var exceptionJObject = new JObject()
            {
                {"type", e.GetType().ToString()},
                {"message", e.Message},
                {"stackTrace", e.StackTrace}
            };

            // Anything inside it?
            if (e.InnerException != null)
                exceptionJObject.Add("innerException", this.GetExceptionJObject(e.InnerException));
            return exceptionJObject;
        }

        /// <inheritdoc />
        public override string GetFailedOutput(Exception e)
        {
            // Create the basic data.
            var jObject = new JObject
            {
                new JProperty("successful", false)
            };

            // Include the exception details if we are told to.
            if (this.IncludeExceptionDetailsInResponse)
            {
                var exceptionJObject = this.GetExceptionJObject(e);
                if(null != exceptionJObject)
                    jObject.Add(new JProperty("exception", exceptionJObject));
            }

            // Return the data to the caller.
            return jObject.ToString();
        }


        /// <inheritdoc />
        public override string GetSuccessfulOutput()
            => @"{ ""successful"": true }";

        /// <inheritdoc />
        protected override object Deserialize(string input, Type t)
            => JsonConvert.DeserializeObject(input, t);

        /// <inheritdoc />
        protected override string Serialize(object input)
            => JsonConvert.SerializeObject(input);
    }
}